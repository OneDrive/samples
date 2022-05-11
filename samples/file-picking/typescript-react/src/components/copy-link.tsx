import { SPItem } from "@pnp/picker-api";

function CopyLink(item: SPItem) {

    const onClick = () => {

        // copy the webUrl to the clipboard - this would work for any of the document data (i.e. you could have a copy title button)
        navigator.clipboard.writeText(item.webUrl);
    }

    return (<button onClick={onClick}>Copy Link</button>);
}

export default CopyLink;
