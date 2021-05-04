import type { ICollectionChange, IEvent, INotifyCollectionChanged, INotifyPropertyChanged } from '../../events';
import { DispatchEvent } from '../../events';

export interface IReadOnlyObservableCollection<TItem> extends Readonly<Array<TItem>>, INotifyPropertyChanged, INotifyCollectionChanged<TItem> {
}

export interface IObservableCollection<TItem> extends IReadOnlyObservableCollection<TItem> {
    push(...items: readonly TItem[]): number;

    pop(): TItem | undefined;

    unshift(...items: readonly TItem[]): number;

    shift(): TItem | undefined;

    splice(start: number, deleteCount?: number): TItem[];

    clear(): void;

    reset(items: readonly TItem[]): void;
}

export function observableCollection<TItem>(...items: readonly TItem[]): IObservableCollection<TItem> {
    return new ObservableCollection<TItem>(...items);
}

class ObservableCollection<TItem> extends Array<TItem> implements IObservableCollection<TItem> {
    private readonly _propertyChangedEvent: DispatchEvent<readonly string[]>;
    private readonly _collectionChangedEvent: DispatchEvent<ICollectionChange<TItem>>;

    public constructor(...items: readonly TItem[]) {
        super(...items);
        this.propertyChanged = this._propertyChangedEvent = new DispatchEvent<readonly string[]>();
        this.colllectionChanged = this._collectionChangedEvent = new DispatchEvent<ICollectionChange<TItem>>();
    }

    public readonly propertyChanged: IEvent<readonly string[]>;

    public readonly colllectionChanged: IEvent<ICollectionChange<TItem>>;

    public push(...items: readonly TItem[]): number {
        const result = super.push(...items);
        this._collectionChangedEvent.dispatch(this, { addedItems: items, removedItems: [] });
        this._propertyChangedEvent.dispatch(this, ['length']);
        return result;
    }

    public pop(): TItem | undefined {
        if (this.length > 0) {
            const removedItem = super.pop() as TItem;
            this._collectionChangedEvent.dispatch(this, { addedItems: [], removedItems: [removedItem] });
            this._propertyChangedEvent.dispatch(this, ['length']);
            return removedItem;
        }
        else
            return undefined;
    }

    public unshift(...items: readonly TItem[]): number {
        const result = super.unshift(...items);
        this._collectionChangedEvent.dispatch(this, { addedItems: items, removedItems: [] });
        this._propertyChangedEvent.dispatch(this, ['length']);
        return result;
    }

    public shift(): TItem | undefined {
        if (this.length > 0) {
            const removedItem = super.shift() as TItem;
            this._collectionChangedEvent.dispatch(this, { addedItems: [], removedItems: [removedItem] });
            this._propertyChangedEvent.dispatch(this, ['length']);
            return removedItem;
        }
        else
            return undefined;
    }

    public splice(start: number, deleteCount?: number): TItem[] {
        const result = super.splice(start, deleteCount);
        this._collectionChangedEvent.dispatch(this, { addedItems: [], removedItems: result });
        this._propertyChangedEvent.dispatch(this, ['length']);
        return result;
    }

    public clear = (): void => {
        const removedItems = this.splice(0);
        this._collectionChangedEvent.dispatch(this, { addedItems: [], removedItems });
        this._propertyChangedEvent.dispatch(this, ['length']);
    }

    public reset = (items: readonly TItem[]): void => {
        const previousLength = this.length;
        const removedItems = this.splice(0);
        this.push(...items);
        this._collectionChangedEvent.dispatch(this, { addedItems: items, removedItems });
        if (previousLength !== this.length)
            this._propertyChangedEvent.dispatch(this, ['length']);
    }
}