import type { Account } from '../../../../view-models/accounts-view-model';
import React from 'react';
import classnames from 'classnames';
import { AccountDisplay } from './account-display';

import Style from '../../../style.scss';

export interface IAccountsDisplayListProps {
    readonly accounts: readonly Account[];
}

export function AccountsDisplayList({ accounts }: IAccountsDisplayListProps): JSX.Element {
    return (
        <div className={classnames(Style.px3, Style.pt2, Style.listView, Style.flexFill)}>
            {accounts.map(account => <AccountDisplay key={account.id} account={account} />)}
        </div>
    );
}