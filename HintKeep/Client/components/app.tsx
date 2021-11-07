import type { PropsWithChildren } from 'react';
import React from 'react';
import { BrowserRouter, Switch, Route, Redirect } from 'react-router-dom';
import classnames from 'classnames';
import { Message } from './i18n';
import { Alerts } from './alerts';
import { useViewModel } from './use-view-model';
import { Login, Register, Confirmation, Recovery, PasswordReset, Extra, Accounts, AddAccount, EditAccount, AccountHints, DeletedAccountDetails, DeletedAccounts, TermsOfService } from './pages';

import Style from './style.scss';

export function App(): JSX.Element {
    const $vm = useViewModel(({ sessionViewModel }) => sessionViewModel);

    const routes = $vm.isSessionActive
        ? [
            <Redirect key="accounts-redirect" from="/" to="/accounts" exact />,
            <Route key="extra" path="/extra" exact>
                <Extra />
            </Route>,
            <Route key="accounts" path="/accounts" exact>
                <Accounts />
            </Route>,
            <Route key="accounts-bin" path="/accounts/bin" exact>
                <DeletedAccounts />
            </Route>,
            <Route key="accounts-bin-details" path="/accounts/bin/:id" exact>
                <DeletedAccountDetails />
            </Route>,
            <Route key="accounts-add" path="/accounts/add" exact>
                <AddAccount />
            </Route>,
            <Route key="accounts-details" path="/accounts/:id" exact>
                <EditAccount />
            </Route>,
            <Route key="accounts-details-hints" path="/accounts/:id/hints" exact>
                <AccountHints />
            </Route>
        ]
        : [
            <Route key="login" path="/" exact>
                <Login />
            </Route>,
            <Route key="register" path="/register" exact>
                <Register />
            </Route>,
            <Route key="confirm" path="/confirm" exact>
                <Confirmation />
            </Route>,
            <Route key="recovery" path="/recovery" exact>
                <Recovery />
            </Route>,
            <Route key="password-reset" path="/password-reset" exact>
                <PasswordReset />
            </Route>
        ];

    return (
        <div className={classnames(Style.app, Style.m3, Style.border, Style.dFlex, Style.flexColumn, Style.flexFill)}>
            <AppBanner className={Style.appHeader}><Message id="pages.header.banner" /></AppBanner>
            <AppContent>
                <Alerts />
                <BrowserRouter>
                    <Switch>
                        {routes}
                        <Route path="/terms" exact>
                            <TermsOfService />
                        </Route>
                        <Redirect to="/" />
                    </Switch>
                </BrowserRouter>
            </AppContent>
            <AppBanner className={Style.appFooter}><Message id="pages.footer.banner" /></AppBanner>
        </div >
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
        <div className={classnames(Style.appContent, Style.dFlex, Style.flexFill)}>
            <div className={classnames(Style.dFlex, Style.flexColumn, Style.flexFill, Style.w100)}>
                {children}
            </div>
        </div>
    );
}