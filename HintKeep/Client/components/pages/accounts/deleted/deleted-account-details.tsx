import { useEffect, useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { type IEventHandler, useViewModelDependency } from 'react-model-view-viewmodel';
import classnames from 'classnames';
import { Message } from '../../../i18n';
import { BusyContent } from '../../../loaders';
import { DeletedAccountDetailsViewModel } from '../../../../view-models/accounts/deleted/deleted-account-details-view-model';
import { FormInput, FormCheckboxInput, FormTextArea } from '../../../forms';
import { Else, If, Then } from '../../../conditionals';

import Style from '../../../style.scss';

export function DeletedAccountDetails(): JSX.Element {
    const { id = "" } = useParams();

    const navigate = useNavigate();

    const deletedAccountDetailsViewModel = useViewModelDependency(DeletedAccountDetailsViewModel);

    const [isConfirmationHidden, setIsConfirmationHidden] = useState(true);
    useEffect(
        () => {
            deletedAccountDetailsViewModel.loadAsync(id);
        },
        [deletedAccountDetailsViewModel, id]
    );

    useEffect(
        () => {
            const restoredOrDeletedEventHandler: IEventHandler<unknown> = {
                handle() {
                    navigate('/accounts/bin');
                }
            }

            deletedAccountDetailsViewModel.restoredEvent.subscribe(restoredOrDeletedEventHandler);
            deletedAccountDetailsViewModel.deletedEvent.subscribe(restoredOrDeletedEventHandler);

            return () => {
                deletedAccountDetailsViewModel.deletedEvent.unsubscribe(restoredOrDeletedEventHandler);
                deletedAccountDetailsViewModel.restoredEvent.unsubscribe(restoredOrDeletedEventHandler);
            }
        },
        [deletedAccountDetailsViewModel, navigate]
    )

    return (
        <div className={Style.mx3}>
            <h1 className={classnames(Style.container, Style.containerFluid, Style.p0)}>
                <div className={classnames(Style.row, Style.g0, Style.dFlex, Style.alignItemsCenter)}>
                    <div className={classnames(Style.col2, Style.textStart)}>
                        <Link to="/accounts/bin" className={classnames(Style.btn, Style.btnSm, Style.btnPrimary)}>
                            <Message id="pages.deletedAccountDetails.back.label" />
                        </Link>
                    </div>
                    <div className={classnames(Style.col8, Style.textCenter)}>
                        <Message id="pages.deletedAccountDetails.pageTitle" />
                    </div>
                </div>
            </h1>

            <BusyContent apiViewModel={deletedAccountDetailsViewModel}>
                <FormInput className={Style.mb3} id="name" type="text" disabled label="pages.deletedAccountDetails.name.label" field={deletedAccountDetailsViewModel.form.name} />
                <FormInput className={Style.mb3} id="hint" type="text" disabled label="pages.deletedAccountDetails.hint.label" field={deletedAccountDetailsViewModel.form.hint} />
                <FormCheckboxInput className={Style.mb3} id="isPinned" disabled label="pages.deletedAccountDetails.isPinned.label" field={deletedAccountDetailsViewModel.form.isPinned} />
                <FormTextArea className={Style.mb3} id="notes" disabled label="pages.deletedAccountDetails.notes.label" field={deletedAccountDetailsViewModel.form.notes} />

                <If condition={isConfirmationHidden}>
                    <Then>
                        <div className={classnames(Style.dFlex, Style.flexRow, Style.mb3)}>
                            <button type="button" disabled={!deletedAccountDetailsViewModel.isLoaded} className={classnames(Style.btn, Style.btnPrimary)} onClick={() => deletedAccountDetailsViewModel.restoreAsync()}>
                                <Message id="pages.deletedAccountDetails.restore.label" />
                            </button>
                            <Link to="/accounts/bin" className={classnames(Style.ms2, Style.btn, Style.btnLight)}>
                                <Message id="pages.deletedAccountDetails.cancel.label" />
                            </Link>
                            <button type="button" disabled={!deletedAccountDetailsViewModel.isLoaded} className={classnames(Style.btn, Style.btnDanger, Style.msAuto)} onClick={() => setIsConfirmationHidden(false)}>
                                <Message id="pages.deletedAccountDetails.delete.label" />
                            </button>
                        </div>
                    </Then>
                    <Else>
                        <div className={classnames(Style.card, Style.mb3)}>
                            <div className={Style.cardBody}>
                                <h5 className={Style.cardTitle}>
                                    <Message id="pages.deletedAccountDetails.delete.confirmationModalTitle" />
                                </h5>
                                <p className={Style.cardText}>
                                    <Message id="pages.deletedAccountDetails.delete.confirmation" />
                                </p>

                                <div className={classnames(Style.dFlex, Style.flexRow)}>
                                    <button type="button" className={classnames(Style.btn, Style.btnDanger)} onClick={() => { setIsConfirmationHidden(true); deletedAccountDetailsViewModel.deleteAsync(); }}>
                                        <Message id="pages.deletedAccountDetails.delete.label" />
                                    </button>
                                    <button type="button" className={classnames(Style.msAuto, Style.btn, Style.btnSecondary)} onClick={() => setIsConfirmationHidden(true)}>
                                        <Message id="pages.deletedAccountDetails.cancel.label" />
                                    </button>
                                </div>
                            </div>
                        </div>
                    </Else>
                </If>
            </BusyContent>
        </div>
    );
}