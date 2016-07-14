(function (ng, app) {
    app.factory("httpBusyInterceptor", ['$q', '$rootScope', function ($q, $rootScope) {
        var isBusy = false;

        function makeBusy() {
            isBusy = true;
            waitCursor();

            $rootScope.$broadcast("httpBusy");
        }

        function clearBusy() {
            if (isBusy) {
                isBusy = false;
                normalCursor();

                $rootScope.$broadcast("httpUnbusy");
            }
        }

        return {
            'request': function (config) {
                var qConfig = $q.when(config);

                return qConfig.then(function(cfg) {
                    if (cfg.method !== 'GET') {
                        if (cfg.handleBusy) {
                            if (isBusy) {
                                return $q.reject("BUSY");
                            } else {
                                makeBusy();
                            }
                        }
                    }

                    return cfg;
                });


            },

            'response': function (response) {
                clearBusy();

                return response || $q.when(response);
            },

            'responseError': function (rejection) {
                clearBusy();

                return $q.reject(rejection);
            }
        };
    }]);
})(angular, app);
