import type { IFormHandler } from "../Core/FormHandlers/IFormHandler";
import type { HintKeepForm, HintKeepFormField } from "../Core/Forms/ViewModels";
import type { IAppProps } from "../Pages/App";
import type { JSDOM } from "jsdom";
import type { ResolvableSimpleDependency, IDependencyResolver } from "react-model-view-viewmodel";
import type { createBrowserRouter } from "react-router";
import { World } from "@cucumber/cucumber";
import { type ComponentType, type Context, createElement } from "react";
import { type Matcher, type RenderResult, findByText, render, createJsDom } from "./_TestingLibraryBootstrap";

export class JSDomWorld extends World {
    private _router: ReturnType<typeof createBrowserRouter> | null = null;
    private _dependencyResolver: IDependencyResolver | null = null;
    public readonly dom: JSDOM = createJsDom();

    public get document(): Document {
        return this.dom.window.document;
    }

    public get container(): HTMLElement {
        return this.dom.window.document.getElementById("app")!;
    }

    public get router(): ReturnType<typeof createBrowserRouter> {
        if (this._router === null)
            throw new Error("App router has not been initialized, please check feature test configuration.");

        return this._router;
    }

    public get dependencyResolver(): IDependencyResolver {
        if (this._dependencyResolver === null)
            throw new Error("App dependency resolver has not been initialized, please check feature test configuration.");

        return this._dependencyResolver;
    }

    public resolve<T>(dependency: ResolvableSimpleDependency<T>): T {
        return this.dependencyResolver.resolve(dependency);
    }

    public async submitFormAsync<TForm extends HintKeepForm, TFormHandler extends IFormHandler<TForm, TResult>, TResult>(formHandler: ResolvableSimpleDependency<TFormHandler>, form: ResolvableSimpleDependency<TForm>, formData: HintKeepFormData<TForm>): Promise<TResult | null> {
        const resolvedForm = Object
            .getOwnPropertyNames(formData)
            .reduce(
                (form, field) => {
                    (form[field as keyof TForm] as HintKeepFormField<any>).value = formData[field as keyof HintKeepFormData<TForm>];

                    return form;
                },
                this.resolve(form)
            );

        if (resolvedForm.isInvalid)
            throw new Error("Cannot submit invalid form.");

        const resolvedFormHandler = this.resolve(formHandler);
        const result = await resolvedFormHandler.handleAsync(resolvedForm);
        if (resolvedForm.isInvalid || result === null || result === undefined)
            throw new Error("Form submission failed.");

        return result;
    }

    public render(): RenderResult {
        const App: ComponentType<IAppProps> = require("../Pages/App").App;
        const WindowContext: Context<Window> = require("../Pages/WindowContext").WindowContext;

        return render(
            createElement(
                WindowContext.Provider,
                { value: this.dom.window as any },
                createElement(
                    App,
                    {
                        configure: (dependencyContainer, router) => {
                            this._router = router;
                            this._dependencyResolver = dependencyContainer;

                            return dependencyContainer;
                        }
                    }
                )
            ),
            {
                container: this.container
            }
        );
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

type HintKeepFormData<TForm extends HintKeepForm> = {
    readonly [field in HintKeepFormFields<TForm>]: TForm[field] extends HintKeepFormField<infer TValue> ? TValue : void
};

type HintKeepFormFields<TForm extends HintKeepForm> = {
    readonly [field in keyof TForm]: TForm[field] extends HintKeepFormField<any> ? field : never
}[keyof TForm];