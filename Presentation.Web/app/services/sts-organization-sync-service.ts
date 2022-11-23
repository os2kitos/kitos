module Kitos.Services.Organization {
    export interface IStsOrganizationSyncService {
        getConnectionStatus(organizationUuid: string): ng.IPromise<Models.Api.Organization.StsOrganizationSynchronizationStatusResponseDTO>;
        createConnection(organizationUuid: string, synchronizationDepth: number | null, subscribesToUpdates: boolean): ng.IPromise<void>;
        getConnectionUpdateConsequences(organizationUuid: string, synchronizationDepth: number | null): ng.IPromise<Models.Api.Organization.ConnectionUpdateConsequencesResponseDTO>;
        getSnapshot(organizationUuid: string): ng.IPromise<Models.Api.Organization.StsOrganizationOrgUnitDTO>;
        disconnect(organizationUuid: string): ng.IPromise<boolean>;
        updateConnection(organizationUuid: string, synchronizationDepth: number | null, subscribesToUpdates: boolean): ng.IPromise<void>;
        getConnectionChangeLogs(organizationUuid: string, numberOfLogs: number): ng.IPromise<Array<Models.Api.Organization.ConnectionChangeLogDTO>>;
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

        createConnection(organizationUuid: string, synchronizationDepth: number | null, subscribesToUpdates: boolean): ng.IPromise<void> {
            return this.apiUseCaseFactory.createCreation("Forbindelse til FK Organisation", () => {
                return this.genericApiWrapper.post<void>(`${this.getBasePath(organizationUuid)}/connection`, {
                    synchronizationDepth: synchronizationDepth,
                    subscribeToUpdates: subscribesToUpdates
                });
            }).executeAsync(() => {
                //Clear cache after
                this.purgeCache(organizationUuid);
            });
        }

        disconnect(organizationUuid: string): ng.IPromise<boolean> {
            return this.apiUseCaseFactory.createDeletion("Forbindelse til FK Organisation", () => {
                return this.genericApiWrapper.delete(`${this.getBasePath(organizationUuid)}/connection`);
            }).executeAsync((result) => {
                //Clear cache after
                this.purgeCache(organizationUuid);
                return result;
            });
        }

        updateConnection(organizationUuid: string, synchronizationDepth: number | null, subscribesToUpdates: boolean): ng.IPromise<void> {
            return this.apiUseCaseFactory.createUpdate("Forbindelse til FK Organisation", () => {
                return this.genericApiWrapper.put(`${this.getBasePath(organizationUuid)}/connection`, {
                    synchronizationDepth: synchronizationDepth,
                    subscribeToUpdates: subscribesToUpdates
                });
            }).executeAsync(() => {
                //Clear cache after
                this.purgeCache(organizationUuid);
            });
        }

        getConnectionChangeLogs(organizationUuid: string, numberOfLogs: number): ng.IPromise<Array<Models.Api.Organization.ConnectionChangeLogDTO>> {
            return this.genericApiWrapper.getDataFromUrl<Array<Models.Api.Organization.ConnectionChangeLogDTO>>(`${this.getBasePath(organizationUuid)}/connection/change-log?numberOfChangeLogs=${numberOfLogs}`);
        }
    }

    app.service("stsOrganizationSyncService", StsOrganizationSyncService);
}