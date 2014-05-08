(function(ng, app) {
    app.config(['$stateProvider', function($stateProvider) {

        $stateProvider.state('it-project.edit', {
            url: '/edit/{id:[0-9]+}',
            templateUrl: 'partials/it-project/edit-it-project.html',
            controller: 'project.EditCtrl',
            resolve: {
                itProject: ['$http', '$stateParams', function($http, $stateParams) {
                    return $http.get("api/itproject/" + $stateParams.id)
                        .then(function(result) {
                            return result.data.response;
                        });
                }],
                itProjectTypes: ['$http', function ($http) {
                    return $http.get("api/itprojecttype/")
                        .then(function (result) {
                            return result.data.response;
                        });
                }],
                itProjectCategories: ['$http', function ($http) {
                    return $http.get("api/itprojectcategory/")
                        .then(function (result) {
                            return result.data.response;
                        });
                }],
                user: ['userService', function (userService) {
                    return userService.getUser();
                }]
            }
        });
    }]);


    app.controller('project.EditCtrl', ['$scope', '$http', 'notify',
            'itProject', 'itProjectTypes', 'itProjectCategories', 'user',
            function ($scope, $http, notify, itProject, itProjectTypes, itProjectCategories, user) {
                $scope.project = itProject;
                if ($scope.project.associatedProgramId) {
                    $scope.project.associatedProgram = {
                        id: $scope.project.associatedProgramId,
                        text: $scope.project.associatedProgramName
                    };
                }

                $scope.autosaveUrl = "api/itproject/" + itProject.id;

                $scope.itProjectTypes = itProjectTypes;
                $scope.itProjectCategories = itProjectCategories;
                
                //ItProgram type TODO: don't hardcode this?
                $scope.itProgramType = _.findWhere(itProjectTypes, { name: "IT Program" });

                $scope.programSelectOptions = selectLazyLoading('api/itproject?programs&orgId=' + user.currentOrganizationId);
                $scope.programSelectOptions.allowClear = true;
                $scope.programSelectOptions.placeholder = "Vælg program";

                function selectLazyLoading(url) {
                    return {
                        minimumInputLength: 1,
                        initSelection: function (elem, callback) {
                        },
                        ajax: {
                            data: function (term, page) {
                                return { query: term };
                            },
                            quietMillis: 500,
                            transport: function (queryParams) {
                                var res = $http.get(url + '&q=' + queryParams.data.query).then(queryParams.success);
                                res.abort = function () {
                                    return null;
                                };

                                return res;
                            },

                            results: function (data, page) {
                                var results = [];

                                _.each(data.data.response, function (obj) {

                                    results.push({
                                        id: obj.id,
                                        text: obj.name
                                    });
                                });

                                return { results: results };
                            }
                        }
                    };
                }

                $scope.updateAssociatedProgram = function() {

                    var val = null;
                    if ($scope.project.associatedProgram != null) val = $scope.project.associatedProgram.id;

                    var payload = { associatedProgramId: val };

                    $http({
                        method: 'PATCH',
                        url: $scope.autosaveUrl,
                        data: payload
                    }).success(function(result) {
                        notify.addSuccessMessage("Feltet er opdateret");
                    }).error(function(result) {
                        notify.addErrorMessage("Feltet kunne ikke opdateres!");
                    });
                };
            }]);

})(angular, app);