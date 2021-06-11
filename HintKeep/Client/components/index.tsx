import React from 'react';
import ReactDOM from 'react-dom';
import classnames from 'classnames';
import { App } from './app';
import { I18nProvider } from './i18n';
import { ServicesProvider } from './services-context';

import Style from './style.scss';

const appElement = document.getElementById('app');

if (appElement !== null) {
    appElement.className = classnames(Style.dFlex, Style.flexFill, Style.h100);

    ReactDOM.render(
        <I18nProvider>
            <ServicesProvider>
                <App />
            </ServicesProvider>
        </I18nProvider>,
        appElement
    );
}
else
    console.error("Element with id 'app' not found.");