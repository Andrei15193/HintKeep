import React, { useEffect, useRef } from "react";
import { type ICollectionChangedEventHandler, useDependency, useObservableCollection } from "react-model-view-viewmodel";
import { GlobalNotification } from "./GlobalNotification";
import { type INotification, Notifications } from "./ViewModels/Notifications";

export function GlobalNotificationsContainer(): React.JSX.Element {
    const notifications = useDependency(Notifications);
    useObservableCollection(notifications);
    const notificationIdsRef = useRef<Map<INotification, string> | null>(null);
    if (notificationIdsRef.current === null)
        notificationIdsRef.current = new Map<INotification, string>();
    const { current: notificationIds } = notificationIdsRef;

    useEffect(
        () => {
            const notificationsChangedEventHandler: ICollectionChangedEventHandler<Notifications, INotification> = {
                handle(_, { addedItems: addedNotifications, removedItems: removedNotifications }) {
                    addedNotifications.forEach((addedNotification) => {
                        notificationIds.set(addedNotification, crypto.randomUUID());
                    });
                    removedNotifications.forEach((removedNotification) => {
                        notificationIds.delete(removedNotification);
                    });
                }
            };

            notifications.forEach((notification) => {
                notificationIds.set(notification, crypto.randomUUID());
            });
            notifications.collectionChanged.subscribe(notificationsChangedEventHandler);

            return () => {
                notifications.collectionChanged.unsubscribe(notificationsChangedEventHandler);
                notificationIds.clear();
            };
        },
        [notifications, notificationIds]
    );

    return (
        <>
            {
                notifications
                    .map((notification) => (
                        <GlobalNotification
                            key={notificationIds.get(notification)}
                            notification={notification}
                        />
                    ))
            }
        </>
    );
}