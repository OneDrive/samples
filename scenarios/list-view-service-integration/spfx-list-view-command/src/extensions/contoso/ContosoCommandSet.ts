import { override } from '@microsoft/decorators';
import { Log } from '@microsoft/sp-core-library';
import {
  BaseListViewCommandSet,
  IListViewCommandSetListViewUpdatedParameters,
  IListViewCommandSetExecuteEventParameters
} from '@microsoft/sp-listview-extensibility';
import { Dialog } from '@microsoft/sp-dialog';
import { AadHttpClient, HttpClientResponse } from '@microsoft/sp-http';

import * as strings from 'ContosoCommandSetStrings';

/**
 * If your command set uses the ClientSideComponentProperties JSON input,
 * it will be deserialized into the BaseExtension.properties object.
 * You can define an interface to describe it.
 */
export interface IContosoCommandSetProperties {
  /**
   * abolute base url to the azure functions app
   */
  apiAbsUrl: string;
  /**
   * app id of the AAD application backing the functions app
   */
  appId: string;
}

const LOG_SOURCE: string = 'ContosoCommandSet';

export default class ContosoCommandSet extends BaseListViewCommandSet<IContosoCommandSetProperties> {

  private processing = false;

  @override
  public async onInit(): Promise<void> {
    Log.info(LOG_SOURCE, 'Initialized ContosoCommandSet.');
  }

  @override
  public onListViewUpdated(event: IListViewCommandSetListViewUpdatedParameters): void { }

  @override
  public onExecute(event: IListViewCommandSetExecuteEventParameters): void {

    // determine which event fired, you can have multiple commands registered for a single class
    // in this sample we only have one, but we retain the pattern
    switch (event.itemId) {

      case "InvokeServiceCommand":

        // create an async function to handle the web request to the API
        // setTimeout with 0 will execute on the next event loop
        setTimeout(async () => {

          if (this.processing) {
            // we have a pending request
            return;
          }

          this.processing = true;

          try {

            // we create an aadHttpClient based on the url of the the api we will call.
            const client = await this.context.aadHttpClientFactory.getClient(this.properties.appId);

            const reqUrl = (new URL("/api/ReceiveInvocation", this.properties.apiAbsUrl)).toString();

            const res: HttpClientResponse = await client.post(reqUrl, AadHttpClient.configurations.v1, {
              body: JSON.stringify({
                user: this.context.pageContext.user,
                siteId: this.context.pageContext.site.id.toString(),
                siteUrl: this.context.pageContext.site.absoluteUrl,
                webId: this.context.pageContext.web.id.toString(),
                webRelUrl: this.context.pageContext.web.serverRelativeUrl,
                webAbsUrl: this.context.pageContext.web.absoluteUrl,
                listId: this.context.pageContext.list.id.toString(),
                listUrl: this.context.pageContext.list.serverRelativeUrl,
                listTitle: this.context.pageContext.list.title,
                aadInfo: {
                  tenantId: this.context.pageContext.aadInfo.tenantId.toString(),
                  userId: this.context.pageContext.aadInfo.userId.toString(),
                },
              }),
            });

            if (!res.ok) {

              const resp = await res.text();
              console.error(Error(`Error [${res.status}: ${res.statusText}]: ${resp}`));

            } else {

              Dialog.alert("API Invoked Successfully");
            }

          } catch (e) {

            console.error(e);

          } finally {

            this.processing = false;
          }

        }, 0);

        break;

      default:
        throw Error('Unknown command');
    }
  }
}
