(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-contract.edit.economy', {
            url: '/economy',
            templateUrl: 'partials/it-contract/tab-economy.html',
            controller: 'contract.EditEconomyCtrl',
            resolve: {
                orgUnits: ['$http', 'userService', function ($http, userService) {
                    return userService.getUser().then(function (user) {
                        return $http.get('api/organizationunit?organizationId=' + user.currentOrganizationId).then(function (result) {
                            return result.data.response;
                        });
                    });
                }]
            }
        });
    }]);

    app.controller('contract.EditEconomyCtrl', ['$scope', '$http', 'notify', 'contract', 'orgUnits',
        function ($scope, $http, notify, contract, orgUnits) {
            
            var baseUrl = "api/economyStream";

            var externEconomyStreams = [];
            $scope.externEconomyStreams = externEconomyStreams;
            _.each(contract.externEconomyStreams, function(stream) {
                pushStream(stream, externEconomyStreams);
            });
            
            var internEconomyStreams = [];
            $scope.internEconomyStreams = internEconomyStreams;
            _.each(contract.internEconomyStreams, function (stream) {
                pushStream(stream, internEconomyStreams);
            });

            
            function pushStream(stream, collection) {
                stream.show = true;
                stream.updateUrl = baseUrl + "/" + stream.id;
                
                stream.delete = function () {
                    var msg = notify.addInfoMessage("Sletter række...");

                    $http.delete(this.updateUrl).success(function() {
                        stream.show = false;

                        msg.toSuccessMessage("Rækken er slettet!");
                    }).error(function() {
                        msg.toErrorMessage("Fejl! Kunne ikke slette rækken!");
                    });
                };

                function updateEan() {
                    stream.ean = " - ";

                    if (stream.organizationUnitId) {
                        var orgUnit = _.findWhere(orgUnits, { id: parseInt(stream.organizationUnitId) });
                        
                        if (orgUnit && orgUnit.ean) stream.ean = orgUnit.ean;
                    }
                };
                stream.updateEan = updateEan;

                updateEan();
                collection.push(stream);
            }
            
            function postNewStream(field, collection) {
                
                var stream = {};
                stream[field] = contract.id;

                var msg = notify.addInfoMessage("Tilføjer ny række...");
                $http.post(baseUrl, stream).success(function(result) {
                    msg.toSuccessMessage("Rækken er tilføjet!");

                    //push result to view
                    pushStream(result.response, collection);

                }).error(function() {
                    msg.toErrorMessage("Fejl! Kunne ikke tilføje række");
                });
            }

            $scope.newExtern = function() {
                postNewStream("ExternPaymentForId", externEconomyStreams);
            };
            $scope.newIntern = function () {
                postNewStream("InternPaymentForId", internEconomyStreams);
            };
            



        }]);


})(angular, app);