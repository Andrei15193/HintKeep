import React, { useEffect } from 'react';
import { Link, useHistory, useParams } from 'react-router-dom';
import classnames from 'classnames';
import { Message } from '../../i18n';
import { BusyContent } from '../../loaders';
import { watchEvent, useViewModel } from '../../view-model-wrappers';
import { DeletedAccountDetailsViewModel } from '../../../view-models/deleted-account-details-view-model';
import { FormInput, CheckboxFormInput } from '../forms';

import Style from '../../style.scss';

export interface IDeletedAccountDetailsRouteParams {
    readonly id: string
}

export function DeletedAccountDetails(): JSX.Element {
    const { push } = useHistory();
    const $vm = useViewModel(DeletedAccountDetailsViewModel);
    const { id } = useParams<IDeletedAccountDetailsRouteParams>();

    useEffect(() => { $vm.loadAsync(id); }, [$vm, id]);
    watchEvent($vm.restoredEvent, () => push('/accounts/bin'));

    return (
        <div className={Style.m2}>
            <h1 className={Style.textCenter}><Message id="pages.deletedAccountDetails.pageTitle" /></h1>
            <BusyContent $vm={$vm}>
                <FormInput className={Style.mb3} id="name" type="text" disabled label="pages.deletedAccountDetails.name.label" field={$vm.name} />
                <FormInput className={Style.mb3} id="hint" type="text" disabled label="pages.deletedAccountDetails.hint.label" field={$vm.hint} />
                <CheckboxFormInput className={Style.mb3} id="isPinned" disabled label="pages.deletedAccountDetails.isPinned.label" field={$vm.isPinned} />

                <div className={classnames(Style.dFlex, Style.flexRow)}>
                    <button type="button" disabled={!$vm.isLoaded} className={classnames(Style.btn, Style.btnPrimary)} onClick={() => $vm.restoreAsync()}>
                        <Message id="pages.deletedAccountDetails.restore.label" />
                    </button>
                    <Link to="/accounts/bin" className={classnames(Style.ml2, Style.btn, Style.btnLight)}>
                        <Message id="pages.deletedAccountDetails.cancel.label" />
                    </Link>
                </div>
            </BusyContent>
        </div>
    );
}