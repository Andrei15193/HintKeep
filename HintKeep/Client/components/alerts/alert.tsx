import type { AlertViewModel } from '../../view-models/alerts-view-model';
import React from 'react';
import classnames from 'classnames';
import { watchViewModel } from 'react-model-view-viewmodel';
import { Message } from '../i18n';

import Style from './../style.scss';

export interface IAlertProps {
    readonly $vm: AlertViewModel;
};

export function Alert({ $vm }: IAlertProps): JSX.Element {
    watchViewModel($vm);

    return (
        <div className={classnames(Style.m2, Style.alert, Style.alertDanger, Style.alertDismissible)} role="alert">
            <Message id={$vm.message} />
            <button type="button" className={Style.close} onClick={() => $vm.dismiss()}>
                <span aria-hidden="true">&times;</span>
            </button>
        </div>
    );
}