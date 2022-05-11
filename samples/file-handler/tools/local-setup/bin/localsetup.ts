#!/usr/bin/env node

import setup from "../index";

(async function () {

    // we just run the setup
    await setup();

    process.exit();
})();
