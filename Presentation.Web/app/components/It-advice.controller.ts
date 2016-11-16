(function (ng, app) {
    app.controller('object.EditAdviceCtrl', ['$scope', '$http', '$state', '$stateParams', '$timeout', 'notify',//'advices','roles',
        function ($scope, $http, $state, $stateParams, $timeout, notify) {
            $scope.type = $stateParams.type;
        }]);
})(angular, app);