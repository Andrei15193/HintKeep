import React, { useEffect } from "react";
import { generatePath, Link, useParams } from "react-router";
import { Checkbox, Label, TextArea, TextInput } from "../../Core/Forms/Components";
import { useEditFlow } from "../../Core/PageFlows";
import { useViewEditToggleContext } from "../../Core/ViewEditToggle";
import { useShowConfirmationPrompt } from "../Prompt";
import { AccountDetailsDataSource } from "./DataSources/AccountDetailsDataSource";
import { AccountFormHandler } from "./FormHandlers/AccountFormHandler";
import { AccountForm } from "./Forms/AccountForm";

export function AccountDetailsPageEdit(): React.JSX.Element {
    const { id } = useParams<{ readonly id: string }>();
    const { goToViewMode } = useViewEditToggleContext();

    const {
        form,
        isLoading,
        isFaulted,
        isSubmitted,
        submitAsync
    } = useEditFlow({
        entityId: id!,
        dataSource: AccountDetailsDataSource,
        form: AccountForm,
        formHandler: AccountFormHandler
    });

    const discardChangesCallback = useShowConfirmationPrompt({
        onConfirm: goToViewMode
    });

    useEffect(
        () => {
            if (isSubmitted)
                goToViewMode();
        },
        [isSubmitted, goToViewMode]
    );

    return (
        <>
            <h1>
                Edit Account
            </h1>
            {
                isLoading
                    ? "Loading"
                    : isFaulted
                        ? "Faulted"
                        : (
                            <form onSubmit={submitAsync}>
                                <div>
                                    <Label field={form.name} />
                                    <TextInput field={form.name} />
                                </div>

                                <div>
                                    <Label field={form.username} />
                                    <TextInput field={form.username} />
                                </div>

                                <div>
                                    <Label field={form.hint} />
                                    <TextInput field={form.hint} />
                                </div>

                                <div>
                                    <Label field={form.pinned} />
                                    <Checkbox field={form.pinned} />
                                </div>

                                <div>
                                    <Label field={form.notes} />
                                    <TextArea field={form.notes} />
                                </div>

                                <div>
                                    <button type="submit">
                                        Save
                                    </button>
                                    <Link
                                        to={generatePath("/:id", { id: id || "" })}
                                        onClick={discardChangesCallback}
                                        replace
                                    >
                                        Cancel
                                    </Link>
                                </div>
                            </form>
                        )
            }
        </>
    );
}