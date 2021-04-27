import type { ViewModel } from '../../view-models/core/view-model';

export type ElementFactoryCallback<TViewModel> = ($vm: TViewModel) => JSX.Element;

export type ElementWithPropsFactoryCallback<TViewModel, TComponentProps = {}> = ($vm: TViewModel, props: TComponentProps) => JSX.Element;

export type ViewModelType<TViewModel extends ViewModel> = {
    new(): TViewModel;
};
