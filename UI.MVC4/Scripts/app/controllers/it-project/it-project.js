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
                }],
                hasWriteAccess: ['$http', '$stateParams', function ($http, $stateParams) {
                    return $http.get("api/itproject/" + $stateParams.id + "?hasWriteAccess")
                        .then(function (result) {
                            return result.data.response;
                        });
                }],
            }
        });
    }]);


    app.controller('project.EditCtrl', ['$scope', '$http', 'notify',
            'itProject', 'itProjectTypes', 'itProjectCategories', 'user', 'hasWriteAccess',
            function ($scope, $http, notify, itProject, itProjectTypes, itProjectCategories, user, hasWriteAccess) {
                $scope.project = itProject;
                if ($scope.project.associatedProgramId) {
                    $scope.project.associatedProgram = {
                        id: $scope.project.associatedProgramId,
                        text: $scope.project.associatedProgramName
                    };
                }

                $scope.selectSettings = { dynamicTitle: false, defaultText: 'Tabs' };
                $scope.selectedData = [];
                if (itProject.isStatusGoalVisible) 
                    $scope.selectedData.push({ id: 1 });
                if (itProject.isEconomyVisible)
                    $scope.selectedData.push({ id: 2 });
                if (itProject.isStakeholderVisible)
                    $scope.selectedData.push({ id: 3 });
                if (itProject.isCommunicationVisible)
                    $scope.selectedData.push({ id: 4 });
                if (itProject.isHandoverVisible)
                    $scope.selectedData.push({ id: 5 });
                $scope.dropdownData = [
                    {id: 1, label: 'Vis Status: Mål'},
                    {id: 2, label: 'Vis Økonomi'},
                    {id: 3, label: 'Vis Interessanter'},
                    {id: 4, label: 'Vis Kommunikation'},
                    {id: 5, label: 'Vis Overlevering'}
                ];
                // TODO refactor this garbage!
                $scope.$watch('selectedData', function (newValue, oldValue) {
                    var payload = {};
                    if (newValue.length > oldValue.length) {
                        // something was added
                        var addIds = _.difference(_.pluck(newValue, 'id'), _.pluck(oldValue, 'id'));
                        _.each(addIds, function(id) {
                            switch (id) {
                            case 1:
                                payload.isStatusGoalVisible = true;
                                break;
                            case 2:
                                payload.isEconomyVisible = true;
                                break;
                            case 3:
                                payload.isStakeholderVisible = true;
                                break;
                            case 4:
                                payload.isCommunicationVisible = true;
                                break;
                            case 5:
                                payload.isHandoverVisible = true;
                                break;
                            }
                        });
                    } else if (newValue.length < oldValue.length) {
                        // something was removed
                        var removedIds = _.difference(_.pluck(oldValue, 'id'), _.pluck(newValue, 'id'));
                        _.each(removedIds, function(id) {
                            switch (id) {
                                case 1:
                                    payload.isStatusGoalVisible = false;
                                    break;
                                case 2:
                                    payload.isEconomyVisible = false;
                                    break;
                                case 3:
                                    payload.isStakeholderVisible = false;
                                    break;
                                case 4:
                                    payload.isCommunicationVisible = false;
                                    break;
                                case 5:
                                    payload.isHandoverVisible = false;
                                    break;
                            }
                        });
                    }
                    if (_.size(payload) > 0) {
                        $http({ method: 'PATCH', url: $scope.autosaveUrl, data: payload }).success(function (result) {
                            $scope.project.isStatusGoalVisible = result.response.isStatusGoalVisible;
                            $scope.project.isEconomyVisible = result.response.isEconomyVisible;
                            $scope.project.isStakeholderVisible = result.response.isStakeholderVisible;
                            $scope.project.isCommunicationVisible = result.response.isCommunicationVisible;
                            $scope.project.isHandoverVisible = result.response.isHandoverVisible;
                        });
                    }
                }, true);

                $scope.hasWriteAccess = hasWriteAccess;

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