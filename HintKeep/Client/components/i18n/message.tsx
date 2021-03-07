import React from 'react';
import { WithViewModel } from '../view-model-wrappers';
import { TranslationViewModel } from '../../view-models/translation-view-model';

export interface IMessageProps {
    readonly id: string | undefined
}

export function Message({ id }: IMessageProps): JSX.Element {
    return (
        <WithViewModel viewModelType={TranslationViewModel}>{$vm =>
            <>{$vm.getMessage(id)}</>
        }</WithViewModel>
    );
}