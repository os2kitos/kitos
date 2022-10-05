module Kitos.Services.Organization {
    export interface IStsOrganizationSyncService {
        getConnectionStatus(organizationId: string): ng.IPromise<Models.Api.Organization.CheckStsOrganizationConnectionResponseDTO>
    }

    export class StsOrganizationSyncService implements IStsOrganizationSyncService {

        static $inject = ["genericApiWrapper", "inMemoryCacheService", "$q"];
        constructor(
            private readonly genericApiWrapper: Services.Generic.ApiWrapper,
            private readonly inMemoryCacheService: Kitos.Shared.Caching.IInMemoryCacheService,
            private readonly $q: ng.IQService) {
        }

        private getBasePath(organizationUuid: string) {
            return `api/v1/organizations/${organizationUuid}/sts-organization-synchronization`;
        }

        getConnectionStatus(organizationUuid: string): ng.IPromise<Models.Api.Organization.CheckStsOrganizationConnectionResponseDTO> {
            const cacheKey = `FK_CONNECTION_STATUS_${organizationUuid}`;
            const result = this.inMemoryCacheService.getEntry<Models.Api.Organization.CheckStsOrganizationConnectionResponseDTO>(cacheKey);
            if (result != null) {
                return this.$q.resolve(result);
            }
            return this.genericApiWrapper
                .getDataFromUrl<Models.Api.Organization.CheckStsOrganizationConnectionResponseDTO>(`${this.getBasePath(organizationUuid)}/connection-status`)
                .then(connectionStatus => {
                    this.inMemoryCacheService.setEntry(cacheKey, connectionStatus, Kitos.Shared.Time.Offset.compute(Kitos.Shared.Time.TimeUnit.Minutes, 1));
                    return connectionStatus;
                });
        }
    }

    app.service("stsOrganizationSyncService", StsOrganizationSyncService);
}