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
        saveGridOrganizationalConfiguration: (grid: Kitos.IKendoGrid<any>, overviewType: Models.Generic.OverviewType) => void;
        doesGridProfileExist: () => boolean;
        removeProfile: () => void;
        removeLocal: () => void;
        removeSession: () => void;
        deleteGridOrganizationalConfiguration: (overviewType: Models.Generic.OverviewType) => void;
        doesGridDivergeFromOrganizationalConfiguration: (overviewType: Models.Generic.OverviewType, grid: Kitos.IKendoGrid<any>) => boolean;
        canDeleteGridOrganizationalConfiguration: () => boolean;
    }

    gridStateService.$inject = [
        "$window",
        "$timeout",
        "$",
        "JSONfn",
        "_",
        "KendoFilterService",
        "notify",
        "$state",
        "sha256"
    ];

    function gridStateService(
        $window: ng.IWindowService,
        $timeout: ng.ITimeoutService,
        $: JQueryStatic,
        JSONfn: JSONfn.JSONfnStatic,
        _: _.LoDashStatic,
        KendoFilterService: KendoFilterService,
        notify,
        $state: ng.ui.IStateService,
        sha256: Hash
    ): IGridStateFactory {
        var factory: IGridStateFactory = {
            getService: getService
        };

        return factory;

        function getService(storageKey: string, user: Services.IUser, overviewType?: Models.Generic.OverviewType): IGridStateService {
            if (!storageKey)
                throw new Error("Missing parameter: storageKey");

            storageKey = `${user.id}-${user.currentOrganizationId}-${storageKey}`;
            const locallyChangedKey = storageKey + "-changed";
            var profileStorageKey = storageKey + "-profile";
            var organizationalConfigurationColumnsKey = storageKey + "-OrgProfile";
            var organizationalConfigurationVersionKey = storageKey + "-version";

            var badOrganizationalConfigExists = false;

            var service: IGridStateService = {
                saveGridOptions: saveGridOptions,
                loadGridOptions: loadGridOptions,
                saveGridProfile: saveGridProfile,
                loadGridProfile: loadGridProfile,
                saveGridOrganizationalConfiguration: saveGridOrganizationalConfiguration,
                doesGridProfileExist: doesGridProfileExist,
                removeProfile: removeProfile,
                removeLocal: removeLocal,
                removeSession: removeSession,
                deleteGridOrganizationalConfiguration: deleteGridOrganizationalConfiguration,
                doesGridDivergeFromOrganizationalConfiguration: doesGridDivergeFromOrganizationalConfiguration,
                canDeleteGridOrganizationalConfiguration: canDeleteGridOrganizationalConfiguration
            };
            return service;

            async function getOrgFilterOptions(overviewType: Models.Generic.OverviewType) {
                // Organizational configuration not yet activated for overview
                if (overviewType === null || overviewType === undefined) {
                    return null;
                }

                if ($window.localStorage.getItem(locallyChangedKey)) {
                    // User has at some point made local changes so we don't override these until the user resets to default settings
                    return null;
                }

                return getGridVersion().then((result) => {
                    if (result !== null && result !== $window.localStorage.getItem(organizationalConfigurationVersionKey)) {
                        return getGridOrganizationalConfiguration();
                    }
                    return null;
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
                    
                    // Compare the visible columns with the visible columns retrieved from the server. The sort is made case insensitive as the backend ignores case.
                    
                });
            }

            // loads kendo grid options from localStorage
            function loadGridOptions(grid: Kitos.IKendoGrid<any>, initialFilter?: kendo.data.DataSourceFilters): void {
                // Wait for the data from the server before updating the grid.
                // Update the grid regardless of the callback from the server response as we always need to load the grid.
                getOrgFilterOptions(overviewType).then(
                    () => {
                        loadGrid(grid, initialFilter);
                    },
                    () => {
                        loadGrid(grid, initialFilter);
                    });
            }

            // gets all the saved options, both session and local, and merges
            // them together so that the correct options are overwritten
            function getStoredOptions(grid: Kitos.IKendoGrid<any>): IGridSavedState {
                // load options from org storage
                var orgStorageColumns: Models.Generic.IKendoColumnConfigurationDTO[];
                var orgStorageColumnsItem = $window.localStorage.getItem(organizationalConfigurationColumnsKey);
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
                else if (localOptions) {
                    options = <IGridSavedState> localOptions;
                }

                if ($window.localStorage.getItem(locallyChangedKey) !== "true") {
                    // Session updates has not changed the grid as updates to the grid which changes the columns causes the version to be deleted
                    // So we use the local organization configuration if it exists
                    if (orgStorageColumns) {
                        var columns: { [persistId: string]: { index: number; width: number, hidden?: boolean } } = {};

                        var gridColumnWidths: { [persistId: string]: { width: number, originallyHidden: boolean } } = {};

                        // We need to iterate over all columns to hide them. During the iteration we also store the column widths and original hidden value in a map
                        grid.columns.forEach(x => {
                            gridColumnWidths[x.persistId] = { width: x.width as number, originallyHidden: x.hidden };
                            x.hidden = true;
                        });

                        // Keep track of how many columns are being set
                        var columnsBeingSet = 0;

                        // The visible columns from the server are then made visible 
                        orgStorageColumns.forEach(x => {
                            var gridWidth = gridColumnWidths[x.persistId];
                            if (gridWidth !== undefined && gridWidth !== null) {
                                columns[x.persistId] = { index: x.index, width: gridWidth.width, hidden: false };
                                columnsBeingSet ++;
                            }
                        });
                        if (columnsBeingSet === 0) {
                            badOrganizationalConfigExists = true;
                            removeOrgConfig(); // Remove the saved data from the server as the grid don't have any similar fields and is therefore invalid.
                            // We have to make the original columns visible again in order to not break the grid
                            grid.columns.forEach(x => {
                                x.hidden = gridColumnWidths[x.persistId].originallyHidden;
                            });
                        } else {
                            badOrganizationalConfigExists = false;
                            options.columnState = columns;
                        }
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
                    if (!isOrgConfigServerVersionEqualToLocalGrid(options)) {
                        $window.localStorage.setItem(locallyChangedKey, "true");
                    }
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
                        () => {
                            removeOrgConfig(); // If we can't get a version from the server we remove the locally saved data as it will be invalid.
                            return null;
                        });
            }

            function getGridOrganizationalConfiguration() {
                return KendoFilterService
                    .getConfigurationFromOrg(user.currentOrganizationId, overviewType)
                    .then((result) => {
                        if (result.status === 200) {
                            const version = result.data.response.version;
                            const localVersion = $window.localStorage.getItem(organizationalConfigurationVersionKey);
                            if (localVersion !== version) {
                                const columns = result.data.response.visibleColumns;
                                $window.localStorage.setItem(organizationalConfigurationColumnsKey, JSONfn.stringify(columns));
                                $window.localStorage.setItem(organizationalConfigurationVersionKey, version);
                            }
                        }
                    })
                    .catch((result) => {
                        if (result.status === 404) {
                            // Make sure there is no data as we can't find an organizational configuration for the kendo grid.
                            removeOrgConfig();
                        }
                    });
            }

            function saveGridOrganizationalConfiguration(grid: Kitos.IKendoGrid<any>, overviewType: Models.Generic.OverviewType): void {
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
                            getGridOrganizationalConfiguration(); // Load the newly saved grid
                        }
                    })
                    .catch((res) => {
                        notify.addErrorMessage("Der opstod en fejl i forsøget på at gemme det nye filter");
                    });
            }

            function deleteGridOrganizationalConfiguration(overviewType: Models.Generic.OverviewType) {
                KendoFilterService.deleteConfigurationFromOrg(user.currentOrganizationId, overviewType)
                    .then((res) => {
                        if (res.status === 200) {
                            notify.addSuccessMessage("Organisationens gemte filtre og sorteringer er slettet");
                            removeSession();
                            removeLocal();
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
            }

            function removeLocal(): void {
                $window.localStorage.removeItem(storageKey);
                $window.localStorage.removeItem(locallyChangedKey);
                removeOrgConfig();
            }

            function removeProfile(): void {
                $window.localStorage.removeItem(profileStorageKey);
            }

            function removeOrgConfig(): void {
                $window.localStorage.removeItem(organizationalConfigurationColumnsKey);
                $window.localStorage.removeItem(organizationalConfigurationVersionKey);
            }

            function doesGridDivergeFromOrganizationalConfiguration(overviewType: Models.Generic.OverviewType, grid: Kitos.IKendoGrid<any>): boolean {
                if (overviewType === null || overviewType === undefined) {
                    return false; // No defaults defined for unknown overview type
                }

                if ($window.localStorage.getItem(organizationalConfigurationColumnsKey) === null) {
                    return false; 
                }

                if (grid === undefined || grid === null) { // The grid is not always initialized the first time this code is called.
                    return false;
                }

                var options = grid.getOptions();
                return !isOrgConfigServerVersionEqualToLocalGrid(options);
            }

            function isOrgConfigServerVersionEqualToLocalGrid(options: Kitos.IKendoGridOptions<any>) {
                const localVersion = computeGridVersion(options);
                const organizationalConfigurationVersion = $window.localStorage.getItem(organizationalConfigurationVersionKey);
                if (organizationalConfigurationVersion !== null && localVersion !== organizationalConfigurationVersion) {
                    return false;
                }
                return true;
            }

            function computeGridVersion(options: Kitos.IKendoGridOptions<any>) {
                return sha256(options.columns.filter(x => !x.hidden).map(x => x.persistId).sort((a, b) => a.toLowerCase().localeCompare(b.toLowerCase())).join(""));
            }

            function canDeleteGridOrganizationalConfiguration() {
                if (badOrganizationalConfigExists) {
                    return true;
                }
                return $window.localStorage.getItem(organizationalConfigurationColumnsKey) !== null;
            }


           function loadGrid(grid: Kitos.IKendoGrid<any>, initialFilter?: kendo.data.DataSourceFilters) {
                var gridId = grid.element[0].id;
                var storedState = getStoredOptions(grid);
                var columnState = <IGridSavedState>_.pick(storedState, "columnState");

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
        }
    }

    angular.module("app").factory("gridStateService", gridStateService);
}
