(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-project.catalog', {
            url: '/catalog',
            templateUrl: 'partials/it-project/catalog.html',
            controller: 'project.CatalogCtrl',
            resolve: {
                user: ['userService', function(userService) {
                    return userService.getUser();
                }],
                projects: ['$http', 'userService', function ($http, userService) {
                    return userService.getUser().then(function(user) {
                        var orgId = user.currentOrganizationId;
                        return $http.get('api/itproject?catalog&orgId=' + orgId).then(function (result) {
                            return result.data.response;
                        });
                    });
                }]
            }
        });
    }]);

    app.controller('project.CatalogCtrl',
        ['$scope', '$http', '$state', '$stateParams', '$timeout', 'notify', 'user', 'projects',
            function ($scope, $http, $state, $stateParams, $timeout, notify, user, projects) {
                
                $scope.pagination = {
                    skip: 0,
                    take: 20
                };

                $scope.$watchCollection('pagination', loadProjects);
                
                function loadProjects() {
                    var url = 'api/itProject?catalog&orgId=' + user.currentOrganizationId;

                    url += '&skip=' + $scope.pagination.skip;
                    url += '&take=' + $scope.pagination.take;

                    $scope.projects = [];
                    
                    $http.get(url).success(function(result, status, headers) {

                        var paginationHeader = JSON.parse(headers('X-Pagination'));
                        $scope.pagination.count = paginationHeader.TotalCount;
                        
                        _.each(result.response, pushProject);
                        
                    }).error(function() {
                        notify.addErrorMessage("Kunne ikke hente projekter!");
                    });

                }

                //adds a project to the list of projects
                function pushProject(project) {

                    $scope.projects.push(project);

                    project.baseUrl = 'api/itproject/' + project.id;
                    project.show = true;
                    
                    $http.get(project.baseUrl + "?hasWriteAccess").success(function (result) {
                        project.hasWriteAccess = result.response;
                    });
                    
                    //clone the project
                    project.clone = function () {
                        var url = project.baseUrl + '?clone';
                        var payload = { organizationId: user.currentOrganizationId };
                        
                        var msg = notify.addInfoMessage("Kloner projekt...", false);
                        $http.post(url, payload).success(function (result) {
                            msg.toSuccessMessage("Projektet er klonet!");

                            //push the new project
                            pushProject(result.response);
                        }).error(function () {
                            msg.toErrorMessage("Fejl! Kunne ikke klone projektet!");
                        });
                    };

                    //delete the project
                    project.delete = function() {
                        var msg = notify.addInfoMessage("Sletter projekt...", false);
                        $http.delete(project.baseUrl).success(function(result) {
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