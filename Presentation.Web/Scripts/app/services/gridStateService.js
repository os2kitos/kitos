(function (ng, app) {
    app.factory("gridStateService", gridStateService);
    
    function gridStateService() {
        var service = {
            save: saveGridOptions,
            get: getGridOptions,
            clear: clearOptions
        };
        return service;

        // saves grid state
        function saveGridOptions(localStorageKey, sessionStorageKey, options) {
            if (options) {
                var pickedLocalOptions = _.pick(options, 'columns');
                pickedLocalOptions.dataSource = _.pick(options.dataSource, ['pageSize']);

                localStorage[localStorageKey] = kendo.stringify(pickedLocalOptions);

                var pickedSessionOptions = {};
                pickedSessionOptions.dataSource = _.pick(options.dataSource, ['filter', 'sort', 'page']);

                sessionStorage[sessionStorageKey] = kendo.stringify(pickedSessionOptions);
            };
        }

        // loads kendo grid options from storages
        function getGridOptions(localStorageKey, sessionStorageKey) {
            // load options from session storage
            var sessionOptions = sessionStorage[sessionStorageKey];
            if (sessionOptions) {
                sessionOptions = JSON.parse(sessionOptions);
            }

            // load options from local storage
            var localOptions = localStorage[localStorageKey];
            if (localOptions) {
                localOptions = JSON.parse(localOptions);
            }

            // merge them
            var options = _.merge({}, sessionOptions, localOptions);
            return options;
        }

        // clears grid filters by removing the StorageItems
        function clearOptions(localStorageKey, sessionStorageKey) {
            localStorage.removeItem(localStorageKey);
            sessionStorage.removeItem(sessionStorageKey);
        }
    }
})(angular, app);
