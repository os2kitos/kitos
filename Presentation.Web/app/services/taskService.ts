(function (ng, app) {
    app.factory('taskService', [
        '$http', '$q', function ($http, $q) {
            var baseUrl = 'api/taskRef/';

            function getRoots() {
                var deferred = $q.defer();

                var url = baseUrl + '?roots=true&take=200';

                $http.get(url).then(function onSuccess(result) {

                    deferred.resolve(result.data.response);

                }, function onError(result) {
                    reject("Couldn't load tasks");
                });

                function reject(reason) {
                    return function () {
                        deferred.reject(reason);
                    };
                }

                return deferred.promise;
            }

            function getChildren(id) {
                var deferred = $q.defer();

                var url = baseUrl + id + '?children=true';

                $http.get(url).then(function onSuccess(result) {

                    deferred.resolve(result.data.response);

                }, function onError(result) {
                    reject("Couldn't load tasks");
                });

                function reject(reason) {
                    return function () {
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
