((ng, app) => {
    app.config(["$stateProvider", $stateProvider => {
        $stateProvider.state("it-project.edit.itsys", {
            url: "/itsys",
            templateUrl: "app/shared/select-it-systems/generic-tab-it-systems.view.html",
            controller: "project.EditItsysCtrl",
            resolve: {
                user: ["userService", userService => userService.getUser()],
                usages: ["$http", "$stateParams", ($http, $stateParams) => $http.get(`api/itproject/${$stateParams.id}?usages=true`)
                    .then(result => result.data.response)]
            }
        });
    }]);

    app.controller("project.EditItsysCtrl",
        ["$scope", "$http", "$state", "$stateParams", "user", "notify", "usages", "hasWriteAccess","project",
            ($scope, $http, $state, $stateParams, user, notify, usages, hasWriteAccess, project) => {
                var projectId = $stateParams.id;
                var save = () => {
                    $http.post(`api/itproject/${projectId}?usageId=${$scope.itSysAssignmentVm.selectedSystemUsage.id}&organizationId=${user.currentOrganizationId}`)
                        .success(() => {
                            notify.addSuccessMessage("Systemet er tilknyttet.");
                            reload();
                        })
                        .error(() => {
                            notify.addErrorMessage("Fejl! Kunne ikke tilknytte systemet!");
                        });
                };

                var deleteFunc = usageId => {
                    $http.delete(`api/itproject/${projectId}?usageId=${usageId}&organizationId=${user.currentOrganizationId}`)
                        .success(() => {
                            notify.addSuccessMessage("Systemets tilknyttning er fjernet.");
                            reload();
                        })
                        .error(() => { 
                            notify.addErrorMessage("Fejl! Kunne ikke fjerne systemets tilknyttning!");
                        });
                };

                function reload() {
                    $state.go(".", null, { reload: true });
                };

                //select2 options for looking up it system usages
                var itSystemUsagesSelectOptions = {
                    minimumInputLength: 1,
                    initSelection(elem, callback) {
                    },
                    ajax: {
                        data(term, page) {
                            return { query: term };
                        },
                        quietMillis: 500,
                        transport(queryParams) {
                            var res = $http.get("api/itSystemUsage?organizationId=" + user.currentOrganizationId + "&q=" + queryParams.data.query + "&take=25").then(queryParams.success);
                            res.abort = () => null;

                            return res;
                        },

                        results(data, page) {
                            var results = [];

                            //for each system usages
                            _.each(data.data.response, (usage: { id; itSystemName; itSystemDisabled;}) => {
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

                $scope.itSysAssignmentVm = {
                    ownerName: project.name,
                    hasWriteAccess: hasWriteAccess,
                    overviewHeader: "Projektet vedrører følgende IT Systemer",
                    systemUsages: usages,
                    formatSystemName: Kitos.Helpers.SystemNameFormat.apply,
                    delete: deleteFunc,
                    save: save,
                    itSystemUsagesSelectOptions: itSystemUsagesSelectOptions,
                    selectedSystemUsage: null
                }
            }]);
})(angular, app);
