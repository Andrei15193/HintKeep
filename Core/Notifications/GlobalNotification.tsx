import type { INotification, NotificationType } from "./ViewModels/Notifications";
import React, { useCallback } from "react";

export interface IGlobalNotificationProps {
    readonly notification: INotification;
}

export function GlobalNotification({ notification }: IGlobalNotificationProps): React.JSX.Element {
    const dismissCallback = useCallback(
        () => {
            notification.dismiss();
        },
        [notification]
    );

    return (
        <div className={`global-notification-item ${getNotificationItemClassName(notification.type)}`.trimEnd()}>
            {
                typeof notification.message === "function"
                    ? notification.message(notification)
                    : (
                        <>
                            <div>
                                {notification.message}
                            </div>
                            <button onClick={dismissCallback}>
                                Dismiss
                            </button>
                        </>
                    )
            }
        </div>
    );
}

function getNotificationItemClassName(notificationType: NotificationType): string {
    switch (notificationType) {
        case "info":
            return "";

        case "error":
            return "error";

        default:
            return "";
    }
}