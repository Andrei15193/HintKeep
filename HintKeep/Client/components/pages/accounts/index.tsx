import React, { useEffect } from 'react';
import { useHistory } from 'react-router-dom';
import classnames from 'classnames';
import { LoginGuard } from '../login';
import { Message } from '../../i18n';
import { useViewModel } from '../../view-model-wrappers';
import { AccountsViewModel } from '../../../view-models/accounts-view-model';
import { ApiViewModelState } from '../../../view-models/core';
import { BusyContent } from '../../loaders';
import { NoAccountsMessage } from './no-accounts-message';
import { AccountsDisplayList } from './accounts-display-list';

import Style from '../../style.scss';

export function Accounts(): JSX.Element {
    const { push } = useHistory();
    const $vm = useViewModel(AccountsViewModel);
    useEffect(() => { $vm.loadAsync(); }, [$vm]);

    return (
        <LoginGuard>
            <div className={Style.mx3}>
                <h1 className={classnames(Style.container, Style.containerFluid, Style.p0)}>
                    <div className={classnames(Style.row, Style.noGutters, Style.dFlex, Style.alignItemsCenter)}>
                        <div className={classnames(Style.col6, Style.offset3, Style.textCenter)}>
                            <Message id="pages.accounts.pageTitle" />
                        </div>
                        <div className={classnames(Style.col3, Style.textRight)}>
                            <button type="button" disabled={$vm.state === ApiViewModelState.Busy} onClick={() => push('/accounts/add')} className={classnames(Style.btn, Style.btnPrimary)}>
                                <Message id="pages.accounts.add.label" />
                            </button>
                        </div>
                    </div>
                </h1>
                <hr className={Style.w100} />
            </div>
            <BusyContent $vm={$vm}>
                {
                    $vm.accounts.length === 0
                        ? <NoAccountsMessage />
                        : <AccountsDisplayList accounts={$vm.accounts} />
                }
            </BusyContent>
        </LoginGuard>
    );
}