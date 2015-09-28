(function(ng, app) {
    app.config(['$stateProvider', function($stateProvider) {
        $stateProvider.state('it-project.edit', {
            url: '/edit/{id:[0-9]+}',
            templateUrl: 'partials/it-project/edit-it-project.html',
            controller: 'project.EditCtrl',
            resolve: {
                project: ['$http', '$stateParams', function($http, $stateParams) {
                    return $http.get("api/itproject/" + $stateParams.id)
                        .then(function(result) {
                            return result.data.response;
                        });
                }],
                projectTypes: ['$http', function ($http) {
                    return $http.get("api/itprojecttype/")
                        .then(function (result) {
                            return result.data.response;
                        });
                }],
                user: ['userService', function (userService) {
                    return userService.getUser();
                }],
                hasWriteAccess: ['$http', '$stateParams', 'user', function ($http, $stateParams, user) {
                    return $http.get("api/itproject/" + $stateParams.id + "?hasWriteAccess&organizationId=" + user.currentOrganizationId)
                        .then(function (result) {
                            return result.data.response;
                        });
                }],
            }
        });
    }]);

    app.controller('project.EditCtrl', ['$scope', '$http', 'notify',
            'project', 'projectTypes', 'user', 'hasWriteAccess', 'autofocus',
            function ($scope, $http, notify, project, projectTypes, user, hasWriteAccess, autofocus) {
                autofocus();

                $scope.project = project;
                $scope.projectTypes = projectTypes;

                if (!_.find(projectTypes, function(type) { return type.id == project.itProjectTypeId; })) {
                    $scope.projectTypes.unshift({ id: project.itProjectTypeId, name: project.itProjectTypeName });
                }

                if ($scope.project.parentId) {
                    $scope.project.parent = {
                        id: $scope.project.parentId,
                        text: $scope.project.parentName
                    };
                }

                $scope.allowClearOption = {
                    allowClear: true
                };

                $scope.selectSettings = { dynamicTitle: false, buttonClasses: 'btn btn-default btn-sm' };
                $scope.selectTranslation = {
                    checkAll: 'Vis alle',
                    uncheckAll: 'Skjul alle',
                    buttonDefaultText: 'Faner '
                };
                $scope.selectedData = [];
                if (project.isStatusGoalVisible)
                    $scope.selectedData.push({ id: 1 });
                if (project.isStrategyVisible)
                    $scope.selectedData.push({ id: 2 });
                if (project.isHierarchyVisible)
                    $scope.selectedData.push({ id: 3 });
                if (project.isEconomyVisible)
                    $scope.selectedData.push({ id: 4 });
                if (project.isStakeholderVisible)
                    $scope.selectedData.push({ id: 5 });
                if (project.isRiskVisible)
                    $scope.selectedData.push({ id: 6 });
                if (project.isCommunicationVisible)
                    $scope.selectedData.push({ id: 7 });
                if (project.isHandoverVisible)
                    $scope.selectedData.push({ id: 8 });
                $scope.dropdownData = [
                    {id: 1, label: 'Vis Status: Mål'},
                    { id: 2, label: 'Vis Strategi' },
                    { id: 3, label: 'Vis Hierarki' },
                    { id: 4, label: 'Vis Økonomi'},
                    { id: 5, label: 'Vis Interessenter' },
                    { id: 6, label: 'Vis Risiko' },
                    { id: 7, label: 'Vis Kommunikation'},
                    { id: 8, label: 'Vis Overlevering'}
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
                                payload.isStrategyVisible = true;
                                break;
                            case 3:
                                payload.isHierarchyVisible = true;
                                break;
                            case 4:
                                payload.isEconomyVisible = true;
                                break;
                            case 5:
                                payload.isStakeholderVisible = true;
                                break;
                            case 6:
                                payload.isRiskVisible = true;
                                break;
                            case 7:
                                payload.isCommunicationVisible = true;
                                break;
                            case 8:
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
                                    payload.isStrategyVisible = false;
                                    break;
                                case 3:
                                    payload.isHierarchyVisible = false;
                                    break;
                                case 4:
                                    payload.isEconomyVisible = false;
                                    break;
                                case 5:
                                    payload.isStakeholderVisible = false;
                                    break;
                                case 6:
                                    payload.isRiskVisible = false;
                                    break;
                                case 7:
                                    payload.isCommunicationVisible = false;
                                    break;
                                case 8:
                                    payload.isHandoverVisible = false;
                                    break;
                            }
                        });
                    }
                    if (_.size(payload) > 0) {
                        $http({ method: 'PATCH', url: $scope.autosaveUrl + '?organizationId=' + user.currentOrganizationId, data: payload }).success(function (result) {
                            $scope.project.isStatusGoalVisible = result.response.isStatusGoalVisible;
                            $scope.project.isStrategyVisible = result.response.isStrategyVisible;
                            $scope.project.isHierarchyVisible = result.response.isHierarchyVisible;
                            $scope.project.isEconomyVisible = result.response.isEconomyVisible;
                            $scope.project.isStakeholderVisible = result.response.isStakeholderVisible;
                            $scope.project.isRiskVisible = result.response.isRiskVisible;
                            $scope.project.isCommunicationVisible = result.response.isCommunicationVisible;
                            $scope.project.isHandoverVisible = result.response.isHandoverVisible;
                        });
                    }
                }, true);

                $scope.hasWriteAccess = hasWriteAccess;
                $scope.autosaveUrl = "api/itproject/" + project.id;

                $scope.parentSelectOptions = selectLazyLoading('api/itproject', true, ['overview', 'orgId=' + user.currentOrganizationId]);

                function selectLazyLoading(url, excludeSelf, paramAry) {
                    return {
                        minimumInputLength: 1,
                        allowClear: true,
                        placeholder: ' ',
                        initSelection: function (elem, callback) {
                        },
                        ajax: {
                            data: function (term, page) {
                                return { query: term };
                            },
                            quietMillis: 500,
                            transport: function (queryParams) {
                                var extraParams = paramAry ? '&' + paramAry.join('&') : '';
                                var res = $http.get(url + '?q=' + queryParams.data.query + extraParams).then(queryParams.success);
                                res.abort = function () {
                                    return null;
                                };

                                return res;
                            },

                            results: function (data, page) {
                                var results = [];

                                _.each(data.data.response, function (obj) {
                                    if (excludeSelf && obj.id == project.id)
                                        return; // don't add self to result

                                    results.push({
                                        id: obj.id,
                                        text: obj.name ? obj.name : 'Unavngiven',
                                        cvr: obj.cvr
                                    });
                                });

                                return { results: results };
                            }
                        }
                    };
                }
            }]);
})(angular, app);
