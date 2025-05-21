import type { INotification, NotificationType } from "./ViewModels/Notifications";
import React, { useCallback } from "react";
import { Button } from "../Forms/Components";

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
                            <Button
                                neutral
                                onClick={dismissCallback}
                            >
                                Dismiss
                            </Button>
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