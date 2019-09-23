module Kitos.Utility {
    import SystemDeleteMessages = Models.ItSystem.SystemDeleteMessages;

    export class ToastTranslatorTool {
        static translateItSystemDeletionConflictResponse(response: string): string {
            switch (response) {
                case "InUse":{
                        return SystemDeleteMessages.errorMessagesSystemDelete509Status[0];
                    }
                case "HasChildren":{
                    return SystemDeleteMessages.errorMessagesSystemDelete509Status[1];
                    }
                case "HasInterfaceExhibits":{
                    return SystemDeleteMessages.errorMessagesSystemDelete509Status[2];
                    }
                default:{
                    return SystemDeleteMessages.errorMessagesSystemDelete509Status[3];
                }
            }
        }
    }


}