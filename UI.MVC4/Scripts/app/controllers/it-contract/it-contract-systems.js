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


            function formatAssociatedInterfaces(associatedInterfaceUsages, associatedInterfaceExposures) {
                
                function patchContractId(associatedInterface, contractId) {
                    console.log(associatedInterface);
                    return $http({
                        method: 'PATCH',
                        url: associatedInterface.baseUrl + associatedInterface.id,
                        data: {
                            itContractId: contractId
                        }
                    });
                }

                $scope.associatedInterfaces = [];
                function pushAssociatedInterface(associatedInterface) {

                    associatedInterface.show = true;
                    

                    associatedInterface.delete = function () {

                        patchContractId(associatedInterface, null).success(function () {
                            notify.addSuccessMessage("Kontrakten er ikke længere tilknyttet snitfladerelationen");
                            associatedInterface.show = false;
                        }).error(function () {
                            notify.addErrorMessage("Fejl! Kontrakten blev ikke fjernet fra snitfladerelationen");
                        });
                    };

                    $scope.associatedInterfaces.push(associatedInterface);
                }
                
                function pushAssociatedUsage(associatedUsage) {
                    associatedUsage.relation = "Usage";
                    associatedUsage.baseUrl = "api/interfaceUsage/";
                    pushAssociatedInterface(associatedUsage);
                }

                function pushAssociatedExposure(associatedExposure) {
                    associatedExposure.relation = "Exposure";
                    associatedExposure.baseUrl = "api/interfaceExposure/";
                    pushAssociatedInterface(associatedExposure);
                }

                _.each(associatedInterfaceExposures, pushAssociatedExposure);
                _.each(associatedInterfaceUsages, pushAssociatedUsage);


                function initNewRow() {
                    var newInterface = {};
                    $scope.newAssociatedInterface = newInterface;

                    //this is called when the system select or the relation select is changed
                    newInterface.loadInterfaceList = function() {

                        //if not both are set to something valid, use default values
                        if (!newInterface.selectedSystem || !newInterface.relation) {

                            newInterface.usagesOrExposures = [];
                            newInterface.placeholder = "Vælg først IT system og relation";

                            return;
                        }

                        //if a system and a relation have been selected, the user can now find an interface

                        //whether the user is looking for an exposed interface
                        //or a used interface depends on the relation field
                        if (newInterface.relation == "Exposure") {
                            newInterface.usagesOrExposures = _.where(newInterface.selectedSystem.usage.interfaceExposures, { itContractId: null });
                            newInterface.placeholder = "Vælg udstilte snitflade";

                        } else if (newInterface.relation == "Usage") {
                            newInterface.usagesOrExposures = _.where(newInterface.selectedSystem.usage.interfaceUsages, { itContractId: null });
                            newInterface.placeholder = "Vælg anvendte snitflade";
                        }
                    };

                    //initial setup
                    newInterface.loadInterfaceList();

                    //save function for the new row
                    newInterface.save = function() {
                        if (!newInterface.interfaceId) return;

                        newInterface.baseUrl = newInterface.relation == "Exposure" ? "api/interfaceExposure/" : "api/interfaceUsage/";
                        newInterface.id = newInterface.interfaceId;

                        patchContractId(newInterface, contract.id).success(function(result) {
                            notify.addSuccessMessage("Kontrakten er knyttet til snitfladerelationen");

                            if (newInterface.relation == "Exposure") pushAssociatedExposure(result.response);
                            else pushAssociatedUsage(result.response);

                            initNewRow();
                            
                        }).error(function() {
                            notify.addErrorMessage("Fejl! Kontrakten blev ikke knyttet til snitfladerelationen");
                        });

                    };
                }

                initNewRow();
            }

            formatAssociatedInterfaces(contract.associatedInterfaceUsages, contract.associatedInterfaceExposures);
            

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