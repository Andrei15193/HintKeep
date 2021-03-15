import React from 'react';
import { Link, useHistory } from 'react-router-dom';
import classnames from 'classnames';
import { Message } from '../i18n';
import { BusyContent } from '../loaders';
import { useEvent, useViewModel } from '../view-model-wrappers';
import { AddAccountViewModel } from '../../view-models/add-account-view-model';
import { FormInput, CheckboxFormInput } from './forms';

import Style from '../style.scss';

export function AddAccount(): JSX.Element {
    const history = useHistory();
    const $vm = useViewModel(AddAccountViewModel);
    useEvent($vm.submittedEvent, () => history.push('/'));

    return (
        <div className={Style.m2}>
            <h1 className={Style.textCenter}><Message id="pages.addAccount.pageTitle" /></h1>
            <BusyContent $vm={$vm}>
                <FormInput className={Style.mb3} id="name" type="text" label="pages.addAccount.name.label" field={$vm.name} placeholder="pages.addAccount.name.placeholder" />
                <FormInput className={Style.mb3} id="hint" type="text" label="pages.addAccount.hint.label" field={$vm.hint} placeholder="pages.addAccount.hint.placeholder" />
                <CheckboxFormInput className={Style.mb3} id="isPinned" label="pages.addAccount.isPinned.label" field={$vm.isPinned} description="pages.addAccount.isPinned.description" />

                <div>
                    <button type="submit" disabled={($vm.isInvalid && $vm.areAllFieldsTouched)} className={classnames(Style.btn, Style.btnPrimary)} onClick={() => $vm.submitAsync()}>
                        <Message id="pages.addAccount.add.label" />
                    </button>
                    <Link to="/" className={classnames(Style.ml2, Style.btn, Style.btnLight)}>
                        <Message id="pages.addAccount.cancel.label" />
                    </Link>
                </div>
            </BusyContent>
        </div>
    );
}