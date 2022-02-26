const path = require('path');
const HtmlWebpackPlugin = require('html-webpack-plugin');
const MiniCssExtractPlugin = require('mini-css-extract-plugin');
const { CleanWebpackPlugin } = require('clean-webpack-plugin');

const isProduction = ((process.argv.find(function (arg) { return arg.startsWith('--mode='); }) || '--mode=').substring('--mode='.length) || process.env.WEBPACK_MODE) === 'production';

module.exports = {
    mode: isProduction ? 'production' : 'development',
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
                use: (isProduction ? [MiniCssExtractPlugin.loader] : ['style-loader', 'css-modules-typescript-loader']),
                include: /components/
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
                                    require('autoprefixer')
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
        new CleanWebpackPlugin({
            cleanOnceBeforeBuildPatterns: [
                path.resolve(__dirname, 'HintKeep', 'wwwroot', '**', '*')
            ]
        }),
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
        <script crossorigin src="https://unpkg.com/core-js-bundle@3.21.1/minified.js"></script>
        <script crossorigin src="https://unpkg.com/react@17.0.2/umd/react.${isProduction ? 'production.min' : 'development'}.js"></script>
        <script crossorigin src="https://unpkg.com/react-dom@17.0.2/umd/react-dom.${isProduction ? 'production.min' : 'development'}.js"></script>
        <script crossorigin src="https://unpkg.com/history@5.3.0/umd/history.${isProduction ? 'production.min' : 'development'}.js"></script>
        <script crossorigin src="https://unpkg.com/react-router@6.2.1/umd/react-router.${isProduction ? 'production.min' : 'development'}.js"></script>
        <script crossorigin src="https://unpkg.com/react-router-dom@6.2.1/umd/react-router-dom.${isProduction ? 'production.min' : 'development'}.js"></script>
        <script crossorigin src="https://unpkg.com/axios/dist/axios.min.js"></script>
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