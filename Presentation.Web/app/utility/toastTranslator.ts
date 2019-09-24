module Kitos.Utility {
    export class ToastTranslatorTool {
        static translateItSystemDeletionConflictResponse(response: string): string {
            var errorMsg = Models.ItSystem.SystemDeleteErrorMessages;
            switch (response.toLowerCase()) {
                case "inuse":{
                    return errorMsg.systemInUse;
                }
                case "haschildren":{
                    return errorMsg.systemDependsOnThis;
                    }
                case "hasinterfaceexhibits":{
                    return errorMsg.interfaceDependsOnThis;
                    }
                default:{
                    return errorMsg.deleteDefault;
                }
            }
        }
    }
}