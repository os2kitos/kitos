module Kitos.Utility {


    export class ToastTranslatorTool {

        static translateItSystemDeletionConflictResponse(response: string): string {

            switch (response) {
                case "InUse":
                    {
                        return "Systemet kan ikke slettes! <br/>Da Systemet er i brug";
                    }
                case "HasChildren":
                    {
                        return "Systemet kan ikke slettes! <br/>Da andre systemer afhænger af dette system";
                    }
                case "HasInterfaceExhibits":
                    {
                        return "Systemet kan ikke slettes! <br/>Da en snitflade afhænger af dette system";
                    }

                default:
                    {
                        return "Systemet kan ikke slettes!";
                    }
            }
        }
    }
}