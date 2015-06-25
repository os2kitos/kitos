(function (ng, app) {
    'use strict';

    app.factory("gridStateService", gridStateService);

    gridStateService.$inject = ["$timeout", "gridOptionsService"];

    function gridStateService($timeout, gridOptionsService) {
        var factory = {
            getService: getService
        };
        return factory;

        function getService(localStorageKey, sessionStorageKey) {
            var service = {
                saveGridOptions : saveGridOptions,
                loadGridOptions : loadGridOptions,
                clearOptions    : clearOptions
            };
            return service;

            // saves grid state to localStorage
            function saveGridOptions(grid) {
                // timeout fixes columnReorder saves before the column is actually reordered 
                // http://stackoverflow.com/questions/21270748/kendo-grid-saving-state-in-columnreorder-event
                $timeout(function () {
                    var options = grid.getOptions();
                    gridOptionsService.save(localStorageKey, sessionStorageKey, options);
                });
            }

            // loads kendo grid options from localstorage
            function loadGridOptions(grid) {
                var persistedState = gridOptionsService.get(localStorageKey, sessionStorageKey);
                var gridOptions = _.omit(persistedState, "columnState");
                var columnState = _.pick(persistedState, "columnState");

                _.forEach(columnState.columnState, function (state, key) {
                    var columnIndex = _.findIndex(grid.columns, function (column) {
                        return column.persistId == key;
                    });

                    if (columnIndex === -1) {
                        throw new Error("Unable to find persistId='" + key + "' in grid columns.");
                    }

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
                        $timeout(function () {
                            $(".k-grid-content")
                                .find("colgroup col")
                                .eq(columnIndex)
                                .width(state.width);

                            // NOTE make sure that this id actually matches the id in the view
                            $("#mainGrid").find("col").eq(columnIndex).width(state.width);
                        });
                    }
                });

                grid.setOptions(gridOptions);
            }

            // clears grid options
            function clearOptions() {
                gridOptionsService.clear(localStorageKey, sessionStorageKey);
            }
        }
    }
})(angular, app);
