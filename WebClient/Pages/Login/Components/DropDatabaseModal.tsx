import React, { useState, useCallback, useLayoutEffect } from "react";
import { useDependency } from "react-model-view-viewmodel";
import { IndexedDatabaseHandler } from "../../../Core/Data/IndexedDatabase";
import { Button } from "../../../Core/Forms/Components";
import { Modal } from "../../../Core/Modals";

export function DropDatabaseModal(): React.JSX.Element {
    const indexedDatabaseHandler = useDependency(IndexedDatabaseHandler);

    const [deleteDatabaseStatus, setDeleteDatabaseStatus] = useState<"ready" | "confirming" | "confirmed" | "deleting">("ready");

    const showDatabaseDeleteModal = useCallback(() => setDeleteDatabaseStatus("confirming"), [setDeleteDatabaseStatus]);
    const dismissDatabaseDeleteModal = useCallback(() => setDeleteDatabaseStatus("ready"), [setDeleteDatabaseStatus]);
    const confiemDatabaseDeletion = useCallback(() => setDeleteDatabaseStatus("confirmed"), [setDeleteDatabaseStatus]);

    const deleteDatabaseAsyncCallback = useCallback(
        async () => {
            try {
                setDeleteDatabaseStatus("deleting");
                await indexedDatabaseHandler.dropAsync();
            }
            finally {
                setDeleteDatabaseStatus("ready");
            }
        },
        [indexedDatabaseHandler, setDeleteDatabaseStatus]
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
                type="button"
                text="Drop Local DB"
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