import { applyToken } from "./security.js";

const btn = document.getElementById("go");
const log = <HTMLDivElement>document.getElementById("log");

function logger(message: string) {

    const p = document.createElement("p");
    p.innerHTML = message;
    log.prepend(p);
}

btn.addEventListener("click", async (e) => {

    e.preventDefault();

    const input = <HTMLInputElement>document.getElementById("the-file");


    if (input.files && input.files.length > 0) {

        const file = input.files[0];

        const getFolderInit = await applyToken({
            method: "GET",
        });

        const folderInfoResponse = await fetch("https://graph.microsoft.com/v1.0/me/drive/special/approot", getFolderInit);

        const folderInfo = await folderInfoResponse.json();

        const { driveId } = folderInfo.parentReference;

        // rebase the URL to the /drives/{id} pattern
        const baseUrl = `https://graph.microsoft.com/v1.0/drives/${driveId}/items/${folderInfo.id}`;

        // 1. create upload session
        const createUploadSessionUrl = `${baseUrl}:/${encodeURIComponent(file.name)}:/createUploadSession`;
        const createUploadSessionInit = await applyToken({
            method: "POST",
            body: JSON.stringify({
                item: {
                    "@microsoft.graph.conflictBehavior": "rename",
                    name: file.name,
                },
            }),
            headers: {
                "Content-Type": "application/json",
            }
        });
        const uploadSessionInfoResponse = await fetch(createUploadSessionUrl, createUploadSessionInit);
        const uploadSessionInfo = await uploadSessionInfoResponse.json();
        if (!uploadSessionInfoResponse.ok) {
            throw Error(JSON.stringify(uploadSessionInfo, null, 2));
        }
        logger(`Created upload session: <pre>${JSON.stringify(uploadSessionInfo, null, 2)}</pre>`);

        // this is the upload url for our new file, it is opaque
        const { uploadUrl } = uploadSessionInfo;

        // 2. Get a stream representing the file
        const stream = file.stream();
        const reader = stream.getReader();

        // 3. Use the reader pattern (or any other) to upload the file
        let bytePointer = 0;
        let uploadBytesInfo;
        reader.read().then(async function pump({ done, value }) {

            if (done) {

                if (value) {
                    console.error("had value at done");
                }

                logger(`Upload complete <a href="${uploadBytesInfo.webUrl}" target="_blank">${uploadBytesInfo.name}</a>`);

                return;
            }

            const contentLength = value.length - 1;
            const contentRangeStr = `bytes ${bytePointer}-${bytePointer + contentLength}/${(file.size)}`;
            logger(`Uploading: ${contentRangeStr}`);

            // 4. Execute a series of PUT commands to upload the file in chunks
            const uploadBytesInit = await applyToken({
                method: "PUT",
                body: value,
                headers: {
                    "Content-Length": contentLength.toString(),
                    "Content-Range": contentRangeStr,
                }
            });
            const uploadBytesInfoResponse = await fetch(uploadUrl, uploadBytesInit);
            uploadBytesInfo = await uploadBytesInfoResponse.json();

            if (!uploadBytesInfoResponse.ok) {
                throw Error(JSON.stringify(uploadBytesInfo));
            }

            bytePointer += value.length;

            // Read some more, and call this function again
            return reader.read().then(pump);
        });

    } else {

        alert("Please select a file.");
    }
});
