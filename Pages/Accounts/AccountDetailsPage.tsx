import type { IAccountDetails } from "./Models/IAcountDetails";
import React, { useCallback, useEffect, useRef, useState } from "react";
import { useDependency, useViewModel } from "react-model-view-viewmodel";
import { Link, useNavigate, useParams } from "react-router";
import { isUniqueId } from "../../Crypto";
import { Checkbox, Label, TextArea, TextInput } from "../Forms";
import { AccountDetailsDataSource } from "./DataSources/AccountDetailsDataSource";
import { AccountForm } from "./Forms/AccountForm";

export function AccountDetailsPage(): React.JSX.Element {
    const navigate = useNavigate();
    const { id } = useParams<{ readonly id: string }>();
    const accountDetailsRef = useRef<IAccountDetails | undefined>(undefined);
    const [isLoading, setIsLoading] = useState(true);

    const accountDetailsDataSource = useDependency(AccountDetailsDataSource);
    const form = useViewModel(AccountForm, [accountDetailsRef.current]);

    const loadAccountAsyncCallback = useCallback(
        async () => {
            if (!isUniqueId(id))
                navigate("/");
            else
                try {
                    setIsLoading(true);
                    accountDetailsRef.current = await accountDetailsDataSource.getDataAsync({ id });
                }
                finally {
                    setIsLoading(false);
                }
        },
        [id, accountDetailsDataSource, accountDetailsRef, setIsLoading, navigate]
    );

    useEffect(
        () => {
            loadAccountAsyncCallback();
        },
        [loadAccountAsyncCallback]
    );

    return (
        <>
            <h1>
                View Account
            </h1>
            {
                isLoading
                    ? <p>
                        Loading
                    </p>
                    : (
                        <form>
                            <div>
                                <Label field={form.name} />
                                <TextInput
                                    field={form.name}
                                    disabled
                                />
                            </div>

                            <div>
                                <Label field={form.username} />
                                <TextInput
                                    field={form.username}
                                    disabled
                                />
                            </div>

                            <div>
                                <Label field={form.hint} />
                                <TextInput
                                    field={form.hint}
                                    disabled
                                />
                            </div>

                            <div>
                                <Label field={form.pinned} />
                                <Checkbox
                                    field={form.pinned}
                                    disabled
                                />
                            </div>

                            <div>
                                <Label field={form.notes} />
                                <TextArea
                                    field={form.notes}
                                    disabled
                                />
                            </div>

                            <div>
                                <Link to="/">
                                    Cancel
                                </Link>
                            </div>
                        </form>
                    )
            }
        </>
    );
}