import React, { type MouseEvent, useCallback, useEffect } from "react";
import { Link, useNavigate } from "react-router";
import { useIndexedDatabase } from "../../Core/Data/IndexedDatabase";
import { HintKeepDatabaseDefinition } from "../../Core/Data/IndexedDatabase/HintKeep";
import { Button } from "../../Core/Forms/Components";
import { Content, Header } from "../../Core/PageParts";
import { useShowConfirmationPrompt } from "../../Core/Prompt";

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
    const dropDatabaseCallback = useShowConfirmationPrompt({
        message: "This action will remove all locally stored data, all user accounts and their stored hints. Are you sure you want to continue?",
        confirmButtonLabel: "Yes, delete everything, I'm done with this app",
        dismissButtonLabel: "No, I do not want to remove all data",
        onConfirm: deleteAllLocalData
    });

    useEffect(
        () => {
            closeDatabase();
        },
        [closeDatabase]
    );

    return (
        <>
            <Header>
                HintKeep
            </Header>
            <Content>
                <p>
                    Welcome to HintKeep! Currently, we only have the option to sign up and store your hints locally, in the browser. Don't worry, we save the data if you close it!
                </p>
                <p>
                    Click on the link below to get started.
                </p>
                <div className="toolbar">
                    <Link
                        to="/login"
                        className={isOpening ? "prevent-pointer-events" : undefined}
                        onClick={initializeIndexedDatabaseCallback}
                    >
                        Use application locally
                    </Link>

                    <Button
                        text="Drop database"
                        danger
                        onClick={dropDatabaseCallback}
                    />
                </div>
            </Content>
        </>
    );
}

function deleteAllLocalData(): void {
    indexedDB.deleteDatabase(HintKeepDatabaseDefinition.name);
}