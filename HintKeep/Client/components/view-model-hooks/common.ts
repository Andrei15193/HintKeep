import type { INotifyPropertyChanged } from '../../events';

export type ViewModelType<TViewModel extends INotifyPropertyChanged> = {
    new(): TViewModel;
};