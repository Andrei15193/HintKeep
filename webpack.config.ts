import type { Configuration } from "webpack";
import path from "path";
import HtmlInlineCSSWebpackPlugin from "html-inline-css-webpack-plugin";
import HtmlWebpackPlugin from "html-webpack-plugin";
import MiniCssExtractPlugin from "mini-css-extract-plugin";
import "webpack-dev-server";

interface IBuildOptions {
    readonly mode?: "development" | "production";
}

export default function (_: any, { mode }: IBuildOptions): Configuration {
    return {
        entry: {
            app: path.resolve(__dirname, "index.ts"),
            index: path.resolve(__dirname, "index.scss")
        },
        mode: "development",
        devtool: mode === "production" ? false : "inline-source-map",
        output: {
            clean: true,
            filename: mode === "production" ? "[name].[fullhash].js" : "[name].js",
            chunkFilename: mode === "production" ? "[id].[fullhash].js" : "[id].js",
            path: path.resolve(__dirname, "bin")
        },
        module: {
            rules: [
                {
                    test: /\.tsx?$/,
                    exclude: /node_modules/,
                    use: "ts-loader"
                },
                {
                    test: /\.scss$/i,
                    exclude: [/node_modules/],
                    use: [
                        MiniCssExtractPlugin.loader,
                        "css-loader",
                        "sass-loader"
                    ]
                }
            ]
        },
        plugins: [
            new HtmlWebpackPlugin({
                template: "!!handlebars-loader!index.hbs",
                title: "HintKeep",
                meta: {
                    charset: "utf-8",
                    viewport: "width=device-width,initial-scale=1"
                }
            }),
            new MiniCssExtractPlugin({
                filename: mode === "production" ? "[name].[fullhash].css" : "[name].css"
            }),
            new HtmlInlineCSSWebpackPlugin({
                filter(fileName) {
                    return /^index(\.[a-z0-9]+)?\.(css|html)$/i.test(fileName);
                }
            })
        ],
        resolve: {
            extensions: [
                ".tsx",
                ".ts",
                ".jsx",
                ".js"
            ]
        },
        devServer: {
            open: true,
            client: {
                overlay: false
            }
        }
    };
}