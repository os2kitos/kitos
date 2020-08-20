(function (ng, app) {
    app.config(["$stateProvider", function ($stateProvider) {
        $stateProvider.state("it-project.edit.itsys", {
            url: "/itsys",
            templateUrl: "app/components/it-project/tabs/it-project-tab-itsys.view.html",
            controller: "project.EditItsysCtrl",
            resolve: {
                user: ["userService", function (userService) {
                    return userService.getUser();
                }],
                usages: ["$http", "$stateParams", function ($http, $stateParams) {
                    return $http.get("api/itproject/" + $stateParams.id + "?usages=true")
                        .then(function (result) {
                            return result.data.response;
                        });
                }]
            }
        });
    }]);

    app.controller("project.EditItsysCtrl",
    ["$scope", "$http", "$timeout", "$state", "$stateParams", "user", "notify", "usages",
        function ($scope, $http, $timeout, $state, $stateParams, user, notify, usages) {
            var projectId = $stateParams.id;
            $scope.systemUsages = usages;
            $scope.formatSystemName = Kitos.Helpers.SystemNameFormat.apply;
            $scope.save = function () {
                $http.post("api/itproject/" + projectId + "?usageId=" + $scope.selectedSystemUsage.id + "&organizationId=" + user.currentOrganizationId)
                    .success(function () {
                        notify.addSuccessMessage("Systemet er tilknyttet.");
                        reload();
                    })
                    .error(function () {
                        notify.addErrorMessage("Fejl! Kunne ikke tilknytte systemet!");
                    });
            };

            $scope.delete = function(usageId) {
                $http.delete("api/itproject/" + projectId + "?usageId=" + usageId + "&organizationId=" + user.currentOrganizationId)
                    .success(function() {
                        notify.addSuccessMessage("Systemets tilknyttning er fjernet.");
                        reload();
                    })
                    .error(function() {
                        notify.addErrorMessage("Fejl! Kunne ikke fjerne systemets tilknyttning!");
                    });
            };

            function reload() {
                $state.go(".", null, { reload: true });
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
                        var res = $http.get("api/itSystemUsage?organizationId=" + user.currentOrganizationId + "&q=" + queryParams.data.query + "&take=25").then(queryParams.success);
                        res.abort = function () {
                            return null;
                        };

                        return res;
                    },

                    results: function (data, page) {
                        var results = [];

                        //for each system usages
                        _.each(data.data.response, function (usage: { id; itSystemName; itSystemDisabled;}) {
                            if (!usage.itSystemDisabled) {
                                results.push({
                                    //the id of the system usage is the id, that is selected
                                    id: usage.id,
                                    //but the name of the system is the label for the select2
                                    text: usage.itSystemName,
                                    //saving the usage for later use
                                    usage: usage
                                });
                            }
                        });

                        return { results: results };
                    }
                }
            };
        }]);
})(angular, app);
