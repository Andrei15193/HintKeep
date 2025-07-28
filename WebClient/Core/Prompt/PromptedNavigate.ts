import { useCallback, useEffect, useRef } from "react";
import { type NavigateOptions, type To, useBlocker, useNavigate } from "react-router";
import { useShowConfirmationPrompt, type IConfirmationPromptOptions } from "./ConfirmationPromptContext";

export interface IPromptedNavigateOptions extends IConfirmationPromptOptions {
    readonly blockNavigation: boolean;
}

export interface PromptedNavigateFunction {
    (to: To, options?: NavigateOptions): boolean | Promise<boolean>;
    (delta: number): boolean | Promise<boolean>;
}

export function usePromptedNavigate(options: IPromptedNavigateOptions): PromptedNavigateFunction {
    const navigate = useNavigate();
    const {
        blockNavigation,
        onConfirm,
        onDismiss,
        ...otherConfirmationPromptOptions
    } = options;

    const preventResetRef = useRef(false);

    const blocker = useBlocker(blockNavigation);
    const onConfirmCallback = useCallback(
        () => {
            onConfirm && onConfirm();
            blocker.state === "blocked" && blocker.proceed();
        },
        [blocker, onConfirm]
    );
    const onDismissCallback = useCallback(
        () => {
            onDismiss && onDismiss();
            if (!preventResetRef.current)
                blocker.state === "blocked" && blocker.reset();
        },
        [blocker, onDismiss]
    );

    const showDiscardConfirmationPrompt = useShowConfirmationPrompt({
        onConfirm: onConfirmCallback,
        onDismiss: onDismissCallback,
        ...otherConfirmationPromptOptions
    });

    useEffect(
        () => {
            if (blockNavigation) {
                const beforeUnloadEventHandler = (event: BeforeUnloadEvent) => {
                    event.preventDefault();
                    event.returnValue = ""; // Chrome requires returnValue to be set.
                };
                window.addEventListener("beforeunload", beforeUnloadEventHandler);

                return () => {
                    window.removeEventListener("beforeunload", beforeUnloadEventHandler);
                };
            }
            else
                return undefined;
        },
        [blockNavigation]
    );

    useEffect(
        () => {
            if (blocker.state === "blocked") {
                preventResetRef.current = true;
                showDiscardConfirmationPrompt();
            }
            else
                preventResetRef.current = false;
        },
        [blocker, showDiscardConfirmationPrompt]
    );

    return useCallback<PromptedNavigateFunction>(
        (toOrDelta, options?) => {
            if (blockNavigation)
                return new Promise<boolean>((resolve, reject) => {
                    try {
                        showDiscardConfirmationPrompt(undefined, {
                            onConfirm() {
                                resolve(true);
                            },
                            onDismiss() {
                                resolve(false);
                            }
                        });
                    }
                    catch (error) {
                        reject(error);
                    }
                });
            else {
                const navigateResult = typeof toOrDelta === "number" ? navigate(toOrDelta) : navigate(toOrDelta, options!);

                return navigateResult === null || navigateResult === undefined || typeof navigateResult !== "object"
                    ? true
                    : navigateResult.then(() => true);
            }
        },
        [blockNavigation, showDiscardConfirmationPrompt, navigate]
    );
}