import React from 'react';
import { Message } from '../../../i18n';

import Style from '../../../style.scss';

export function NoAccountsMessage(): JSX.Element {
    return (
        <div className={Style.textCenter}>
            <h6 className={Style.mb3}><Message id="pages.deletedAccounts.noAccounts" /></h6>
        </div>
    );
}