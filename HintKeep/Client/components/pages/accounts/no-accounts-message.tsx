import React from 'react';
import { Link } from 'react-router-dom';
import classnames from 'classnames';
import { Message } from '../../i18n';

import Style from '../../style.scss';

export function NoAccountsMessage(): JSX.Element {
    return (
        <div className={Style.textCenter}>
            <h6 className={Style.mb3}><Message id="pages.accounts.noAccounts" /></h6>
            <Link to="/accounts/add" className={classnames(Style.btn, Style.btnPrimary)}><Message id="pages.accounts.add.label" /></Link>
        </div>
    );
}