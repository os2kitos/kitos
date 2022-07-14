module Kitos.Helpers {
    export interface ISelectDataSource {
        id: number;
        text: string;
        emptyOption?: boolean;
    }
    
    export interface IKendoElement {
        kendoDropDownList(options: kendo.ui.DropDownListOptions) : JQuery;
    }
    
    export class KendoOverviewHelper {
        /**
          *  creates a select dropdown for given dataSource
          * @param element - kendo element containing kendoDropDownList method
          * @param dataSource - dataSource of the dropdown
          * @param insertEmptyOption - if true and if dataSource doesn't contain an empty option, method will add an empty option to select
          */
        public static createSelectDropdownTemplate(element: IKendoElement, dataSource: ISelectDataSource[], insertEmptyOption: boolean): JQuery{
            if (insertEmptyOption && dataSource.filter(x => x.emptyOption).length === 0) {
                dataSource.unshift({ id: 0, text: "", emptyOption: true });
            }

            return element.kendoDropDownList({
                dataSource: dataSource.map(value => {
                    return {
                        remoteValue: value.id,
                        text: value.text,
                        optionalContext: value
                    };
                }),
                dataTextField: "text",
                dataValueField: "remoteValue",
                valuePrimitive: true,
            });
        }

        public static mapDataForKendoDropdown(dataSource: ISelectDataSource[], insertEmptyOption: boolean): Utility.KendoGrid.IKendoParameter[]{
            if (insertEmptyOption && dataSource.filter(x => x.emptyOption).length === 0) {
                dataSource.unshift({ id: 0, text: "", emptyOption: true });
            }

            return dataSource.map(value => {
                    return {
                        textValue: value.text,
                        remoteValue: value.id,
                        optionalContext: value
                    };
            });
        }
    }
}