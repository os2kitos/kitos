module Kitos.Services.UserNotification {
    export interface IUserNotificationService {
        delete(userNotificationId: number): angular.IPromise<IUserNotificationDeletedResult>;
    }

    export interface IUserNotificationDeletedResult {
        deletedObjectId: number;
    }

    export class UserNotificationService implements IUserNotificationService {

        delete(userNotificationId: number): angular.IPromise<IUserNotificationDeletedResult> {
            return this
                .$http
                .delete<API.Models.IApiWrapper<any>>(`api/v1/user-notification/${userNotificationId}`)
                .then(
                    response => {
                        return <IUserNotificationDeletedResult>{
                            deletedObjectId: response.data.response.id,
                        };
                    },
                    error => this.handleServerError(error)
                );
        }

        private handleServerError(error) {
            console.log("Request failed with:", error);
            let errorCategory: Models.Api.ApiResponseErrorCategory;
            switch (error.status) {
                case 400:
                    errorCategory = Models.Api.ApiResponseErrorCategory.BadInput;
                    break;
                case 404:
                    errorCategory = Models.Api.ApiResponseErrorCategory.NotFound;
                    break;
                case 409:
                    errorCategory = Models.Api.ApiResponseErrorCategory.Conflict;
                    break;
                case 500:
                    errorCategory = Models.Api.ApiResponseErrorCategory.ServerError;
                    break;
                default:
                    errorCategory = Models.Api.ApiResponseErrorCategory.UnknownError;
            }
            throw errorCategory;
        }

        static $inject = ["$http"];

        constructor(private readonly $http: ng.IHttpService) {
        }
    }

    app.service("userNotificationService", UserNotificationService);
}