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
            const profileStorageKey = storageKey + "-profile";
            const organizationalConfigurationColumnsKey = storageKey + "-OrgProfile";
            const organizationalConfigurationVersionKey = storageKey + "-version";
            const nonExistingOrganizationalConfigurationColumnsKey = storageKey + "-NonExistingColumns";

            const locallyChangedValue = "true";

            var badOrganizationalConfigExists: boolean = false;
            var gridLoading: boolean = true;

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

            async function checkServerGridConfig(overviewType: Models.Generic.OverviewType) {
                // Organizational configuration not yet activated for overview
                if (overviewType === null || overviewType === undefined) {
                    return false;
                }

                return await getGridVersion().then((result) => {
                    if (result !== null && result !== $window.localStorage.getItem(organizationalConfigurationVersionKey)) {
                        return getGridOrganizationalConfiguration();
                    }
                    return false;
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
                });
            }

            // loads kendo grid options from localStorage
            function loadGridOptions(grid: Kitos.IKendoGrid<any>, initialFilter?: kendo.data.DataSourceFilters): void {
                // load grid immediately from local storage data
                loadGrid(grid, initialFilter);
                
                // Check with server if grid has updates, and apply them if needed
                checkServerGridConfig(overviewType).then(
                    (remoteConfigurationSaved : boolean) => {
                        //Reload the grid if the server had changes, after these changes are saved locally.
                        if (remoteConfigurationSaved) {
                            loadGrid(grid, initialFilter);
                        }
                    },
                    () => { });
            }

            // gets all the saved options, both session and local, and merges
            // them together so that the correct options are overwritten
            function getStoredOptions(grid: Kitos.IKendoGrid<any>): IGridSavedState {
                // load options from organization configuration storage
                var orgStorageColumns: Models.Generic.IKendoColumnConfigurationDTO[];
                var orgStorageColumnsItem = $window.localStorage.getItem(organizationalConfigurationColumnsKey);
                if (orgStorageColumnsItem) {
                    orgStorageColumns = <Models.Generic.IKendoColumnConfigurationDTO[]>JSONfn.parse(orgStorageColumnsItem, true);
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

                var options: IGridSavedState = { columnState: null };


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
                    options = <IGridSavedState>_.merge({}, localOptions, profileOptions);
                }
                else if (localOptions) {
                    options = <IGridSavedState> localOptions;
                }


                // If user has not made local changes to the grid we update it according to the server configuration 
                if ($window.localStorage.getItem(locallyChangedKey) !== locallyChangedValue) {

                    // If user has only changed the index of the columns we don't force the server indexes to be applied
                    if (shouldUseLocalIndexes()) {
                        return options;
                    }

                    // We make sure the organization configuration exists
                    if (orgStorageColumns) {
                        var columns: { [persistId: string]: { index: number; width: number, hidden?: boolean } } = {};

                        var gridColumnWidths: { [persistId: string]: { width: number, originallyHidden: boolean } } = {};

                        // We need to iterate over all columns to hide them. During the iteration we also store the column widths and original hidden value in a map (In case we have a bad configuration and need to rebuild the old configuration)
                        grid.columns.forEach(x => {
                            gridColumnWidths[x.persistId] = { width: x.width as number, originallyHidden: x.hidden };
                            x.hidden = true;
                        });

                        // Keep track of how many columns are being set
                        var columnsBeingSet = 0;
                        var nonExistingColumns: string[] = [];

                        // The visible columns from the server are then made visible and given the index of the server 
                        orgStorageColumns.forEach(x => {
                            var gridWidth = gridColumnWidths[x.persistId];
                            if (gridWidth !== undefined && gridWidth !== null) {
                                columns[x.persistId] = { index: x.index, width: gridWidth.width, hidden: false };
                                columnsBeingSet++;
                            } else {
                                nonExistingColumns.push(x.persistId);
                            }
                        });

                        // We save the "bad" columns from the server as we need them to correctly calculate the version in the frontend
                        $window.localStorage.setItem(nonExistingOrganizationalConfigurationColumnsKey, JSONfn.stringify(nonExistingColumns));

                        if (columnsBeingSet === 0) {
                            badOrganizationalConfigExists = true;
                            // Remove the saved data from the server as the grid don't have any similar fields and is therefore invalid.
                            removeOrgConfig(); 
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
                pickedOptions.dataSource = <kendo.data.DataSourceOptions>_.pick(options.dataSource, ["filter", "sort", "page"]);
                $window.sessionStorage.setItem(storageKey, JSONfn.stringify(pickedOptions));
            }

            // save grid options that should be stored in localStorage
            function saveGridStateForever(options: Kitos.IKendoGridOptions<any>): void {
                if (options) {
                    var pickedOptions: IGridSavedState = {};
                    // save pageSize
                    pickedOptions.dataSource = <kendo.data.DataSourceOptions>_.pick(options.dataSource, ["pageSize"]);

                    // save column state - dont use the kendo function for it as it breaks more than it fixes...
                    pickedOptions.columnState = {};
                    for (var i = 0; i < options.columns.length; i++) {
                        var column = options.columns[i];
                        pickedOptions.columnState[column.persistId] = { index: i, width: <number>column.width, hidden: column.hidden };
                    }
                    
                    $window.localStorage.setItem(storageKey, JSONfn.stringify(pickedOptions));

                    // We check if the changes causes the grid to deviate from the server configuration
                    if (!isOrgConfigServerVersionEqualToLocalGrid(options)) {
                        $window.localStorage.setItem(locallyChangedKey, locallyChangedValue);
                    } else {
                        $window.localStorage.removeItem(locallyChangedKey);
                    }
                }
            }

            function saveGridProfile(grid: Kitos.IKendoGrid<any>): void {
                var options = grid.getOptions();
                var pickedOptions: IGridSavedState = {};
                // save filter and sort
                pickedOptions.dataSource = <kendo.data.DataSourceOptions>_.pick(options.dataSource, ["filter", "sort"]);

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
                                // If no or new version we store the data locally to be reused.
                                const columns = result.data.response.visibleColumns;
                                $window.localStorage.setItem(organizationalConfigurationColumnsKey, JSONfn.stringify(columns));
                                $window.localStorage.setItem(organizationalConfigurationVersionKey, version);
                                $window.localStorage.removeItem(nonExistingOrganizationalConfigurationColumnsKey);
                                return true;
                            }
                        }
                        return false;
                    })
                    .catch((result) => {
                        removeOrgConfig();
                        return false;
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
                            $window.localStorage.removeItem(locallyChangedKey);
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
                $window.localStorage.removeItem(nonExistingOrganizationalConfigurationColumnsKey);
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

                if (gridLoading) { // If grid is still loading we can't correctly calculate if it is deviating
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
                var gridColumnsPersistIds = options.columns.filter(x => !x.hidden).map(x => x.persistId);

                // Add items from the server which doesn't exist in kendogrid to be able to calculate the same version as the backend.
                var nonExistingColumnsItem = $window.localStorage.getItem(nonExistingOrganizationalConfigurationColumnsKey);
                if (nonExistingColumnsItem) {
                    var nonExistingColumns = <string[]>JSONfn.parse(nonExistingColumnsItem, true);
                    gridColumnsPersistIds.pushArray(nonExistingColumns);
                }

                return sha256(gridColumnsPersistIds.sort((a, b) => a.toLowerCase().localeCompare(b.toLowerCase())).join(""));
            }

            function shouldUseLocalIndexes() {

                var localStorageItem = $window.localStorage.getItem(storageKey);
                if (!localStorageItem) {
                    return false;
                }
                var localColumns = <IGridSavedState>JSONfn.parse(localStorageItem, true);

                var orgStorageColumnsItem = $window.localStorage.getItem(organizationalConfigurationColumnsKey);
                if (orgStorageColumnsItem) {
                    var orgStorageColumns = <Models.Generic.IKendoColumnConfigurationDTO[]>JSONfn.parse(orgStorageColumnsItem, true);

                    var numberOfVisibleLocalColumnStates = 0;
                    _.forEach(localColumns.columnState, (state, key) => {
                        if (!state.hidden) {
                            numberOfVisibleLocalColumnStates++;
                        }
                    });

                    if (numberOfVisibleLocalColumnStates !== orgStorageColumns.length) {
                        return false; // Not same amount of visible columns
                    }

                    // If the indexes are not equal we should use the local indexes.
                    return !orgStorageColumns.sort(x => x.index).every((column, index) => localColumns.columnState[column.persistId].index === index);
                }

                return false; // We only have 1 set of indexes so we just use those.
            }

            function canDeleteGridOrganizationalConfiguration() {
                // As we remove the bad configurations from the local storage, we use this flag to allow admins to delete a bad configuration.
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
                gridLoading = false;
            }
        }
    }

    angular.module("app").factory("gridStateService", gridStateService);
}
