import React, { useEffect } from "react";
import { useNavigate } from "react-router";
import { useCreateFlow } from "../../PageFlows";
import { Checkbox, Label, TextArea, TextInput } from "../Forms";
import { useConfirmationPromptFlow } from "../Prompt";
import { AddAccountFormHandler } from "./FormHandlers/AddAccountFormHandler";
import { AccountForm } from "./Forms/AccountForm";

export function AccountAddPage(): React.JSX.Element {
    const navigate = useNavigate();

    const {
        form,
        isSubmitted,
        submitAsync
    } = useCreateFlow({
        form: AccountForm,
        formHandler: AddAccountFormHandler
    });

    const {
        isConfirmed: isCancelConfirmed,
        show: prompCancelConfirmation
    } = useConfirmationPromptFlow({
        message: "Any unsaved changes will be discarded, continue?",
        confirmButtonLabel: "Yes, cancel",
        dismissButtonLabel: "No, continue adding hint"
    });

    useEffect(
        () => {
            if (isSubmitted || isCancelConfirmed)
                navigate("/");
        },
        [isSubmitted, isCancelConfirmed, navigate]
    );

    return (
        <>
            <h1>
                Add hint
            </h1>
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
                    <button
                        type="button"
                        onClick={prompCancelConfirmation}
                    >
                        Cancel
                    </button>
                </div>
            </form>
        </>
    );
}