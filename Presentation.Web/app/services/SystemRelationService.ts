module Kitos.Services {
    import IItSystemUsageRelationDTO = Models.ItSystemUsage.Relation.IItSystemUsageRelationDTO;

    export interface ISystemRelationService {
        getFromRelations(systemUsageId: number): ng.IPromise<[IItSystemUsageRelationDTO]>;
        getToRelations(systemUsageId: number): ng.IPromise<[IItSystemUsageRelationDTO]>;
    }

    export class SystemRelationService implements ISystemRelationService {

        static $inject = ["$http"];
        constructor(private readonly $http: ng.IHttpService) {
        }


        getFromRelations(systemUsageId: number) {
            return this.$http.get(`api/v1/systemrelations/from/${systemUsageId}`)
                .then(response => {
                    var kitosSystemRelationResponse = response.data as { msg: string, response: [IItSystemUsageRelationDTO] }
                    return kitosSystemRelationResponse.response;
                });
        }

        getToRelations(systemUsageId: number) {
            return this.$http.get(`api/v1/systemrelations/to/${systemUsageId}`)
                .then(response => {
                    var kitosSystemRelationResponse = response.data as { msg: string, response: [IItSystemUsageRelationDTO] }
                    return kitosSystemRelationResponse.response;
                });
        }
    }

    app.service("systemRelationService", SystemRelationService);
}