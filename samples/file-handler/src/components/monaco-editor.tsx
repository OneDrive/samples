import dynamic from "next/dynamic";
import { MonacoEditorProps } from "react-monaco-editor";

// we can only include the MonacoEditor control on the client as it is not designed to work server-side
const MonacoEditor = dynamic(import("react-monaco-editor"), { ssr: false });

// augment some typings
declare var window: Window & typeof globalThis & { MonacoEnvironment: any };

// the monaco editor requires some help to find the worker files in the way nextjs will bundle them
// we re-route the calls to the _next/static folder
function editorDidMount(editor, _monaco) {

    window.MonacoEnvironment.getWorkerUrl = (_moduleId: string, label: string) => {

        if (label === "typescript" || label === "javascript") {
            return "/_next/static/ts.worker.js";
        }

        return "/_next/static/editor.worker.js";
    };

    // give the editor focus
    editor.focus();
}

// we create a small wrapper for the editor and export it
const Editor = (props: Exclude<MonacoEditorProps, "editorDidMount">) => (<MonacoEditor editorDidMount={editorDidMount} {...props} />);
export default Editor;
