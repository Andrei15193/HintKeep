import React from 'react';
import { Link, useHistory } from 'react-router-dom';
import classnames from 'classnames';
import { watchEvent, useViewModel } from '../view-model-wrappers';
import { UserConfirmationViewModel } from '../../view-models/users/confirmation-view-model';
import { Message } from '../i18n';
import { BusyContent } from '../loaders';
import { FormInput } from './forms';

import Style from '../style.scss';

export function UserConfirmation(): JSX.Element {
    const { push } = useHistory();
    const $vm = useViewModel(UserConfirmationViewModel);
    watchEvent($vm.submittedEvent, () => push('/'));

    return (
        <div className={Style.m2}>
            <h1 className={Style.textCenter}><Message id="pages.userConfirmation.pageTitle" /></h1>
            <BusyContent $vm={$vm}>
                <FormInput className={Style.mb3} id="email" type="text" label="pages.userConfirmation.email.label" field={$vm.email} placeholder="pages.signUp.email.placeholder" />
                <FormInput className={Style.mb3} id="token" type="text" label="pages.userConfirmation.token.label" description="pages.userConfirmation.token.description" field={$vm.confirmationToken} />
                <div>
                    <button type="button" disabled={($vm.isInvalid && $vm.areAllFieldsTouched)} className={classnames(Style.btn, Style.btnPrimary)} onClick={() => $vm.submitAsync()}>
                        <Message id="pages.userConfirmation.submit.label" />
                    </button>
                    <Link to="/" className={classnames(Style.ml2, Style.btn, Style.btnLight)}>
                        <Message id="pages.userConfirmation.cancel.label" />
                    </Link>
                </div>
            </BusyContent>
        </div>
    );
}