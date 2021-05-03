import React, { useEffect, useState } from 'react';
import { Link, useHistory, useParams } from 'react-router-dom';
import classnames from 'classnames';
import { Message } from '../../i18n';
import { BusyContent } from '../../loaders';
import { watchEvent, useViewModel } from '../../view-model-hooks';
import { EditAccountViewModel } from '../../../view-models/edit-account-view-model';
import { FormInput, FormCheckboxInput, FormTextArea } from '../forms';
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
        <div className={Style.mx3}>
            <h1 className={classnames(Style.container, Style.containerFluid, Style.p0)}>
                <div className={classnames(Style.row, Style.noGutters, Style.dFlex, Style.alignItemsCenter)}>
                    <div className={classnames(Style.col2, Style.textLeft)}>
                        <button type="button" onClick={() => push('/')} className={classnames(Style.btn, Style.btnSm, Style.btnPrimary)}>
                            <Message id="pages.editAccount.back.label" />
                        </button>
                    </div>
                    <div className={classnames(Style.col8, Style.textCenter)}>
                        <Message id="pages.editAccount.pageTitle" />
                    </div>
                </div>
            </h1>

            <BusyContent $vm={$vm}>
                <FormInput className={Style.mb3} id="name" type="text" disabled={!isConfirmationHidden} label="pages.editAccount.name.label" field={$vm.name} placeholder="pages.editAccount.name.placeholder" />
                <FormInput className={Style.mb3} id="hint" type="text" disabled={!isConfirmationHidden} label="pages.editAccount.hint.label" field={$vm.hint} placeholder="pages.editAccount.hint.placeholder" />
                <FormCheckboxInput className={Style.mb3} id="isPinned" disabled={!isConfirmationHidden} label="pages.editAccount.isPinned.label" field={$vm.isPinned} description="pages.editAccount.isPinned.description" />
                <FormTextArea className={Style.mb3} id="notes" label="pages.editAccount.notes.label" field={$vm.notes} />

                <If condition={isConfirmationHidden}>
                    <Then>
                        <div className={classnames(Style.dFlex, Style.flexRow, Style.mb3)}>
                            <button type="button" disabled={(!$vm.isLoaded || ($vm.isInvalid && $vm.areAllFieldsTouched))} className={classnames(Style.btn, Style.btnPrimary)} onClick={() => $vm.submitAsync()}>
                                <Message id="pages.editAccount.save.label" />
                            </button>
                            <Link to={`/accounts/${id}/hints`} className={classnames(Style.ml2, Style.btn, Style.btnLight)}>
                                <Message id="pages.editAccount.viewAllHints.label" />
                            </Link>
                            <Link to="/" className={classnames(Style.ml2, Style.btn, Style.btnLight)}>
                                <Message id="pages.editAccount.cancel.label" />
                            </Link>
                            <button type="button" disabled={!$vm.isLoaded} className={classnames(Style.btn, Style.btnDanger, Style.mlAuto)} onClick={() => setIsConfirmationHidden(false)}>
                                <Message id="pages.editAccount.delete.label" />
                            </button>
                        </div>
                    </Then>
                    <Else>
                        <div className={classnames(Style.card, Style.mb3)}>
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