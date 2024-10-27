import { useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import classnames from 'classnames';
import { type IEventHandler, useViewModel, useViewModelDependency } from 'react-model-view-viewmodel';
import { Message } from '../../i18n';
import { BusyContent } from '../../loaders';
import { FormCheckboxInput, FormInput } from '../../forms';
import { RegisterUserViewModel } from '../../../view-models/users/register-view-model';

import Style from '../../style.scss';

export function Register(): JSX.Element {
    const navigate = useNavigate();

    const registerUserViewModel = useViewModelDependency(RegisterUserViewModel);
    useViewModel(registerUserViewModel.form);
    useViewModel(registerUserViewModel.form.termsOfServiceAcceptance);

    useEffect(
        () => {
            const registeredEventHandler: IEventHandler<unknown> = {
                handle() {
                    navigate('/confirm')
                }
            }

            registerUserViewModel.registered.subscribe(registeredEventHandler);

            return () => {
                registerUserViewModel.registered.unsubscribe(registeredEventHandler);
            }
        },
        [registerUserViewModel, navigate]
    )

    return (
        <div className={Style.mx3}>
            <h1 className={classnames(Style.container, Style.textCenter)}>
                <Message id="pages.register.pageTitle" />
            </h1>

            <BusyContent apiViewModel={registerUserViewModel}>
                <form onSubmit={event => { event.preventDefault(); registerUserViewModel.submitAsync(); }}>
                    <FormInput className={Style.mb3} id="email" type="email" label="pages.register.email.label" field={registerUserViewModel.form.email} placeholder="pages.register.email.placeholder" />
                    <FormInput className={Style.mb3} id="hint" type="text" label="pages.register.hint.label" field={registerUserViewModel.form.hint} placeholder="pages.register.hint.placeholder" />
                    <FormInput className={Style.mb3} id="password" type="password" label="pages.register.password.label" field={registerUserViewModel.form.password} placeholder="pages.register.password.placeholder" />
                    <FormCheckboxInput className={Style.mb3} id="termsOfServiceAcceptance" label="pages.register.termsOfServiceAcceptance.label" field={registerUserViewModel.form.termsOfServiceAcceptance}>
                        <Link to="/terms"><Message id="pages.register.terms" /></Link>
                    </FormCheckboxInput>

                    <div className={Style.mb3}>
                        <button type="submit" disabled={((registerUserViewModel.form.isInvalid && registerUserViewModel.form.areAllFieldsTouched) || !registerUserViewModel.form.termsOfServiceAcceptance.value)} className={classnames(Style.btn, Style.btnPrimary)}>
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