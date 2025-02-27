import type { Configuration } from "webpack";
import path from "path";
import HtmlWebpackPlugin from "html-webpack-plugin";
import "webpack-dev-server";

interface IBuildOptions {
    readonly mode?: "development" | "production";
}

export default function (_: any, { mode }: IBuildOptions): Configuration {
    return {
        entry: path.resolve(__dirname, "index.tsx"),
        mode: "development",
        devtool: "inline-source-map",
        output: {
            filename: mode === "production" ? "app.[hash].js" : "app.js",
            path: path.resolve(__dirname, "bin")
        },
        module: {
            rules: [
                {
                    test: /\.tsx?$/,
                    use: "ts-loader",
                    exclude: /node_modules/
                }
            ]
        },
        plugins: [new HtmlWebpackPlugin()],
        resolve: {
            extensions: [
                ".tsx",
                ".ts",
                ".jsx",
                ".js"
            ]
        },
        devServer: {
            open: true
        }
    };
}