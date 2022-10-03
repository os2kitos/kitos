module Kitos.Services.Organization {
    export interface IStsOrganizationSyncService {
        getConnectionStatus(organizationUuid: string): ng.IPromise<Models.Api.Organization.StsOrganizationSynchronizationStatusResponseDTO>
        createConnection(organizationUuidid: string, synchronizationDepth: number | null): ng.IPromise<void>
    }

    export class StsOrganizationSyncService implements IStsOrganizationSyncService {

        static $inject = ["genericApiWrapper", "inMemoryCacheService", "$q", "apiUseCaseFactory"];
        constructor(
            private readonly genericApiWrapper: Services.Generic.ApiWrapper,
            private readonly inMemoryCacheService: Kitos.Shared.Caching.IInMemoryCacheService,
            private readonly $q: ng.IQService,
            private readonly apiUseCaseFactory: Services.Generic.IApiUseCaseFactory) {
        }

        private getBasePath(organizationUuid: string) {
            return `api/v1/organizations/${organizationUuid}/sts-organization-synchronization`;
        }

        private getCacheKey(organizationUuid: string) {
            return `FK_CONNECTION_STATUS_${organizationUuid}`;
        }

        private purgeCache(organizationUuid: string) {
            this.inMemoryCacheService.deleteEntry(this.getCacheKey(organizationUuid));
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

        createConnection(organizationUuidid: string, synchronizationDepth: number | null): ng.IPromise<void> {
            return this.apiUseCaseFactory.createCreation("Forbindelse til FK Organisation", () => {
                return this.genericApiWrapper.post<void>(`${this.getBasePath(organizationUuidid)}/connection`, {
                    synchronizationDepth: synchronizationDepth
                });
            }).executeAsync(() => {
                //Clear cache after
                this.purgeCache(organizationUuidid);
            });
        }
    }

    app.service("stsOrganizationSyncService", StsOrganizationSyncService);
}