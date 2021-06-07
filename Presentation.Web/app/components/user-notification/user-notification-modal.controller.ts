module Kitos.UserNotifications {
    "use strict";

    export class UserNotificationsModalController {

        mainGridOptions;
        mainGrid; // We don't set the grid. It is being set by the options that we define. But we need access to this in order to manually force a refresh of the data.

        static $inject: Array<string> = [
            "notify",
            "$uibModalInstance",
            "userNotificationService",
            "contextType",
            "user"
        ];

        constructor(
            private readonly notify,
            private readonly $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
            private readonly userNotificationService: Services.UserNotification.IUserNotificationService,
            private readonly contextType: Models.UserNotification.RelatedEntityType,
            private readonly user
        ) {
            this.mainGridOptions = {
                dataSource: {
                    transport: {
                        read: {
                            url: `/api/v1/user-notification/organization/${this.user.currentOrganizationId}/context/${this.contextType}/user/${this.user.id}`,
                            dataType: "json",
                            contentType: "application/json"
                        },
                    },
                    schema: {
                        data: (apiResponse: API.Models.IApiWrapper<Models.UserNotification.UserNotificationDTO[]>) => {
                            return apiResponse.response;
                        }
                    },
                    serverPaging: false,
                    serverFiltering: false,
                    serverSorting: false
                },
                columns: [
                    {
                        field: "NotificationType", 
                        title: "Type",
                        width: 30,
                        template: (dataItem: Models.UserNotification.UserNotificationDTO) => {
                            if (dataItem.notificationType === Models.UserNotification.NotificationType.advice) {
                                return "Advis";
                            }
                        },
                        attributes: { "class": "might-overflow fixed-grid-left-padding" },
                        sortable: false
                    },
                    {
                        field: "Name",
                        title: "Navn",
                        width: 50,
                        template: (dataItem: Models.UserNotification.UserNotificationDTO) => {
                            var pathToItem = null;
                            switch (dataItem.relatedEntityType) {
                                case Models.UserNotification.RelatedEntityType.itContract:
                                    pathToItem = Helpers.RenderFieldsHelper.renderInternalReferenceFromModal(`kendo-user-notification-rendering`, "it-contract.edit.advice-generic", dataItem.relatedEntityId, dataItem.name);
                                    break;
                                case Models.UserNotification.RelatedEntityType.itSystemUsage:
                                    pathToItem = Helpers.RenderFieldsHelper.renderInternalReferenceFromModal(`kendo-user-notification-rendering`, "it-system.usage.advice", dataItem.relatedEntityId, dataItem.name);
                                    break;
                                case Models.UserNotification.RelatedEntityType.itProject:
                                    pathToItem = Helpers.RenderFieldsHelper.renderInternalReferenceFromModal(`kendo-user-notification-rendering`, "it-project.edit.advice-generic", dataItem.relatedEntityId, dataItem.name);
                                    break;
                                case Models.UserNotification.RelatedEntityType.dataProcessingRegistration:
                                    pathToItem = Helpers.RenderFieldsHelper.renderInternalReferenceFromModal(`kendo-user-notification-rendering`, "data-processing.edit-registration.advice", dataItem.relatedEntityId, dataItem.name);
                                    break;
                                default:
                                    // Do nothing to path so there is not created a link as we don't know where we should link to.
                                    break;
                            }
                            if (pathToItem != null && pathToItem !== "") {
                                return pathToItem;
                            }
                            else {
                                return dataItem.name;
                            }

                        },
                        attributes: { "class": "might-overflow fixed-grid-left-padding" },
                        sortable: false
                    },
                    {
                        field: "Created", 
                        title: "Dato",
                        width: 50,
                        template: (dataItem: Models.UserNotification.UserNotificationDTO) => {
                            return moment(dataItem.created).format("DD-MM-YYYY");
                        },
                        attributes: { "class": "fixed-grid-left-padding" },
                        sortable: false
                    },
                    {
                        field: "NotificationMessage", 
                        title: "Besked",
                        width: 150,
                        template: (dataItem: Models.UserNotification.UserNotificationDTO) => {
                            return dataItem.notificationMessage;
                        },
                        attributes: { "class": "might-overflow" },
                        sortable: false
                    },
                    {
                        width: 50,
                        template: (dataItem: Models.UserNotification.UserNotificationDTO) => {
                            return `<button class="btn btn-success modal-btn" data-confirm-click="Er du sikker på at du vil slette?" data-confirmed-click="vm.deleteNotification(${dataItem.id})">Ok og slet</button>`;
                        },
                        sortable: false
                    }
                ]
            }
        }

        deleteNotification(notificationId: number) {
            var self = this;
            this.userNotificationService.delete(notificationId)
                .then(
                    (onSuccess) => {
                        self.notify.addSuccessMessage("Notifikationen blev fjernet");
                        self.mainGrid.dataSource.read(); //Used to refresh overview once an item is deleted.
                    },
                    (onError) => {
                        self.notify.addErrorMessage("Kunne ikke fjerne notifikationen. Prøv igen senere.");
                    }
                );
        }

        private close() {
            this.$uibModalInstance.close();
        }

        cancel(): void {
            this.close();
        }
    }

    angular
        .module("app")
        .directive("userNotifications", [
            () => {
                return {
                    templateUrl: "app/components/user-notification/user-notification.view.html",
                    scope: {
                        stateName: "@"
                    },
                    controller: [
                        '$scope', '$uibModal', 'userNotificationService', 'userService', function ($scope, $uibModal, userNotificationService: Services.UserNotification.IUserNotificationService, userService: Services.IUserService) {

                            $scope.$watch("stateName", function (newValue, oldValue) {
                                if ($scope.stateName === Constants.SRef.ContractOverview ||
                                    $scope.stateName === Constants.SRef.ContractPlanOverview ||
                                    $scope.stateName === Constants.SRef.ProjectOverview ||
                                    $scope.stateName === Constants.SRef.SystemUsageOverview ||
                                    $scope.stateName === Constants.SRef.DataProcessingRegistrationOverview) {
                                    userService
                                        .getUser()
                                        .then(user => {
                                            var contextType = resolveContextType($scope.stateName);
                                            userNotificationService
                                                .getNumberOfUnresolvedNotifications(user.currentOrganizationId, user.id, contextType)
                                                .then(
                                                    numberOfNotifications => {
                                                        $scope.numberOfUserNotifications = numberOfNotifications;
                                                    });
                                        });

                                    $scope.enableUserNotifications = true;
                                }

                                else {
                                    $scope.enableUserNotifications = false;
                                    $scope.hasNotifications = false;
                                    $scope.hasManyNotifications = false;
                                }
                            });

                            $scope.showUserNotifications = () => {
                                var modalInstance = $uibModal.open({
                                    windowClass: "modal fade in",
                                    templateUrl: "app/components/user-notification/user-notification-modal.view.html",
                                    controller: UserNotifications.UserNotificationsModalController,
                                    controllerAs: "vm",
                                    size: 'lg',
                                    resolve: {
                                        contextType: [() => resolveContextType($scope.stateName)],
                                        user: ["userService", (userService: Services.IUserService) => userService.getUser()]
                                    }
                                }).closed.then(() => {
                                    userService
                                        .getUser()
                                        .then(user => {
                                            var contextType = resolveContextType($scope.stateName);
                                            userNotificationService
                                                .getNumberOfUnresolvedNotifications(user.currentOrganizationId, user.id, contextType)
                                                .then(
                                                    numberOfNotifications => {
                                                        $scope.numberOfUserNotifications = numberOfNotifications;
                                                    });
                                        });
                                });
                            }

                            function resolveContextType(contextAsString: string): Models.UserNotification.RelatedEntityType {
                                switch (contextAsString) {
                                    case Constants.SRef.ContractOverview:
                                        return Models.UserNotification.RelatedEntityType.itContract;
                                    case Constants.SRef.ContractPlanOverview:
                                        return Models.UserNotification.RelatedEntityType.itContract;
                                    case Constants.SRef.SystemUsageOverview:
                                        return Models.UserNotification.RelatedEntityType.itSystemUsage;
                                    case Constants.SRef.ProjectOverview:
                                        return Models.UserNotification.RelatedEntityType.itProject;
                                    case Constants.SRef.DataProcessingRegistrationOverview:
                                        return Models.UserNotification.RelatedEntityType.dataProcessingRegistration;
                                    default:
                                        return null;
                                }
                            }


                        }]
                };
            }
        ]);
}