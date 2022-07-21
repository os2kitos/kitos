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
    }
}