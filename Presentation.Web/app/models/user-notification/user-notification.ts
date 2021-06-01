module Kitos.Models.UserNotification {

    export interface UserNotificationDTO {
        id: number;
        name: string;
        notificationMessage: string;
        relatedEntityId: number;
        relatedEntityType: ObjectType;
        notificationType: NotificationType;
        lastChanged: string;
    }

    export enum ObjectType {
        itContract = 0,
        itSystemUsage = 1,
        itProject = 2,
        itInterface = 3,
        dataProcessingRegistration = 4
    }

    export enum NotificationType {
        advice = 0
    }

}