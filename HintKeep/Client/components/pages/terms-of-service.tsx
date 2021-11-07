import React from 'react';
import classnames from 'classnames';
import { Message } from '../i18n';
import { Link } from 'react-router-dom';

import Style from '../style.scss';

export function TermsOfService(): JSX.Element {
    return (
        <div className={Style.mx3}>

            <h1 className={classnames(Style.container, Style.containerFluid, Style.p0)}>
                <div className={classnames(Style.row, Style.g0, Style.dFlex, Style.alignItemsCenter)}>
                    <div className={classnames(Style.col2, Style.textStart)}>
                        <Link to="/register" className={classnames(Style.btn, Style.btnSm, Style.btnPrimary)}>
                            <Message id="pages.terms.back.label" />
                        </Link>
                    </div>
                    <div className={classnames(Style.col8, Style.textCenter)}>
                        <Message id="pages.terms.pageTitle" />
                    </div>
                </div>
            </h1>
            <p className={Style.textCenter}><small><Message id="pages.terms.lastModified" /></small></p>
            <hr className={Style.w100} />

            <p><Message id="pages.terms.paragraph.1" /></p>
            <p><Message id="pages.terms.paragraph.2" /></p>
            <p><Message id="pages.terms.paragraph.3" /></p>

            <ul>
                <li><Message id="pages.terms.list.1.item.1" /></li>
                <li><Message id="pages.terms.list.1.item.2" /></li>
                <li><Message id="pages.terms.list.1.item.3" /></li>
                <li><Message id="pages.terms.list.1.item.4" /></li>
                <li><Message id="pages.terms.list.1.item.5" /></li>
                <li><Message id="pages.terms.list.1.item.6" /></li>
                <li><Message id="pages.terms.list.1.item.7" /></li>
                <li><Message id="pages.terms.list.1.item.8.text.1" /><a href="mailto:hintkeep@gmail.com" target="_blank">hintkeep@gmail.com</a><Message id="pages.terms.list.1.item.8.text.2" /></li>
                <li><Message id="pages.terms.list.1.item.9.text.1" /><a href="mailto:hintkeep@gmail.com" target="_blank">hintkeep@gmail.com</a><Message id="pages.terms.list.1.item.9.text.2" /></li>
            </ul>
        </div>
    );
}