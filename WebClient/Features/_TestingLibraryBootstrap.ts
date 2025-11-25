import { subtle } from "crypto";
import { IDBFactory } from "fake-indexeddb";
import { JSDOM } from "jsdom";

export function createJsDom(): JSDOM {
    return new JSDOM("<!DOCTYPE html><html><body></body></html>", {
        url: "http://localhost/",
        beforeParse(window) {
            Object.assign(window, {
                indexedDB: new IDBFactory()
            });
            Object.assign(window.crypto, {
                subtle
            });
        }
    });
}

// React Testing Library bootstrap
// When importing the library it requires window and document to be defined
const globalDom = createJsDom();

global.window = globalDom.window as any;
global.document = globalDom.window.document;
global.sessionStorage = globalDom.window.sessionStorage;
global.localStorage = globalDom.window.localStorage;

export * from "@testing-library/react";