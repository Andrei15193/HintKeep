import type { IObservable, IObserver } from '../../observer';
import { useEffect } from 'react';

export type EventHandler = (subject: any) => void;

export function useEvent(event: IObservable, handler: EventHandler): void {
    useEffect(
        () => {
            const observer: IObserver = { notifyChanged: handler };
            event.subscribe(observer);
            return () => event.unsubscribe(observer);
        },
        [event]
    );
}