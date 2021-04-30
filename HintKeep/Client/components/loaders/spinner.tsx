import React from 'react';
import classnames from 'classnames';

import Style from './../style.scss';

export function Spinner(): JSX.Element {
    return (
        <div className={classnames(Style.dFlex, Style.justifyContentCenter, Style.w100)}>
            <div className={classnames(Style.spinnerBorder, Style.my3)} style={{ width: "3rem", height: "3rem" }} role="status"></div>
        </div>
    );
}