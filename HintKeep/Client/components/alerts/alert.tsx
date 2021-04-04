import { PropsWithViewModel, watchViewModel } from '../view-model-hooks';
import React from 'react';
import classnames from 'classnames';
import { AlertViewModel } from '../../view-models/alert-view-model';
import { Message } from '../i18n';

import Style from './../style.scss';

export function Alert({ $vm }: PropsWithViewModel<AlertViewModel>): JSX.Element {
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