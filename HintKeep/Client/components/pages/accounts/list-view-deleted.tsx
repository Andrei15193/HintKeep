import React, { useEffect } from 'react';
import { Link } from 'react-router-dom';
import classnames from 'classnames';
import { Message } from '../../i18n'
import { useViewModel } from '../../use-view-model';
import { DeletedAccountsViewModel } from '../../../view-models/deleted-accounts-view-model';
import { BusyContent } from '../../loaders';
import { AccountsSearchList } from './common/accounts-search-list';

import Style from '../../style.scss';

export function DeletedAccounts(): JSX.Element {
    const $vm = useViewModel(({ alertsViewModel }) => new DeletedAccountsViewModel(alertsViewModel));
    useEffect(() => { $vm.loadAsync(); }, [$vm]);

    return (
        <>
            <div className={Style.mx3}>
                <h1 className={classnames(Style.container, Style.containerFluid, Style.p0)}>
                    <div className={classnames(Style.row, Style.noGutters, Style.dFlex, Style.alignItemsCenter)}>
                        <div className={classnames(Style.col2, Style.textLeft)}>
                            <Link to="/extra" className={classnames(Style.btn, Style.btnSm, Style.btnPrimary)}>
                                <Message id="pages.deletedAccounts.back.label" />
                            </Link>
                        </div>
                        <div className={classnames(Style.col8, Style.textCenter)}>
                            <Message id="pages.deletedAccounts.pageTitle" />
                        </div>
                    </div>
                </h1>
                <hr className={Style.w100} />
            </div>

            <div className={classnames(Style.dFlex, Style.flexFill, Style.flexColumn)}>
                <BusyContent $vm={$vm}>
                    <AccountsSearchList $vm={$vm.accounts} noItemsComponent={NoAccountsMessage} getDetailsRoute={account => `/accounts/bin/${account.id}`} />
                </BusyContent>
            </div>
        </>
    );
}

export function NoAccountsMessage(): JSX.Element {
    return (
        <div className={Style.textCenter}>
            <h6 className={Style.mb3}><Message id="pages.deletedAccounts.noAccounts" /></h6>
        </div>
    );
}