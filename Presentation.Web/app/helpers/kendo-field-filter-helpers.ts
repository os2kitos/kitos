module Kitos.Helpers {
    /**
     * Updates the "filter" on a user "Name" with a filter that works on both first and lastname properties
     * @param filterUrl The OData filter string
     * @param replaceQueryParameter The OData filter parameter e.g. LastChangedBy/Name
     * @param userDataPropertyName The "User" property (the root) e.g. LastChangedBy
     */
    export function fixODataUserByNameFilter(filterUrl: string, replaceQueryParameter: string, userDataPropertyName: string) {
        const pattern = new RegExp(`(\\w+\\()${replaceQueryParameter}(.*?\\))`, "i");
        return filterUrl.replace(pattern, `contains(${userDataPropertyName}/Name$2 or contains(${userDataPropertyName}/LastName$2`);
    }
}
