var filePicker = new function () {

    var messagingPort;
    var expectedChannelId = '15';
    var postBackUrl;
    var modalDialog;
    var messageLevel0;
    var messageLevel1;
    var messageLevel2;
    var getAccessTokenPromise;

    function initialListener(event) {
        console.log('====window message received====');
        console.log('type= ' + event.data.type);
        console.log('channelid= ' + event.data.channelId);
        //debugger;
        console.log('port = ' + event.ports[0]);
        console.log('===============================');

        // check that channel matches with the channel you've used to configure the file picker
        if (event.data.channelId === expectedChannelId) {

            // grab the port to use for messaging with the iframe
            messagingPort = event.ports[0];

            // detach event listener from window as from now on the messages will be sent to the messageport
            this.window.removeEventListener('message', initialListener);

            // hookup the event listener on the received port
            messagingPort.addEventListener('message', messageListener);
            messagingPort.start();

            // start communication by sending the active message
            messagingPort.postMessage({ type: 'activate' });
        }
        else {
            console.error("ChannelId mismatch");
        }
    }

    function messageListener(event) {
        console.log('====iframe message received====');
        //debugger;
        console.log(event.data.type);

        $(messageLevel0).html('Type: ' + event.data.type);

        switch (event.data.type) {
            case 'command':
                console.log(event.data.data.command);

                //debugger;                
                $(messageLevel1).html('Action: ' + event.data.data.command);

                // acknowledge message was received
                sendAcknowledgement(event.data.id);

                switch (event.data.data.command) {
                    case 'pick':

                        $(modalDialog).modal('hide');

                        console.log('file picked');
                        //debugger;
                        event.data.data.items.forEach((item, index) => {
                            console.log('item' + index);
                            console.log('graph item id: ' + item.id);
                            console.log('graph drive id: ' + item.parentReference.driveId);
                        });

                        $(messageLevel2).html('Item id: ' + event.data.data.items[0].id);

                        // do something
                        $.ajax({
                            type: 'POST',
                            url: postBackUrl,
                            data: {
                                AccessToken: "",
                                FileId: event.data.data.items[0].id,
                                DriveId: event.data.data.items[0].parentReference.driveId
                            },
                            success: function (result) {
                                $("#selectedThumbnailImg").attr("src", result);
                                console.log('Data received: ');
                                console.log(result);
                                // if success:
                                sendCommandResult(event.data.id);
                            },
                            error: function (error) {
                                console.error('ajax error: ' + error);
                                // if failure:
                                sendErrorResult(event.data.id, 'Something went wrong!');
                            }
                        });

                        // disconnect the listener now that the action happened
                        // messagingPort.removeEventListener('message', messageListener);

                        break;
                    case 'close':
                        console.log('file picker closed');

                        // disconnect the listener now that the action happened
                        // messagingPort.removeEventListener('message', messageListener);

                        $(modalDialog).modal('hide');
                        break;
                    case 'authenticate':
                        console.log('file picker requests a new token of type: ' + event.data.data.type);
                        if (event.data.data.type === 'SharePoint' ||
                            event.data.data.type === 'SharePoint_SelfIssued' ||
                            event.data.data.type === 'SharePoint_media' ||
                            event.data.data.type === 'Graph') {
                            //debugger;
                            getAccessTokenPromise(event.data.data.resource).then(function (token) {
                                sendAuthenticationResult(event.data.id, token);
                                console.log('token retrieved and sent to iframe');
                            }, function (err) {
                                console.error(err);
                            });
                        }
                        else {
                            console.log('Token acquisition skipped')
                        }
                        break;
                    default:
                        // In response to any unrecognized command, send an 'unsupported' error.
                        sendErrorResult(event.data.id, 'Unsupported command received!');
                        break;
                }                

                break;
            case 'notification':
                console.log(event.data.data.notification);

                $(messageLevel1).html('Action: ' + event.data.data.notification);

                switch (event.data.data.notification) {
                    case 'page-loaded':
                        // your logic
                        break;
                    case 'navigation-started':
                        // your logic
                        break;
                    case 'navigation-ended':
                        // your logic
                        break;
                }

                break;
        }        
    }

    function sendAcknowledgement(id) {
        messagingPort.postMessage({ type: 'acknowledge', id: id});
    }

    function sendCommandResult(id) {
        messagingPort.postMessage({ type: 'result', id: id, data: {} });
    }

    function sendAuthenticationResult(id, token) {
        messagingPort.postMessage({ type: 'result', id: id, data: { result: 'token', token: token} });
    }

    function sendErrorResult(id, error) {
        messagingPort.postMessage({ type: 'result', id: id, data: { error: { code: '1', message: error } } });
    }

    // connect to the actual page hosting the file picker iframe
    this.init = function (url, modal, message, getAccessToken) {
        postBackUrl = url;
        console.log('Postback url set to: ' + url);
        modalDialog = modal;
        console.log('modalDialog set to: ' + modal);
        messageLevel0 = message + '0';
        messageLevel1 = message + '1';
        messageLevel2 = message + '2';
        getAccessTokenPromise = getAccessToken;
    }

    // hookup the listener so we can setup the file picker
    if (window.addEventListener) {
        console.log('adding message event handler on the window');
        window.addEventListener('message', initialListener);
    }

}