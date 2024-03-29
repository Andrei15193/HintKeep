import React, { useEffect } from 'react';
import { Link } from 'react-router-dom';
import classnames from 'classnames';
import { Message } from '../../i18n';
import { useViewModel } from '../../use-view-model';
import { AccountsViewModel } from '../../../view-models/accounts/accounts-view-model';
import { BusyContent } from '../../loaders';
import { AccountsSearchList } from './common/accounts-search-list';

import Style from '../../style.scss';

export function Accounts(): JSX.Element {
    const $vm = useViewModel(({ axios, alertsViewModel, sessionViewModel }) => new AccountsViewModel(axios, alertsViewModel, sessionViewModel));
    useEffect(() => { $vm.loadAsync(); }, [$vm]);

    return (
        <>
            <div className={Style.mx3}>
                <h1 className={classnames(Style.container, Style.containerFluid, Style.p0)}>
                    <div className={classnames(Style.row, Style.g0, Style.dFlex, Style.alignItemsCenter)}>
                        <div className={classnames(Style.col2, Style.textStart)}>
                            <Link to="/extra" className={classnames(Style.btn, Style.btnSm, Style.btnPrimary, Style.px2, Style.textBold)}>
                                <Message id="pages.accounts.extra.label" />
                            </Link>
                        </div>
                        <div className={classnames(Style.col8, Style.textCenter)}>
                            <Message id="pages.accounts.pageTitle" />
                        </div>
                        <div className={classnames(Style.col2, Style.textEnd)}>
                            <Link to="/accounts/add" className={classnames(Style.btn, Style.btnSm, Style.btnPrimary)}>
                                <Message id="pages.accounts.add.label" />
                            </Link>
                        </div>
                    </div>
                </h1>
                <hr />
            </div>

            <div className={classnames(Style.dFlex, Style.flexFill, Style.flexColumn)}>
                <BusyContent $vm={$vm}>
                    <AccountsSearchList $vm={$vm.accounts} noItemsComponent={NoAccountsMessage} getDetailsRoute={account => `/accounts/${account.id}`} />
                </BusyContent>
            </div>
        </>
    );
}

export function NoAccountsMessage(): JSX.Element {
    return (
        <div className={Style.textCenter}>
            <h6 className={Style.mb3}><Message id="pages.accounts.noAccounts" /></h6>
            <Link to="/accounts/add" className={classnames(Style.btn, Style.btnPrimary)}><Message id="pages.accounts.add.label" /></Link>
        </div>
    );
}