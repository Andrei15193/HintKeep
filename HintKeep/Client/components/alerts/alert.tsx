import type { AlertViewModel } from '../../view-models/alerts-view-model';
import classnames from 'classnames';
import { Message } from '../i18n';
import { useViewModel } from 'react-model-view-viewmodel';

import Style from './../style.scss';

export interface IAlertProps {
    readonly alertViewModel: AlertViewModel;
};

export function Alert({ alertViewModel }: IAlertProps): JSX.Element {
    useViewModel(alertViewModel);

    return (
        <div className={classnames(Style.m2, Style.alert, Style.alertDanger, Style.alertDismissible)} role="alert">
            <Message id={alertViewModel.message} />
            <button type="button" className={Style.btnClose} onClick={() => alertViewModel.dismiss()} />
        </div>
    );
}