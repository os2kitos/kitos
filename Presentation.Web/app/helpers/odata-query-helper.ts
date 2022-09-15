module Kitos.Helpers {

    export class OdataQueryHelper {

        static replaceOptionQuery = (filterUrl: string, optionName: string, emptyOptionKey: number): string => {
            if (filterUrl.indexOf(optionName) === -1) {
                return filterUrl; // optionName not found in filter so return original filter. Can be updated to .includes() instead of .indexOf() in later typescript versions
            }

            var pattern = new RegExp(`(.+)?(${optionName} eq '\\d')( and .+'\\)|\\)|)`, "i");
            var key = OdataQueryHelper.extractOptionKey(filterUrl, optionName);
            if (key === emptyOptionKey) {
                return filterUrl.replace(pattern, `$1(${optionName} eq '${key}' or ${optionName} eq null)$3`);
            }
            return filterUrl;
        };

        static replaceQueryByMultiplePropertyContains(filterUrl: string, replaceQueryParameter: string, columnName: string, propertyNames: string[], addSlashBeforeProperty: boolean = true) {
            const pattern = new RegExp(`(\\w+\\()${replaceQueryParameter}(.*?\\))`, "i");
            const matchingFilterPart = pattern.exec(filterUrl);
            if (matchingFilterPart?.length !== 3) {
                return filterUrl;
            }
            const userFilterQueryElements = matchingFilterPart[2].replace(",'", "").replace(/\)$/, "").replace(/'$/, "").split(" ");

            var result = "(";
            userFilterQueryElements.forEach((value, i) => {
                result += this.createContainsQueryForProperties(columnName, propertyNames, value, addSlashBeforeProperty);
                if (i < userFilterQueryElements.length - 1) {
                    result += " and ";
                } else {
                    result += ")";
                }
            });

            filterUrl = filterUrl.replace(pattern, result);
            return filterUrl;
        }

        /**
             * Changes ordering from one properties to multiple properties while maintaining direction on each property
             * @param initialOrderBy
             * @param initialProperty
             * @param propertiesToExtendTo
             */
        static expandOrderingToMultipleProperties(initialOrderBy: string, initialProperty: string, propertiesToExtendTo: Array<string>): string {
            const pattern = new RegExp(`^${initialProperty}( desc| asc)?$`, "i");
            const matches = pattern.exec(initialOrderBy);
            let result = initialOrderBy;
            if (matches?.length === 2) {
                let properties = propertiesToExtendTo;
                const direction = matches[1];

                //Append the direction to each property if defined
                if (direction !== undefined) {
                    properties = properties.map(p => { return `${p}${direction}`; });
                }

                //Create the updated ordering
                result = result.replace(pattern, properties.join(","));
            }
            return result;
        }

        private static createContainsQueryForProperties(columnName: string, propertyNames: string[], searchValue: string, addSlashBeforeProperty: boolean): string {
            var result = "(";
            propertyNames.forEach((name, i) => {
                var propName = "";
                if (addSlashBeforeProperty) {
                    propName = "/";
                }
                propName += name;

                result += `contains(${columnName}${propName},'${searchValue}')`;
                if (i < propertyNames.length - 1) {
                    result += " or ";
                }
            });
            result += ")";

            return result;
        }

        private static extractOptionKey = (filterRequest: string, optionName: string): number => {
            var pattern = new RegExp(`(.*\\(?${optionName} eq ')(\\d)('.*)`);
            var matchedString = filterRequest.replace(pattern, "$2");
            return parseInt(matchedString);
        }

        /**
         * Cleans up odata filter query from kendo config, so that it remains valid or is deleted alltogether.
         * @param parameterMap
         */
        static cleanupModifiedKendoFilterConfig(parameterMap: { $filter :string}): void {

            if (parameterMap.$filter) {
                parameterMap.$filter = parameterMap.$filter
                    .replace("and  and", "and") //in the middle of other criteria
                    .replace(/\( and /, "(") //First criteria removed
                    .replace(/ and \)/, ")"); // Last criteria removed
            }

            //Cleanup filter if invalid ODATA Filter (can happen when we strip params such as responsible org unit)
            if (parameterMap.$filter === "" || parameterMap.$filter === null) {
                delete parameterMap.$filter;
            }
        }
    }
}