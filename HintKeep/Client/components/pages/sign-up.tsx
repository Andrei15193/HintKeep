import React from 'react';
import { Link, useHistory } from 'react-router-dom';
import classnames from 'classnames';
import { Message } from '../i18n';
import { BusyContent } from '../loaders';
import { WithViewModel, useEvent } from '../view-model-wrappers';
import { SignUpViewModel } from '../../view-models/users/sign-up-view-model';
import { FormInput } from './forms';

import Style from '../style.scss';

export function SignUp(): JSX.Element {
    return (
        <WithViewModel viewModelType={SignUpViewModel}>{$vm => {
            const history = useHistory();
            useEvent($vm.submittedEvent, () => history.push('/user-accounts/confirm'));

            return <>
                <h1 className={Style.textCenter}><Message id="pages.signUp.pageTitle" /></h1>
                <BusyContent $vm={$vm}>
                    <FormInput className={Style.mb3} id="email" type="text" label="pages.signUp.email.label" field={$vm.email} placeholder="pages.signUp.email.placeholder" />
                    <FormInput className={Style.mb3} id="password" type="password" label="pages.signUp.password.label" description="pages.signUp.password.description" field={$vm.password} />
                    <div>
                        <button type="submit" disabled={($vm.isInvalid && $vm.areAllFieldsTouched)} className={classnames(Style.btn, Style.btnPrimary)} onClick={() => $vm.submitAsync()}>
                            <Message id="pages.signUp.submit.label" />
                        </button>
                        <Link to="/" className={classnames(Style.ml2, Style.btn, Style.btnLight)}>
                            <Message id="pages.signUp.cancel.label" />
                        </Link>
                    </div>
                </BusyContent>
            </>;
        }}</WithViewModel>
    );
}