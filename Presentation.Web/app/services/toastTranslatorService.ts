module Kitos.Services {
    "use strict";
    export class ToastTranslatorService {

        translateResponse = (name: string) => {
            return Utility.ToastTranslatorTool.translateItSystemDeletionConflictResponse(name);
        }
    }
    app.service("toastTranslatorService", ToastTranslatorService);
}
