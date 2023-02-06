module Kitos.Helpers {

    export class Select2ApiQueryHelper {

        static getOrganizationQueryParams(pageSize: number): string[] {
            return [`pageSize=${pageSize}`];
        }
    }
}