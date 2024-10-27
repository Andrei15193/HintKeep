import { Link } from 'react-router-dom';
import classnames from 'classnames';
import { Message } from '../i18n';
import { useViewModelDependency } from 'react-model-view-viewmodel';
import { SessionViewModel } from '../../view-models/session-view-model';

import Style from '../style.scss';

export function Extra(): JSX.Element {
    const sessionViewModel = useViewModelDependency(SessionViewModel);

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
                            <li><Link to="/" onClick={() => sessionViewModel.endSession()}><Message id="pages.extra.logOut.label" /></Link></li>
                        </ul>
                    </div>
                </div>
            </div>
        </>
    );
}