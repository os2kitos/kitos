(function(ng, app) {
    app.config(['$stateProvider', function($stateProvider) {
        $stateProvider.state('it-project.edit.advice-generic', {
            url: '/advice',
            templateUrl: 'app/components/it-advice/it-advice.view.html',
            controller: 'object.EditAdviceCtrl',
            controllerAs: 'Vm',
            resolve: {
                Roles: ['$http', function ($http) {
                    return $http.get("odata/LocalItProjectRoles?$filter=IsLocallyAvailable eq true or IsObligatory&$orderby=Priority desc")
                        .then(function (result) {
                            return result.data.value;
                        });
                }],
                advices: ['$http', '$stateParams', function ($http, $stateParams) {
                    return $http.get('api/itProject/' + $stateParams.id).then(function (result) {
                        return result.data.response.advices;
                        });
                }],
                object: ['project', function (project) {
                    return project;
                }],
                type: [function () {
                    return "itProject";
                }],
                advicename: [
                    '$http', '$stateParams', function ($http, $stateParams) {
                        return $http.get('api/itProject/' + $stateParams.id).then(function (result) {
                            return result.data.response;
                        });
                    }
                ]
            }
        });
    }]);
})(angular, app);
