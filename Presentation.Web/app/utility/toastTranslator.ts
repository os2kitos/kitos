module Kitos.Utility {
    import SystemDeleteMessages = Models.ItSystem.SystemDeleteMessages;
    export class ToastTranslatorTool {
        static translateItSystemDeletionConflictResponse(response: string): string {
            var errorMsg = new SystemDeleteMessages();
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