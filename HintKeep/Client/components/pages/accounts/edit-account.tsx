import { useEffect, useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import classnames from 'classnames';
import { type IEventHandler, useViewModel, useViewModelDependency } from 'react-model-view-viewmodel';
import { Message } from '../../i18n';
import { BusyContent } from '../../loaders';
import { EditAccountViewModel } from '../../../view-models/accounts/edit-account-view-model';
import { FormInput, FormCheckboxInput, FormTextArea } from '../../forms';
import { Else, If, Then } from '../../conditionals';

import Style from '../../style.scss';

export interface IEditAccountRouteParams {
    readonly id: string
}

export function EditAccount(): JSX.Element {
    const { id = "" } = useParams();

    const navigate = useNavigate();

    const editAccountViewModel = useViewModelDependency(EditAccountViewModel);
    useViewModel(editAccountViewModel.form);

    const [isConfirmationHidden, setIsConfirmationHidden] = useState(true);
    useEffect(
        () => {
            editAccountViewModel.loadAsync(id);
        },
        [editAccountViewModel, id]
    );

    useEffect(
        () => {
            const editedOrDeletedEventHandler: IEventHandler<unknown> = {
                handle() {
                    navigate('/')
                }
            }

            editAccountViewModel.editedEvent.subscribe(editedOrDeletedEventHandler);
            editAccountViewModel.deletedEvent.subscribe(editedOrDeletedEventHandler);

            return () => {
                editAccountViewModel.deletedEvent.unsubscribe(editedOrDeletedEventHandler);
                editAccountViewModel.editedEvent.unsubscribe(editedOrDeletedEventHandler);
            }
        },
        [editAccountViewModel, navigate]
    )

    return (
        <div className={Style.mx3}>
            <h1 className={classnames(Style.container, Style.containerFluid, Style.p0)}>
                <div className={classnames(Style.row, Style.g0, Style.dFlex, Style.alignItemsCenter)}>
                    <div className={classnames(Style.col2, Style.textStart)}>
                        <Link to="/" className={classnames(Style.btn, Style.btnSm, Style.btnPrimary)}>
                            <Message id="pages.editAccount.back.label" />
                        </Link>
                    </div>
                    <div className={classnames(Style.col8, Style.textCenter)}>
                        <Message id="pages.editAccount.pageTitle" />
                    </div>
                </div>
            </h1>

            <BusyContent apiViewModel={editAccountViewModel}>
                <form onSubmit={event => { event.preventDefault(); editAccountViewModel.submitAsync(); }}>
                    <FormInput className={Style.mb3} id="name" type="text" disabled={!isConfirmationHidden} label="pages.editAccount.name.label" field={editAccountViewModel.form.name} placeholder="pages.editAccount.name.placeholder" />
                    <FormInput className={Style.mb3} id="hint" type="text" disabled={!isConfirmationHidden} label="pages.editAccount.hint.label" field={editAccountViewModel.form.hint} placeholder="pages.editAccount.hint.placeholder" />
                    <FormCheckboxInput className={Style.mb3} id="isPinned" disabled={!isConfirmationHidden} label="pages.editAccount.isPinned.label" field={editAccountViewModel.form.isPinned} description="pages.editAccount.isPinned.description" />
                    <FormTextArea className={Style.mb3} id="notes" label="pages.editAccount.notes.label" field={editAccountViewModel.form.notes} />

                    <If condition={isConfirmationHidden}>
                        <Then>
                            <div className={classnames(Style.dFlex, Style.flexRow, Style.mb3)}>
                                <button type="submit" disabled={(!editAccountViewModel.isLoaded || (editAccountViewModel.form.isInvalid && editAccountViewModel.form.fields.every(field => field.isTouched)))} className={classnames(Style.btn, Style.btnPrimary)}>
                                    <Message id="pages.editAccount.save.label" />
                                </button>
                                <Link to={`/accounts/${id}/hints`} className={classnames(Style.ms2, Style.btn, Style.btnLight)}>
                                    <Message id="pages.editAccount.viewAllHints.label" />
                                </Link>
                                <Link to="/" className={classnames(Style.ms2, Style.btn, Style.btnLight)}>
                                    <Message id="pages.editAccount.cancel.label" />
                                </Link>
                                <button type="button" disabled={!editAccountViewModel.isLoaded} className={classnames(Style.btn, Style.btnDanger, Style.msAuto)} onClick={() => setIsConfirmationHidden(false)}>
                                    <Message id="pages.editAccount.delete.label" />
                                </button>
                            </div>
                        </Then>
                        <Else>
                            <div className={classnames(Style.card, Style.mb3)}>
                                <div className={Style.cardBody}>
                                    <h5 className={Style.cardTitle}>
                                        <Message id="pages.editAccount.delete.confirmationModalTitle" />
                                    </h5>
                                    <p className={Style.cardText}>
                                        <Message id="pages.editAccount.delete.confirmation" />
                                    </p>

                                    <div className={classnames(Style.dFlex, Style.flexRow)}>
                                        <button type="button" className={classnames(Style.btn, Style.btnDanger)} onClick={() => { setIsConfirmationHidden(true); editAccountViewModel.deleteAsync(); }}>
                                            <Message id="pages.editAccount.moveToBin.label" />
                                        </button>
                                        <button type="button" className={classnames(Style.msAuto, Style.btn, Style.btnSecondary)} onClick={() => setIsConfirmationHidden(true)}>
                                            <Message id="pages.editAccount.cancel.label" />
                                        </button>
                                    </div>
                                </div>
                            </div>
                        </Else>
                    </If>
                </form>
            </BusyContent>
        </div>
    );
}