const MonacoWebpackPlugin = require("monaco-editor-webpack-plugin");
const withCSS = require("@zeit/next-css");
const withFonts = require("next-fonts");

module.exports = withCSS(withFonts({

  poweredByHeader: false,

  webpack(config, { buildId, dev, isServer, defaultLoaders, webpack }) {

    config.module.rules.push({
      test: /\.(eot|woff|woff2|ttf|svg|png|jpg|gif)$/,
      use: {
        loader: 'url-loader',
        options: {
          limit: 100000,
          name: '[name].[ext]',
        },
      },
    });

    config.plugins.push(new MonacoWebpackPlugin({
      languages: ["markdown", "javascript", "typescript"],
      filename: "static/[name].worker.js",
    }));

    return config;
  },

  typescript: {
    ignoreDevErrors: true,
    ignoreBuildErrors: false,
  }
}));
