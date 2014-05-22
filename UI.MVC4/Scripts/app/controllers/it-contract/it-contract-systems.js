(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-contract.edit.systems', {
            url: '/systems',
            templateUrl: 'partials/it-contract/tab-systems.html',
            controller: 'contract.EditSystemsCtrl',
            resolve: {
                user: ['userService', function (userService) {
                    return userService.getUser();
                }]
            }
        });
    }]);

    app.controller('contract.EditSystemsCtrl', ['$scope', '$http', 'notify', 'user', 'contract',
        function ($scope, $http, notify, user, contract) {
            
            function formatAssociatedSystems(associatedSystemUsages) {
                
                //helper functions
                function deleteAssociatedSystem(associatedSystem) {
                    return $http.delete("api/itContract/" + contract.id + "?systemUsageId=" + associatedSystem.id);
                }
                
                function postAssociatedSystem(associatedSystem) {
                    return $http.post("api/itContract/" + contract.id + "?systemUsageId=" + associatedSystem.selectedSystem.id);
                }

                //for each row of associated system
                _.each(associatedSystemUsages, function(systemUsage) {

                    systemUsage.show = true;

                    //for select2
                    systemUsage.selectedSystem = {
                        id: systemUsage.id,
                        text: systemUsage.itSystemName
                    };

                    //update the row
                    systemUsage.update = function () {
                        //first delete the old binding
                        deleteAssociatedSystem(systemUsage).success(function () {
                            //then create new binding
                            postAssociatedSystem(systemUsage).success(function (result) {

                                notify.addSuccessMessage("Rækken er opdateret.");
                                //then reformat and redraw the rows
                                formatAssociatedSystems(result.response);
                                
                            }).error(function (result) {
                                
                                //couldn't add new binding
                                notify.addErrorMessage("Fejl! Kunne ikke opdatere rækken!");
                                systemUsage.show = false;
                                
                            });
                        }).error(function () {
                            //couldn't delete old binding
                            notify.addErrorMessage("Kunne ikke opdatere rækken!");
                        });
                    };


                    //delete the row
                    systemUsage.delete = function() {
                        deleteAssociatedSystem(systemUsage).success(function() {
                            notify.addSuccessMessage("Rækken er slettet.");
                            systemUsage.show = false;
                        }).error(function() {
                            notify.addErrorMessage("Kunne ikke slette rækken");
                        });
                    };
                    
                });

                $scope.associatedSystemUsages = associatedSystemUsages;


                $scope.newAssociatedSystemUsage = {
                    save: function() {
                        //post new binding
                        postAssociatedSystem($scope.newAssociatedSystemUsage).success(function(result) {

                            notify.addSuccessMessage("Rækken er tilføjet.");

                            //then reformat and redraw the rows
                            formatAssociatedSystems(result.response);

                        }).error(function(result) {

                            //couldn't add new binding
                            notify.addErrorMessage("Fejl! Kunne ikke tilføje rækken!");

                        });
                    }
                };
            }

            formatAssociatedSystems(contract.associatedSystemUsages);

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
                                text: usage.itSystem.name
                            });
                        });

                        return { results: results };
                    }
                }
            };
            

        }]);
})(angular, app);