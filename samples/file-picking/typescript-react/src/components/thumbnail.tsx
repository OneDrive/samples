import { isArray } from "@pnp/core";
import { SPItem } from "@pnp/picker-api";
import { useEffect, useState } from "react";
import { request } from "../client";

interface ThumbnailImageInfo {
    height: number;
    width: number;
    url: string;
}

interface ThumbnailInfo {
    value: [
        {
            id: string;
            small: ThumbnailImageInfo;
            medium: ThumbnailImageInfo;
            large: ThumbnailImageInfo;
        }
    ]
}

function Thumbnail(item: SPItem) {

    // default state here could also be a loading image
    const [thumbnail, setThumbnail] = useState<ThumbnailImageInfo>(null);

    useEffect(() => {

        let active = true
        load()
        return () => { active = false }

        async function load() {
            // here we load a thumbnail of the document
            const previewInfo = await request<ThumbnailInfo>(`drives/${item.parentReference.driveId}/items/${item.id}/thumbnails`);
            if (!active) { return }
            if (isArray(previewInfo.value) && previewInfo.value.length > 0) {
                setThumbnail(previewInfo.value[0].medium);
            }
        }

    }, [item.id, item.parentReference.driveId]);

    return ((thumbnail && <img src={`${thumbnail.url}`} alt={item.name} height={thumbnail.height} width={thumbnail.width} />) || <div>Loading...</div>);
}

export default Thumbnail;