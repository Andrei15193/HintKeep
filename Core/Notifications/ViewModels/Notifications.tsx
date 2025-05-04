import { ReadOnlyObservableCollection } from "react-model-view-viewmodel";

export class Notifications extends ReadOnlyObservableCollection<INotification> {
    public constructor() {
        super();
    }

    public add({ message, type = "info" }: INotificationOptions): void {
        const notificationsList = this;

        const notification: INotification = {
            message,
            type,

            dismiss() {
                notificationsList.dismiss(this);
            }
        };

        this.push(notification);
    }

    public dismiss(notification: INotification) {
        const notificationIndex = this.indexOf(notification);
        if (notificationIndex >= 0)
            this.splice(notificationIndex, 1);
    }
}

export interface INotificationOptions {
    readonly message: string | ((notification: Omit<INotification, "message">) => React.JSX.Element);
    readonly type?: NotificationType;
}

export interface INotification {
    readonly message: string | ((notification: Omit<INotification, "message">) => React.JSX.Element);
    readonly type: NotificationType;

    dismiss(): void;
}
export type NotificationType = "info" | "error";