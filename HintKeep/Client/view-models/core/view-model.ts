import type { IObservable, IObserver, ObserverCallback, UnsubscribeCallback } from '../../observer';

export class ViewModel implements IObservable {
    private readonly _aggregateObserver: IObserver;
    private readonly _aggregateObservables: Readonly<IObservable[]>;
    private _observers: IObserver[];

    public constructor(...aggregateObservables: IObservable[]) {
        this._aggregateObserver = {
            notifyChanged: this.aggregateObservablesChanged.bind(this)
        };
        this._aggregateObservables = aggregateObservables ? aggregateObservables.concat() : [];
        this._observers = [];
    }

    public subscribe(observer: IObserver): void {
        this._observers = this._observers.concat([observer]);
        if (this._observers.length === 1)
            this._aggregateObservables.forEach(aggregateObservable => aggregateObservable.subscribe(this._aggregateObserver));
    }

    public subscribeWithCallback(callback: ObserverCallback): UnsubscribeCallback {
        const observer: IObserver = { notifyChanged: callback };
        this.subscribe(observer);
        return () => this.unsubscribe(observer);
    }

    public unsubscribe(observer: IObserver): void {
        this._observers = this._observers.filter(registeredObserver => registeredObserver !== observer);
        if (this._observers.length === 0)
            this._aggregateObservables.forEach(aggregateObservable => aggregateObservable.unsubscribe(this._aggregateObserver));
    }

    protected aggregateObservablesChanged(): void {
        this.notifyChanged();
    }

    protected notifyChanged(): void {
        this._observers.forEach(observer => observer.notifyChanged(this));
    }
}