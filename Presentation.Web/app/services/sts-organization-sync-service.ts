module Kitos.Services.Organization {
    export interface IStsOrganizationSyncService {
        getConnectionStatus(organizationId: string): ng.IPromise<Models.Api.Organization.StsOrganizationSynchronizationStatusResponseDTO>
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

        private getCacheKey(organizationUuid: string) {
            return `FK_CONNECTION_STATUS_${organizationUuid}`;
        }

        getConnectionStatus(organizationUuid: string): ng.IPromise<Models.Api.Organization.StsOrganizationSynchronizationStatusResponseDTO> {
            const cacheKey = this.getCacheKey(organizationUuid);
            const result = this.inMemoryCacheService.getEntry<Models.Api.Organization.StsOrganizationSynchronizationStatusResponseDTO>(cacheKey);
            if (result != null) {
                return this.$q.resolve(result);
            }
            return this.genericApiWrapper
                .getDataFromUrl<Models.Api.Organization.StsOrganizationSynchronizationStatusResponseDTO>(`${this.getBasePath(organizationUuid)}/connection-status`)
                .then(connectionStatus => {
                    this.inMemoryCacheService.setEntry(cacheKey, connectionStatus, Kitos.Shared.Time.Offset.compute(Kitos.Shared.Time.TimeUnit.Minutes, 1));
                    return connectionStatus;
                });
        }

        //TODO: Purge cache after doing a command!
    }

    app.service("stsOrganizationSyncService", StsOrganizationSyncService);
}