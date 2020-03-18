module Kitos.Services.ItSystemUsage {
    import SensitiveDataLevel = Models.ViewModel.ItSystemUsage.SensitiveDataLevel;

    export interface IGDPRService {
        addDataLevel(systemUsageId: number, dataLevel: SensitiveDataLevel);
        removeDataLevel(systemUsageId: number, dataLevel: SensitiveDataLevel);
    }

    export class GDPRService implements IGDPRService {

        static $inject = ["$http", "notify"];
        constructor(private readonly $http: ng.IHttpService) {
        }

        addDataLevel(systemUsageId: number, dataLevel: SensitiveDataLevel) {
            return this.$http.patch(`api/v1/itsystemusage/${systemUsageId}/sensitivityLevel/add`, dataLevel);
        }

        removeDataLevel(systemUsageId: number, dataLevel: SensitiveDataLevel) {
            return this.$http.patch(`api/v1/itsystemusage/${systemUsageId}/sensitivityLevel/remove`, dataLevel);
        }
    }

    app.service("GDPRService", GDPRService);
}