import { createRoot } from 'react-dom/client';
import classnames from 'classnames';
import { App } from './app';
import { I18nProvider } from './i18n';
import { ServicesProvider } from './services-context';

import Style from './style.scss';

const appElement = document.getElementById('app');

if (appElement !== null) {
    appElement.className = classnames(Style.dFlex, Style.flexFill, Style.h100);

    const root = createRoot(appElement);
    root.render(
        <I18nProvider>
            <ServicesProvider>
                <App />
            </ServicesProvider>
        </I18nProvider>,
    );
}
else
    console.error("Element with id 'app' not found.");