import type { IServices } from "./services-context";
import type { INotifyPropertiesChanged } from 'react-model-view-viewmodel';
import { useContext } from "react";
import { useViewModelFactory } from 'react-model-view-viewmodel';
import { ServicesContext } from "./services-context";

export type ViewModelFactory<TViewModel extends INotifyPropertiesChanged> = (services: IServices) => TViewModel;

export function useViewModel<TViewModel extends INotifyPropertiesChanged>(viewModelFactory: ViewModelFactory<TViewModel>, watchedProperties?: readonly (keyof TViewModel)[]): TViewModel {
    const services = useContext(ServicesContext);
    const viewModel = useViewModelFactory(() => viewModelFactory(services), watchedProperties);
    return viewModel;
}

export function requireViewModel<TViewModel extends INotifyPropertiesChanged>(viewModelFactory: ViewModelFactory<TViewModel>): TViewModel {
    const services = useContext(ServicesContext);
    const viewModel = viewModelFactory(services);
    return viewModel;
}