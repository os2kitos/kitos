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

        private static extractOptionKey = (filterRequest: string, optionName: string): number => {
            var pattern = new RegExp(`(.*\\(?${optionName} eq ')(\\d)('.*)`);
            var matchedString = filterRequest.replace(pattern, "$2");
            return parseInt(matchedString);
        }
    }
}