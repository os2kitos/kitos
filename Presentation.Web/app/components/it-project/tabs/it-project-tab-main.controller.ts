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
                ]
            }
        });
    }]);

    app.controller("project.EditMainCtrl",
        ["$scope", "$http", "_", "project", "projectTypes", "user", "hasWriteAccess", "autofocus",
            function ($scope, $http, _, project, projectTypes, user, hasWriteAccess, autofocus) {
                $scope.project = project;
                $scope.projectTypes = projectTypes;
                $scope.hasWriteAccess = hasWriteAccess;
                $scope.autosaveUrl = `api/itproject/${project.id}`;
            }]);
})(angular, app);
