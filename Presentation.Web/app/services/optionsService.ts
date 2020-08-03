module Kitos.Services {
    //TODO-MRJ_FRONTEND: Use the new local options type
    export interface IOptionsService {
        getLocalInterfaceTypes();
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

    }
    app.service("optionsService", OptionsService);
}