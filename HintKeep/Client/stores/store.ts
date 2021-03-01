import type { IObservable, IObserver, ObserverCallback, UnsubscribeCallback } from '../observer';

export class Store implements IObservable {
    private _observers: IObserver[] = [];

    public subscribe(observer: IObserver): void {
        this._observers = this._observers.concat([observer]);
    }

    public subscribeWithCallback(callback: ObserverCallback): UnsubscribeCallback {
        const observer: IObserver = { notifyChanged: callback };
        this.subscribe(observer);
        return () => this.unsubscribe(observer);
    }

    public unsubscribe(observer: IObserver): void {
        this._observers = this._observers.filter(registeredObserver => registeredObserver !== observer);
    }

    protected notifyChanged(): void {
        this._observers.forEach(observer => observer.notifyChanged(this));
    }
}