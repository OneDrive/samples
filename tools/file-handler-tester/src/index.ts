import { argv } from "process";
import { readFileSync } from "fs";
import fetch from "node-fetch";

const params = {
    url: "",
    bodyPath: "",
}

for (let i = 2; i < argv.length; i++) {

    if (argv[i] === "--url" || argv[i] === "-u") {
        params.url = argv[++i];
    } else if (argv[i] === "--file" || argv[i] === "-f") {
        params.bodyPath = argv[++i];
    }
}

const data = JSON.parse(readFileSync(params.bodyPath, "utf-8"));

const keys: string[] = <any>Reflect.ownKeys(data);

const body = [];

for(let i = 0; i < keys.length; i++) {
    const key = keys[i];

    const value = key === "items" ? JSON.stringify(data[keys[i]]) : data[keys[i]];

    body.push(`${key}=${encodeURIComponent(value)}`);   
}

const u = await fetch(params.url, {
    headers: {
        "Content-Type": "application/x-www-form-urlencoded",
    },
    body: body.join("&"),
    method: "POST",
});

const y = await u.text();

console.log(y);





// POST https://contoso.com/_api/filehandlers/preview
// 

// cultureName=en-us&client=OneDrive&userId=rgregg%40contoso.com&items=%5B%22https%3A%2F%2Fgraph.microsoft.com%2Fv1.0%2Fme%2Fdrive%2Fitems%2F4D679BEA-6F9B-4106-AB11-101DDE52B06E%22%5D

// [
//     'C:\\Program Files\\nodejs\\node.exe',
//     'C:\\github\\od-samples\\tools\\file-handler-tester\\lib\\index.js',
//     '4'
//   ]





