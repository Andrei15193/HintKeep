import type { PropsWithChildren } from 'react'
import React, { useState } from 'react';
import { BrowserRouter, Switch, Route } from 'react-router-dom';
import classnames from 'classnames';
import { Accounts, SignUp, UserConfirmation } from './pages';
import { Alerts } from './alerts';
import { Message } from './i18n';

import Style from './style.scss';

export function App(): JSX.Element {
    const [] = useState();

    return (
        <div className={classnames(Style.app, Style.m3, Style.border, Style.dFlex, Style.flexColumn, Style.flexFill)}>
            <AppBanner className={Style.appHeader}><Message id="pages.header.banner" /></AppBanner>
            <AppContent>
                <>
                    <Alerts />
                    <BrowserRouter>
                        <Switch>
                            <Route path="/user-accounts/create">
                                <SignUp />
                            </Route>
                            <Route path="/user-confirmations">
                                <UserConfirmation />
                            </Route>
                            <Route path="/">
                                <Accounts />
                            </Route>
                        </Switch>
                    </BrowserRouter>
                </>
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
        <div className={classnames(Style.appContent, Style.flexFill, Style.textJustify)}>
            <div className={Style.m2}>
                {children}
            </div>
        </div>
    );
}