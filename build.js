const webpack = require('webpack');

webpack(
    getWebpackConfig(),
    function (error, stats) {
        if (error)
            console.error(error);
        else {
            console.log(stats.toString({ colors: true }));
            if (!stats.hasErrors())
                procesAzureB2CFiles();
        }
    }
);

function procesAzureB2CFiles() {
    const path = require('path');
    const fs = require('fs');

    const filesToRemove = [
        'azure-b2c-style.css',
        'azure-b2c-style.js'
    ]

    const outputDirectoryPath = path.resolve(__dirname, 'HintKeep', 'wwwroot');
    const outputFileNames = fs.readdirSync(outputDirectoryPath);
    const styleMatchRegex = new RegExp(
        '<style>(' + outputFileNames.filter(function (outputFileName) { return outputFileName.toLowerCase().endsWith('.css'); }).join('|') + ')<\/style>',
        'i'
    );

    outputFileNames
        .filter(function (outputFileName) {
            return outputFileName.toLowerCase().endsWith('.html');
        })
        .forEach(function (outputFileName) {
            const outputFilePath = path.resolve(outputDirectoryPath, outputFileName);
            const outputFileContents = fs.readFileSync(outputFilePath, { encoding: "utf8" });

            const updatedOutputFileContents = outputFileContents.replace(styleMatchRegex, function (_, styleFileName) {
                const styleFilePath = path.resolve(outputDirectoryPath, styleFileName);
                const styleFileContent = fs.readFileSync(styleFilePath, { encoding: "utf8" });
                return `<style>${styleFileContent.trim()}</style>`
            });

            fs.writeFileSync(outputFilePath, updatedOutputFileContents, { encoding: "utf8" });
        });

    outputFileNames
        .filter(function (outputFileName) {
            return filesToRemove.includes(outputFileName.toLowerCase());
        })
        .forEach(function (outputFileName) {
            fs.unlinkSync(path.resolve(outputDirectoryPath, outputFileName));
        })
}

function getWebpackConfig() {
    const path = require('path');
    const HtmlWebpackPlugin = require('html-webpack-plugin');
    const MiniCssExtractPlugin = require('mini-css-extract-plugin');
    const { CleanWebpackPlugin } = require('clean-webpack-plugin');

    const applicationUrl = 'https://hintkeep.azurewebsites.net/';
    const isProduction = ((process.argv.find(function (arg) { return arg.startsWith('--mode='); }) || '--mode=').substring('--mode='.length) || process.env.WEBPACK_MODE) === 'production';

    const baseHtmlWebpackPluginOptions = {
        title: 'HintKeep',
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
    };

    return {
        mode: isProduction ? 'production' : 'development',
        devtool: isProduction ? false : 'eval-source-map',
        watch: process.argv.includes('--watch'),
        target: ['web', 'es5'],
        entry: {
            app: path.resolve(__dirname, 'HintKeep', 'Client', 'components', 'index.tsx'),
            'azure-b2c-style': path.resolve(__dirname, 'HintKeep', 'Client', 'azure-b2c', 'azure-b2c-style.scss')
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
                    use: MiniCssExtractPlugin.loader,
                    include: /azure-b2c/
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
            new HtmlWebpackPlugin(Object.assign({}, baseHtmlWebpackPluginOptions, {
                publicPath: '/',
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
        <script crossorigin src="https://unpkg.com/core-js-bundle@3.13.0/minified.js"></script>
        <script crossorigin src="https://unpkg.com/react@17/umd/react.${isProduction ? 'production.min' : 'development'}.js"></script>
        <script crossorigin src="https://unpkg.com/react-dom@17/umd/react-dom.${isProduction ? 'production.min' : 'development'}.js"></script>
        <script crossorigin src="https://unpkg.com/react-router-dom/umd/react-router-dom.min.js"></script>
        <script crossorigin src="https://unpkg.com/axios/dist/axios.min.js"></script>
        ${htmlWebpackPlugin.tags.bodyTags}
    </body>
</html>`;
                }
            })),
            new HtmlWebpackPlugin(Object.assign({}, baseHtmlWebpackPluginOptions, {
                publicPath: applicationUrl,
                filename: 'unified.html',
                template: './HintKeep/Client/azure-b2c/unified.html'
            })),
            new HtmlWebpackPlugin(Object.assign({}, baseHtmlWebpackPluginOptions, {
                publicPath: applicationUrl,
                filename: 'selfAsserted.html',
                template: './HintKeep/Client/azure-b2c/selfAsserted.html'
            })),
            new HtmlWebpackPlugin(Object.assign({}, baseHtmlWebpackPluginOptions, {
                publicPath: applicationUrl,
                filename: 'register.html',
                template: './HintKeep/Client/azure-b2c/register.html'
            }))
        ],
        externals: {
            'react': 'React',
            'react-dom': 'ReactDOM',
            'react-router-dom': 'ReactRouterDOM',
            'axios': 'axios'
        }
    };
}