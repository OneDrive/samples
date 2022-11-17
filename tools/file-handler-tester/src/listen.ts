import { createServer } from "https";
import { readFileSync } from "fs";
import { argv } from "process";

let port = 3000;
let verbose = false;

for (let i = 2; i < argv.length; i++) {
    if (argv[i] === "--port" || argv[i] === "-p") {
        port = parseInt(argv[++i], 10);
    } else if (argv[i] === "--verbose") {
        verbose = true;
    }
}

createServer({
    key: readFileSync('key.pem'),
    cert: readFileSync('cert.pem'),
},
    (req, res) => {

        if (verbose) {
            console.log(req.method, req.url, req.headers);
        }

        var body = "";

        req.on("readable", function () {
            const part = req.read();
            if (part && part !== null) {
                body += part;
            }
        });

        req.on("end", async function () {
            // close the response before processing the notification
            res.end();

            console.log(body);

            console.log();
            console.log();

            const params = new URLSearchParams(body);

            console.log(params);
        });

    }).listen(port, function () {

        if (arguments[0]) {
            throw arguments[0];
        }

        console.log(`server listening on ${port}`);
    });
