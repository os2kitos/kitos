(function (ng, app) {

    var subnav = [
            { state: "org-view", text: "Organisation" }
    ];


    app.config(['$stateProvider', '$urlRouterProvider', function ($stateProvider, $urlRouterProvider) {

        $stateProvider.state('org-view', {
            url: '/organization',
            templateUrl: 'partials/org/org.html',
            controller: 'org.OrgViewCtrl'
        });

    }]);

    app.controller('org.OrgViewCtrl', ['$rootScope', '$scope', '$http', 'growl', function ($rootScope, $scope, $http, growl) {
        $rootScope.page.title = 'Organisation';
        $rootScope.page.subnav = subnav;

        var munId = $rootScope.user.municipality;

        $http.get("api/organizationunit?Organization=" + munId).success(function(result) {
            $scope.node = result.Response;
        });

    }]);

})(angular, app);