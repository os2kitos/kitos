(function (ng, app) {
    'use strict';

    app.factory("gridStateService", gridStateService);

    gridStateService.$inject = ["$timeout"];

    // this serivce is not really a service but a trick to avoid 
    // repeating logic in controllers that need to persist 
    // the grid state - which is pretty much all of them
    function gridStateService($timeout) {
        var factory = {
            getService: getService
        };
        return factory;

        function getService(storageKey) {
            if (!storageKey)
                throw new Error('Missing parameter: storageKey');

            var profileStorageKey = storageKey + "-profile";
            var service = {
                saveGridOptions     : saveGridOptions,
                loadGridOptions     : loadGridOptions,
                saveGridProfile     : saveGridProfile,
                clearGridProfile    : clearGridProfile,
                clearOptions        : clearOptions
            };
            return service;

            // saves grid state to localStorage
            function saveGridOptions(grid) {
                // timeout fixes columnReorder saves before the column is actually reordered 
                // http://stackoverflow.com/questions/21270748/kendo-grid-saving-state-in-columnreorder-event
                $timeout(function () {
                    var options = grid.getOptions();

                    saveGridStateForever(options);
                    saveGridStateForSession(options);
                });
            }

            // loads kendo grid options from localstorage
            function loadGridOptions(grid) {
                var persistedState = getOptions();
                var gridOptions = _.omit(persistedState, "columnState");
                var columnState = _.pick(persistedState, "columnState");

                _.forEach(columnState.columnState, function (state, key) {
                    var columnIndex = _.findIndex(grid.columns, function (column) {
                        if (!column.hasOwnProperty("persistId"))
                            throw new Error("Unable to find persistId property in grid column with field=" + column.field);

                        return column.persistId == key;
                    });

                    if (columnIndex !== -1) {
                        var columnObj = grid.columns[columnIndex];
                        // reorder column
                        if (state.index != columnIndex) {
                            grid.reorderColumn(state.index, columnObj);
                        }
                        // show / hide column
                        if (state.hidden != columnObj.hidden) {
                            if (state.hidden) {
                                grid.hideColumn(columnObj);
                            } else {
                                grid.showColumn(columnObj);
                            }
                        }
                        // resize column
                        if (state.width != columnObj.width) {
                            // manually set the width on the column option, cause changing the css doesn't
                            columnObj.width = state.width;
                            // $timeout is required here, else the jQuery select doesn't work
                            $timeout(function() {
                                $(".k-grid-content")
                                    .find("colgroup col")
                                    .eq(columnIndex)
                                    .width(state.width);

                                // NOTE make sure that this id actually matches the id in the view
                                $("#mainGrid").find("col").eq(columnIndex).width(state.width);
                            });
                        }
                    }
                });

                grid.setOptions(gridOptions);
            }

            // gets all the saved options, both session and local, and merges
            // them together so that the correct options are overwritten
            function getOptions() {
                // load options from local storage
                var localOptions = localStorage.getItem(storageKey);
                if (localOptions) {
                    localOptions = JSON.parse(localOptions);
                }

                // load options profile from local storage
                var profileOptions = localStorage.getItem(profileStorageKey);
                if (profileOptions) {
                    profileOptions = JSON.parse(profileOptions);
                }

                // load options from session storage
                var sessionOptions = sessionStorage.getItem(storageKey);
                if (sessionOptions) {
                    sessionOptions = JSON.parse(sessionOptions);
                }

                var options;
                if (sessionOptions) {
                    // if session options are set then use them
                    options = _.merge({}, localOptions, sessionOptions);
                } else {
                    // else use the profile options
                    // this should only happen the first time the page loads
                    // or when the session optinos are deleted
                    options = _.merge({}, localOptions, profileOptions);
                }
                return options;
            }

            // save grid options that should be stored in sessionStorage
            function saveGridStateForSession(options) {
                var pickedOptions = {};
                // save filter, sort and page
                pickedOptions.dataSource = _.pick(options.dataSource, ['filter', 'sort', 'page']);
                sessionStorage.setItem(storageKey, JSON.stringify(pickedOptions));
            }

            // save grid options that should be stored in localStorage
            function saveGridStateForever(options) {
                if (options) {
                    var pickedOptions = {};
                    // save pageSize
                    pickedOptions.dataSource = _.pick(options.dataSource, ['pageSize']);

                    // save column state - dont use the kendo function for it as it breaks more than if fixes...
                    pickedOptions.columnState = {};
                    for (var i = 0; i < options.columns.length; i++) {
                        var column = options.columns[i];
                        pickedOptions.columnState[column.persistId] = { index: i, width: column.width, hidden: column.hidden };
                    }

                    localStorage.setItem(storageKey, JSON.stringify(pickedOptions));
                }
            }

            function saveGridProfile(grid) {
                var options = grid.getOptions();
                var pickedOptions = {};
                // save filter and sort
                pickedOptions.dataSource = _.pick(options.dataSource, ['filter', 'sort']);

                localStorage.setItem(profileStorageKey, JSON.stringify(pickedOptions));
            }

            function clearGridProfile() {
                localStorage.removeItem(profileStorageKey);
            }

            // clears grid options
            function clearOptions() {
                localStorage.removeItem(storageKey);
                sessionStorage.removeItem(storageKey);
            }
        }
    }
})(angular, app);
