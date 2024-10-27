import type { Argument, Configuration } from 'webpack';
import autoprefixer from 'autoprefixer';
import path from 'path';
import HtmlWebpackPlugin from 'html-webpack-plugin';
import MiniCssExtractPlugin from 'mini-css-extract-plugin';

interface IArguments {
    readonly mode: Configuration['mode'],
    readonly env: Readonly<Record<string, string>>;
}

export default function (env: Readonly<Record<string, string>>, argv: IArguments): Configuration {
    const isProduction = argv.mode === 'production';

    return {
        mode: 'development',
        devtool: isProduction ? false : 'eval-source-map',
        target: ['web', 'es5'],
        entry: {
            app: path.resolve(__dirname, 'HintKeep', 'Client', 'components', 'index.tsx')
        },
        output: {
            path: path.resolve(__dirname, 'HintKeep', 'wwwroot')
        },
        resolve: {
            extensions: isProduction
                ? ['.production.ts', '.ts', '.tsx']
                : ['.development.ts', '.ts', '.tsx']
        },
        module: {
            rules: [
                {
                    test: /\.tsx?$/,
                    exclude: /node_modules/,
                    use: 'ts-loader'
                },
                {
                    test: /\.s[ac]ss$/,
                    sideEffects: true,
                    use: (isProduction
                        ? [MiniCssExtractPlugin.loader]
                        : ['style-loader', 'css-modules-typescript-loader'])
                },
                {
                    test: /\.s[ac]ss$/,
                    use: [
                        {
                            loader: 'css-loader',
                            options: {
                                modules: {
                                    mode: 'local',
                                    exportGlobals: true,
                                    exportLocalsConvention: 'camelCaseOnly',
                                    localIdentName: '[local]'
                                }
                            },
                        },
                        {
                            loader: 'postcss-loader',
                            options: {
                                postcssOptions: {
                                    plugins: [
                                        autoprefixer
                                    ]
                                }
                            }
                        },
                        'sass-loader'
                    ]
                }
            ]
        },
        plugins: [
            new MiniCssExtractPlugin(),
            new HtmlWebpackPlugin({
                title: 'HintKeep',
                publicPath: '/',
                hash: true,
                inject: false,
                scriptLoading: 'blocking',
                meta: {
                    charset: 'utf-8',
                    viewport: 'width=device-width, initial-scale=1, shrink-to-fit=no'
                },
                minify: {
                    collapseWhitespace: isProduction,
                    removeComments: true,
                    removeRedundantAttributes: true,
                    removeScriptTypeAttributes: true,
                    removeStyleLinkTypeAttributes: true,
                    useShortDoctype: true
                },
                templateContent: function ({ htmlWebpackPlugin }) {
                    return `<!DOCTYPE html>
<html>
    <head>
        <meta http-equiv="Pragma" content="no-cache">
        <meta http-equiv="Cache-Control" content="no-cache">
        <meta http-equiv="Expires" content="0">
        <meta http-equiv="Content-Type" content="text/html;charset=UTF-8">
        <meta charset="utf-8">
        ${htmlWebpackPlugin.tags.headTags}
        <title>${htmlWebpackPlugin.options.title}</title>
    </head>
    <body>
        <div id="app"></div>
        <script crossorigin src="https://unpkg.com/react@18.3.1/umd/react.${isProduction ? 'production.min' : 'development'}.js"></script>
        <script crossorigin src="https://unpkg.com/react-dom@18.3.1/umd/react-dom.${isProduction ? 'production.min' : 'development'}.js"></script>
        <script crossorigin src="https://unpkg.com/@remix-run/router@1.20.0/dist/router.${isProduction ? 'umd.min' : 'umd'}.js"></script>
        <script crossorigin src="https://unpkg.com/react-router@6.27.0/dist/umd/react-router.${isProduction ? 'production.min' : 'development'}.js"></script>
        <script crossorigin src="https://unpkg.com/react-router-dom@6.27.0/dist/umd/react-router-dom.${isProduction ? 'production.min' : 'development'}.js"></script>
        <script crossorigin src="https://unpkg.com/axios@1.7.7/dist/axios.min.js"></script>
        ${htmlWebpackPlugin.tags.bodyTags}
    </body>
</html>`;
                }
            })
        ],
        externals: {
            'react': 'React',
            'react-dom': 'ReactDOM',
            'react-router': 'ReactRouter',
            'react-router-dom': 'ReactRouterDOM',
            'axios': 'axios'
        }
    };
}