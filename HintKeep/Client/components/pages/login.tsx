import type { ComponentType, PropsWithChildren } from 'react';
import React, { memo } from 'react';
import { Link } from 'react-router-dom';
import classnames from 'classnames';
import { useViewModel } from '../view-model-wrappers';
import { LoginViewModel } from '../../view-models/users/login-view-model';
import { UserViewModel } from '../../view-models/users/user-view-model';
import { BusyContent } from '../loaders';
import { FormInput } from './forms';
import { Message } from '../i18n';

import Style from '../style.scss';

export const LoginGuard: ComponentType<PropsWithChildren<{}>> = memo(
    function ({ children }): JSX.Element {
        const $vm = useViewModel(UserViewModel);

        return $vm.isAuthenticated ? <>{children}</> : <Login />;
    },
    () => false
);

export function Login(): JSX.Element {
    const $vm = useViewModel(LoginViewModel);

    return (
        <div className={Style.m2}>
            <h1 className={Style.textCenter}><Message id="pages.login.pageTitle" /></h1>
            <BusyContent $vm={$vm}>
                <FormInput className={Style.mb3} id="email" type="text" label="pages.login.email.label" field={$vm.email} placeholder="pages.login.email.placeholder" />
                <FormInput className={Style.mb3} id="password" type="password" label="pages.login.password.label" field={$vm.password} />

                <div className={Style.mb3}>
                    <button type="button" disabled={($vm.isInvalid && $vm.areAllFieldsTouched)} className={classnames(Style.btn, Style.btnPrimary)} onClick={() => $vm.loginAsync()}>
                        <Message id="pages.login.submit.label" />
                    </button>
                </div>

                <ul>
                    <li><Link to="/user-accounts/create"><Message id="pages.login.signUp.label" /></Link></li>
                    <li><a href="#"><Message id="pages.login.recoverUserAccount.label" /></a></li>
                </ul>
            </BusyContent>
        </div>
    );
}