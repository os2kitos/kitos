module Kitos.Helpers {
    import IItProjectInactiveOverview = ItProject.OverviewInactive.IItProjectInactiveOverview;

    interface IStatusColor {
        danish: string;
        english: string;
    }

    export class ExcelExportHelper {

        private static readonly noValueFallback = "";

        private static readonly colors = {
            red: <IStatusColor>{ danish: "Rød", english: "Red" },
            green: <IStatusColor>{ danish: "Grøn", english: "Green" },
            yellow: <IStatusColor>{ danish: "Gul", english: "Yellow" },
            white: <IStatusColor>{ danish: "Hvid", english: "White" }
        }

        static renderUrlWithOptionalTitle(title: string | null | undefined, url: string) {
            if (Utility.Validation.isValidExternalReference(url)) {
                return url;
            }
            if (title === null || _.isUndefined(title)) {
                return ExcelExportHelper.noValueFallback;
            }
            return title;
        }

        static renderReference(referenceTitle: string, referenceUrl: string) {
            return ExcelExportHelper.renderUrlWithOptionalTitle(referenceTitle, referenceUrl);
        }

        static renderReferenceUrl(reference: Models.Reference.IOdataReference) {
            if (reference === null || _.isUndefined(reference)) {
                return ExcelExportHelper.noValueFallback;
            }
            return ExcelExportHelper.renderReference(reference.Title, reference.URL);
        }

        static renderReferenceId(externalReferenceId: string) {
            if (externalReferenceId != null) {
                return externalReferenceId;
            }
            return ExcelExportHelper.noValueFallback;
        }

        static renderExternalReferenceId(reference: Models.Reference.IOdataReference) {
            if (reference === null || _.isUndefined(reference)) {
                return ExcelExportHelper.noValueFallback;
            }
            return ExcelExportHelper.renderReferenceId(reference.ExternalReferenceId);
        }

        static renderUrlOrFallback(url, fallback) {
            if (Utility.Validation.validateUrl(url)) {
                return url;
            }
            if (fallback != null) {
                return fallback;
            }
            return ExcelExportHelper.noValueFallback;
        }

        static renderProjectStatusColor(status: Models.ItProject.IItProjectStatusUpdate[]) {

            const getColor = (statusArray: Array<string>) => {
                var prioritizedColorOrder = [
                    ExcelExportHelper.colors.red,
                    ExcelExportHelper.colors.yellow,
                    ExcelExportHelper.colors.green,
                    ExcelExportHelper.colors.white
                ];

                const statusMap = _.reduce(statusArray, (acc: any, current) => {
                    if (!!current) {
                        acc[current.toLowerCase()] = true;
                    }
                    return acc;
                }, <any>{});

                for (let currentPrioritizedColor of prioritizedColorOrder) {
                    if (statusMap.hasOwnProperty(currentPrioritizedColor.english.toLowerCase())) {
                        return currentPrioritizedColor.danish;
                    }
                }

                return ExcelExportHelper.noValueFallback;
            };

            if (status.length > 0) {
                const latestStatus = status[0];

                if (latestStatus.IsCombined) {
                    return this.convertColorsToDanish(latestStatus.CombinedStatus);
                }
                else {
                    const statusArray = [latestStatus.TimeStatus, latestStatus.QualityStatus, latestStatus.ResourcesStatus];
                    return getColor(statusArray);
                }
            }
            else {
                return ExcelExportHelper.noValueFallback;
            }
        }

        static renderDate(date: Date) {
            if (!!date) {
                return moment(date).format(Constants.DateFormat.DanishDateFormat);
            }
            return ExcelExportHelper.noValueFallback;
        }

        static renderStatusColorWithStatus(dataItem: IItProjectInactiveOverview, status) {
            const latestStatus = dataItem.ItProjectStatusUpdates[0];
            const statusToShow = (latestStatus.IsCombined) ? latestStatus.CombinedStatus : status;
            return ExcelExportHelper.convertColorsToDanish(statusToShow);
        }

        static convertColorsToDanish(color: string) {
            if (color === null || _.isUndefined(color)) {
                return ExcelExportHelper.noValueFallback;
            }
            const knownColor = ExcelExportHelper.colors[color.toLowerCase()];
            if (!_.isUndefined(knownColor)) {
                return knownColor.danish;
            }
        }

        static getGoalStatus(goalStatus: Models.TrafficLight) {
            if (goalStatus === null || _.isUndefined(goalStatus)) {
                return ExcelExportHelper.noValueFallback;
            }
            return this.convertColorsToDanish(goalStatus.toString());
        }

        static renderUserRoles(rights: any[], projectRoles) {
            let result = "";
            _.each(rights,
                (right, index,) => {
                    if (!_.find(projectRoles, (option: any) => (option.Id === parseInt(right.Role.Id, 10)))) {
                        result += `${right.Role.Name} (udgået)`;
                    } else {
                        result += `${right.Role.Name}`;
                    }

                    if (index !== rights.length - 1) {
                        result += ", ";
                    }

                });
            return result;
        }

        static renderString(value: string) {
            return value || ExcelExportHelper.noValueFallback;
        }

        /**
         * Wrapper method which creates and configures a dropdown for excel export
         * @param entry
         * @param scope
         * @param toolbar
         */
        static setupExcelExportDropdown(excelConfig: () => Models.IExcelConfig, mainGrid: () => IKendoGrid<any>, scope: ng.IScope, toolbar: IKendoGridToolbarItem[]) {
            const entry = this.createExcelExportDropdownEntry(excelConfig, mainGrid);
            this.addExcelExportDropdownToToolbar(toolbar, entry);
            this.setupKendoVm(scope, entry);
        }

        /**
          * Creates an object of type IKendoToolbarEntry configured as a dropdown for excel export
         * @param excelConfig
         * @param mainGrid
         */
        static createExcelExportDropdownEntry(excelConfig: () => Models.IExcelConfig, mainGrid: () => IKendoGrid<any>): Utility.KendoGrid.IKendoToolbarEntry {
            return {
                show: true,
                id: Constants.ExcelExportDropdown.Id,
                title: Constants.ExcelExportDropdown.DefaultTitle,
                color: Utility.KendoGrid.KendoToolbarButtonColor.Grey,
                position: Utility.KendoGrid.KendoToolbarButtonPosition.Right,
                implementation: Utility.KendoGrid.KendoToolbarImplementation.DropDownList,
                margins: [Utility.KendoGrid.KendoToolbarMargin.Right],
                enabled: () => true,
                dropDownConfiguration: {
                    selectedOptionChanged: newItem => {
                        if (newItem === null || newItem.id === Constants.ExcelExportDropdown.ChooseWhichExcelOptionId)
                            return;

                        const config = excelConfig();
                        config.onlyVisibleColumns = newItem.id === Constants.ExcelExportDropdown.SelectOnlyVisibleId;

                        mainGrid().saveAsExcel();

                        jQuery(`#${Constants.ExcelExportDropdown.Id}`).data(Constants.ExcelExportDropdown.DataKey)
                            .value(Constants.ExcelExportDropdown.ChooseWhichExcelOptionId);
                    },
                    availableOptions: [
                        {
                            id: Constants.ExcelExportDropdown.ChooseWhichExcelOptionId,
                            text: Constants.ExcelExportDropdown.ChooseWhichExcelOptionValue
                        },
                        {
                            id: Constants.ExcelExportDropdown.SelectAllId,
                            text: Constants.ExcelExportDropdown.SelectAllValue
                        },
                        {
                            id: Constants.ExcelExportDropdown.SelectOnlyVisibleId,
                            text: Constants.ExcelExportDropdown.SelectOnlyVisibleValue
                        }]
                }
            }
        }

        /**
         * Renders the text choice param as a text with an excel icon on the left side
         * @param text
         */
        static renderExcelChoice(text: string) {
            return `<span class="k-button-icontext"><span class="k-icon k-i-file-excel"></span>${text}</span>`;
        }

        /**
         * Configures a specified entry to be a Dropdown with classes required for excel export to work,
         * Adds the entry to the selected toolbar
         * @param toolbar
         * @param entry
         */
        static addExcelExportDropdownToToolbar(toolbar: IKendoGridToolbarItem[], entry: Utility.KendoGrid.IKendoToolbarEntry) {
            toolbar.push({
                name: entry.id,
                text: entry.title,
                template: `<input id='${entry.id}' data-element-type='${entry.id}DropDownList' class='${Helpers.KendoToolbarCustomizationHelper.getPositionClass(entry.position)} ${Helpers.KendoToolbarCustomizationHelper.getMargins(entry.margins)}' kendo-drop-down-list="kendoVm.${entry.id}.list" k-options="kendoVm.${entry.id}.getOptions()"/>`
            });
        }

        /**
         * Creates kendo dropdown options for the specific excel dropdown type
         * @param entry
         */
        static createExportToExcelDropDownOptions(entry: Utility.KendoGrid.IKendoToolbarEntry) {
            return {
                autoWidth: true,
                autoBind: false,
                dataSource: entry.dropDownConfiguration.availableOptions,
                dataTextField: "text",
                dataValueField: "id",
                template: (item) => {
                    if (item.id === Constants.ExcelExportDropdown.ChooseWhichExcelOptionId) {
                        return `<span>${item.text}</span>`;
                    }
                    return this.renderExcelChoice(item.text);
                },
                //Always show the title in the combobox selector
                valueTemplate: (_) => this.renderExcelChoice(entry.title),
                change: e => {
                    var selectedId = e.sender.value();
                    const newSelection = entry.dropDownConfiguration.availableOptions.filter(x => x.id === selectedId);
                    entry.dropDownConfiguration.selectedOptionChanged(newSelection.length > 0 ? newSelection[0] : null);
                }
            }
        }

        /**
         * Adds/updates the entry in the KendoVm and configures the dropdown for excel export
         * @param scope
         * @param entry
         */
        private static setupKendoVm(scope: ng.IScope, entry: Utility.KendoGrid.IKendoToolbarEntry) {
            if (scope.kendoVm === undefined)
                scope.kendoVm = {
                    standardToolbar: {}
                }
            scope.kendoVm[entry.id] = {
                enabled: true,
                getOptions: () => this.createExportToExcelDropDownOptions(entry)
            };
        }
    }
}