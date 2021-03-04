export interface IObserver {
    notifyChanged(subject: any): void;
}

export interface IObservable {
    subscribe(observer: IObserver): void;

    unsubscribe(observer: IObserver): void;
}

export class DispatchObservable implements IObservable {
    private _observers: IObserver[] = [];

    public subscribe(observer: IObserver): void {
        this._observers = this._observers.concat([observer]);
    }

    public unsubscribe(observer: IObserver): void {
        this._observers = this._observers.filter(registeredObserver => registeredObserver !== observer);
    }

    public dispatch(event: any): void {
        this._observers.forEach(observer => observer.notifyChanged(event));
    }
}