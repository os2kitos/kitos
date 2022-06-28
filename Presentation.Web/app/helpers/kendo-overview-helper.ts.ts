module Kitos.Helpers {

    export class KendoOverviewHelper {

        /**
          *  creates a select dropdown for given dataSource
          * @param args - should contain kenoGrid elements
          * @param dataSource - dataSource of the dropdown, should contain 'id' and 'text' properties
          * @param insertEmptyOption - if true and if dataSource doesn't contain an item with an empty name, method will add an empty option to select
          */
        public static createSelectDropdownTemplate(args: any, dataSource: any, insertEmptyOption: boolean) {
            if (insertEmptyOption && dataSource.filter(x => x.text === "").length === 0) {
                dataSource.push({ id: 0, text: "" });
            }

            return args.element.kendoDropDownList({
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
    }
}