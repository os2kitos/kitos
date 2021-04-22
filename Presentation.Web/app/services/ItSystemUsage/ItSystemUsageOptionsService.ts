module Kitos.Services.ItSystemUsage {

    export interface IItSystemUsageOptionsService {
        getOptions(): ng.IPromise<Models.ItSystemUsage.IItSystemUsageOverviewOptionsDTO>;
    }

    export class ItSystemUsageOptionsService implements IItSystemUsageOptionsService {
        static $inject = ["$http", "userService"];
        constructor(
            private readonly $http: ng.IHttpService,
            private readonly userService: Services.IUserService) {
        }

        getOptions(): angular.IPromise<Models.ItSystemUsage.IItSystemUsageOverviewOptionsDTO> {
            return this.userService.getUser().then(user => this.$http
                .get<any>(`/api/v1/itsystem-usage/options/overview?organizationId=${user.currentOrganizationId}`)
                .then(result => result.data.response));
        }
    }

    app.service("itSystemUsageOptionsService", ItSystemUsageOptionsService);
}