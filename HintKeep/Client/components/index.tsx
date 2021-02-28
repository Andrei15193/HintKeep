import React from 'react';
import ReactDOM from 'react-dom';
import classnames from 'classnames';
import { App } from './app';

import Style from './style.scss';

const appElement = document.getElementById('app');

if (appElement !== null) {
    appElement.className = classnames(Style.dFlex, Style.flexFill, Style.h100);

    ReactDOM.render(<App />, appElement);
}
else
    console.error("Element with id 'app' not found.");