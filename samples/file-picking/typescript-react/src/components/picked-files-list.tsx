import { SPItem } from "@pnp/picker-api";
import Thumbnail from "./thumbnail";
import Preview from "./preview";
import CopyLink from "./copy-link";
import Download from "./download";
import Delete from "./delete";

export interface PickedFileProps {
    items: SPItem[];
}

// here we create a set of actions that will be applied to each selected file
const actions = [
    Preview,
    CopyLink,
    Download,
    Delete,
];

// this is the picked file list control that renders a simply table
function pickedFilesList(props: PickedFileProps) {

    const { items } = props;

    return (
        <div>
            <table>
                {
                    (items && items.map((item) => {

                        // we generate a thumbnail for each document, and then list any actions to the right
                        return (<tr><td>{Thumbnail(item)}</td>{actions && actions.map(action => (<td>{action(item)}</td>))}</tr>)

                    })) || <div>No Items selected</div>
                }
            </table>
        </div>
    );

}

export default pickedFilesList;
