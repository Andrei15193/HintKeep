import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { type IEventHandler, useViewModel, useViewModelDependency } from "react-model-view-viewmodel";
import classnames from "classnames";
import { FormInput } from "../../forms";
import { Message } from "../../i18n";
import { BusyContent } from "../../loaders";
import { PasswordResetViewModel } from "../../../view-models/users/password-reset-view-model";

import Style from '../../style.scss';

export function PasswordReset(): JSX.Element {
    const [message, setMessage] = useState<string | null>(null);

    const passwordResetViewModel = useViewModelDependency(PasswordResetViewModel);
    useViewModel(passwordResetViewModel.form);

    useEffect(
        () => {
            const submittedEventHandler: IEventHandler<unknown> = {
                handle() {
                    setMessage("pages.passwordReset.confirmation")
                }
            }

            passwordResetViewModel.passwordReset.subscribe(submittedEventHandler);

            return () => {
                passwordResetViewModel.passwordReset.unsubscribe(submittedEventHandler);
            }
        },
        [passwordResetViewModel, setMessage]
    )

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

            <BusyContent apiViewModel={passwordResetViewModel}>
                <form onSubmit={event => { event.preventDefault(); passwordResetViewModel.resetPasswordAsync(); }}>
                    <FormInput className={Style.mb3} id="email" type="email" label="pages.passwordReset.email.label" field={passwordResetViewModel.form.email} placeholder="pages.passwordReset.email.placeholder" />
                    <FormInput className={Style.mb3} id="token" type="text" label="pages.passwordReset.token.label" field={passwordResetViewModel.form.token} placeholder="pages.passwordReset.token.placeholder" />
                    <FormInput className={Style.mb3} id="password" type="password" label="pages.passwordReset.password.label" field={passwordResetViewModel.form.password} placeholder="pages.passwordReset.password.placeholder" />
                    <FormInput className={Style.mb3} id="passwordConfirmation" type="password" label="pages.passwordReset.passwordConfirmation.label" field={passwordResetViewModel.form.passwordConfirmation} placeholder="pages.passwordReset.passwordConfirmation.placeholder" />

                    <div className={Style.mb3}>
                        <button type="submit" disabled={(passwordResetViewModel.form.isInvalid && passwordResetViewModel.form.areAllFieldsTouched)} className={classnames(Style.btn, Style.btnPrimary)}>
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