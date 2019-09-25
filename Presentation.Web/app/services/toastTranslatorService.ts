module Kitos.Services.System {
    "use strict";
    export class SystemDeletedErrorResponseTranslationService { 
        private errorMsg = Models.ItSystem.SystemDeleteErrorMessages;
        translateResponse = (httpErrorCode: number, actionErrorCode: string) => { 
            switch (httpErrorCode) {
                case 401:
                    return this.errorMsg.notAuthorized;
                case 403:
                    return this.errorMsg.noPermission;
                case 409:
                    return this.translate409Response(actionErrorCode);
                default:
                    return this.errorMsg.deleteDefault;
            }
        }

        private translate409Response(actionErrorCode: string) {
            switch (actionErrorCode.toLowerCase()) {
                case "inuse": {
                    return this.errorMsg.systemInUse;
                }
                case "haschildren": {
                    return this.errorMsg.systemDependsOnThis;
                }
                case "hasinterfaceexhibits": {
                    return this.errorMsg.interfaceDependsOnThis;
                }
                default: {
                    return this.errorMsg.deleteDefault;
                }
            }
        }
    }
    app.service("SystemDeletedErrorResponseTranslationService", SystemDeletedErrorResponseTranslationService);
}
