import { useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import classnames from 'classnames';
import { IEventHandler, useViewModel, useViewModelDependency } from 'react-model-view-viewmodel';
import { Message } from '../../i18n';
import { ConfirmUserViewModel } from '../../../view-models/users/confirm-user-view-model';
import { BusyContent } from '../../loaders';
import { FormInput } from '../../forms';

import Style from '../../style.scss';

export function Confirmation(): JSX.Element {
    const navigate = useNavigate();

    const confirmUserViewModel = useViewModelDependency(ConfirmUserViewModel);
    useViewModel(confirmUserViewModel.form);

    useEffect(
        () => {
            const confirmedEventHandler: IEventHandler<unknown> = {
                handle() {
                    navigate('/')
                }
            }

            confirmUserViewModel.confirmed.subscribe(confirmedEventHandler);

            return () => {
                confirmUserViewModel.confirmed.unsubscribe(confirmedEventHandler);
            }
        },
        [confirmUserViewModel, navigate]
    )

    return (
        <div className={Style.mx3}>
            <h1 className={classnames(Style.container, Style.textCenter)}>
                <Message id="pages.confirmation.pageTitle" />
            </h1>

            <p>
                <Message id="pages.confirmation.description" />
            </p>

            <BusyContent apiViewModel={confirmUserViewModel}>
                <form onSubmit={event => { event.preventDefault(); confirmUserViewModel.confirmAsync(); }}>
                    <FormInput className={Style.mb3} id="token" type="text" label="pages.confirmation.token.label" field={confirmUserViewModel.form.token} placeholder="pages.confirmation.token.placeholder" />

                    <div className={Style.mb3}>
                        <button type="submit" disabled={(confirmUserViewModel.form.isInvalid && confirmUserViewModel.form.areAllFieldsTouched)} className={classnames(Style.btn, Style.btnPrimary)}>
                            <Message id="pages.confirmation.confirm.label" />
                        </button>
                        <Link to="/" className={classnames(Style.ms2, Style.btn, Style.btnLight)}>
                            <Message id="pages.confirmation.cancel.label" />
                        </Link>
                    </div>
                </form>
            </BusyContent>
        </div>
    );
}