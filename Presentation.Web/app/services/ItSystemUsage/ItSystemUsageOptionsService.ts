module Kitos.Services.ItSystemUsage {

    export interface IItSystemUsageOptionsService {
        getOptions(): ng.IPromise<Models.ItSystemUsage.IItSystemUsageOverviewOptionsDTO>;
    }

    export class ItSystemUsageOptionsService implements IItSystemUsageOptionsService {
        static $inject = ["$http", "userService", "$q", "inMemoryCacheService"];
        constructor(
            private readonly $http: ng.IHttpService,
            private readonly userService: Services.IUserService,
            private readonly $q: ng.IQService,
            private readonly inMemoryCacheService: Kitos.Shared.Caching.IInMemoryCacheService) {
        }

        getOptions(): angular.IPromise<Models.ItSystemUsage.IItSystemUsageOverviewOptionsDTO> {
            const cacheKey = "systemOverviewFilterOptions";
            const existingValue = this.inMemoryCacheService.getEntry<Models.ItSystemUsage.IItSystemUsageOverviewOptionsDTO>(cacheKey);
            if (existingValue != null) {
                return this.$q.resolve(existingValue);
            }

            return this.userService
                .getUser()
                .then(user => this.$http.get<any>(`/api/v1/itsystem-usage/options/overview?organizationId=${user.currentOrganizationId}`))
                .then(result => {
                    const value = result.data.response;
                    this.inMemoryCacheService.setEntry(cacheKey, value, Kitos.Shared.Time.Offset.compute(Kitos.Shared.Time.TimeUnit.Minutes, 10))
                    return value;
                }
                );
        }
    }

    app.service("itSystemUsageOptionsService", ItSystemUsageOptionsService);
}