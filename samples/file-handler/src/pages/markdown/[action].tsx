/**
 * Page used to preview, edit, and create markdown files within our File Handler
 */
import { useState } from "react";
import Head from "next/head";
import { useRouter } from "next/router";
import { GetServerSideProps } from "next/types";

import useDebounce from "../../hooks/useDebounce";

import Layout from "../../components/layout";
import MonacoEditor from "../../components/monaco-editor";
import MarkdownPreview from "../../components/markdown-preview";

import { IActivationProps } from "../../lib/types";
import { withSession } from "../../lib/withSession";
import { initHandler } from "../../lib/initHandler";

import { v4 } from "uuid";

import {
  PrimaryButton,
  DefaultButton,
  MessageBarButton,
  MessageBar,
  MessageBarType,
  IStackTokens,
  Stack,
} from "office-ui-fabric-react/lib-commonjs";

const stackTokens: IStackTokens = { childrenGap: 40 };

// shape of our message bar state
interface MessageBarState {
  show: boolean;
  message?: string;
  type?: MessageBarType;
}

/**
 * nextjs method to handle server-side initialization of the page
 * 
 * @see https://nextjs.org/docs/basic-features/data-fetching#getserversideprops-server-side-rendering
 */
const getServerSidePropsHandler: GetServerSideProps = async ({ req, res }) => {

  const [token, activationParams] = await initHandler(req as any, res);

  if (token === null) {
    // return nothing if we don't have a token, we are likely doing a redirect
    return { props: {} };
  }

  // read the file from the url supplied via the activation params
  // we do this on the server due to cors and redirect issues when trying to do it on the client
  const response = await fetch(`${activationParams.items[0]}/content`, {
    headers: {
      "authorization": `Bearer ${token}`,
    },
  });

  // set the content for the client
  activationParams.content = await response.text();

  return {
    props: {
      activationParams,
    },
  };
};
export const getServerSideProps = withSession(getServerSidePropsHandler);

/**
 * Our functional page component
 * 
 * @param props supplied by getServerSidePropsHandler
 */
const Handler = (props: { activationParams: IActivationProps }) => {

  if (!props.activationParams) {
    return;
  }

  // track the content in state
  const [content, setContent] = useState<string>(props.activationParams.content);

  // track if there are changes to the content
  const [dirty, setDirty] = useState(false);

  // track our message bar state
  const [messageBar, setMessageBar] = useState<MessageBarState>({ show: false });

  // determine what action is being invoked (create/edit/preview)
  const { action } = useRouter().query;

  // handle content changes
  function contentChange(value: string) {
    // update our content state
    setContent(value);

    // update our dirty flag
    if (!dirty) {
      setDirty(true);
    }
  }

  // handle close
  function close(skipDirty = false) {
    if (!skipDirty && dirty && !confirm("You have unsaved changes, are you sure you want to close?")) {
      return;
    }
    window.close();
  }

  // handle save
  function save(andClose: boolean): any {

    return async () => {

      // let them know we are doing something
      setMessageBar({ show: true, type: MessageBarType.info, message: "Saving Changes..." });

      try {
        const response = await fetch("/api/filehandler/save", {
          body: JSON.stringify({
            content: content,
            fileUrl: props.activationParams.items[0],
            requestId: v4(),
          }),
          headers: {
            "Content-Type": "application/json",
          },
          method: "POST",
        });

        if (response.ok) {

          setDirty(false);

          if (andClose) {
            close(true);
          }

          // show message bar
          setMessageBar({ show: true, type: MessageBarType.success, message: "Success: changes saved." });

        } else {
          throw Error();
        }

      } catch (e) {

        // show error bar
        setMessageBar({ show: true, type: MessageBarType.error, message: "Error: There was a problem saving your changes." });
      }
    };
  }

  function hideMessageBar(): void {
    setMessageBar({ show: false });
  }

  if (action === "preview") {

    return (
      <Layout>
        <Head>
          <title>{action} Markdown File</title>
        </Head>

        <article>
          <h1 className="ms-fontWeight-bold ms-fontSize-28">Preview</h1>

          <div className="ms-Grid" dir="ltr">
            <div className="ms-Grid-row">
              <div className="ms-Grid-col ms-sm12">
                <div className="ms-depth-8">
                  <MarkdownPreview markdown={content} />
                </div>
              </div>
            </div>
          </div>
        </article>
      </Layout >
    );
  } else {

    // help reduce costly re-renders of the preview content
    const debouncedContent = useDebounce(content);

    return (
      <Layout>
        <Head>
          <title>{action} Markdown File</title>
        </Head>

        <article>
          <h1 className="ms-fontWeight-bold ms-fontSize-28">{action} Markdown</h1>

          <div className="ms-Grid" dir="ltr">
            <div className="ms-Grid-row">
              <div className="ms-Grid-col ms-sm12">
                <Stack horizontal tokens={stackTokens} className="action-buttons">
                  <DefaultButton text="Close" allowDisabledFocus onClick={() => close()} />
                  <PrimaryButton text="Save" allowDisabledFocus menuProps={{
                    items: [
                      {
                        iconProps: { iconName: "Mail" },
                        key: "save",
                        onClick: save(false),
                        text: "Save",
                      },
                      {
                        iconProps: { iconName: "Mail" },
                        key: "saveandclose",
                        onClick: save(true),
                        text: "Save & Close",
                      },
                    ],
                  }} />
                </Stack>
              </div>
            </div>
            {messageBar.show &&
              <div className="ms-Grid-row">
                <div className="ms-Grid-col ms-sm12">
                  <MessageBar actions={
                    <div>
                      <MessageBarButton onClick={hideMessageBar}>Close</MessageBarButton>
                    </div>
                  } isMultiline={false} messageBarType={messageBar.type}>{messageBar.message}</MessageBar>
                </div>
              </div>
            }
            <div className="ms-Grid-row">
              <div className="ms-Grid-col ms-sm6">
                <MonacoEditor
                  defaultValue={content}
                  onChange={contentChange}
                  width="100%"
                  height="600px"
                  language="markdown"
                  options={{
                    selectOnLineNumbers: true,
                    wordWrap: "on",
                  }}
                  theme="vs" />
              </div>
              <div className="ms-Grid-col ms-sm6">
                <div className="ms-depth-8">
                  <MarkdownPreview markdown={debouncedContent} />
                </div>
              </div>
            </div>
          </div>
        </article>
      </Layout >
    );
  }
};
export default Handler;
