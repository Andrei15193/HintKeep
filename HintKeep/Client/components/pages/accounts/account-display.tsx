import type { Account } from '../../../view-models/accounts-view-model';
import React from 'react';

import Style from '../../style.scss';

export interface IAccountDisplayProps {
    readonly account: Account;
}

export function AccountDisplay({ account }: IAccountDisplayProps): JSX.Element {
    return (
        <>
            <h6 className={Style.m0}>{account.name}</h6>
            <p className={Style.mb3}>{account.hint}</p>
        </>
    );
}