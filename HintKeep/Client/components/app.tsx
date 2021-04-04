import type { PropsWithChildren } from 'react'
import React from 'react';
import { BrowserRouter, Switch, Route, Redirect } from 'react-router-dom';
import classnames from 'classnames';
import { AuthenticationGuardViewModel } from '../view-models/authentication-guard-view-model';
import { useViewModel } from './view-model-hooks';
import { I18nProvider, Message } from './i18n';
import { Alerts } from './alerts';
import { Accounts, AddAccount, Authentication, AuthenticationGuard, DeletedAccountDetails, DeletedAccounts, EditAccount } from './pages';

import Style from './style.scss';

export function App(): JSX.Element {
    const $vm = useViewModel(AuthenticationGuardViewModel);

    return (
        <I18nProvider>
            <div className={classnames(Style.app, Style.m3, Style.border, Style.dFlex, Style.flexColumn, Style.flexFill)}>
                <AppBanner className={Style.appHeader}><Message id="pages.header.banner" /></AppBanner>
                <AppContent>
                    <Alerts />
                    <BrowserRouter>
                        <Switch>
                            <Redirect from="/" to="/accounts" exact />
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
                        </Switch>
                    </BrowserRouter>
                </AppContent>
                <AppBanner className={Style.appFooter}><Message id="pages.footer.banner" /></AppBanner>
            </div>
        </I18nProvider>
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
            <div className={classnames(Style.dFlex, Style.flexColumn, Style.flexFill)}>
                {children}
            </div>
        </div>
    );
}