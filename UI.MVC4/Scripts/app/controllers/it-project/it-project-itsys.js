(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-project.edit.itsys', {
            url: '/itsys',
            templateUrl: 'partials/it-project/tab-itsys.html',
            controller: 'project.EditItsysCtrl',
            resolve: {
                // re-resolve data from parent cause changes here wont cascade to it
                project: ['$http', '$stateParams', function ($http, $stateParams) {
                    return $http.get("api/itproject/" + $stateParams.id)
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

    app.controller('project.EditItsysCtrl',
    ['$scope', '$http', '$stateParams', 'user', 'notify', 'project',
        function ($scope, $http, $stateParams, user, notify, project) {
            $scope.systemUsages = project.itSystems;

            $scope.save = function () {
                $http.post('api/itproject/' + project.id + '?usageId=' + $scope.selectedSystemUsage.id)
                    .success(function () {
                        notify.addSuccessMessage("Systemet er tilknyttet.");
                    })
                    .error(function () {
                        notify.addErrorMessage("Fejl! Kunne ikke tilknytte systemet!");
                    });
            };

            $scope.delete = function(usageId) {
                $http.delete('api/itproject/' + project.id + '?usageId=' + usageId)
                    .success(function() {
                        notify.addSuccessMessage("Systemets tilknyttning er fjernet.");
                    })
                    .error(function() {
                        notify.addErrorMessage("Fejl! Kunne ikke fjerne systemets tilknyttning!");
                    });
            };

            //select2 options for looking up it system usages
            $scope.itSystemUsagesSelectOptions = {
                minimumInputLength: 1,
                initSelection: function (elem, callback) {
                },
                ajax: {
                    data: function (term, page) {
                        return { query: term };
                    },
                    quietMillis: 500,
                    transport: function (queryParams) {
                        var res = $http.get('api/itSystemUsage?organizationId=' + user.currentOrganizationId + '&q=' + queryParams.data.query).then(queryParams.success);
                        res.abort = function () {
                            return null;
                        };

                        return res;
                    },

                    results: function (data, page) {
                        var results = [];

                        //for each system usages
                        _.each(data.data.response, function (usage) {

                            results.push({
                                //the id of the system usage is the id, that is selected
                                id: usage.id,
                                //but the name of the system is the label for the select2
                                text: usage.itSystem.name,
                                //saving the usage for later use
                                usage: usage
                            });
                        });

                        return { results: results };
                    }
                }
            };
        }]);
})(angular, app);
