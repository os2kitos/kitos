(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-contract.edit.economy', {
            url: '/economy',
            templateUrl: 'partials/it-contract/tab-economy.html',
            controller: 'contract.EditEconomyCtrl',
            resolve: {
            }
        });
    }]);

    app.controller('contract.EditEconomyCtrl', ['$scope', '$http', 'notify', 'contract',
        function ($scope, $http, notify, contract) {

            var baseUrl = "api/economyStream";

            var externEconomyStreams = [];
            $scope.externEconomyStreams = externEconomyStreams;
            
            var internEconomyStreams = [];
            $scope.internEconomyStreams = internEconomyStreams;

            _.each(contract.externEconomyStreams, function(stream) {
                pushStream(stream, externEconomyStreams);
            });

            
            function pushStream(stream, collection) {
                stream.show = true;
                stream.updateUrl = baseUrl + "/" + stream.id;


                stream.delete = function () {
                    var msg = notify.addInfoMessage("Sletter række...");

                    $http.delete(this.updateUrl).success(function() {
                        this.show = false;

                        msg.toSuccessMessage("Rækken er slettet!");
                    }).errro(function() {
                        msg.toErrorMessage("Fejl! Kunne ikke slette rækken!");
                    });
                };

                stream.patch = function(field, value) {
                    var data = {};
                    data[field] = value;

                    $http({
                        method: 'PATCH',
                        url: this.updateUrl,
                        data: data
                    }).success(function(result) {
                        notify.addSuccessMessage("Feltet er opdateret!");
                    }).error(function(result) {
                        notify.addErrorMessage("Fejl! Feltet blev ikke opdateret!");
                    });
                };

                stream.updateStatus = function () {
                    patch("auditStatus", stream.auditStatus);
                };

                stream.updateDate = function() {
                    patch("auditDate", stream.auditDate);
                };

                collection.push(stream);
            }
            
            function postNewStream(isExternStream) {
                
                var field = isExternStream ? "ExternPaymentForId" : "InternPaymentForId";
                
                var stream = {};
                stream[field] = contract.id;

                var msg = notify.addInfoMessage("Tilføjer ny række...");
                $http.post(baseUrl, stream).success(function(result) {
                    msg.toSuccessMessage("Rækken er tilføjet!");

                    //push result to view
                    var collection = isExternStream ? externEconomyStreams : internEconomyStreams;
                    pushStream(result.response, collection);

                }).error(function() {
                    msg.toErrorMessage("Fejl! Kunne ikke tilføje række");
                });
            }

            $scope.newExtern = function() {
                postNewStream(true);
            };
            



        }]);


})(angular, app);