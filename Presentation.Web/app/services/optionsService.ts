module Kitos.Services {

    export interface IOptionsService {
        getLocalInterfaceTypes();
        getLocalDataTypes();
    }

    export class OptionsService implements IOptionsService {
        

        static $inject = ["$http", "_"];
        constructor(private readonly $http: ng.IHttpService, private readonly _: _.LoDashStatic) {
        }

        getLocalInterfaceTypes() {
            return this.$http
                .get<Models.IODataResult<Models.IOptionEntity>>("odata/LocalInterfaceTypes?$filter=IsLocallyAvailable eq true or IsObligatory&$orderby=Priority desc")
                .then(result => result.data.value);
        }

        getLocalDataTypes() {
            return this.$http
                .get<Models.IODataResult<Models.IOptionEntity>>("odata/LocalDataTypes?$filter=IsLocallyAvailable eq true or IsObligatory&$orderby=Priority desc")
                .then(result => result.data.value);
        }

    }
    app.service("optionsService", OptionsService);
}