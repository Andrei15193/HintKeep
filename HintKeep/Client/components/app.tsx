import type { PropsWithChildren } from 'react'
import React from 'react';
import { BrowserRouter, Switch, Route, Redirect } from 'react-router-dom';
import classnames from 'classnames';
import { Accounts, AddAccount, EditAccount, Login, SignUp, UserConfirmation } from './pages';
import { Alerts } from './alerts';
import { I18nProvider, Message } from './i18n';
import { useViewModel } from './view-model-wrappers';
import { UserViewModel } from '../view-models/users/user-view-model';
import { LoginGuard } from './pages/login';

import 'bootstrap';
import Style from './style.scss';

export function App(): JSX.Element {
    const $vm = useViewModel(UserViewModel);

    return (
        <I18nProvider>
            <div className={classnames(Style.app, Style.m3, Style.border, Style.dFlex, Style.flexColumn, Style.flexFill)}>
                <AppBanner className={Style.appHeader}><Message id="pages.header.banner" /></AppBanner>
                <AppContent>
                    <Alerts />
                    <BrowserRouter>
                        <Switch>
                            {$vm.isAuthenticated ? <Redirect to="/accounts" from="/" exact /> : <Redirect to="/" from="/accounts" exact />}
                            <Route path="/user-accounts/create" exact>
                                <SignUp />
                            </Route>
                            <Route path="/user-accounts/confirm" exact>
                                <UserConfirmation />
                            </Route>
                            <Route path="/accounts" exact>
                                <LoginGuard>
                                    <Accounts />
                                </LoginGuard>
                            </Route>
                            <Route path="/accounts/add" exact>
                                <LoginGuard>
                                    <AddAccount />
                                </LoginGuard>
                            </Route>
                            <Route path="/accounts/:id" exact>
                                <LoginGuard>
                                    <EditAccount />
                                </LoginGuard>
                            </Route>
                            <Route path="/">
                                <Login />
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