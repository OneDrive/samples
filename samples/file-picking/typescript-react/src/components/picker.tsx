import React from "react";
import { MouseEvent, useEffect, useState } from "react";
import { Picker, IFilePickerOptions, Popup, IAuthenticateCommand, IPicker, LamdaAuthenticate, SPItem } from "@pnp/picker-api";

export interface PickerProps {
  baseUrl: string;
  getToken: (message: IAuthenticateCommand) => Promise<string>;
  options: IFilePickerOptions;
  onResults: (items: SPItem[]) => void;
}

// picker button used to launch the picker
function PickerButton(props: PickerProps) {

  const { baseUrl, getToken, options } = props;

  const [contentWindow, setContentWindow] = useState<Window | null>(null);

  const [picker, setPicker] = useState<IPicker | null>(null);

  useEffect(() => {

    const { onResults } = props;

    if (picker) {

      // optionally log notifications to the console
      picker.on.notification(function (this: IPicker, message) {
        this.log("notification: " + JSON.stringify(message));
      });

      // optionially log any logging from the library itself to the console
      picker.on.log(function (this: IPicker, message, level) {
        console.log(`log: [${level}] ${message}`);
      });

      // optionially log any logging from the library itself to the console
      picker.on.error(function (this: IPicker, err) {
        this.log(`error: ${err}`);
      });

      (async () => {

        const results: any = await picker.activate({
          baseUrl,
          options,
        });

        onResults(results.items);

      })();
    }

    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [picker]);

  useEffect(() => {

    if (contentWindow) {

      // create and set the picker API using the content window
      setPicker(Picker(contentWindow).using(
        Popup(),
        LamdaAuthenticate(getToken)));

    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [contentWindow]);

  async function click(e: MouseEvent<HTMLButtonElement>): Promise<void> {

    e.preventDefault();

    // open a pop-up
    setContentWindow(window.open("", "Picker", "width=800,height=600"));
  }

  return (
    <button onClick={click}>Launch Picker</button>
  );
}

export default PickerButton;
