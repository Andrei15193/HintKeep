import React from 'react';
import ReactDom from 'react-dom';
import { ComponentLoader } from './component-loader';

import Style from './app.scss';

ReactDom.render(<RootComponent />, document.getElementById('app'));

function RootComponent(): JSX.Element {
    return (
        <div className={Style.m3}>
            <ComponentLoader loadAsync={() => import('./app').then(module => module.App)} />
        </div>
    );
}