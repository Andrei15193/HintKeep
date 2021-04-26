import React, { useEffect, useContext } from 'react';
import classnames from 'classnames';
import { I18nContext, Message } from '../../../i18n';
import { useViewModel } from '../../../view-model-hooks';
import { DeletedAccountsViewModel } from '../../../../view-models/deleted-accounts-view-model';
import { BusyContent } from '../../../loaders';
import { NoAccountsMessage } from './no-accounts-message';
import { AccountsDisplayList } from './accounts-display-list';

import Style from '../../../style.scss';
import { Link } from 'react-router-dom';

export function DeletedAccounts(): JSX.Element {
    const messageResolver = useContext(I18nContext);
    const $vm = useViewModel(DeletedAccountsViewModel);
    useEffect(() => { $vm.loadAsync(); }, [$vm]);

    return (
        <>
            <div className={Style.mx3}>
                <h1 className={classnames(Style.container, Style.textCenter)}>
                    <Message id="pages.deletedAccounts.pageTitle" />
                </h1>
                <hr className={Style.w100} />
            </div>
            <div className={classnames(Style.dFlex, Style.flexFill, Style.flexColumn)}>
                <div className={Style.flexFill}>
                    <BusyContent $vm={$vm}>
                        {
                            !$vm.hasAccounts ? <NoAccountsMessage /> : <>
                                <div className={Style.mx3}>
                                    <div className={classnames(Style.inputGroup, Style.inputGroupSm, Style.mb3)}>
                                        <input
                                            type="text"
                                            inputMode="search"
                                            className={Style.formControl}
                                            value={$vm.searchText}
                                            onChange={event => $vm.searchText = event.target.value}
                                            placeholder={messageResolver.resolve('pages.accounts.search.placeholder')}
                                        />
                                    </div>
                                </div>

                                <AccountsDisplayList accounts={$vm.accounts} />
                            </>
                        }
                    </BusyContent>
                </div>
                <div className={classnames(Style.mb2, Style.mx3, Style.textCenter)}>
                    <hr className={Style.my2} />
                    <Link to="/accounts"><Message id="pages.deletedAccounts.viewAccounts.labels" /></Link>
                </div>
            </div>
        </>
    );
}