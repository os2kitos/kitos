(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-contract.edit.economy', {
            url: '/economy',
            templateUrl: 'app/components/it-contract/tabs/it-contract-tab-economy.view.html',
            controller: 'contract.EditEconomyCtrl',
            resolve: {
                orgUnits: ['$http', 'userService', function ($http, userService) {
                    return userService.getUser().then(function (user) {
                        return $http.get('api/organizationunit/?organizationid=' + user.currentOrganizationId).then(function (result) {
                            return result.data.response;
                        });
                    });
                }]
            }
        });
    }]);

    app.controller('contract.EditEconomyCtrl', ['$scope', '$http', '$timeout', '$state', '$stateParams', 'notify', 'contract', 'orgUnits', 'user',
        function ($scope, $http, $timeout, $state, $stateParams, notify, contract, orgUnits: { ean; }[], user) {
            $scope.datepickerOptions = {
                format: "dd-MM-yyyy",
                parseFormats: ["yyyy-MM-dd"]
            };

            var baseUrl = "api/economyStream";

            var externEconomyStreams = [];
            $scope.externEconomyStreams = externEconomyStreams;
            _.each(contract.externEconomyStreams, function (stream) {
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

                    $http.delete(this.updateUrl + '?organizationId=' + user.currentOrganizationId).success(function () {
                        stream.show = false;

                        msg.toSuccessMessage("Rækken er slettet!");
                    }).error(function () {
                        msg.toErrorMessage("Fejl! Kunne ikke slette rækken!");
                    }).finally(reload);
                };

                function updateEan() {
                    stream.ean = " - ";

                    if (stream.organizationUnitId) {
                        var orgUnit: { ean } = _.find(orgUnits, { id: parseInt(stream.organizationUnitId) });

                        if (orgUnit && orgUnit.ean) stream.ean = orgUnit.ean;
                    }
                };
                stream.updateEan = updateEan;

                updateEan();
                collection.push(stream);
            }

            function postNewStream(field) {

                var stream = {};
                stream[field] = contract.id;

                var msg = notify.addInfoMessage("Tilføjer ny række...");
                $http.post(baseUrl, stream).success(function (result) {
                    msg.toSuccessMessage("Rækken er tilføjet!");

                    //push result to view
                    //pushStream(result.response, collection);
                }).error(function () {
                    msg.toErrorMessage("Fejl! Kunne ikke tilføje række");
                }).finally(reload);
            }

            $scope.newExtern = function () {
                postNewStream("ExternPaymentForId");
            };
            $scope.newIntern = function () {
                postNewStream("InternPaymentForId");
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
        }]);
})(angular, app);