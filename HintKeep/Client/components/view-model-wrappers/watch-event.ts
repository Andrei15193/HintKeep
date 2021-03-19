import type { IEvent, IEventHandler } from '../../events';
import { useEffect } from 'react';

export type EventHandler<TEventArgs> = (subject: object, args: TEventArgs) => void;

export function watchEvent<TEventArgs>(event: IEvent<TEventArgs>, handler: EventHandler<TEventArgs>): void {
    useEffect(
        () => {
            const eventHandler: IEventHandler<TEventArgs> = { handle: handler };
            event.subscribe(eventHandler);
            return () => event.unsubscribe(eventHandler);
        },
        [event]
    );
}