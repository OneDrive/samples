const webpack = require("webpack");
const path = require("path");
const dotenv = require("dotenv").config({
  path: path.join(__dirname, ".env")
});

module.exports = {
  entry: "./src/main.ts",
  mode: "development",
  module: {
    rules: [
      {
        test: /\.tsx?$/,
        use: "ts-loader",
        exclude: /node_modules/,
      },
    ],
  },
  plugins: [
    new webpack.DefinePlugin({
      "process.env": dotenv.parsed,
    }),
  ],
  resolve: {
    extensions: [".tsx", ".ts", ".js"],
  },
  output: {
    libraryTarget: "umd",
    filename: "bundle.js",
    path: path.resolve(__dirname, "dist"),
  },
  devServer: {
    static: {
      directory: path.join(__dirname, "static"),
    },
    compress: true,
    port: 3000,
  },
};
