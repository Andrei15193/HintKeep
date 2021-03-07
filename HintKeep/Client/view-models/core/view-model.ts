import type { IEvent, IEventHandler, INotifyPropertyChanged } from '../../events';
import { DispatchEvent } from '../../events';

export class ViewModel implements INotifyPropertyChanged {
    private readonly _stores: readonly INotifyPropertyChanged[];
    private readonly _storePropertyChanged: IEventHandler<readonly string[]>;
    private readonly _propertyChangedEvent: DispatchEvent<readonly string[]>;

    public constructor(...stores: readonly INotifyPropertyChanged[]) {
        this._storePropertyChanged = { handle: this.storeChanged.bind(this) };
        this._stores = stores ? stores : [];
        this._propertyChangedEvent = new DispatchEvent<readonly string[]>(
            () => this._stores.forEach(store => store.propertyChanged.subscribe(this._storePropertyChanged)),
            () => this._stores.forEach(store => store.propertyChanged.unsubscribe(this._storePropertyChanged))
        );
    }

    public get propertyChanged(): IEvent<readonly string[]> {
        return this._propertyChangedEvent;
    }

    protected storeChanged(store: object, changedProperties: readonly string[]): void {
    }

    protected notifyPropertyChanged(changedProperty: string, ...otherChangedProperties: readonly string[]): void {
        this._propertyChangedEvent.dispatch(this, [changedProperty, ...otherChangedProperties]);
    }
}