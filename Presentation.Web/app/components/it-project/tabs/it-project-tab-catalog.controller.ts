(function (ng, app) {
    app.config(["$stateProvider", function ($stateProvider) {
        $stateProvider.state("it-project.catalog", {
            url: "/catalog",
            templateUrl: "app/components/it-project/tabs/it-project-tab-catalog.view.html",
            controller: "project.CatalogCtrl",
            resolve: {
                user: ["userService", function(userService) {
                    return userService.getUser();
                }]
            }
        });
    }]);

    app.controller("project.CatalogCtrl",
        ["$scope", "$http", "$state", "$stateParams", "$timeout", "notify", "user",
            function ($scope, $http, $state, $stateParams, $timeout, notify, user) {

                $scope.pagination = {
                    search: "",
                    skip: 0,
                    take: 20
                };

                $scope.$watchCollection("pagination", function() {
                    var url = "api/itProject?csvcat&orgId=" + user.currentOrganizationId;

                    url += "&skip=" + $scope.pagination.skip;
                    url += "&take=" + $scope.pagination.take;

                    if ($scope.pagination.orderBy) {
                        url += "&orderBy=" + $scope.pagination.orderBy;
                        if ($scope.pagination.descending) url += "&descending=" + $scope.pagination.descending;
                    }

                    if ($scope.pagination.search) {
                        url += "&q=" + $scope.pagination.search;
                    } else {
                        url += "&q=";
                    }

                    $scope.csvUrl = url;
                    loadProjects();
                });

                function loadProjects() {
                    var url = "api/itProject?catalog&orgId=" + user.currentOrganizationId;

                    url += "&skip=" + $scope.pagination.skip;
                    url += "&take=" + $scope.pagination.take;

                    if ($scope.pagination.orderBy) {
                        url += "&orderBy=" + $scope.pagination.orderBy;
                        if ($scope.pagination.descending) url += "&descending=" + $scope.pagination.descending;
                    }

                    if ($scope.pagination.search) {
                        url += "&q=" + $scope.pagination.search;
                    } else {
                        url += "&q=";
                    }

                    $http.get(url).success(function(result, status, headers) {

                        var paginationHeader = JSON.parse(headers("X-Pagination"));
                        $scope.totalCount = paginationHeader.TotalCount;

                        $scope.projects = [];
                        _.each(result.response, pushProject);

                    }).error(function() {
                        notify.addErrorMessage("Kunne ikke hente projekter!");
                    });

                }

                //adds a project to the list of projects
                function pushProject(project) {

                    $scope.projects.push(project);

                    project.baseUrl = "api/itproject/" + project.id;
                    project.show = true;

                    $http.get(project.baseUrl + "?hasWriteAccess").success(function (result) {
                        project.hasWriteAccess = result.response;
                    });

                    //clone the project
                    project.clone = function () {
                        var url = project.baseUrl + "?clone";
                        var payload = { organizationId: user.currentOrganizationId };

                        var msg = notify.addInfoMessage("Kloner projekt...", false);
                        $http.post(url, payload).success(function (result) {
                            msg.toSuccessMessage("Projektet er klonet!");

                            loadProjects();
                        }).error(function () {
                            msg.toErrorMessage("Fejl! Kunne ikke klone projektet!");
                        });
                    };

                    //delete the project
                    project.delete = function() {
                        var msg = notify.addInfoMessage("Sletter projekt...", false);
                        $http.delete(project.baseUrl + "?organizationId=" + user.currentOrganizationId).success(function (result) {
                            project.show = false;

                            msg.toSuccessMessage("Projektet er slettet!");
                        }).error(function() {
                            msg.toErrorMessage("Fejl! Kunne ikke slette projektet!");
                        });
                    };
                }
        }]
    );
})(angular, app);
