export type ObserverCallback = (subject: any) => void;

export type UnsubscribeCallback = () => void;

export interface IObserver {
    notifyChanged(subject: any): void;
}

export interface IObservable {
    subscribe(observer: IObserver): void;

    subscribeWithCallback(callback: ObserverCallback): UnsubscribeCallback;

    unsubscribe(observer: IObserver): void;
}

export class DispatchObservable implements IObservable {
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

    public dispatch(event: any): void {
        this._observers.forEach(observer => observer.notifyChanged(event));
    }
}