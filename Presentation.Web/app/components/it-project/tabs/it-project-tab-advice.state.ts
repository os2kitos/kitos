(function(ng, app) {
    app.config(['$stateProvider', function($stateProvider) {
        $stateProvider.state('it-project.edit.advice-generic', {
            url: '/advice',
            templateUrl: 'app/components/it-advice/it-advice.view.html',
            controller: 'object.EditAdviceCtrl',
            controllerAs: 'Vm',
            resolve: {
                Roles: ["localOptionServiceFactory", (localOptionServiceFactory: Kitos.Services.LocalOptions.ILocalOptionServiceFactory) =>
                    localOptionServiceFactory.create(Kitos.Services.LocalOptions.LocalOptionType.ItProjectRoles).getAll()],
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
