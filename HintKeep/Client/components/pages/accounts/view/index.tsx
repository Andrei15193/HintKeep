import React, { useEffect } from 'react';
import { Link, useHistory } from 'react-router-dom';
import classnames from 'classnames';
import { Message } from '../../../i18n';
import { useViewModel } from '../../../view-model-hooks';
import { AccountsViewModel } from '../../../../view-models/accounts-view-model';
import { ApiViewModelState } from '../../../../view-models/core';
import { BusyContent } from '../../../loaders';
import { NoAccountsMessage } from './no-accounts-message';
import { AccountsDisplayList } from './accounts-display-list';

import Style from '../../../style.scss';

export function Accounts(): JSX.Element {
    const { push } = useHistory();
    const $vm = useViewModel(AccountsViewModel);
    useEffect(() => { $vm.loadAsync(); }, [$vm]);

    return (
        <>
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
                <hr />
            </div>
            <div className={classnames(Style.dFlex, Style.flexFill, Style.flexColumn)}>
                <div className={Style.flexFill}>
                    <BusyContent $vm={$vm}>
                        {
                            $vm.accounts.length === 0
                                ? <NoAccountsMessage />
                                : <AccountsDisplayList accounts={$vm.accounts} />
                        }
                    </BusyContent>
                </div>
                <div className={classnames(Style.mb2, Style.mx3, Style.textCenter)}>
                    <hr className={Style.my2} />
                    <Link to="/accounts/bin"><Message id="pages.accounts.viewAccountsBin.labels" /></Link>
                </div>
            </div>
        </>
    );
}