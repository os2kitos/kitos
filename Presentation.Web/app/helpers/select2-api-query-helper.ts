module Kitos.Helpers {

    export class Select2ApiQueryHelper {

        static getOrganizationQueryParams(onlyWhereUserHasMembership: boolean, pageSize: number): string[] {
            return [`onlyWhereUserHasMembership=${onlyWhereUserHasMembership}`, `pageSize=${pageSize}`];
        }
    }
}