import type { Configuration } from "webpack";
import path from "path";
import CopyWebpackPlugin from "copy-webpack-plugin";
import HtmlInlineCSSWebpackPlugin from "html-inline-css-webpack-plugin";
import HtmlInlineScriptPlugin from "html-inline-script-webpack-plugin";
import HtmlWebpackPlugin from "html-webpack-plugin";
import MiniCssExtractPlugin from "mini-css-extract-plugin";
import "webpack-dev-server";

interface IBuildOptions {
    readonly mode?: "development" | "production";
}

/**
 * Must match with environment variables on Netlify to ensure consistency.
 * These are only included in the bundle for development builds, for release
 * builds make sure there is a post-process script added that injects the information.
 *
 * Related post-process script can be found in `WebClient.Config.liquid` under `.infrastructure`.
 */
const developmentEnvironmentVariables: Readonly<Record<string, string>> = {
    HINTKEEP_API_URL: "http://localhost:8081/"
};

let lastGeneratedChunkId = 0;
const chunkNamesById: Record<string | number, number> = {};

export default function (_: any, { mode = "development" }: IBuildOptions): Configuration {
    return {
        entry: {
            index: path.resolve(__dirname, "index.ts"),
            app: path.resolve(__dirname, "App.tsx")
        },
        mode,
        devtool: mode === "production" ? false : "inline-source-map",
        optimization: {
            chunkIds: "named"
        },
        output: {
            clean: true,
            path: path.resolve(__dirname, "publish"),
            publicPath: "/",
            filename: mode === "production" ? "[name].[contenthash].js" : "[name].js",
            chunkFilename(pathData) {
                const chunkId = pathData.chunk?.id;
                const packageChunkMapping = typeof chunkId === "string"
                    ? packageChunkMappings.find((mapping) => mapping.pattern.test(chunkId))
                    : undefined;

                if (packageChunkMapping)
                    return `${packageChunkMapping.packageId}.[contenthash].js`;
                else {
                    let generatedChunkId: number;
                    if (pathData.chunk && pathData.chunk.id !== undefined && pathData.chunk.id !== null)
                        if (pathData.chunk.id in chunkNamesById)
                            generatedChunkId = chunkNamesById[pathData.chunk.id]!;
                        else {
                            lastGeneratedChunkId++;
                            chunkNamesById[pathData.chunk.id] = generatedChunkId = lastGeneratedChunkId;
                        }
                    else {
                        lastGeneratedChunkId++;
                        generatedChunkId = lastGeneratedChunkId;
                    }

                    if (mode === "production")
                        return `${generatedChunkId}.[contenthash].js`;
                    else
                        return `${generatedChunkId}.js`;
                }
            }
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
                        to: path.resolve(__dirname, "publish")
                    }
                ]
            }),
            new HtmlWebpackPlugin({
                template: "!!handlebars-loader!index.hbs",
                templateParameters: {
                    isDevelopment: mode === "development",
                    isProduction: mode !== "development",
                    developmentEnvironmentVariables
                },
                title: "HintKeep",
                inject: "body",
                meta: {
                    charset: "utf-8",
                    viewport: "width=device-width,initial-scale=1"
                }
            }),
            new HtmlInlineScriptPlugin({
                scriptMatchPattern: [/^\/?index(\.\w+)?\.js$/]
            }),
            new HtmlInlineCSSWebpackPlugin({
                filter(fileName) {
                    return /^\/?index(\.[a-z0-9]+)?\.(css|html)$/i.test(fileName);
                }
            }),
            new MiniCssExtractPlugin({
                filename: mode === "production" ? "[name].[contenthash].css" : "[name].css"
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