import React, { useEffect } from "react";
import { Link, useNavigate } from "react-router";
import { useCreateFlow } from "../../PageFlows";
import { Checkbox, Label, TextArea, TextInput } from "../Forms";
import { usePrompt } from "../Prompt";
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

    const { showPrompt, hidePrompt, PromptTrigger, Prompt } = usePrompt();

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

                <PromptTrigger>
                    <div>
                        <button type="submit">
                            Save
                        </button>
                        <button onClick={showPrompt}>
                            Cancel
                        </button>
                    </div>
                </PromptTrigger>
                <Prompt>
                    <Link to="/">
                        Yes, cancel
                    </Link>
                    <button onClick={hidePrompt}>
                        No, continue adding hint
                    </button>
                </Prompt>
            </form>
        </>
    );
}