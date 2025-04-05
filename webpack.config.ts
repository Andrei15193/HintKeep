import type { Configuration } from "webpack";
import path from "path";
import CopyWebpackPlugin from "copy-webpack-plugin";
import HtmlInlineCSSWebpackPlugin from "html-inline-css-webpack-plugin";
import HtmlWebpackPlugin from "html-webpack-plugin";
import MiniCssExtractPlugin from "mini-css-extract-plugin";
import "webpack-dev-server";

interface IBuildOptions {
    readonly mode?: "development" | "production";
}

export default function (_: any, { mode }: IBuildOptions): Configuration {
    let generatedChunkId = 0;

    return {
        entry: {
            index: path.resolve(__dirname, "index.scss"),
            app: path.resolve(__dirname, "index.ts")
        },
        mode: "development",
        devtool: mode === "production" ? false : "inline-source-map",
        optimization: {
            chunkIds: "named"
        },
        output: {
            clean: true,
            filename: mode === "production" ? "[name].[contenthash].js" : "[name].js",
            chunkFilename(pathData) {
                const chunkId = pathData.chunk?.id;
                const packageChunkMapping = typeof chunkId === "string"
                    ? packageChunkMappings.find((mapping) => mapping.pattern.test(chunkId))
                    : undefined;

                if (packageChunkMapping)
                    return `${packageChunkMapping.packageId}.[contenthash].js`;
                else {
                    generatedChunkId++;
                    if (mode === "production")
                        return `${generatedChunkId}.[fullhash].js`;
                    else
                        return `${generatedChunkId}.js`;
                }
            },
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
            new CopyWebpackPlugin({
                patterns: [
                    {
                        from: path.resolve(__dirname, "_redirects"),
                        to: path.resolve(__dirname, "bin")
                    }
                ]
            }),
            new HtmlWebpackPlugin({
                template: "!!handlebars-loader!index.hbs",
                title: "HintKeep",
                inject: "body",
                meta: {
                    charset: "utf-8",
                    viewport: "width=device-width,initial-scale=1"
                }
            }),
            new MiniCssExtractPlugin({
                filename: mode === "production" ? "index.[contenthash].css" : "index.css",
                chunkFilename: mode === "production" ? "app.[contenthash].css" : "app.css"
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
            historyApiFallback: true,
            client: {
                overlay: false
            }
        }
    };
}

const packageChunkMappings: readonly IPackageChunkMapping[] = [
    {
        pattern: /node_modules_react_index_js/i,
        packageId: "react"
    },
    {
        pattern: /node_modules_react-dom_client_js/i,
        packageId: "react-dom"
    },
    {
        pattern: /node_modules_react-model-view-viewmodel/i,
        packageId: "react-model-view-viewmodel"
    },
    {
        pattern: /node_modules_react-router/i,
        packageId: "react-router"
    }
];

interface IPackageChunkMapping {
    readonly pattern: RegExp;
    readonly packageId: string;
}