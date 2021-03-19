import React, { useEffect, useState } from 'react';
import { Link, useHistory, useParams } from 'react-router-dom';
import classnames from 'classnames';
import { Message } from '../../i18n';
import { BusyContent } from '../../loaders';
import { watchEvent, useViewModel } from '../../view-model-wrappers';
import { EditAccountViewModel } from '../../../view-models/edit-account-view-model';
import { FormInput, CheckboxFormInput } from '../forms';
import { Else, If, Then } from '../../conditionals';

import Style from '../../style.scss';

export interface IEditAccountRouteParams {
    readonly id: string
}

export function EditAccount(): JSX.Element {
    const { push } = useHistory();
    const $vm = useViewModel(EditAccountViewModel);
    const { id } = useParams<IEditAccountRouteParams>();

    const [isConfirmationHidden, setIsConfirmationHidden] = useState(true);
    useEffect(() => { $vm.loadAsync(id); }, [$vm, id]);
    watchEvent($vm.editedEvent, () => push('/'));
    watchEvent($vm.deletedEvent, () => push('/'));

    return (
        <div className={Style.m2}>
            <h1 className={Style.textCenter}><Message id="pages.editAccount.pageTitle" /></h1>
            <BusyContent $vm={$vm}>
                <FormInput className={Style.mb3} id="name" type="text" disabled={!isConfirmationHidden} label="pages.editAccount.name.label" field={$vm.name} placeholder="pages.editAccount.name.placeholder" />
                <FormInput className={Style.mb3} id="hint" type="text" disabled={!isConfirmationHidden} label="pages.editAccount.hint.label" field={$vm.hint} placeholder="pages.editAccount.hint.placeholder" />
                <CheckboxFormInput className={Style.mb3} id="isPinned" disabled={!isConfirmationHidden} label="pages.editAccount.isPinned.label" field={$vm.isPinned} description="pages.editAccount.isPinned.description" />

                <If condition={isConfirmationHidden}>
                    <Then>
                        <div className={classnames(Style.dFlex, Style.flexRow)}>
                            <button type="button" disabled={(!$vm.isLoaded || ($vm.isInvalid && $vm.areAllFieldsTouched))} className={classnames(Style.btn, Style.btnPrimary)} onClick={() => $vm.submitAsync()}>
                                <Message id="pages.editAccount.edit.label" />
                            </button>
                            <Link to="/" className={classnames(Style.ml2, Style.btn, Style.btnLight)}>
                                <Message id="pages.editAccount.cancel.label" />
                            </Link>
                            <button type="button" disabled={!$vm.isLoaded} className={classnames(Style.btn, Style.btnDanger, Style.mlAuto)} onClick={() => setIsConfirmationHidden(false)}>
                                <Message id="pages.editAccount.delete.label" />
                            </button>
                        </div>
                    </Then>
                    <Else>
                        <div className={Style.card}>
                            <div className={Style.cardBody}>
                                <h5 className={Style.cardTitle}>
                                    <Message id="pages.editAccount.delete.confirmationModalTitle" />
                                </h5>
                                <p className={Style.cardText}>
                                    <Message id="pages.editAccount.delete.confirmation" />
                                </p>

                                <div className={classnames(Style.dFlex, Style.flexRow)}>
                                    <button type="button" className={classnames(Style.btn, Style.btnDanger)} onClick={() => { setIsConfirmationHidden(true); $vm.deleteAsync(); }}>
                                        <Message id="pages.editAccount.moveToBin.label" />
                                    </button>
                                    <button type="button" className={classnames(Style.mlAuto, Style.btn, Style.btnSecondary)} onClick={() => setIsConfirmationHidden(true)}>
                                        <Message id="pages.editAccount.cancel.label" />
                                    </button>
                                </div>
                            </div>
                        </div>
                    </Else>
                </If>
            </BusyContent>
        </div>
    );
}