import { SPItem } from "@pnp/picker-api";
import { request } from "../client";

interface PreviewInfo {
    getUrl: string;
    postParameters: string;
    postUrl: string;
}

// opens a preview of the document in a new window
function Preview(item: SPItem) {

    let running = false;

    const onClick = async () => {

        if (running) {
            return;
        }

        running = true;

        try {

            const previewInfo = await request<PreviewInfo>(`drives/${item.parentReference.driveId}/items/${item.id}/preview`, { method: "POST" });

            window.open(previewInfo.getUrl, "demo_preview_window");

        } finally {

            running = false;
        }
    }

    return (<button onClick={onClick}>Preview</button>);
}

export default Preview;
