import type { AuthenticationGuardViewModel } from '../../view-models/authentication-guard-view-model';
import React from 'react';
import { Link } from 'react-router-dom';
import classnames from 'classnames';
import { watchEvent } from 'react-model-view-viewmodel';
import { useViewModel } from '../use-view-model';
import { Message } from '../i18n';
import { AuthenticationViewModel } from '../../view-models/authentication-view-model';

import Style from '../style.scss';

export interface IExtraProps {
    readonly $vm: AuthenticationGuardViewModel;
}

export function Extra({ $vm: $ensureAuthenticationViewModel }: IExtraProps): JSX.Element {
    const $authenticationViewModel = useViewModel(({ axios }) => new AuthenticationViewModel(axios));
    watchEvent($authenticationViewModel.loggedOut, () => $ensureAuthenticationViewModel.reset());

    return (
        <>
            <div className={Style.mx3}>
                <h1 className={classnames(Style.container, Style.containerFluid, Style.p0)}>
                    <div className={classnames(Style.row, Style.noGutters, Style.dFlex, Style.alignItemsCenter)}>
                        <div className={classnames(Style.col2, Style.textLeft)}>
                            <Link to="/" className={classnames(Style.btn, Style.btnSm, Style.btnPrimary, Style.px2)}>
                                <Message id="pages.extra.back.label" />
                            </Link>
                        </div>
                        <div className={classnames(Style.col8, Style.textCenter)}>
                            <Message id="pages.extra.pageTitle" />
                        </div>
                    </div>
                </h1>
                <hr />

                <div className={classnames(Style.dFlex, Style.flexFill, Style.flexColumn)}>
                    <div className={Style.flexFill}>
                        <ul>
                            <li><Link to="/accounts/bin"><Message id="pages.extra.accountsBin.label" /></Link></li>
                            <li><Link to="/" onClick={() => $authenticationViewModel.logOut()}><Message id="pages.extra.logOut.label" /></Link></li>
                        </ul>
                    </div>
                </div>
            </div>
        </>
    );
}