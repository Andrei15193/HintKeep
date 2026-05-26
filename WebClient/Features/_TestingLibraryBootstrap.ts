import { subtle } from "crypto";
import fs from "fs";
import { IDBFactory } from "fake-indexeddb";
import { JSDOM } from "jsdom";
import { type IFeatureTestOptions, featureTestOptions } from "./_FeatureTestOptions";

function getJsDocTemplate(featureTestOptions: IFeatureTestOptions): string {
    return (
        featureTestOptions.templateFilePath
            ? fs
                .readFileSync(featureTestOptions.templateFilePath)
                .toString()
                .replace(" class=\"loader\"", "")
                .replace("\"/app.", "\"./app.")
            : defaultTemplate
    );
}

const defaultTemplate: string = `
<!DOCTYPE html>
<html>
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
</head>
<body>
    <div id="app"></div>
</body>
</html>
`.trim();

const jsDocTemplate = getJsDocTemplate(featureTestOptions);

export function createJsDom(): JSDOM {
    return new JSDOM(
        jsDocTemplate,
        {
            url: "http://localhost/",
            beforeParse(window) {
                Object.assign(window, {
                    indexedDB: new IDBFactory()
                });
                Object.assign(window.crypto, {
                    subtle
                });
            }
        }
    );
}

// React Testing Library bootstrap
// When importing the library it requires window and document to be defined
const globalDom = createJsDom();

global.window = globalDom.window as any;
global.document = globalDom.window.document;
global.sessionStorage = globalDom.window.sessionStorage;
global.localStorage = globalDom.window.localStorage;

export * from "@testing-library/react";