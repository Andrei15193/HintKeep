import React from 'react';
import { Link } from 'react-router-dom';
import classnames from 'classnames';
import { useViewModel } from '../use-view-model';
import { Message } from '../i18n';

import Style from '../style.scss';

export function Extra(): JSX.Element {
    const $vm = useViewModel(({ sessionViewModel }) => sessionViewModel);

    return (
        <>
            <div className={Style.mx3}>
                <h1 className={classnames(Style.container, Style.containerFluid, Style.p0)}>
                    <div className={classnames(Style.row, Style.g0, Style.dFlex, Style.alignItemsCenter)}>
                        <div className={classnames(Style.col2, Style.textStart)}>
                            <Link to="/" className={classnames(Style.btn, Style.btnSm, Style.btnPrimary, Style.px2)}>
                                <Message id="pages.extra.back.label" />
                            </Link>
                        </div>
                        <div className={classnames(Style.col8, Style.textCenter)}>
                            <Message id="pages.extra.pageTitle" />
                        </div>
                    </div>
                </h1>
                <hr />

                <div className={classnames(Style.dFlex, Style.flexFill, Style.flexColumn)}>
                    <div className={Style.flexFill}>
                        <ul>
                            <li><Link to="/accounts/bin"><Message id="pages.extra.accountsBin.label" /></Link></li>
                            <li><Link to="/" onClick={() => $vm.endSession()}><Message id="pages.extra.logOut.label" /></Link></li>
                        </ul>
                    </div>
                </div>
            </div>
        </>
    );
}