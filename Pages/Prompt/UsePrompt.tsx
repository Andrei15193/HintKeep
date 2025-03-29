import React, { createContext, type PropsWithChildren, useCallback, useContext, useMemo, useState } from "react";

export interface IPrompt {
    readonly isPromptVisible: boolean;

    showPrompt(): void;
    hidePrompt(): void;
    togglePrompt(): void;
}

const PromptContext = createContext<IPrompt>(null!);

export function usePromptContext(): IPrompt {
    return useContext(PromptContext);
}

export interface IUsePromptResult extends IPrompt {
    readonly PromptTrigger: React.ComponentType<PropsWithChildren<{}>>;
    readonly Prompt: React.ComponentType<PropsWithChildren<{}>>;
}

export function usePrompt(): IUsePromptResult {
    const [isVisible, setIsVisible] = useState(false);

    const showCallback = useCallback(
        () => setIsVisible(true),
        [setIsVisible]
    );

    const hideCallback = useCallback(
        () => setIsVisible(false),
        [setIsVisible]
    );

    const toggleCallback = useCallback(
        () => setIsVisible((isVisible) => !isVisible),
        [setIsVisible]
    );

    const promptContext = useMemo<IPrompt>(
        () => ({
            isPromptVisible: isVisible,

            showPrompt: showCallback,
            hidePrompt: hideCallback,
            togglePrompt: toggleCallback
        }),
        [isVisible, showCallback, hideCallback, toggleCallback]
    );

    const promptTriggerComponent = useCallback(
        ({ children }: PropsWithChildren<{}>) => (
            <PromptContext.Provider value={promptContext}>
                {
                    !promptContext.isPromptVisible
                        ? children
                        : null
                }
            </PromptContext.Provider>
        ),
        [promptContext]
    );

    const promptComponent = useCallback(
        ({ children }: PropsWithChildren<{}>) => (
            <PromptContext.Provider value={promptContext}>
                {
                    promptContext.isPromptVisible
                        ? children
                        : null
                }
            </PromptContext.Provider>
        ),
        [promptContext]
    );

    return useMemo<IUsePromptResult>(
        () => ({
            isPromptVisible: isVisible,

            showPrompt: showCallback,
            hidePrompt: hideCallback,
            togglePrompt: toggleCallback,

            PromptTrigger: promptTriggerComponent,
            Prompt: promptComponent
        }),
        [isVisible, showCallback, hideCallback, toggleCallback, promptTriggerComponent, promptComponent]
    );
}