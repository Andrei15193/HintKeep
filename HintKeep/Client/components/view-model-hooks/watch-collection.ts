import type { ICollectionChange, IEventHandler } from "../../events";
import type { IReadOnlyObservableCollection } from "../../view-models/core";
import { useState, useEffect } from 'react';

export type ItemChangeHandler<TItem> = (item: TItem) => void;

export function watchCollection<TItem>($collection: IReadOnlyObservableCollection<TItem>, itemAddedCallback?: ItemChangeHandler<TItem>, itemRemovedCallback?: ItemChangeHandler<TItem>): void {
    const [_, setState] = useState<readonly TItem[]>([]);

    useEffect(
        () => {
            if ($collection) {
                const propertyChangedEventHandler: IEventHandler<any> = {
                    handle(subject: IReadOnlyObservableCollection<TItem>, collectionChange: ICollectionChange<TItem>): void {
                        if (collectionChange.addedItems && itemAddedCallback)
                            collectionChange.addedItems.forEach(item => itemAddedCallback(item));
                        if (collectionChange.removedItems && itemRemovedCallback)
                            collectionChange.removedItems.forEach(item => itemRemovedCallback(item));

                        setState(subject.concat([]));
                    }
                }
                $collection.colllectionChanged.subscribe(propertyChangedEventHandler);
                return () => $collection.colllectionChanged.unsubscribe(propertyChangedEventHandler);
            }
        },
        [$collection]
    );
}