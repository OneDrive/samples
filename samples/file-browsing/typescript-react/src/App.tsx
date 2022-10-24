import React from 'react';
import './App.css';
import { IFileBrowserOptions } from "./types";
import { getToken } from "./auth";
import Browser from "./components/Browser";

function App() {

  const paramsTest: IFileBrowserOptions = {
    sdk: "8.0",
    entry: {
      sharePoint: {
        byPath: {
          web: "/sites/dev",
        },
      },
    },
    authentication: {},
    messaging: {
      origin: "http://localhost:3000",
      channelId: "27"
    },
    commands: {
      open: {
        /**
         * Specifies a series of 'handlers' for clicking files and folders.
         * For each handler, the invoked item is tested against `filters`,
         * and the first matching handler has its behavior applied.
         * If no handler matches, the default behavior applies.
         */
        handlers: [{
          // filters: ["!folder"],
          filters: [".txt"],
          /**
           * Specifies the target for opening the item
           * - `none`: Do not allow the item to be opened.
           * - `navigate`: Open the item within the experience.
           * - `external`: Open the item in a new tab.
           * - `host`: Ask the host to open the item.
           */
          target: "host",
        }]
      },
    },
  };

  return (
    <div className="App">
      <Browser baseUrl="https://{tenant}.sharepoint.com/" getToken={getToken} options={paramsTest} />
    </div>
  );
}

export default App;
