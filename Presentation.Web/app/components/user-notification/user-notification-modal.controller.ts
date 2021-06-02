module Kitos.UserNotifications {
    "use strict";

    export class UserNotificationsModalController {

        mainGridOptions;
        mainGrid; // We don't set the grid. It is being set by the options that we define. But we need access to this in order to manually force a refresh of the data.

        static $inject: Array<string> = [
            "$scope",
            "notify",
            "$uibModalInstance",
            "userNotificationService",
            "contextType"
        ];

        constructor(
            private readonly $scope,
            private readonly notify,
            private readonly $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
            private readonly userNotificationService: Services.UserNotification.IUserNotificationService,
            private readonly contextType: Models.UserNotification.ObjectType
        ) {
            this.mainGridOptions = {
                dataSource: {
                    transport: {
                        read: {
                            url: `/api/v1/user-notification/organization/${this.$scope.user.currentOrganizationId}/context/${this.contextType}/user/${this.$scope.user.id}`,
                            dataType: "json",
                            contentType: "application/json"
                        },
                    },
                    schema: {
                        data: (apiResponse: API.Models.IApiWrapper<Models.UserNotification.UserNotificationDTO[]>) => {
                            return apiResponse.response;
                        }
                    },
                    pageSize: 10,
                    serverPaging: false,
                    serverFiltering: false,
                    serverSorting: false
                },
                columns: [
                    {
                        field: "NotificationType", // It's not odata so this field doesn't work to provide the data.
                        title: "Type",
                        width: 30,
                        template: (dataItem: Models.UserNotification.UserNotificationDTO) => {
                            if (dataItem.notificationType === Models.UserNotification.NotificationType.advice) {
                                return "Advis";
                            }
                        },
                        attributes: { "class": "might-overflow" },
                        sortable: false
                    },
                    {
                        field: "Name", // It's not odata so this field doesn't work to provide the data.
                        title: "Navn",
                        width: 50,
                        template: (dataItem: Models.UserNotification.UserNotificationDTO) => {
                            var pathToItem;
                            switch (dataItem.relatedEntityType) {
                                case Models.UserNotification.ObjectType.itContract:
                                    pathToItem = `#/contract/edit/${dataItem.relatedEntityId}`;
                                    break;
                                case Models.UserNotification.ObjectType.itSystemUsage:
                                    pathToItem = `#/system/usage/${dataItem.relatedEntityId}`;
                                    break;
                                case Models.UserNotification.ObjectType.itProject:
                                    pathToItem = `#/project/edit/${dataItem.relatedEntityId}`;
                                    break;
                                case Models.UserNotification.ObjectType.dataProcessingRegistration:
                                    pathToItem = `#/data-processing/edit/${dataItem.relatedEntityId}`;
                                    break;
                                default:
                                    // Do nothing to path so there is not created a link as we don't know where we should link to.
                                    break;
                            }

                            if (pathToItem != null) {
                                switch (dataItem.notificationType) {
                                    case Models.UserNotification.NotificationType.advice:
                                        pathToItem += "/advice";
                                        break;
                                    default:
                                        pathToItem = null;
                                        break;
                                }
                            }

                            if (pathToItem != null) {
                                return `<a ng-click="$dismiss()" href="${window.location.origin}/${pathToItem}">${dataItem.name}</a>`;
                            }
                            else {
                                return dataItem.name;
                            }

                        },
                        attributes: { "class": "might-overflow" },
                        sortable: false
                    },
                    {
                        field: "LastChanged", // It's not odata so this field doesn't work to provide the data.
                        title: "Dato",
                        width: 50,
                        template: (dataItem: Models.UserNotification.UserNotificationDTO) => {
                            return moment(dataItem.lastChanged).format("DD-MM-YYYY");
                        },
                        sortable: false
                    },
                    {
                        field: "NotificationMessage", // It's not odata so this field doesn't work to provide the data.
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
                                if ($scope.stateName === "it-project.overview" || $scope.stateName === "it-system.overview" || $scope.stateName === "it-contract.overview" || $scope.stateName === "it-contract.plan" || $scope.stateName === "data-processing.overview") {

                                    userService
                                        .getUser()
                                        .then(user => {
                                            var contextType = resolveContextType($scope.stateName);
                                            userNotificationService
                                                .getNumberOfUnresolvedNotifications(user.currentOrganizationId, user.id, contextType)
                                                .then(
                                                    numberOfNotifications => {
                                                        $scope.numberOfUserNotifications = numberOfNotifications;
                                                        if (numberOfNotifications > 0) {
                                                            $scope.hasNotifications = true;

                                                            if (numberOfNotifications > 9) { // Used to handle styling of many notifications.
                                                                $scope.hasManyNotifications = true;
                                                            }
                                                            else {
                                                                $scope.hasManyNotifications = false;
                                                            }

                                                        }
                                                        else {
                                                            $scope.hasNotifications = false;
                                                            $scope.hasManyNotifications = false;
                                                        }
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
                                        contextType: [() => resolveContextType($scope.stateName)]
                                    }
                                });
                            }

                            function resolveContextType(contextAsString: string): Models.UserNotification.ObjectType {
                                switch (contextAsString) {
                                    case "it-contract.overview":
                                        return Models.UserNotification.ObjectType.itContract;
                                    case "it-contract.plan":
                                        return Models.UserNotification.ObjectType.itContract;
                                    case "it-system.overview":
                                        return Models.UserNotification.ObjectType.itSystemUsage;
                                    case "it-project.overview":
                                        return Models.UserNotification.ObjectType.itProject;
                                    case "data-processing.overview":
                                        return Models.UserNotification.ObjectType.dataProcessingRegistration;
                                    default:
                                        return null;
                                }
                            }


                        }]
                };
            }
        ]);
}