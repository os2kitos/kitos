(function (ng, app) {
    app.factory("gridOptionsService", gridOptionsService);
    
    function gridOptionsService() {
        var service = {
            save: saveGridOptions,
            get: getGridOptions,
            clear: clearOptions
        };
        return service;

        // saves grid state
        function saveGridOptions(localStorageKey, sessionStorageKey, options) {
            if (!localStorageKey) 
                throw new Error('Missing parameter: localStorageKey');

            if (!sessionStorageKey)
                throw new Error('Missing parameter: sessionStorageKey');
            
            if (options) {
                var pickedLocalOptions = {};
                pickedLocalOptions.dataSource = _.pick(options.dataSource, ['pageSize']);
                
                pickedLocalOptions.columnState = {};
                for (var i = 0; i < options.columns.length; i++) {
                    var column = options.columns[i];
                    pickedLocalOptions.columnState[column.persistId] = { index: i, width: column.width, hidden: column.hidden };
                }
                localStorage.setItem(localStorageKey, JSON.stringify(pickedLocalOptions));

                var pickedSessionOptions = {};
                pickedSessionOptions.dataSource = _.pick(options.dataSource, ['filter', 'sort', 'page']);

                sessionStorage.setItem(sessionStorageKey, JSON.stringify(pickedSessionOptions));
            };
        }

        // loads kendo grid options from storages
        function getGridOptions(localStorageKey, sessionStorageKey) {
            if (!localStorageKey)
                throw new Error('Missing parameter: localStorageKey');

            if (!sessionStorageKey)
                throw new Error('Missing parameter: sessionStorageKey');

            // load options from session storage
            var sessionOptions = sessionStorage.getItem(sessionStorageKey);
            if (sessionOptions) {
                sessionOptions = JSON.parse(sessionOptions);
            }

            // load options from local storage
            var localOptions = localStorage.getItem(localStorageKey);
            if (localOptions) {
                localOptions = JSON.parse(localOptions);
            }
            
            // merge them
            var options = _.merge({}, sessionOptions, localOptions);
            return options;
        }

        // clears grid filters by removing the StorageItems
        function clearOptions(localStorageKey, sessionStorageKey) {
            if (!localStorageKey)
                throw new Error('Missing parameter: localStorageKey');

            if (!sessionStorageKey)
                throw new Error('Missing parameter: sessionStorageKey');

            localStorage.removeItem(localStorageKey);
            sessionStorage.removeItem(sessionStorageKey);
        }
    }
})(angular, app);
