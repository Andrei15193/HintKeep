import React, { type MouseEvent, useCallback, useEffect } from "react";
import { Link, useNavigate } from "react-router";
import { useIndexedDatabase } from "../../Data/IndexedDatabase";

export function HomePage(): React.JSX.Element {
    const navigate = useNavigate();
    const { isOpening, isReady, initializeAsync } = useIndexedDatabase();

    const initializeIndexedDatabaseCallback = useCallback(
        async (event: MouseEvent<HTMLAnchorElement>) => {
            event.preventDefault();
            await initializeAsync();
        },
        [initializeAsync]
    );

    useEffect(
        () => {
            if (isReady)
                navigate("/login", { replace: true });
        },
        [isReady, navigate]
    );

    return (
        <>
            <p>
                Welcome to HintKeep! Currently, we only have the option to sign up and store your hints locally, in the browser. Don't worry, we save the data if you close it!
            </p>
            <p>
                Click on the link below to get started.
            </p>
            <Link
                to="/login"
                className={isOpening ? "prevent-pointer-events" : undefined}
                onClick={initializeIndexedDatabaseCallback}
            >
                Use application locally
            </Link>
        </>
    );
}