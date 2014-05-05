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
                itProject: ['$http', '$stateParams', function($http, $stateParams) {
                    return $http.get("api/itproject/" + $stateParams.id)
                        .then(function(result) {
                            return result.data.response;
                        });
                }],
                itProjectTypes: ['$http', function ($http) {
                    return $http.get("api/itprojecttype/")
                        .then(function (result) {
                            return result.data.response;
                        });
                }],
                itProjectCategories: ['$http', function ($http) {
                    return $http.get("api/itprojectcategory/")
                        .then(function (result) {
                            return result.data.response;
                        });
                }]
            }
        });

    }]);


    app.controller('project.EditCtrl',
        ['$rootScope', '$scope', 'itProject', 'itProjectTypes', 'itProjectCategories',
            function ($rootScope, $scope, itProject, itProjectTypes, itProjectCategories){
                $rootScope.page.title = 'IT Projekt';
                $rootScope.page.subnav = subnav;

                $scope.project = itProject;
                $scope.autosaveUrl = "api/itproject/" + itProject.id;

                $scope.itProjectTypes = itProjectTypes;
                $scope.itProjectCategories = itProjectCategories;
                
                //ItProgram type TODO: don't hardcode this?
                $scope.itProgramType = _.findWhere(itProjectTypes, { name: "IT Program" }); 


            }]);

})(angular, app);