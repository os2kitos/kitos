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
                programs: ['$http', 'userService', function ($http, userService) {
                    return userService.getUser().then(function(user) {
                        var orgId = user.currentOrganizationId;
                        return $http.get('api/itproject?orgId=' + orgId).then(function (result) {
                            return result.data.response;
                        });
                    });
                }]
            }
        });
    }]);

    app.controller('project.CatalogCtrl',
        ['$scope', '$http', '$state', '$stateParams', '$timeout', 'user', 'programs', function ($scope, $http, $state, $stateParams, $timeout, user, programs) {
            $scope.programs = programs;

            function isSelected(project) {
                //a project is selected if created inside the current organisation
                return project.organizationId == user.currentOrganizationId;
            };

            $scope.isSelected = isSelected;
                
            
            $scope.toggle = function (project) {
                if (!isSelected(project)) {
                    $http.post('api/itproject/' + project.id, { organizationId: user.currentOrganizationId }).finally(reload);
                } else {
                    $http.delete('api/itproject/' + project.id).finally(reload);
                }
                
            };

            // work around for $state.reload() not updating scope
            // https://github.com/angular-ui/ui-router/issues/582
            function reload() {
                return $state.transitionTo($state.current, $stateParams, {
                    reload: true
                }).then(function () {
                    $scope.hideContent = true;
                    return $timeout(function () {
                        return $scope.hideContent = false;
                    }, 1);
                });
            };

            $scope.create = function () {
                var orgUnitId = user.defaultOrganizationUnitId;
                var payload = {
                    itProjectTypeId: 1,
                    ItProjectCategoryId: 1,
                    responsibleOrgUnitId: orgUnitId,
                    organizationId: user.currentOrganizationId,
                };
                $http.post('api/itproject', payload)
                    .success(function (result) {
                        var projectId = result.response.id;
                        if (orgUnitId) {
                            // add users default org unit to the new project
                            $http.post('api/itproject/' + projectId + '?organizationunit=' + orgUnitId);
                        }
                        $state.go('it-project.edit', { id: projectId });
                    })
                    .error(function() {
                        
                    });
            };
        }]
    );
})(angular, app);