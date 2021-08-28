import React from 'react';
import { Link, useHistory } from 'react-router-dom';
import classnames from 'classnames';
import { watchEvent, watchViewModel } from 'react-model-view-viewmodel';
import { Message } from '../../i18n';
import { BusyContent } from '../../loaders';
import { AddAccountViewModel } from '../../../view-models/add-account-view-model';
import { FormInput, FormCheckboxInput, FormTextArea } from '../forms';
import { useViewModel } from '../../use-view-model';

import Style from '../../style.scss';

export function AddAccount(): JSX.Element {
    const { push } = useHistory();
    const $vm = useViewModel(({ axios, alertsViewModel }) => new AddAccountViewModel(axios, alertsViewModel));
    watchViewModel($vm.form, ['isInvalid', 'areAllFieldsTouched']);
    watchEvent($vm.submittedEvent, () => push('/'));

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

            <BusyContent $vm={$vm}>
                <FormInput className={Style.mb3} id="name" type="text" label="pages.addAccount.name.label" field={$vm.form.name} placeholder="pages.addAccount.name.placeholder" />
                <FormInput className={Style.mb3} id="hint" type="text" label="pages.addAccount.hint.label" field={$vm.form.hint} placeholder="pages.addAccount.hint.placeholder" />
                <FormCheckboxInput className={Style.mb3} id="isPinned" label="pages.addAccount.isPinned.label" field={$vm.form.isPinned} description="pages.addAccount.isPinned.description" />
                <FormTextArea className={Style.mb3} id="notes" label="pages.addAccount.notes.label" field={$vm.form.notes} />

                <div className={Style.mb3}>
                    <button type="button" disabled={($vm.form.isInvalid && $vm.form.areAllFieldsTouched)} className={classnames(Style.btn, Style.btnPrimary)} onClick={() => $vm.submitAsync()}>
                        <Message id="pages.addAccount.add.label" />
                    </button>
                    <Link to="/" className={classnames(Style.ms2, Style.btn, Style.btnLight)}>
                        <Message id="pages.addAccount.cancel.label" />
                    </Link>
                </div>
            </BusyContent>
        </div>
    );
}