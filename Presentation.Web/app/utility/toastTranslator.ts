module Kitos.Utility {
    export class ToastTranslatorTool {
        static translateItSystemDeletionConflictResponse(response: string): string {
            var errorMsg = new Models.ItSystem.SystemDeleteMessages();
            switch (response) {
                case "InUse":
                {
                    return errorMsg.errorMessageSystemInUse;
                }
                case "HasChildren":{
                    return errorMsg.errorMessageSystemDependsOnThis;
                    }
                case "HasInterfaceExhibits":{
                    return errorMsg.errorMessageInterfaceDependsOnThis;
                    }
                default:{
                    return errorMsg.errorMessageDeleteDefault;
                }
            }
        }
    }


}