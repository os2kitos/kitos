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

    app.controller('contract.EditSystemsCtrl', ['$scope', '$http', 'notify', 'user',
        function ($scope, $http, notify, user) {
            




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