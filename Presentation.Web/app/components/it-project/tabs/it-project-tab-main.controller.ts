(function (ng, app) {
    app.config(["$stateProvider", function ($stateProvider) {
        $stateProvider.state("it-project.edit.main", {
            url: "/main",
            templateUrl: "app/components/it-project/tabs/it-project-tab-main.view.html",
            controller: "project.EditMainCtrl",
            resolve: {
                project: [
                    "$http", "$stateParams", ($http, $stateParams) => {
                        return $http.get("api/itproject/" + $stateParams.id)
                            .then((result) => result.data.response);
                    }
                ],
                projectTypes: [
                    "$http", $http => {
                        return $http.get("odata/LocalItProjectTypes?$filter=IsLocallyAvailable eq true or IsObligatory&$orderby=Priority desc")
                            .then((result) => result.data.value);
                    }
                ],
                user: [
                    "userService", userService => userService.getUser()
                ],
                hasWriteAccess: [
                    "$http", "$stateParams", "user", ($http, $stateParams, user) => {
                        return $http.get("api/itproject/" + $stateParams.id + "?hasWriteAccess=true&organizationId=" + user.currentOrganizationId)
                            .then((result) => result.data.response);
                    }
                ],
                statusUpdates: [
                    "$http", "$stateParams",
                    ($http, $stateParams) => $http.get(`odata/ItProjects(${$stateParams.id})?$expand=ItProjectStatusUpdates($orderby=Created desc;$expand=ObjectOwner($select=Name,LastName))`)
                        .then(result => {
                            return result.data.ItProjectStatusUpdates;
                        })
                ]
            }
        });
    }]);

    app.controller("project.EditMainCtrl",
        ["$scope", "$http", "_", "project", "projectTypes", "user", "hasWriteAccess", "moment", "autofocus", "statusUpdates",
            function ($scope, $http, _, project, projectTypes, user, hasWriteAccess, moment, autofocus, statusUpdates) {
                $scope.project = project;
                $scope.projectTypes = projectTypes;
                $scope.hasWriteAccess = hasWriteAccess;
                $scope.autosaveUrl = `api/itproject/${project.id}`;

                init();

                function init() {
                    $scope.methodOptions = [{ label: 'Samlet', val: true }, { label: 'Tid, kvalitet og ressourcer', val: false }];

                    $scope.allStatusUpdates = statusUpdates;

                    if ($scope.allStatusUpdates.length > 0) {
                        $scope.currentStatusUpdate = $scope.allStatusUpdates[0];
                        $scope.showCombinedChart = ($scope.currentStatusUpdate.IsCombined) ? $scope.methodOptions[0] : $scope.methodOptions[1];
                    }

                    $scope.combinedStatusUpdates = _.filter($scope.allStatusUpdates, function (s: any) { return s.IsCombined; });
                    $scope.splittedStatusUpdates = _.filter($scope.allStatusUpdates, function (s: any) { return !s.IsCombined; });
                }

                $scope.onSelectStatusMethod = function() {
                if ($scope.showCombinedChart.val) {
                    $scope.currentStatusUpdate = ($scope.combinedStatusUpdates.length > 0) ? $scope.combinedStatusUpdates[0] : null;
                } else {
                    $scope.currentStatusUpdate = ($scope.splittedStatusUpdates.length > 0) ? $scope.splittedStatusUpdates[0] : null;
                }
            }
            }]);
})(angular, app);
