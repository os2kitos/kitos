module Kitos.Services {
    "use strict";

    interface IGridSavedState {
        dataSource?: kendo.data.DataSourceOptions;
        columnState?: { [persistId: string]: { index: number; width: number, hidden?: boolean } };
    }

    export interface IGridStateFactory {
        getService: (storageKey: string, user: Services.IUser, overviewType?: Models.Generic.OverviewType) => IGridStateService;
    }

    export interface IGridStateService {
        saveGridOptions: (grid: Kitos.IKendoGrid<any>) => void;
        loadGridOptions: (grid: Kitos.IKendoGrid<any>, initialFilter?) => void;
        saveGridProfile: (grid: Kitos.IKendoGrid<any>) => void;
        loadGridProfile: (grid: Kitos.IKendoGrid<any>) => void;
        saveGridProfileForOrg: (grid: Kitos.IKendoGrid<any>, overviewType: Models.Generic.OverviewType) => void;
        doesGridProfileExist: () => boolean;
        removeProfile: () => void;
        removeLocal: () => void;
        removeSession: () => void;
        deleteGridProfileForOrg: (overviewType: Models.Generic.OverviewType) => void;
        doesGridDivergeFromDefault: (overviewType: Models.Generic.OverviewType) => boolean;
        canDeleteGridProfileForOrg: () => boolean;
    }

    gridStateService.$inject = [
        "$window",
        "$timeout",
        "$",
        "JSONfn",
        "_",
        "KendoFilterService",
        "notify",
        "$state"
    ];

    function gridStateService(
        $window: ng.IWindowService,
        $timeout: ng.ITimeoutService,
        $: JQueryStatic,
        JSONfn: JSONfn.JSONfnStatic,
        _: _.LoDashStatic,
        KendoFilterService: KendoFilterService,
        notify,
        $state: ng.ui.IStateService
    ): IGridStateFactory {
        var factory: IGridStateFactory = {
            getService: getService
        };

        return factory;

        function getService(storageKey: string, user: Services.IUser, overviewType?: Models.Generic.OverviewType): IGridStateService {
            if (!storageKey)
                throw new Error("Missing parameter: storageKey");

            storageKey = `${user.id}-${user.currentOrganizationId}-${storageKey}`;
            var profileStorageKey = storageKey + "-profile";
            var orgStorageColumnsKey = storageKey + "-OrgProfile";
            var versionKey = storageKey + "-version";
            // Consider this in: https://os2web.atlassian.net/browse/KITOSUDV-1674. Notice. This is an async method and is not handled as such. Do not make async call in the factory
            getOrgFilterOptions(overviewType);

            var service: IGridStateService = {
                saveGridOptions: saveGridOptions,
                loadGridOptions: loadGridOptions,
                saveGridProfile: saveGridProfile,
                loadGridProfile: loadGridProfile,
                saveGridProfileForOrg: saveGridProfileForOrg,
                doesGridProfileExist: doesGridProfileExist,
                removeProfile: removeProfile,
                removeLocal: removeLocal,
                removeSession: removeSession,
                deleteGridProfileForOrg: deleteGridProfileForOrg,
                doesGridDivergeFromDefault: doesGridDivergeFromDefault,
                canDeleteGridProfileForOrg: canDeleteGridProfileForOrg
            };
            return service;

            function getOrgFilterOptions(overviewType: Models.Generic.OverviewType) {
                // Organizational configuration not yet activated for overview
                if (overviewType === null || overviewType === undefined) {
                    return;
                }

                getGridVersion().then((result) => {
                    if (result !== null && result !== $window.sessionStorage.getItem(versionKey)) {
                        getGridProfileForOrg();
                    } 
                });
            } 

            // saves grid state to localStorage
            function saveGridOptions(grid: Kitos.IKendoGrid<any>) {
                // timeout fixes columnReorder saves before the column is actually reordered
                // http://stackoverflow.com/questions/21270748/kendo-grid-saving-state-in-columnreorder-event
                $timeout(() => {
                    var options = grid.getOptions();

                    saveGridStateForever(options);
                    saveGridStateForSession(options);

                    // Compare the visible columns with the visible columns retrieved from the server.
                    const version = options.columns.filter(x => !x.hidden).map(x => x.persistId).sort().join("");
                    const localVersion = $window.sessionStorage.getItem(versionKey);
                    if (localVersion !== null && version !== localVersion) {
                        $window.sessionStorage.removeItem(versionKey);
                    }
                });
            }

            // loads kendo grid options from localStorage
            function loadGridOptions(grid: Kitos.IKendoGrid<any>, initialFilter?: kendo.data.DataSourceFilters): void {
                var gridId = grid.element[0].id;
                var storedState = getStoredOptions(grid);
                var columnState = <IGridSavedState> _.pick(storedState, "columnState");

                var gridOptionsWithInitialFilter = _.merge({ dataSource: { filter: initialFilter } }, storedState);
                var gridOptions = _.omit(gridOptionsWithInitialFilter, "columnState");

                var visableColumnIndex = 0;
                _.forEach(columnState.columnState, (state, key) => {
                    var columnIndex = _.findIndex(grid.columns, column => {
                        if (!column.hasOwnProperty("persistId")) {
                            console.error(`Unable to find persistId property in grid column with field=${column.field}`);
                            return false;
                        }

                        return column.persistId === key;
                    });

                    if (columnIndex !== -1) {
                        var columnObj = grid.columns[columnIndex];
                        // reorder column
                        if (state.index !== columnIndex) {
                            // check if index is out of bounds
                            if (state.index < grid.columns.length) {
                                grid.reorderColumn(state.index, columnObj);
                            }
                        }

                        // show / hide column
                        if (state.hidden !== columnObj.hidden) {
                            if (state.hidden) {
                                grid.hideColumn(columnObj);
                            } else {
                                grid.showColumn(columnObj);
                            }
                        }

                        if (!columnObj.hidden) {
                            visableColumnIndex++;
                        }

                        // resize column
                        if (state.width !== columnObj.width) {
                            // manually set the width on the column, cause changing the css doesn't update it
                            columnObj.width = state.width;
                            // $timeout is required here, else the jQuery select doesn't work
                            $timeout(() => {
                                // set width of column header
                                $(`#${gridId} .k-grid-header`)
                                    .find("colgroup col")
                                    .eq(visableColumnIndex)
                                    .width(state.width);

                                // set width of column
                                $(`#${gridId} .k-grid-content`)
                                    .find("colgroup col")
                                    .eq(visableColumnIndex)
                                    .width(state.width);
                            });
                        }
                    }
                });

                grid.setOptions(gridOptions);
                grid.dataSource.pageSize(grid.dataSource.options.pageSize);
            }

            // gets all the saved options, both session and local, and merges
            // them together so that the correct options are overwritten
            function getStoredOptions(grid: Kitos.IKendoGrid<any>): IGridSavedState {
                // load options from org storage
                var orgStorageColumns: Models.Generic.IKendoColumnConfigurationDTO[];
                var orgStorageColumnsItem = $window.sessionStorage.getItem(orgStorageColumnsKey);
                if (orgStorageColumnsItem) {
                    orgStorageColumns = <Models.Generic.IKendoColumnConfigurationDTO[]> JSONfn.parse(orgStorageColumnsItem, true);
                }

                // load options from local storage
                var localOptions;
                var localStorageItem = $window.localStorage.getItem(storageKey);
                if (localStorageItem) {
                    localOptions = JSONfn.parse(localStorageItem, true);
                }

                // load options profile from local storage
                var profileOptions;
                var profileStorageItem = $window.localStorage.getItem(profileStorageKey);
                if (profileStorageItem) {
                    profileOptions = JSONfn.parse(profileStorageItem, true);
                }

                // load options from session storage
                var sessionOptions;
                var sessionStorageItem = $window.sessionStorage.getItem(storageKey);
                if (sessionStorageItem) {
                    sessionOptions = JSONfn.parse(sessionStorageItem, true);
                }

                var options: IGridSavedState = { columnState : null };
                

                if (sessionOptions) {
                    // if session options are set then use them
                    // note the order the options are merged in (below) is important!
                    options = <IGridSavedState>_.merge(options, localOptions, sessionOptions);
                }
                else if (profileOptions) {
                    // else use the profile options
                    // this should only happen the first time the page loads
                    // or when the session optinos are deleted
                    // note the order the options are merged in (below) is important!
                    options = <IGridSavedState> _.merge({}, localOptions, profileOptions);
                }

                if ($window.sessionStorage.getItem(versionKey) !== null) {
                    // Session updates has not changed the grid as updates to the grid which changes the columns causes the version to be deleted
                    // So we use the local organization configuration if it exists
                    if (orgStorageColumns) {
                        var columns: { [persistId: string]: { index: number; width: number, hidden?: boolean } } = {};

                        // Hide all columns to begin with
                        grid.columns.forEach(x => {
                            x.hidden = true;
                        });

                        // The visible columns from the server are then made visible 
                        orgStorageColumns.forEach(x => {
                            var column = grid.columns.filter(y => y.persistId === x.persistId);
                            if (column.length === 1) { // If this value is not 1 the column doesn't seem to exist and therefore we don't make it visible.
                                columns[x.persistId] = { index: x.index, width: column[0].width as number, hidden: false };
                            }
                        });
                        options.columnState = columns;
                    }
                }

                return options;
            }

            // save grid options that should be stored in sessionStorage
            function saveGridStateForSession(options: Kitos.IKendoGridOptions<any>): void {
                var pickedOptions: IGridSavedState = {};
                // save filter, sort and page
                pickedOptions.dataSource = <kendo.data.DataSourceOptions> _.pick(options.dataSource, ["filter", "sort", "page"]);
                $window.sessionStorage.setItem(storageKey, JSONfn.stringify(pickedOptions));
            }

            // save grid options that should be stored in localStorage
            function saveGridStateForever(options: Kitos.IKendoGridOptions<any>): void {
                if (options) {
                    var pickedOptions: IGridSavedState = {};
                    // save pageSize
                    pickedOptions.dataSource = <kendo.data.DataSourceOptions> _.pick(options.dataSource, ["pageSize"]);

                    // save column state - dont use the kendo function for it as it breaks more than it fixes...
                    pickedOptions.columnState = {};
                    for (var i = 0; i < options.columns.length; i++) {
                        var column = options.columns[i];
                        pickedOptions.columnState[column.persistId] = { index: i, width: <number> column.width, hidden: column.hidden };
                    }

                    $window.localStorage.setItem(storageKey, JSONfn.stringify(pickedOptions));
                }
            }

            function saveGridProfile(grid: Kitos.IKendoGrid<any>): void {
                var options = grid.getOptions();
                var pickedOptions: IGridSavedState = {};
                // save filter and sort
                pickedOptions.dataSource = <kendo.data.DataSourceOptions> _.pick(options.dataSource, ["filter", "sort"]);

                $window.localStorage.setItem(profileStorageKey, JSONfn.stringify(pickedOptions));
            }

            function getGridVersion(): ng.IPromise<string> { 
                return KendoFilterService
                    .getConfigurationVersion(user.currentOrganizationId, overviewType)
                    .then((result) => {
                        if (result.status === 200) {
                            return result.data.response;
                        }
                        return null;
                    },
                        () => null);
            }

            function getGridProfileForOrg() {
                KendoFilterService
                    .getConfigurationFromOrg(user.currentOrganizationId, overviewType)
                    .then((result) => {
                        if (result.status === 200) {
                            const version = result.data.response.version;
                            const localVersion = $window.sessionStorage.getItem(versionKey);
                            if (version !== localVersion) {
                                const columns = result.data.response.visibleColumns;
                                $window.sessionStorage.setItem(orgStorageColumnsKey, JSONfn.stringify(columns));
                                $window.sessionStorage.setItem(versionKey, version);
                            }
                        }
                    })
                    .catch((result) => {
                        if (result.status === 404) {
                            // Make sure there is no data as we can't find an organizational configuration for the kendo grid.
                            $window.sessionStorage.removeItem(orgStorageColumnsKey);
                        }
                    });
            }

            function saveGridProfileForOrg(grid: Kitos.IKendoGrid<any>, overviewType: Models.Generic.OverviewType): void {
                var options = grid.getOptions();

                var columns: Models.Generic.IKendoColumnConfigurationDTO[] = [];
                for (var i = 0; i < options.columns.length; i++) {
                    var column = options.columns[i];
                    if (column.hidden)
                        continue;
                    columns.push({ persistId: column.persistId, index: i });
                }

                KendoFilterService.postConfigurationFromOrg(user.currentOrganizationId, overviewType, columns)
                    .then((res) => {
                        if (res.status === 200) {
                            notify.addSuccessMessage("Filtre og sortering gemt for organisationen");
                            getGridProfileForOrg(); // Load the newly saved grid
                        }
                    })
                    .catch((res) => {
                        notify.addErrorMessage("Der opstod en fejl i forsøget på at gemme det nye filter");
                    });
            }

            function deleteGridProfileForOrg(overviewType: Models.Generic.OverviewType) {
                KendoFilterService.deleteConfigurationFromOrg(user.currentOrganizationId, overviewType)
                    .then((res) => {
                        if (res.status === 200) {
                            notify.addSuccessMessage("Organisationens gemte filtre og sorteringer er slettet");
                            removeSession();
                            $state.go(".", null, { reload: true });
                        }
                    })
                    .catch((res) => {
                        notify.addErrorMessage("Der opstod en fejl i forsøget på at slette det gemte filter");
                    });
            }

            function loadGridProfile(grid: Kitos.IKendoGrid<any>): void {
                removeSession();
                var storedState = getStoredOptions(grid);
                var gridOptions = _.omit(storedState, "columnState");
                grid.setOptions(gridOptions);
            }

            function doesGridProfileExist(): boolean {
                if ($window.localStorage.getItem(profileStorageKey))
                    return true;
                return false;
            }

            function removeSession(): void {
                $window.sessionStorage.removeItem(storageKey);
                $window.sessionStorage.removeItem(orgStorageColumnsKey);
                $window.sessionStorage.removeItem(versionKey);
            }

            function removeLocal(): void {
                $window.localStorage.removeItem(storageKey);
            }

            function removeProfile(): void {
                $window.localStorage.removeItem(profileStorageKey);
            }

            function doesGridDivergeFromDefault(overviewType: Models.Generic.OverviewType): boolean {
                if (overviewType === null || overviewType === undefined) {
                    return false; // No defaults defined for this overview type
                }

                if ($window.sessionStorage.getItem(orgStorageColumnsKey) === null) {
                    return false;
                }

                if ($window.sessionStorage.getItem(versionKey) === null) {
                    return true;
                }

                return false;
            }

            function canDeleteGridProfileForOrg() {
                return $window.sessionStorage.getItem(orgStorageColumnsKey) !== null;
            }
        }
    }

    angular.module("app").factory("gridStateService", gridStateService);
}
