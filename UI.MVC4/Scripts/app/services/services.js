(function (ng, app) {
    app.factory("httpBusyInterceptor", ['$q', function ($q) {
        var isBusy = false;

        return {
            'request': function (config) {
                var qConfig = $q.when(config);

                return qConfig.then(function(cfg) {
                    if (cfg.method !== 'GET') {
                        if (cfg.noResubmit) {
                            if (isBusy) {
                                qConfig.reject("Busy");
                            } else {
                                isBusy = true;
                                
                            }
                        }
                    }

                    return cfg;
                });
                
                
            },
            
            'response': function (response) {
                isBusy = false;

                return response || $q.when(response);
            }
        };
    }]);

})(angular, app);