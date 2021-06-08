module Kitos.Models.UserNotification {

    export interface UserNotificationDTO {
        id: number;
        name: string;
        notificationMessage: string;
        relatedEntityId: number;
        relatedEntityType: RelatedEntityType;
        notificationType: NotificationType;
        created: string;
    }

    export enum RelatedEntityType {
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