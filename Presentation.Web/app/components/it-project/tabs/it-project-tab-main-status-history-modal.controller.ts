(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-project.edit.main.status-history-modal', {
            url: '/history',
            onEnter: ['$state', '$stateParams', '$uibModal', 'project', 'user', 'statusUpdates',
                function ($state, $stateParams, $modal, project, user, statusUpdates) {
                    $modal.open({
                        templateUrl: "app/components/it-project/tabs/it-project-tab-main-status-history-modal.view.html",
                        // fade in instead of slide from top, fixes strange cursor placement in IE
                        // http://stackoverflow.com/questions/25764824/strange-cursor-placement-in-modal-when-using-autofocus-in-internet-explorer
                        windowClass: "modal fade in",
                        resolve: {
                            user: function () {
                                return user;
                            },
                            // workaround to get data inherited from parent state
                            project: function () {
                                return project;
                            },
                            statusUpdates: function () {
                                return statusUpdates;
                            }
                        },
                        controller: "project.statusHistoryModalCtrl",
                        size: 'lg',
                    }).result.then(function () {
                        // OK
                        // GOTO parent state and reload
                        $state.go("^", null, { reload: true });
                    }, function () {
                        // Cancel
                        // GOTO parent state
                        $state.go("^");
                    });
                }
            ]
        });
    }]);

    app.controller("project.statusHistoryModalCtrl",
        ["$scope", "$http", "project", "notify", "user", "statusUpdates", "moment",
            function ($scope, $http, project, notify, user, statusUpdates, moment) {

                $scope.statusUpdates = statusUpdates;
                $scope.project = project;
                $scope.moment = moment;

                $scope.dismiss = function () {
                    $scope.$dismiss();
                };
            }]);
})(angular, app);
