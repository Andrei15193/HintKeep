import React from 'react';
import { Link, useHistory } from 'react-router-dom';
import classnames from 'classnames';
import { watchEvent, watchViewModel } from 'react-model-view-viewmodel';
import { Message } from '../../i18n';
import { useViewModel } from '../../use-view-model';
import { BusyContent } from '../../loaders';
import { FormInput } from '../../forms';
import { LoginViewModel } from '../../../view-models/users/login-view-model';

import Style from '../../style.scss';

export function Login(): JSX.Element {
    const { push } = useHistory();

    const $vm = useViewModel(({ axios, alertsViewModel, sessionViewModel }) => new LoginViewModel(axios, alertsViewModel, sessionViewModel));
    watchEvent($vm.authenticated, () => push('/accounts'));
    watchViewModel($vm.form);

    return (
        <div className={Style.mx3}>
            <h1 className={classnames(Style.container, Style.textCenter)}>
                <Message id="pages.login.pageTitle" />
            </h1>

            <BusyContent $vm={$vm}>
                <form onSubmit={event => { event.preventDefault(); $vm.authenticateAsync(); }}>
                    <FormInput className={Style.mb3} id="email" type="email" label="pages.login.email.label" field={$vm.form.email} placeholder="pages.login.email.placeholder" />
                    <FormInput className={Style.mb3} id="password" type="password" label="pages.login.password.label" field={$vm.form.password} placeholder="pages.login.password.placeholder" />

                    <div className={Style.mb3}>
                        <button type="submit" disabled={($vm.form.isInvalid && $vm.form.areAllFieldsTouched)} className={classnames(Style.btn, Style.btnPrimary)}>
                            <Message id="pages.login.login.label" />
                        </button>
                    </div>
                </form>
            </BusyContent>

            <ul>
                <li><Link to="/recovery"><Message id="pages.login.forgotPassword.label" /></Link></li>
                <li><Link to="/register"><Message id="pages.login.register.label" /></Link></li>
            </ul>

            <p><Message id="pages.login.motivation.paragraph1" /></p>
            <p><Message id="pages.login.motivation.paragraph2" /></p>
            <p><Message id="pages.login.motivation.paragraph3" /></p>
            <p><Message id="pages.login.motivation.paragraph4" /></p>
        </div>
    );
}