import React from 'react';
import { LoginGuard } from './login';

import Style from '../style.scss';

export function Accounts(): JSX.Element {
    return (
        <LoginGuard>
            <h1 className={Style.textCenter}>Accounts</h1>
        </LoginGuard>
    );
}