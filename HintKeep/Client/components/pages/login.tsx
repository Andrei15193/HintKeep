import type { ComponentType, PropsWithChildren } from 'react';
import React, { memo } from 'react';
import { Link } from 'react-router-dom';
import classnames from 'classnames';
import { WithViewModel } from '../view-model-wrappers';
import { LoginViewModel } from '../../view-models/login-view-model';
import { UserViewModel } from '../../view-models/user-view-model';
import { BusyContent } from '../loaders';

import Style from '../style.scss';

export const LoginGuard: ComponentType<PropsWithChildren<{}>> = memo(
    function ({ children }): JSX.Element {
        return (
            <WithViewModel viewModelType={UserViewModel}>{($vm) => <>
                {$vm.isAuthenticated ? <>{children}</> : <Login />}
            </>}</WithViewModel>
        );
    }
);

export function Login(): JSX.Element {
    return (
        <WithViewModel viewModelType={LoginViewModel}>{($vm) => <>
            <h1 className={Style.textCenter}>Login</h1>
            <BusyContent $vm={$vm}>
                <div className={Style.mb3}>
                    <label htmlFor="e-mail" className={Style.colFormLabel}>Email address</label>
                    <input type="email" className={Style.formControl} id="e-mail" placeholder="name@example.com" value={$vm.email} onChange={ev => $vm.email = ev.target.value} />
                </div>
                <div className={Style.mb3}>
                    <label htmlFor="password" className={Style.colFormLabel}>Password</label>
                    <input type="password" className={Style.formControl} id="password" value={$vm.password} onChange={ev => $vm.password = ev.target.value} />
                </div>
                <div className={Style.mb3}>
                    <button type="submit" className={classnames(Style.btn, Style.btnPrimary)} onClick={() => $vm.loginAsync()}>Login</button>
                </div>

                <ul>
                    <li><Link to="/user-accounts/create">Sign-Up</Link></li>
                    <li><a href="#">Recover account</a></li>
                </ul>
            </BusyContent>
        </>}</WithViewModel>
    );
}