import { execFileSync } from "child_process";
import { log, enter, leave } from "../logger";
import { resolve } from "path";
import { randomString } from "../utils";

const WriteLocalSSLCerts = (folderPath: string) => {

    enter("WriteLocalSSLCerts");

    log("Generating certs...");

    const passphrase = randomString(50);

    log(`Using passphrase: ${passphrase}`);

    execFileSync("openssl", [
        "req",
        "-x509",
        "-newkey",
        "rsa:2048",
        "-keyout",
        `${resolve(folderPath, "keytmp.pem")}`,
        "-out",
        `${resolve(folderPath, "cert.pem")}`,
        "-days",
        "365",
        "-passout",
        `pass:${passphrase}`,
        "-subj",
        "/C=US/ST=Washington/L=Seattle/CN=localhost",
    ]);

    execFileSync("openssl", [
        "rsa",
        "-in",
        `${resolve(folderPath, "keytmp.pem")}`,
        "-out",
        `${resolve(folderPath, "key.pem")}`,
        "-passin",
        `pass:${passphrase}`,
    ]);

    log("Generated certs.");

    leave("WriteLocalSSLCerts");
};
export default WriteLocalSSLCerts;

// openssl req -x509 -newkey rsa:2048 -keyout ./dev-secrets/keytmp.pem -out ./dev-secrets/cert.pem -days 365

// openssl rsa -in ./dev-secrets/keytmp.pem -out ./dev-secrets/key.pem

