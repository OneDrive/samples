import { SPItem } from "@pnp/picker-api";

// example showing how to create a simple download link from the file information
function Download(item: SPItem) {

    return (<a href={item.webDavUrl} download={item.name}>Download</a>);
}

export default Download;
