import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import classnames from 'classnames';
import { type IEventHandler, useViewModel, useViewModelDependency } from 'react-model-view-viewmodel';
import { Message } from '../../i18n';
import { BusyContent } from '../../loaders';
import { FormInput } from '../../forms';
import { RecoverUserViewModel } from '../../../view-models/users/recovery-user-view-model';

import Style from '../../style.scss';

export function Recovery(): JSX.Element {
    const [message, setMessage] = useState<string | null>(null);

    const recoverUserViewModel = useViewModelDependency(RecoverUserViewModel);
    useViewModel(recoverUserViewModel.form);

    useEffect(
        () => {
            const hintSentEventHandler: IEventHandler<unknown> = {
                handle() {
                    setMessage("pages.confirmation.hintSent");
                }
            };
            const passwordResetRequestSentEventHandler: IEventHandler<unknown> = {
                handle() {
                    setMessage("pages.confirmation.passwordReset");
                }
            };

            recoverUserViewModel.hintSent.subscribe(hintSentEventHandler);
            recoverUserViewModel.passwordResetRequestSent.subscribe(passwordResetRequestSentEventHandler);

            return () => {
                recoverUserViewModel.passwordResetRequestSent.unsubscribe(passwordResetRequestSentEventHandler);
                recoverUserViewModel.hintSent.unsubscribe(hintSentEventHandler);
            }
        },
        [recoverUserViewModel, setMessage]
    )

    return (
        <div className={Style.mx3}>
            <h1 className={classnames(Style.container, Style.textCenter)}>
                <Message id="pages.recovery.pageTitle" />
            </h1>

            {
                message &&
                <div className={classnames(Style.alert, Style.alertPrimary)}>
                    <Message id={message} />
                </div>
            }

            <BusyContent apiViewModel={recoverUserViewModel}>
                <FormInput className={Style.mb3} id="email" type="email" label="pages.recovery.email.label" field={recoverUserViewModel.form.email} placeholder="pages.recovery.email.placeholder" />

                <div className={Style.mb3}>
                    <button type="button" disabled={(recoverUserViewModel.form.isInvalid && recoverUserViewModel.form.areAllFieldsTouched)} className={classnames(Style.btn, Style.btnPrimary)} onClick={() => recoverUserViewModel.sendHintAsync()}>
                        <Message id="pages.recovery.sendHint.label" />
                    </button>
                    <button type="button" disabled={(recoverUserViewModel.form.isInvalid && recoverUserViewModel.form.areAllFieldsTouched)} className={classnames(Style.ms2, Style.btn, Style.btnDanger)} onClick={() => recoverUserViewModel.resetPasswordAsync()}>
                        <Message id="pages.recovery.resetPassword.label" />
                    </button>
                    <Link to="/" className={classnames(Style.ms2, Style.btn, Style.btnLight)}>
                        <Message id="pages.recovery.cancel.label" />
                    </Link>
                </div>
            </BusyContent>
        </div>
    );
}