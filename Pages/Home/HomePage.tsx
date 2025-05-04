import React, { type MouseEvent, useCallback, useEffect } from "react";
import { Link, useNavigate } from "react-router";
import { useIndexedDatabase } from "../../Core/Data/IndexedDatabase";
import { HintKeepDatabaseDefinition } from "../../Core/Data/IndexedDatabase/HintKeep";

export function HomePage(): React.JSX.Element {
    const navigate = useNavigate();
    const { isOpening, initializeAsync, closeDatabase } = useIndexedDatabase();

    const initializeIndexedDatabaseCallback = useCallback(
        async (event: MouseEvent<HTMLAnchorElement>) => {
            event.preventDefault();
            await initializeAsync();

            navigate("/login", { replace: true });
        },
        [initializeAsync, navigate]
    );

    // Temporary, only for dev/testing!
    const dropDatabaseCallback = useCallback(
        () => {
            indexedDB.deleteDatabase(HintKeepDatabaseDefinition.name);
        },
        []
    );

    useEffect(
        () => {
            closeDatabase();
        },
        [closeDatabase]
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
            <button onClick={dropDatabaseCallback}>
                Drop database
            </button>
        </>
    );
}