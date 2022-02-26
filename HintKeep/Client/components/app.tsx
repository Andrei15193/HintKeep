import type { PropsWithChildren } from 'react';
import React from 'react';
import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom';
import classnames from 'classnames';
import { Message } from './i18n';
import { Alerts } from './alerts';
import { useViewModel } from './use-view-model';
import { Login, Register, Confirmation, Recovery, PasswordReset, Extra, Accounts, AddAccount, EditAccount, AccountHints, DeletedAccountDetails, DeletedAccounts, TermsOfService } from './pages';

import Style from './style.scss';

export function App(): JSX.Element {
    const $vm = useViewModel(({ sessionViewModel }) => sessionViewModel);

    return (
        <div className={classnames(Style.app, Style.m3, Style.border, Style.dFlex, Style.flexColumn, Style.flexFill)}>
            <AppBanner className={Style.appHeader}><Message id="pages.header.banner" /></AppBanner>
            <AppContent>
                <Alerts />
                <BrowserRouter>
                    <Routes>
                        <Route path="/">
                            {$vm.isSessionActive
                                ? <>
                                    <Route path="extra" element={<Extra />} />
                                    <Route path="accounts">
                                        <Route index element={<Accounts />} />
                                        <Route path="add" element={<AddAccount />} />
                                        <Route path="bin">
                                            <Route index element={<DeletedAccounts />} />
                                            <Route path=":id" element={<DeletedAccountDetails />} />
                                        </Route>
                                        <Route path=":id">
                                            <Route index element={<EditAccount />} />
                                            <Route path="hints" element={<AccountHints />} />
                                        </Route>
                                    </Route>
                                    <Route path="terms" element={<TermsOfService />} />
                                    <Route index element={<Navigate replace to="/accounts" />} />
                                    <Route path="*" element={<Navigate replace to="/accounts" />} />
                                </>
                                : <>
                                    <Route index element={<Login />} />,
                                    <Route path="register" element={<Register />} />,
                                    <Route path="confirm" element={<Confirmation />} />,
                                    <Route path="recovery" element={<Recovery />} />,
                                    <Route path="password-reset" element={<PasswordReset />} />
                                    <Route path="terms" element={<TermsOfService />} />
                                    <Route path="*" element={<Navigate to="/" />} />
                                </>}
                        </Route >;
                    </Routes>
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
        <div className={classnames(Style.appContent, Style.dFlex, Style.flexFill)}>
            <div className={classnames(Style.dFlex, Style.flexColumn, Style.flexFill, Style.w100)}>
                {children}
            </div>
        </div>
    );
}