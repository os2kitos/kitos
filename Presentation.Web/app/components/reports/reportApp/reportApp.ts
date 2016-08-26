
var reportApp = angular.module('reportApp', [
    'ui.router',
    'ui.bootstrap',
    'ngAnimate',
    'ngSanitize']);

angular.module("reportApp").service("stimulsoftService", Kitos.Services.StimulsoftService);

angular.element(document).ready(() => {
    angular.bootstrap(document, ["reportApp"]);
});

reportApp.run([
    '$rootScope', '$state',
    ($rootScope, $state) => {
        // init info
        $rootScope.page = {
            title: 'Index',
            subnav: []
        };

        $rootScope.$state = $state;
        $state.go("reports-viewer");
    }
]);
