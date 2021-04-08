const path = require('path');
const HtmlWebpackPlugin = require('html-webpack-plugin');
const MiniCssExtractPlugin = require('mini-css-extract-plugin');
const { CleanWebpackPlugin } = require('clean-webpack-plugin');

const isProduction = (process.env.WEBPACK_MODE || process.argv[process.argv.indexOf('--mode') + 1]) === 'production';

module.exports = {
    mode: isProduction ? 'production' : 'development',
    devtool: isProduction ? false : 'eval-source-map',
    entry: {
        app: path.resolve(__dirname, 'HintKeep', 'Client', 'components', 'index.tsx')
    },
    output: {
        path: path.resolve(__dirname, 'HintKeep', 'wwwroot')
    },
    resolve: {
        extensions: ['.ts', '.tsx']
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
                use: (isProduction ? [MiniCssExtractPlugin.loader] : ['style-loader', 'css-modules-typescript-loader'])
                    .concat([
                        {
                            loader: 'css-loader',
                            options: {
                                modules: {
                                    mode: 'local',
                                    exportGlobals: true,
                                    exportLocalsConvention: 'camelCaseOnly',
                                    localIdentName: isProduction ? '[path][name]--[hash:base64]' : '[path][name]__[local]--[hash:base64:5]'
                                }
                            },
                        },
                        {
                            loader: 'postcss-loader',
                            options: {
                                postcssOptions: {
                                    plugins: [
                                        require('autoprefixer')
                                    ]
                                }
                            }
                        },
                        'sass-loader'
                    ])
            }
        ]
    },
    plugins: [
        new CleanWebpackPlugin({
            cleanOnceBeforeBuildPatterns: [
                path.resolve(__dirname, 'HintKeep', 'wwwroot', '**', '*')
            ]
        }),
        new MiniCssExtractPlugin(),
        new HtmlWebpackPlugin({
            title: 'HintKeep',
            hash: true,
            inject: false,
            publicPath: '/',
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
            templateContent: ({ htmlWebpackPlugin }) => `<!DOCTYPE html>
                <html>
                    <head>
                        ${htmlWebpackPlugin.tags.headTags}
                        <title>${htmlWebpackPlugin.options.title}</title>
                    </head>
                    <body>
                        <div id="app"></div>
                        <script crossorigin src="https://unpkg.com/react@17/umd/react.${isProduction ? 'production.min' : 'development'}.js"></script>
                        <script crossorigin src="https://unpkg.com/react-dom@17/umd/react-dom.${isProduction ? 'production.min' : 'development'}.js"></script>
                        <script crossorigin src="https://unpkg.com/react-router-dom/umd/react-router-dom.min.js"></script>
                        <script crossorigin src="https://unpkg.com/axios/dist/axios.min.js"></script>
                        ${htmlWebpackPlugin.tags.bodyTags}
                    </body>
                </html>`
        })
    ],
    externals: {
        'react': 'React',
        'react-dom': 'ReactDOM',
        'react-router-dom': 'ReactRouterDOM',
        'axios': 'axios'
    }
};