import React, { PropsWithChildren } from 'react';
import classnames from 'classnames';
import { ComponentLoader } from './component-loader';

import Style from './style.scss';

export function App(): JSX.Element {
    return (
        <div className={classnames(Style.app, Style.m3, Style.border, Style.dFlex, Style.flexColumn, Style.flexFill)}>
            <AppBanner className={Style.appHeader}>- HintKeep -</AppBanner>
            <AppContent>
                <ComponentLoader loadAsync={() => import('./home').then(({ Home }) => Home)} />
            </AppContent>
            <AppBanner className={Style.appFooter}>HintKeep - Store hints, not passwords</AppBanner>
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