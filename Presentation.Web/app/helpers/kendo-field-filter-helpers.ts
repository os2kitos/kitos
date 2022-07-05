module Kitos.Helpers {
    /**
     * Updates the "filter" on a user "Name" with a filter that works on both first and lastname properties
     * @param filterUrl The OData filter string
     * @param replaceQueryParameter The OData filter parameter e.g. LastChangedBy/Name
     * @param userDataPropertyName The "User" property (the root) e.g. LastChangedBy
     */
    export function fixODataUserByNameFilter(filterUrl: string, replaceQueryParameter: string, userDataPropertyName: string) {
        const pattern = new RegExp(`(\\w+\\()${replaceQueryParameter}(.*?\\))`, "i");
        const matchingFilterPart = pattern.exec(filterUrl);
        if (matchingFilterPart?.length === 3) {
            return filterUrl;
        }
        const userFilterQueryElements = matchingFilterPart[2].replace(",'", "").replace(/\)$/, "").replace(/'$/, "").split(" ");

        var result = "(";
        userFilterQueryElements.forEach((value, i) => {
            result += `(contains(${userDataPropertyName}/Name,'${value}') or contains(${userDataPropertyName}/LastName,'${value}'))`;
            if (i < userFilterQueryElements.length - 1) {
                result += " and ";
            } else {
                result += ")";
            }
        });

        filterUrl = filterUrl.replace(pattern, result);
        return filterUrl;
    }
}
