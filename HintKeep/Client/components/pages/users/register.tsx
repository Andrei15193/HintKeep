import React from 'react';
import { Link, useNavigate } from 'react-router-dom';
import classnames from 'classnames';
import { watchEvent, watchViewModel } from 'react-model-view-viewmodel';
import { useViewModel } from '../../use-view-model';
import { Message } from '../../i18n';
import { BusyContent } from '../../loaders';
import { FormCheckboxInput, FormInput } from '../../forms';
import { RegisterUserViewModel } from '../../../view-models/users/register-view-model';

import Style from '../../style.scss';

export function Register(): JSX.Element {
    const navigate = useNavigate();

    const $vm = useViewModel(({ axios, alertsViewModel, sessionViewModel }) => new RegisterUserViewModel(axios, alertsViewModel, sessionViewModel));
    watchEvent($vm.registered, () => navigate('/confirm'));
    watchViewModel($vm.form);
    watchViewModel($vm.form.termsOfServiceAcceptance);

    return (
        <div className={Style.mx3}>
            <h1 className={classnames(Style.container, Style.textCenter)}>
                <Message id="pages.register.pageTitle" />
            </h1>

            <BusyContent $vm={$vm}>
                <form onSubmit={event => { event.preventDefault(); $vm.submitAsync(); }}>
                    <FormInput className={Style.mb3} id="email" type="email" label="pages.register.email.label" field={$vm.form.email} placeholder="pages.register.email.placeholder" />
                    <FormInput className={Style.mb3} id="hint" type="text" label="pages.register.hint.label" field={$vm.form.hint} placeholder="pages.register.hint.placeholder" />
                    <FormInput className={Style.mb3} id="password" type="password" label="pages.register.password.label" field={$vm.form.password} placeholder="pages.register.password.placeholder" />
                    <FormCheckboxInput className={Style.mb3} id="termsOfServiceAcceptance" label="pages.register.termsOfServiceAcceptance.label" field={$vm.form.termsOfServiceAcceptance}>
                        <Link to="/terms"><Message id="pages.register.terms" /></Link>
                    </FormCheckboxInput>

                    <div className={Style.mb3}>
                        <button type="submit" disabled={(($vm.form.isInvalid && $vm.form.areAllFieldsTouched) || !$vm.form.termsOfServiceAcceptance.value)} className={classnames(Style.btn, Style.btnPrimary)}>
                            <Message id="pages.register.register.label" />
                        </button>
                        <Link to="/" className={classnames(Style.ms2, Style.btn, Style.btnLight)}>
                            <Message id="pages.register.cancel.label" />
                        </Link>
                    </div>
                </form>
            </BusyContent>
        </div>
    );
}