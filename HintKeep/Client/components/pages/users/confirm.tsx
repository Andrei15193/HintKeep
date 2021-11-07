import React from 'react';
import { Link, useHistory } from 'react-router-dom';
import classnames from 'classnames';
import { watchEvent, watchViewModel } from 'react-model-view-viewmodel';
import { Message } from '../../i18n';
import { useViewModel } from '../../use-view-model';
import { ConfirmUserViewModel } from '../../../view-models/users/confirm-user-view-model';
import { BusyContent } from '../../loaders';
import { FormInput } from '../../forms';

import Style from '../../style.scss';

export function Confirmation(): JSX.Element {
    const { push } = useHistory();

    const $vm = useViewModel(({ axios, alertsViewModel, sessionViewModel }) => new ConfirmUserViewModel(axios, alertsViewModel, sessionViewModel));
    watchEvent($vm.confirmed, () => push('/'));
    watchViewModel($vm.form);

    return (
        <div className={Style.mx3}>
            <h1 className={classnames(Style.container, Style.textCenter)}>
                <Message id="pages.confirmation.pageTitle" />
            </h1>

            <p>
                <Message id="pages.confirmation.description" />
            </p>

            <BusyContent $vm={$vm}>
                <FormInput className={Style.mb3} id="token" type="text" label="pages.confirmation.token.label" field={$vm.form.token} placeholder="pages.confirmation.token.placeholder" />

                <div className={Style.mb3}>
                    <button type="button" disabled={($vm.form.isInvalid && $vm.form.areAllFieldsTouched)} className={classnames(Style.btn, Style.btnPrimary)} onClick={() => $vm.confirmAsync()}>
                        <Message id="pages.confirmation.confirm.label" />
                    </button>
                    <Link to="/" className={classnames(Style.ms2, Style.btn, Style.btnLight)}>
                        <Message id="pages.confirmation.cancel.label" />
                    </Link>
                </div>
            </BusyContent>
        </div>
    );
}