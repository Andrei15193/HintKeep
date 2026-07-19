import React, { type MouseEvent, useCallback, useEffect, useLayoutEffect, useState } from "react";
import { Link, useNavigate } from "react-router";
import { mapDbRequestToPromise, useIndexedDatabase } from "../../Core/Data/IndexedDatabase";
import { HintKeepDatabaseDefinition } from "../../Core/Data/IndexedDatabase/HintKeep";
import { Button } from "../../Core/Forms/Components";
import { Modal } from "../../Core/Modals";
import { Content, Header } from "../../Core/PageParts";
import { useWindow } from "../WindowContext";

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

                    {
                        HINTKEEP_ENVIRONMENT_TYPE === "development"
                        && <DropDatabaseModal />
                    }
                </div>
            </Content>
        </>
    );
}

// Temporary, only for dev/testing!
function DropDatabaseModal(): React.JSX.Element | null {
    const { indexedDB } = useWindow();

    const [deleteDatabaseStatus, setDeleteDatabaseStatus] = useState<"ready" | "confirming" | "confirmed" | "deleting">("ready");

    const showDatabaseDeleteModal = useCallback(() => setDeleteDatabaseStatus("confirming"), [setDeleteDatabaseStatus]);
    const dismissDatabaseDeleteModal = useCallback(() => setDeleteDatabaseStatus("ready"), [setDeleteDatabaseStatus]);
    const confiemDatabaseDeletion = useCallback(() => setDeleteDatabaseStatus("confirmed"), [setDeleteDatabaseStatus]);

    const deleteDatabaseAsyncCallback = useCallback(
        async () => {
            try {
                setDeleteDatabaseStatus("deleting");
                await mapDbRequestToPromise(indexedDB.deleteDatabase(HintKeepDatabaseDefinition.name));
            }
            finally {
                setDeleteDatabaseStatus("ready");
            }
        },
        [indexedDB, setDeleteDatabaseStatus]
    );

    useLayoutEffect(
        () => {
            if (deleteDatabaseStatus === "confirmed")
                deleteDatabaseAsyncCallback();
        },
        [deleteDatabaseStatus, deleteDatabaseAsyncCallback]
    );

    return (
        <>
            <Button
                text="Drop database"
                danger
                onClick={showDatabaseDeleteModal}
            />

            <Modal isVisible={deleteDatabaseStatus === "confirming" || deleteDatabaseStatus === "confirmed" || deleteDatabaseStatus === "deleting"}>
                <div className="confirmation-prompt-message">
                    This action will remove all locally stored data, all user accounts and their stored hints. Are you sure you want to continue?
                </div>
                <nav className="confirmation-prompt-actions">
                    <Button
                        danger
                        onClick={confiemDatabaseDeletion}
                        text="Yes, delete everything, I'm done with this app"
                        disabled={deleteDatabaseStatus === "deleting"}
                    />
                    <Button
                        neutral
                        onClick={dismissDatabaseDeleteModal}
                        text="No, I do not want to remove all data"
                        disabled={deleteDatabaseStatus === "deleting"}
                    />
                </nav>
            </Modal>
        </>
    );
}