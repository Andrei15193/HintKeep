import React, { useState } from 'react';
import { Link, useHistory } from 'react-router-dom';
import classnames from 'classnames';
import { watchEvent, watchViewModel } from 'react-model-view-viewmodel';
import { useViewModel } from '../../use-view-model';
import { Message } from '../../i18n';
import { BusyContent } from '../../loaders';
import { FormInput } from '../../forms';
import { RecoverUserViewModel } from '../../../view-models/users/recovery-user-view-model';

import Style from '../../style.scss';

export function Recovery(): JSX.Element {
    const [message, setMessage] = useState<string | null>(null);

    const $vm = useViewModel(({ axios, alertsViewModel, sessionViewModel }) => new RecoverUserViewModel(axios, alertsViewModel, sessionViewModel));
    watchEvent($vm.hintSent, () => setMessage("pages.confirmation.hintSent"));
    watchEvent($vm.passwordResetRequestSent, () => setMessage("pages.confirmation.passwordReset"));
    watchViewModel($vm.form);

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

            <BusyContent $vm={$vm}>
                <FormInput className={Style.mb3} id="email" type="email" label="pages.recovery.email.label" field={$vm.form.email} placeholder="pages.recovery.email.placeholder" />

                <div className={Style.mb3}>
                    <button type="button" disabled={($vm.form.isInvalid && $vm.form.areAllFieldsTouched)} className={classnames(Style.btn, Style.btnPrimary)} onClick={() => $vm.sendHintAsync()}>
                        <Message id="pages.recovery.sendHint.label" />
                    </button>
                    <button type="button" disabled={($vm.form.isInvalid && $vm.form.areAllFieldsTouched)} className={classnames(Style.ms2, Style.btn, Style.btnDanger)} onClick={() => $vm.resetPasswordAsync()}>
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