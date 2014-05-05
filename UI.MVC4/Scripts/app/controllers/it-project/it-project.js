(function(ng, app) {

    var subnav = [
            { state: 'edit-it-project', text: 'IT Projekt' }
    ];

    app.config(['$stateProvider', '$urlRouterProvider', function($stateProvider, $urlRouterProvider) {

        $stateProvider.state('edit-it-project', {
            url: '/project/edit/{id:[0-9]+}',
            templateUrl: 'partials/it-project/edit-it-project.html',
            controller: 'project.EditCtrl',
            resolve: {
                //itProject: ['$http', '$stateParams', function($http, $stateParams) {
                //    return $http.get("api/itproject/" + $stateParams.id)
                //        .then(function(result) {
                //            return result.data.response;
                //        });
                //}]
            }
        });

    }]);


    app.controller('project.EditCtrl',
        ['$rootScope', '$scope',
            function ($rootScope, $scope){
                $rootScope.page.title = 'IT Projekt';
                $rootScope.page.subnav = subnav;



            }]);

})(angular, app);