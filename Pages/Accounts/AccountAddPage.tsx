import React, { useEffect } from "react";
import { Link, useNavigate } from "react-router";
import { useCreateFlow } from "../../PageFlows";
import { Checkbox, Label, TextArea, TextInput } from "../Forms";
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
        formHandler: AddAccountFormHandler,

        confirmationPrompt: {
            message: "Any unsaved changes will be discarded, continue?",
            confirmButtonLabel: "Yes, cancel",
            dismissButtonLabel: "No, continue adding hint"
        }
    });

    useEffect(
        () => {
            if (isSubmitted)
                navigate("/");
        },
        [isSubmitted, navigate]
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
                    <Link to="/">
                        Cancel
                    </Link>
                </div>
            </form>
        </>
    );
}