import type { ComponentType } from 'react';
import type { AccountsListViewModel, Account } from '../../../../view-models/accounts-list-view-model';
import React, { useContext, Fragment } from 'react';
import { Link } from 'react-router-dom';
import classnames from 'classnames';
import { watchCollection, watchViewModel } from 'react-model-view-viewmodel';
import { I18nContext, Message } from '../../../i18n';

import Style from '../../../style.scss';

export interface IAccountsSearchListProps {
    readonly $vm: AccountsListViewModel;
    readonly noItemsComponent: ComponentType;

    getDetailsRoute(account: Account): string;
}

export function AccountsSearchList({ $vm, noItemsComponent: NoItemsComponent, getDetailsRoute }: IAccountsSearchListProps): JSX.Element {
    const messageResolver = useContext(I18nContext);
    watchViewModel($vm, ['searchText']);
    watchCollection($vm.filteredAccounts);

    if ($vm.accounts.length === 0)
        return <NoItemsComponent />;
    else
        return (
            <>
                <div className={Style.mx3}>
                    <div className={classnames(Style.inputGroup, Style.inputGroupSm, Style.mb3)}>
                        <input
                            type="text"
                            inputMode="search"
                            className={Style.formControl}
                            value={$vm.searchText}
                            onChange={event => $vm.searchText = event.target.value}
                            placeholder={messageResolver.resolve('pages.accounts.search.placeholder')}
                        />
                    </div>
                </div>

                <div className={classnames(Style.px3, Style.pt2, Style.listView, Style.flexFill)}>
                    <AccountsListDisplay accounts={$vm.filteredAccounts} getDetailsRoute={getDetailsRoute} />
                </div>
            </>
        );
}

export interface IAccountsListDisplayProps {
    readonly accounts: readonly Account[];

    getDetailsRoute(account: Account): string;
}

export function AccountsListDisplay({ accounts, getDetailsRoute }: IAccountsListDisplayProps): JSX.Element {
    let previousIsPinned: boolean | null = null;
    return (
        <>
            {
                accounts.map(account => {
                    const showSeparator = account.isPinned !== previousIsPinned && previousIsPinned !== null;
                    previousIsPinned = account.isPinned;
                    return (
                        <Fragment key={account.id}>
                            {showSeparator && <hr className={Style.mt0} />}
                            <AccountDisplay detailsRoute={getDetailsRoute(account)} account={account} />
                        </Fragment>
                    );
                })
            }
        </>
    );
}

export interface IAccountDisplayProps {
    readonly account: Account;
    readonly detailsRoute: string;
}

export function AccountDisplay({ account, detailsRoute: detailsPapth }: IAccountDisplayProps): JSX.Element {
    return (
        <div className={classnames(Style.dFlex, Style.alignItemsStart)}>
            <div className={Style.flexFill}>
                <h6 className={Style.m0}>{account.name}</h6>
                <p className={Style.mb3}>{account.hint}</p>
            </div>
            <Link to={detailsPapth} className={classnames(Style.ms2, Style.btn, Style.btnSm, Style.btnPrimary)}><Message id="pages.accounts.edit.label" /></Link>
        </div>
    );
}