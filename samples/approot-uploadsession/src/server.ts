import express from "express";
import { dirname, resolve } from "path";
import { fileURLToPath } from "url";
import open from "open";


const __dirname = dirname(fileURLToPath(import.meta.url));

const server = express();
const port = 8080;

// setup static paths
server.use("/bundles", express.static(resolve(__dirname, "./bundles")));
server.use(express.static(resolve(__dirname, "../static")));

// start the server
server.listen(port, () => {

    // open the default browser to the sample page
    open("http://localhost:8080");
});
