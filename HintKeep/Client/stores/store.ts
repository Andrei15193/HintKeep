import type { IObservable, IObserver } from '../observer';

export class Store implements IObservable {
    private _observers: IObserver[] = [];

    public subscribe(observer: IObserver): void {
        this._observers = this._observers.concat([observer]);
    }

    public unsubscribe(observer: IObserver): void {
        this._observers = this._observers.filter(registeredObserver => registeredObserver !== observer);
    }

    protected notifyChanged(): void {
        this._observers.forEach(observer => observer.notifyChanged(this));
    }
}