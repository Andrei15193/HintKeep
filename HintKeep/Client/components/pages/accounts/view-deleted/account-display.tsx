import type { Account } from '../../../../view-models/accounts-view-model';
import React from 'react';
import { Link } from 'react-router-dom';
import classnames from 'classnames';
import { Message } from '../../../i18n';

import Style from '../../../style.scss';

export interface IAccountDisplayProps {
    readonly account: Account;
}

export function AccountDisplay({ account }: IAccountDisplayProps): JSX.Element {
    return (
        <div className={classnames(Style.dFlex, Style.alignItemsStart)}>
            <div className={Style.flexFill}>
                <h6 className={Style.m0}>{account.name}</h6>
                <p className={Style.mb3}>{account.hint}</p>
            </div>
            <Link to={`/accounts/bin/${account.id}`} className={classnames(Style.ml2, Style.btn, Style.btnSm, Style.btnPrimary)}><Message id="pages.deletedAccounts.view.label" /></Link>
        </div>
    );
}