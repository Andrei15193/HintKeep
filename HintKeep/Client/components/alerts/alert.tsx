import type { PropsWithViewModel } from '../view-model-wrappers';
import React from 'react';
import classnames from 'classnames';
import { AlertViewModel } from '../../view-models/alert-view-model';
import { requiresViewModel } from '../view-model-wrappers';

import Style from './../style.scss';

export const Alert: React.ComponentType<PropsWithViewModel<AlertViewModel>> = requiresViewModel(
    $vm => (
        <div className={classnames(Style.alert, Style.alertDanger, Style.alertDismissible)} role="alert">
            {$vm.message}
            <button type="button" className={Style.close} onClick={() => $vm.dismiss()}>
                <span aria-hidden="true">&times;</span>
            </button>
        </div>
    )
);