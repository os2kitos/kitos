module Kitos.Services.ItSystemUsage {

    export interface IGDPRService {
        addDataLevel(systemUsageId: number, dataLevel: number);
        removeDataLevel(systemUsageId: number, dataLevel: number);
    }

    export class GDPRService implements IGDPRService {

        static $inject = ["$http", "notify"];
        constructor(private readonly $http: ng.IHttpService) {
        }

        addDataLevel(systemUsageId: number, dataLevel: number) {
            return this.$http.patch(`api/v1/itsystemusage/${systemUsageId}/sensitivityLevel/add`, dataLevel);
        }

        removeDataLevel(systemUsageId: number, dataLevel: number) {
            return this.$http.patch(`api/v1/itsystemusage/${systemUsageId}/sensitivityLevel/remove`, dataLevel);
        }
    }

    app.service("GDPRService", GDPRService);
}