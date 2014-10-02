(function(ng, app) {
    app.factory('taskService', [
        '$http', '$q', '$rootScope', 'userService', function($http, $q, $rootScope, userService) {
            var baseUrl = 'api/taskRef/';

            function getRoots() {
                var deferred = $q.defer();

                userService.getUser().then(function(user) {

                    var url = baseUrl + '?roots&orgId=' + user.currentOrganizationId + "&take=200";

                    $http.get(url).success(function(result) {

                        deferred.resolve(result.response);

                    }).error(reject("Couldn't load tasks"));


                }, reject("Couldn't acquire user!"));

                function reject(reason) {
                    return function() {
                        deferred.reject(reason);
                    };
                }

                return deferred.promise;
            }

            function getChildren(id) {
                var deferred = $q.defer();

                userService.getUser().then(function(user) {

                    var url = baseUrl + id + '?children&orgId=' + user.currentOrganizationId;

                    $http.get(url).success(function(result) {

                        deferred.resolve(result.response);

                    }).error(reject("Couldn't load tasks"));


                }, reject("Couldn't acquire user!"));

                function reject(reason) {
                    return function() {
                        deferred.reject(reason);
                    };
                }

                return deferred.promise;
            }

            return {
                getRoots: getRoots,
                getChildren: getChildren
            };
        }
    ]);
})(angular, app);
