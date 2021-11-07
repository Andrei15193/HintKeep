import type { MouseEvent } from 'react';
import type { AccountHint } from '../../../view-models/accounts-hints/account-hints-view-model';
import React, { useEffect, useState } from 'react';
import { useHistory, useParams } from 'react-router-dom';
import classnames from 'classnames';
import { Message } from '../../i18n';
import { BusyContent } from '../../loaders';
import { AccountHintsViewModel } from '../../../view-models/accounts-hints/account-hints-view-model';
import { If, Then } from '../../conditionals';

import Style from '../../style.scss';
import { useViewModel } from '../../use-view-model';

export interface IAccountHintsRouteParams {
    readonly id: string
}

export function AccountHints(): JSX.Element {
    const { push } = useHistory();
    const { id } = useParams<IAccountHintsRouteParams>();
    const $vm = useViewModel(({ axios, alertsViewModel, sessionViewModel }) => new AccountHintsViewModel(axios, alertsViewModel, sessionViewModel));
    useEffect(() => { $vm.loadAsync(id); }, [$vm]);
    const [deleteConfirmationIndex, setDeleteConfirmationIndex] = useState<number | undefined>(undefined);

    return (
        <>
            <div className={Style.mx3}>
                <h1 className={classnames(Style.container, Style.containerFluid, Style.p0)}>
                    <div className={classnames(Style.row, Style.g0, Style.dFlex, Style.alignItemsCenter)}>
                        <div className={classnames(Style.col2, Style.textStart)}>
                            <button type="button" onClick={() => push(`/accounts/${id}`)} className={classnames(Style.btn, Style.btnSm, Style.btnPrimary)}>
                                <Message id="pages.accountHints.back.label" />
                            </button>
                        </div>
                        <div className={classnames(Style.col8, Style.textCenter)}>
                            <Message id="pages.accountHints.pageTitle" />
                        </div>
                    </div>
                </h1>
                <hr />
            </div>
            <div className={classnames(Style.dFlex, Style.flexFill, Style.flexColumn, Style.mx3)}>
                <BusyContent $vm={$vm}>
                    {
                        $vm.accountHints.length === 0
                            ? <NoAccountHintsMessage />
                            : $vm.accountHints.map((accountHint, accountHintIndex) => (
                                <div key={accountHint.id} className={Style.mb3}>
                                    <AccountHintDisplay accountHint={accountHint} onDeleteButtonClick={() => toggleConfirmation(accountHintIndex)} />
                                    <If condition={deleteConfirmationIndex === accountHintIndex}>
                                        <Then>
                                            <ConfirmAccountHintDeletetion onDeleteButtonClick={() => confirmDeletion(accountHint.id)} onCancelButtonClick={dismissConfimation} />
                                        </Then>
                                    </If>
                                </div>
                            ))
                    }
                </BusyContent>
            </div>
        </>
    );

    function toggleConfirmation(accountHintIndex: number): void {
        if (deleteConfirmationIndex === undefined)
            setDeleteConfirmationIndex(accountHintIndex);
        else
            setDeleteConfirmationIndex(undefined);
    }

    function confirmDeletion(hintId: string): void {
        setDeleteConfirmationIndex(undefined);
        $vm.deleteAsync(hintId);
    }

    function dismissConfimation(): void {
        setDeleteConfirmationIndex(undefined);
    }
}

function NoAccountHintsMessage(): JSX.Element {
    return (
        <div className={Style.textCenter}>
            <h6 className={Style.mb3}><Message id="pages.accountHints.noAccountHints" /></h6>
        </div>
    );
}

interface IAccountHintDisplayProps {
    readonly accountHint: AccountHint;

    onDeleteButtonClick(ev: MouseEvent<HTMLButtonElement>): void;
}

function AccountHintDisplay({ accountHint, onDeleteButtonClick }: IAccountHintDisplayProps): JSX.Element {
    return (
        <div className={classnames(Style.dFlex, Style.alignItemsStart)}>
            <div className={Style.flexFill}>
                <h6 className={Style.m0}>{accountHint.hint}</h6>
                <p className={Style.m0}><Message id="pages.accountHints.dateAdded" values={{ dateAdded: accountHint.dateAdded.toLocaleDateString() }} /></p>
            </div>
            <button className={classnames(Style.ms2, Style.btn, Style.btnSm, Style.btnDanger)} onClick={onDeleteButtonClick}>
                <Message id="pages.accountHints.delete.label" />
            </button>
        </div>
    );
}

export interface IConfirmAccountHintDeletetionProps {
    onDeleteButtonClick(event: MouseEvent<HTMLButtonElement>): void;
    onCancelButtonClick(event: MouseEvent<HTMLButtonElement>): void;
}

function ConfirmAccountHintDeletetion({ onDeleteButtonClick, onCancelButtonClick }: IConfirmAccountHintDeletetionProps): JSX.Element {
    return (
        <div className={classnames(Style.card, Style.mb3)}>
            <div className={Style.cardBody}>
                <h5 className={Style.cardTitle}>
                    <Message id="pages.accountHints.delete.confirmationModalTitle" />
                </h5>
                <p className={Style.cardText}>
                    <Message id="pages.accountHints.delete.confirmation" />
                </p>

                <div className={classnames(Style.dFlex, Style.flexRow)}>
                    <button type="button" className={classnames(Style.btn, Style.btnDanger)} onClick={onDeleteButtonClick}>
                        <Message id="pages.accountHints.delete.label" />
                    </button>
                    <button type="button" className={classnames(Style.msAuto, Style.btn, Style.btnSecondary)} onClick={onCancelButtonClick}>
                        <Message id="pages.accountHints.cancel.label" />
                    </button>
                </div>
            </div>
        </div>
    );
}