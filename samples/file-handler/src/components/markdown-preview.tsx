import { useEffect, useState } from "react";
import { parse } from "marked";
import { sanitize } from "dompurify";

export interface IMarkdownPreviewProps {
    markdown: string;
}

/**
 * React functional component providing a read-only preview of Markdown content
 * 
 * @param props Used to set the content and other control behaviors
 */
const MarkdownPreview = (props: IMarkdownPreviewProps) => {

    // we get the raw markdown from the props
    const { markdown } = props;

    // setup state hooks for handling the content. This allows for async processing with useEffect
    const [parsed, setParsed] = useState<string>("Processing...");

    // on page load, and dependent on markdown changing we process the content from
    // raw markdown into html
    useEffect(() => {

        // self-call an async function as useEffect doesn't allow it at the top
        (async () => {

            // craft the parsing into a promise
            const updated = await new Promise<string>(resolve => {

                parse(markdown, (err, result) => {
                    if (err) {
                        result = err?.message;
                    }

                    resolve(sanitize(result));
                });
            });

            // once we have the content set it
            setParsed(updated);
        })();
    }, [markdown]);

    // we inject the updated content into an iframe so anything
    // it is doing doesn't have access to the current page
    return (<iframe src={`data:text/html;charset=utf-8,${parsed}`} />);
};
export default MarkdownPreview;
