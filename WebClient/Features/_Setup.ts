import type { RenderResult } from "@testing-library/react";
import { World, setWorldConstructor } from "@cucumber/cucumber";
import { JSDOM } from "jsdom";
import { type PropsWithChildren, createElement } from "react";
import { WindowContext } from "../Pages/WindowContext";

// Define scenario context
export class JSDomWorld extends World {
    public readonly dom: JSDOM = new JSDOM("<!DOCTYPE html><html><body></body></html>", {
        url: "http://localhost/"
    });

    public get container(): HTMLElement {
        return this.dom.window.document.body;
    }

    public render(element: React.JSX.Element): RenderResult {
        const { render } = require("@testing-library/react");

        return render(element, {
            container: this.container,
            wrapper: ({ children }: PropsWithChildren<{}>): React.JSX.Element => createElement(
                WindowContext.Provider,
                { value: this.dom.window as any },
                children
            )
        });
    }
}

// Declare the context globally
declare module "@cucumber/cucumber" {
    interface IWorld extends JSDomWorld {
    }
}

declare global {
    var window: Window & typeof globalThis;
    var document: Document;
}

setWorldConstructor(JSDomWorld);

// React Testing Library bootstrap
// When importing the library it requires window and document to be defined
const dom = new JSDOM("<!DOCTYPE html><html><body></body></html>", {
    url: "http://localhost/"
});

global.window = dom.window as any;
global.document = dom.window.document;
global.sessionStorage = dom.window.sessionStorage;
global.localStorage = dom.window.localStorage;