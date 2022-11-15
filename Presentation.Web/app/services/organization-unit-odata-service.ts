module Kitos.Services.Organization {
    "use strict";

    export interface IOrganizationUnitOdataService {
        getOrganizationUnits(): angular.IPromise<Models.IOrganizationUnit[]>;
    }

    export class OrganizationUnitOdataService implements IOrganizationUnitOdataService {

        static $inject: string[] = ["$http", "userService", "$q", "inMemoryCacheService"];

        constructor(
            private readonly $http: ng.IHttpService,
            private readonly userService: Services.IUserService,
            private readonly $q: ng.IQService,
            private readonly inMemoryCacheService: Kitos.Shared.Caching.IInMemoryCacheService) {

        }
        getOrganizationUnits(): ng.IPromise<Models.IOrganizationUnit[]> {
            const cacheKey = "odataOrganizationUnits";
            const existingValue = this.inMemoryCacheService.getEntry<Models.IOrganizationUnit[]>(cacheKey);
            if (existingValue != null) {
                return this.$q.resolve(existingValue);
            }

            return this.userService
                .getUser()
                .then(user => this.$http.get(`/odata/Organizations(${user.currentOrganizationId})/OrganizationUnits`))
                .then(result => {
                    const value = result.data.value;
                    this.inMemoryCacheService.setEntry(cacheKey, value, Kitos.Shared.Time.Offset.compute(Kitos.Shared.Time.TimeUnit.Minutes, 10));
                    return value;
                });
        }
    }

    app.service("organizationUnitOdataService", OrganizationUnitOdataService);
}
