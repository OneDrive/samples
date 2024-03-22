import webpack from "webpack";
import { resolve, dirname } from "path";
import { fileURLToPath } from "url";

const __dirname = dirname(fileURLToPath(import.meta.url));

const NODE_ENV = "development";

export default <webpack.Configuration[]>[
    {
        devtool: "eval-source-map",
        entry: resolve(__dirname, "../build/main.js"),
        mode: NODE_ENV,
        output: {
            library: {
                name: 'BrowserEntry',
                type: 'umd',
            },
            filename: "client-bundle.js",
            path: resolve(__dirname, "../build/bundles"),
        },
        stats: {
            assets: false,
            colors: true,
        },
    }
];
