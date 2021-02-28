import type { IObservable, IObserver } from '../../observer';
import { ViewModel } from './view-model';

export enum AsyncViewModelState {
    Ready,
    Busy,
    Faulted
}

export class AsyncViewModel extends ViewModel {
    private _state: AsyncViewModelState = AsyncViewModelState.Ready;

    public constructor(...aggregateObservables: IObservable[]) {
        super(...aggregateObservables);
    }

    public get state(): AsyncViewModelState {
        return this._state;
    }

    private _setState(value: AsyncViewModelState): void {
        if (this._state !== value) {
            this._state = value;
            this.notifyChanged();
        }
    }

    protected async handleAsync(asyncCallback: () => Promise<void>): Promise<void> {
        try {
            this._setState(AsyncViewModelState.Busy);
            await asyncCallback();
            this._setState(AsyncViewModelState.Ready);
        }
        catch (error) {
            this._setState(AsyncViewModelState.Faulted);
            throw error;
        }
    }

    protected async delay(millisecondsDelay: number): Promise<void> {
        return new Promise(resolve => setTimeout(() => resolve(), millisecondsDelay));
    }

    protected async delayWithResult<TResult>(millisecondsDelay: number, result: TResult): Promise<TResult> {
        return new Promise(resolve => setTimeout(() => resolve(result), millisecondsDelay));
    }
}