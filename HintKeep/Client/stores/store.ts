import type { IEvent, INotifyPropertyChanged } from '../events';
import { DispatchEvent } from '../events';

export class Store implements INotifyPropertyChanged {
    private readonly _propertyChangedEvent: DispatchEvent<readonly string[]> = new DispatchEvent();

    public get propertyChanged(): IEvent<readonly string[]> {
        return this._propertyChangedEvent;
    }

    protected notifyPropertyChanged(changedProperty: string, ...otherChangedProperties: readonly string[]): void {
        this._propertyChangedEvent.dispatch(this, [changedProperty, ...otherChangedProperties]);
    }
}