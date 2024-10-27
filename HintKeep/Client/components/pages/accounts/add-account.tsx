import { useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import classnames from 'classnames';
import { type IEventHandler, useViewModel, useViewModelDependency } from 'react-model-view-viewmodel';
import { Message } from '../../i18n';
import { BusyContent } from '../../loaders';
import { AddAccountViewModel } from '../../../view-models/accounts/add-account-view-model';
import { FormInput, FormCheckboxInput, FormTextArea } from '../../forms';

import Style from '../../style.scss';

export function AddAccount(): JSX.Element {
    const navigate = useNavigate();

    const addAcountViewModel = useViewModelDependency(AddAccountViewModel);
    useViewModel(addAcountViewModel.form);

    useEffect(
        () => {
            const submittedEventHandler: IEventHandler<unknown> = {
                handle() {
                    navigate('/')
                }
            }

            addAcountViewModel.submittedEvent.subscribe(submittedEventHandler);

            return () => {
                addAcountViewModel.submittedEvent.unsubscribe(submittedEventHandler);
            }
        },
        [addAcountViewModel, navigate]
    )

    return (
        <div className={Style.mx3}>
            <h1 className={classnames(Style.container, Style.containerFluid, Style.p0)}>
                <div className={classnames(Style.row, Style.g0, Style.dFlex, Style.alignItemsCenter)}>
                    <div className={classnames(Style.col2, Style.textStart)}>
                        <Link to="/" className={classnames(Style.btn, Style.btnSm, Style.btnPrimary)}>
                            <Message id="pages.addAccount.back.label" />
                        </Link>
                    </div>
                    <div className={classnames(Style.col8, Style.textCenter)}>
                        <Message id="pages.addAccount.pageTitle" />
                    </div>
                </div>
            </h1>

            <BusyContent apiViewModel={addAcountViewModel}>
                <form onSubmit={event => { event.preventDefault(); addAcountViewModel.submitAsync(); }}>
                    <FormInput className={Style.mb3} id="name" type="text" label="pages.addAccount.name.label" field={addAcountViewModel.form.name} placeholder="pages.addAccount.name.placeholder" />
                    <FormInput className={Style.mb3} id="hint" type="text" label="pages.addAccount.hint.label" field={addAcountViewModel.form.hint} placeholder="pages.addAccount.hint.placeholder" />
                    <FormCheckboxInput className={Style.mb3} id="isPinned" label="pages.addAccount.isPinned.label" field={addAcountViewModel.form.isPinned} description="pages.addAccount.isPinned.description" />
                    <FormTextArea className={Style.mb3} id="notes" label="pages.addAccount.notes.label" field={addAcountViewModel.form.notes} />

                    <div className={Style.mb3}>
                        <button type="submit" disabled={(addAcountViewModel.form.isInvalid && addAcountViewModel.form.areAllFieldsTouched)} className={classnames(Style.btn, Style.btnPrimary)}>
                            <Message id="pages.addAccount.add.label" />
                        </button>
                        <Link to="/" className={classnames(Style.ms2, Style.btn, Style.btnLight)}>
                            <Message id="pages.addAccount.cancel.label" />
                        </Link>
                    </div>
                </form>
            </BusyContent>
        </div>
    );
}