import React, { useState } from "react";
import { Link } from "react-router-dom";
import { watchEvent, watchViewModel } from "react-model-view-viewmodel";
import classnames from "classnames";
import { FormInput } from "../../forms";
import { Message } from "../../i18n";
import { BusyContent } from "../../loaders";
import { useViewModel } from "../../use-view-model";
import { PasswordResetViewModel } from "../../../view-models/users/password-reset-view-model";

import Style from '../../style.scss';

export function PasswordReset(): JSX.Element {
    const [message, setMessage] = useState<string | null>(null);

    const $vm = useViewModel(({ axios, alertsViewModel, sessionViewModel }) => new PasswordResetViewModel(axios, alertsViewModel, sessionViewModel));
    watchEvent($vm.passwordReset, () => setMessage("pages.passwordReset.confirmation"));
    watchViewModel($vm.form);

    return (
        <div className={Style.mx3}>
            <h1 className={classnames(Style.container, Style.textCenter)}>
                <Message id="pages.passwordReset.pageTitle" />
            </h1>

            {
                message &&
                <div className={classnames(Style.alert, Style.alertPrimary)}>
                    <Message id={message} />
                </div>
            }

            <BusyContent $vm={$vm}>
                <form onSubmit={event => { event.preventDefault(); $vm.resetPasswordAsync(); }}>
                    <FormInput className={Style.mb3} id="email" type="email" label="pages.passwordReset.email.label" field={$vm.form.email} placeholder="pages.passwordReset.email.placeholder" />
                    <FormInput className={Style.mb3} id="token" type="text" label="pages.passwordReset.token.label" field={$vm.form.token} placeholder="pages.passwordReset.token.placeholder" />
                    <FormInput className={Style.mb3} id="password" type="password" label="pages.passwordReset.password.label" field={$vm.form.password} placeholder="pages.passwordReset.password.placeholder" />
                    <FormInput className={Style.mb3} id="passwordConfirmation" type="password" label="pages.passwordReset.passwordConfirmation.label" field={$vm.form.passwordConfirmation} placeholder="pages.passwordReset.passwordConfirmation.placeholder" />

                    <div className={Style.mb3}>
                        <button type="submit" disabled={($vm.form.isInvalid && $vm.form.areAllFieldsTouched)} className={classnames(Style.btn, Style.btnPrimary)}>
                            <Message id="pages.passwordReset.resetPassword.label" />
                        </button>
                        <Link to="/" className={classnames(Style.ms2, Style.btn, Style.btnLight)}>
                            <Message id="pages.passwordReset.cancel.label" />
                        </Link>
                    </div>
                </form>
            </BusyContent>
        </div>
    );
}