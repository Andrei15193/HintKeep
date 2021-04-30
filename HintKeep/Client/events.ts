export interface IEvent<TEventArgs = void> {
    subscribe(eventHandler: IEventHandler<TEventArgs>): void;

    unsubscribe(eventHandler: IEventHandler<TEventArgs>): void;
}

export interface IEventHandler<TEventArgs = void> {
    handle(subject: object, args: TEventArgs): void;
}

export type SubscriptionCallback = () => void;

export class DispatchEvent<TEventArgs = void> implements IEvent<TEventArgs> {
    private readonly _firstSubscribeCallback?: SubscriptionCallback;
    private readonly _lastUnsubscribeCallback?: SubscriptionCallback;
    private _eventHandlers: readonly IEventHandler<TEventArgs>[] = [];

    public constructor(firstSubscribeCallback?: SubscriptionCallback, lastUnsubscribeCallback?: SubscriptionCallback) {
        this._firstSubscribeCallback = firstSubscribeCallback;
        this._lastUnsubscribeCallback = lastUnsubscribeCallback;
    }

    public subscribe(eventHandler: IEventHandler<TEventArgs>): void {
        if (this._eventHandlers.length === 0 && this._firstSubscribeCallback)
            this._firstSubscribeCallback();
        this._eventHandlers = this._eventHandlers.concat([eventHandler]);
    }

    public unsubscribe(eventHandler: IEventHandler<TEventArgs>): void {
        this._eventHandlers = this._eventHandlers.filter(registeredEventHandler => registeredEventHandler !== eventHandler);
        if (this._eventHandlers.length === 0 && this._lastUnsubscribeCallback)
            this._lastUnsubscribeCallback();
    }

    public dispatch(subject: object, args: TEventArgs): void {
        this._eventHandlers.forEach(eventHandler => {
            if (this._eventHandlers.includes(eventHandler))
                eventHandler.handle(subject, args);
        });
    }
}

export interface INotifyPropertyChanged {
    readonly propertyChanged: IEvent<readonly string[]>;
}

export interface INotifyCollectionChanged<TItem> {
    readonly colllectionChanged: IEvent<ICollectionChange<TItem>>;
}

export interface ICollectionChange<TItem> {
    readonly addedItems: readonly TItem[];
    readonly removedItems: readonly TItem[];
}