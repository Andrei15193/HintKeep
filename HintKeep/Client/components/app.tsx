import type { PropsWithChildren } from 'react';
import React from 'react';
import { BrowserRouter, Switch, Route, Redirect } from 'react-router-dom';
import classnames from 'classnames';
import { AuthenticationGuardViewModel } from '../view-models/authentication-guard-view-model';
import { Message } from './i18n';
import { Alerts } from './alerts';
import { useViewModel } from './use-view-model';
import { AccountHints, Accounts, AddAccount, Authentication, AuthenticationGuard, DeletedAccountDetails, DeletedAccounts, Extra, EditAccount, TermsOfService } from './pages';

import Style from './style.scss';

export function App(): JSX.Element {
    const $vm = useViewModel(({ alertsViewModel }) => new AuthenticationGuardViewModel(alertsViewModel));

    return (
        <div className={classnames(Style.app, Style.m3, Style.border, Style.dFlex, Style.flexColumn, Style.flexFill)}>
            <AppBanner className={Style.appHeader}><Message id="pages.header.banner" /></AppBanner>
            <AppContent>
                <Alerts />
                <BrowserRouter>
                    <Switch>
                        <Redirect from="/" to="/accounts" exact />
                        <Route path="/terms" exact>
                            <TermsOfService />
                        </Route>
                        <Route path="/accounts" exact>
                            <AuthenticationGuard $vm={$vm}>
                                <Accounts />
                            </AuthenticationGuard>
                        </Route>
                        <Route path="/authentications" exact>
                            <Authentication />
                        </Route>
                        <Route path="/accounts/add" exact>
                            <AuthenticationGuard $vm={$vm}>
                                <AddAccount />
                            </AuthenticationGuard>
                        </Route>
                        <Route path="/accounts/bin" exact>
                            <AuthenticationGuard $vm={$vm}>
                                <DeletedAccounts />
                            </AuthenticationGuard>
                        </Route>
                        <Route path="/accounts/bin/:id" exact>
                            <AuthenticationGuard $vm={$vm}>
                                <DeletedAccountDetails />
                            </AuthenticationGuard>
                        </Route>
                        <Route path="/accounts/:id" exact>
                            <AuthenticationGuard $vm={$vm}>
                                <EditAccount />
                            </AuthenticationGuard>
                        </Route>
                        <Route path="/accounts/:id/hints" exact>
                            <AuthenticationGuard $vm={$vm}>
                                <AccountHints />
                            </AuthenticationGuard>
                        </Route>
                        <Route path="/extra" exact>
                            <AuthenticationGuard $vm={$vm}>
                                <Extra $vm={$vm} />
                            </AuthenticationGuard>
                        </Route>
                        <Redirect to="/" />
                    </Switch>
                </BrowserRouter>
            </AppContent>
            <AppBanner className={Style.appFooter}><Message id="pages.footer.banner" /></AppBanner>
        </div>
    );
}

interface IAppBannerProps {
    readonly className?: string
}

function AppBanner({ className, children }: PropsWithChildren<IAppBannerProps>): JSX.Element {
    return (
        <div className={Style.appBanner}>
            <div className={classnames(Style.dFlex, Style.flexRow, Style.alignItemsCenter, Style.appBannerContent)}>
                <div className={classnames(Style.mxAuto, className)}>
                    {children}
                </div>
            </div>
        </div>
    );
}

function AppContent({ children }: PropsWithChildren<{}>): JSX.Element {
    return (
        <div className={classnames(Style.appContent, Style.dFlex, Style.flexFill, Style.textJustify)}>
            <div className={classnames(Style.dFlex, Style.flexColumn, Style.flexFill, Style.w100)}>
                {children}
            </div>
        </div>
    );
}