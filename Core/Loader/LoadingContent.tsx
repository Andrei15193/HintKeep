import React, { type PropsWithChildren } from "react";
import { Loader } from "./Loader";

export interface ILoadingContentProps {
    readonly isLoading: boolean;
}

export function LoadingContent({ isLoading, children }: PropsWithChildren<ILoadingContentProps>): React.JSX.Element {
    if (isLoading)
        return (
            <Loader className="loading-content-loader" />
        );
    else
        return (
            <>
                {children}
            </>
        );
}