import React, { useEffect } from 'react';
import classnames from 'classnames';
import { Message } from '../../../i18n';
import { useViewModel } from '../../../view-model-hooks';
import { DeletedAccountsViewModel } from '../../../../view-models/deleted-accounts-view-model';
import { BusyContent } from '../../../loaders';
import { NoAccountsMessage } from './no-accounts-message';
import { AccountsDisplayList } from './accounts-display-list';

import Style from '../../../style.scss';
import { Link } from 'react-router-dom';

export function DeletedAccounts(): JSX.Element {
    const $vm = useViewModel(DeletedAccountsViewModel);
    useEffect(() => { $vm.loadAsync(); }, [$vm]);

    return (
        <>
            <div className={Style.mx3}>
                <h1 className={classnames(Style.container, Style.textCenter)}>
                    <Message id="pages.deletedAccounts.pageTitle" />
                </h1>
                <hr className={Style.w100} />
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
                    <Link to="/accounts"><Message id="pages.deletedAccounts.viewAccounts.labels" /></Link>
                </div>
            </div>
        </>
    );
}