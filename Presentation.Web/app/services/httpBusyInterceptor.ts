((ng, app) => {
    app.factory("httpBusyInterceptor", ["$q", "$rootScope", ($q: ng.IQService, $rootScope: ng.IRootScopeService) => {
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

        var service: Kitos.IHttpInterceptorWithCustomConfig = {
            request(config) {
                const qConfig = $q.when(config);

                return qConfig.then((cfg) => {
                    if (cfg.method !== "GET") {
                        if (cfg["handleBusy"]) {
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
            response(response) {
                clearBusy();

                return response || $q.when(response);
            },
            responseError(rejection) {
                clearBusy();

                return $q.reject(rejection);
            }
        };
        return service;
    }]);
})(angular, app);
