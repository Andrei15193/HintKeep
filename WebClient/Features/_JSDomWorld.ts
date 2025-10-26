import type { JSDOM } from "jsdom";
import { World } from "@cucumber/cucumber";
import { type Context, type PropsWithChildren, createElement } from "react";
import { type Matcher, type RenderResult, findByText, render, createJsDom } from "./_TestingLibraryBootstrap";

export class JSDomWorld extends World {
    public readonly dom: JSDOM = createJsDom();

    public get document(): Document {
        return this.dom.window.document;
    }

    public get container(): HTMLElement {
        return this.dom.window.document.body;
    }

    public render(element: React.JSX.Element): RenderResult {
        const WindowContext: Context<Window> = require("../Pages/WindowContext").WindowContext;

        return render(element, {
            container: this.container,
            wrapper: ({ children }: PropsWithChildren<{}>): React.JSX.Element => createElement(
                WindowContext.Provider,
                { value: this.dom.window as any },
                children
            )
        });
    }

    public findByTextAsync<TElement extends HTMLElement = HTMLElement>(text: Matcher, container?: HTMLElement | null): Promise<TElement> {
        return findByText(
            container ?? this.container,
            typeof text === "string" ? new RegExp(`^${JSDomWorld.escapeRegExp(text)}(\\*?)$`, "i") : text,
            {
                collapseWhitespace: true,
                trim: true
            },
            {
                interval: 10,
                timeout: 300
            }
        );
    }

    public async findInputByLabelTextAsync(labelText: Matcher, container?: HTMLElement | null): Promise<HTMLInputElement | HTMLTextAreaElement> {
        const label = await this.findByTextAsync(labelText, container);

        return this.document.getElementById(label.getAttribute("for")!) as HTMLInputElement | HTMLTextAreaElement;
    }

    /**
     * Shamelesly taken from https://www.30secondsofcode.org/js/s/escape-reg-exp/
     */
    private static escapeRegExp(value: string) {
        return value.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
    }
}