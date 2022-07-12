import React, { useState } from 'react';
import './App.css';
import { getToken } from "./auth";
import PickerButton from './components/picker';
import { IFilePickerOptions } from "@pnp/picker-api";
import PickedFilesList from "./components/picked-files-list";

const paramsTest: IFilePickerOptions = {
  sdk: "8.0",
  entry: {
    oneDrive: {}
  },
  authentication: {},
  messaging: {
    origin: "http://localhost:3000",
    channelId: "27"
  },
  selection: {
    mode: "multiple",
  },
  typesAndSources: {
    // filters: [".docx"],
    mode: "files",
    pivots: {
      oneDrive: true,
      recent: true,
    }
  }
};

function App() {

  const [results, setResults] = useState(null);

  function onPicked(pickerResults) {
    if (pickerResults) {
      setResults(pickerResults);
    }
  }


  return (
    <div className="App">
      Launch the picker using this button: <br />
      <PickerButton baseUrl="https://{tenant}-my.sharepoint.com/" getToken={getToken} options={paramsTest} onResults={onPicked} />
      <PickedFilesList items={results} />
    </div>
  );
}

export default App;
