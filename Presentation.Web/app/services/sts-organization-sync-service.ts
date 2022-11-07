module Kitos.Services.Organization {
    export interface IStsOrganizationSyncService {
        getConnectionStatus(organizationUuid: string): ng.IPromise<Models.Api.Organization.StsOrganizationSynchronizationStatusResponseDTO>;
        createConnection(organizationUuidid: string, synchronizationDepth: number | null): ng.IPromise<void>;
        getConnectionUpdateConsequences(organizationUuid: string, synchronizationDepth: number | null): ng.IPromise<Models.Api.Organization.ConnectionUpdateConsequencesResponseDTO>;
        getSnapshot(organizationUuid: string): ng.IPromise<Models.Api.Organization.StsOrganizationOrgUnitDTO>;
        disconnect(organizationUuidid: string): ng.IPromise<boolean>;
        updateConnection(organizationUuidid: string, synchronizationDepth: number | null): ng.IPromise<void>;
    }

    export class StsOrganizationSyncService implements IStsOrganizationSyncService {

        static $inject = ["genericApiWrapper", "inMemoryCacheService", "$q", "apiUseCaseFactory"];
        constructor(
            private readonly genericApiWrapper: Services.Generic.ApiWrapper,
            private readonly inMemoryCacheService: Kitos.Shared.Caching.IInMemoryCacheService,
            private readonly $q: ng.IQService,
            private readonly apiUseCaseFactory: Services.Generic.IApiUseCaseFactory) {
        }

        getConnectionUpdateConsequences(organizationUuid: string, synchronizationDepth: number | null): ng.IPromise<Models.Api.Organization.ConnectionUpdateConsequencesResponseDTO> {
            let query = "";
            if (synchronizationDepth) {
                query = `?synchronizationDepth=${synchronizationDepth}`;
            }
            return this.genericApiWrapper
                .getDataFromUrl<Models.Api.Organization.ConnectionUpdateConsequencesResponseDTO>(`${this.getBasePath(organizationUuid)}/connection/update${query}`);
        }

        private getBasePath(organizationUuid: string) {
            return `api/v1/organizations/${organizationUuid}/sts-organization-synchronization`;
        }

        private getStatusCacheKey(organizationUuid: string) {
            return `FK_CONNECTION_STATUS_${organizationUuid}`;
        }

        private getHierarchyCacheKey(organizationUuid: string) {
            return `FK_HIERARCHY_${organizationUuid}`;
        }

        private purgeCache(organizationUuid: string) {
            this.inMemoryCacheService.deleteEntry(this.getStatusCacheKey(organizationUuid));
            this.inMemoryCacheService.deleteEntry(this.getHierarchyCacheKey(organizationUuid));
        }

        getSnapshot(organizationUuid: string): ng.IPromise<Models.Api.Organization.StsOrganizationOrgUnitDTO> {
            const cacheKey = this.getHierarchyCacheKey(organizationUuid);
            const result = this.inMemoryCacheService.getEntry<Models.Api.Organization.StsOrganizationOrgUnitDTO>(cacheKey);
            if (result != null) {
                return this.$q.resolve(result);
            }
            return this.genericApiWrapper
                .getDataFromUrl<Models.Api.Organization.StsOrganizationOrgUnitDTO>(`${this.getBasePath(organizationUuid)}/snapshot`)
                .then(root => {
                    this.inMemoryCacheService.setEntry(cacheKey, root, Kitos.Shared.Time.Offset.compute(Kitos.Shared.Time.TimeUnit.Minutes, 10));
                    return root;
                });
        }

        getConnectionStatus(organizationUuid: string): ng.IPromise<Models.Api.Organization.StsOrganizationSynchronizationStatusResponseDTO> {
            const cacheKey = this.getStatusCacheKey(organizationUuid);
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

        disconnect(organizationUuidid: string): ng.IPromise<boolean> {
            return this.apiUseCaseFactory.createDeletion("Forbindelse til FK Organisation", () => {
                return this.genericApiWrapper.delete(`${this.getBasePath(organizationUuidid)}/connection`);
            }).executeAsync((result) => {
                //Clear cache after
                this.purgeCache(organizationUuidid);
                return result;
            });
        }

        updateConnection(organizationUuidid: string, synchronizationDepth: number | null): ng.IPromise<void> {
            return this.apiUseCaseFactory.createUpdate("Forbindelse til FK Organisation", () => {
                return this.genericApiWrapper.put(`${this.getBasePath(organizationUuidid)}/connection`, {
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