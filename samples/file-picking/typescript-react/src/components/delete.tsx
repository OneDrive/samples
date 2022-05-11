import { SPItem } from "@pnp/picker-api";
import { request } from "../client";

// example showing how to delete a selected file
// NOTE: this doesn't bother to update the UI when a file is deleted
function Delete(item: SPItem) {

    let running = false;

    const onClick = async () => {

        if (running) {
            return;
        }

        running = true;

        try {

            // delete this file (IT WILL REALLY DELETE IT!!)
            await request(`drives/${item.parentReference.driveId}/items/${item.id}`, { method: "DELETE" });

        } finally {

            running = false;
        }
    }

    return (<button onClick={onClick}>Delete</button>);
}

export default Delete;
