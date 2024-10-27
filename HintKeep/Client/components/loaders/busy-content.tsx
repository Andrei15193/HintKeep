import type { PropsWithChildren } from 'react'
import type { ApiViewModel } from '../../view-models/api-view-model';
import { Spinner } from './spinner';
import { ApiViewModelState } from '../../view-models/api-view-model';
import { useViewModel } from 'react-model-view-viewmodel';

export interface IBusyContentProps {
    readonly apiViewModel: ApiViewModel;
}

export function BusyContent({ apiViewModel, children }: PropsWithChildren<IBusyContentProps>): JSX.Element {
    useViewModel(apiViewModel);

    return apiViewModel.state === ApiViewModelState.Busy ? <Spinner /> : <>{children}</>
}