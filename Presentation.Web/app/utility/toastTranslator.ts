module Kitos.Utility {


    export class ToastTranslatorTool {

        static translateItSystemConflictResponse(response: string): string {

            switch (response) {
                case "InUse":
                    {
                        return "Systemet kan ikke slettes! Da Systemet er i brug";
                    }
                case "HasChildren":
                    {
                        return "Systemet kan ikke slettes! Da andre systemer afhænger af dette system";
                    }
                case "HasInterfaceExibits":
                    {
                        return "Systemet kan ikke slettes! Da en snitflade afhænger af dette system";
                    }

                default:
                    {
                        return "Systemet kan ikke slettes!";
                    }
            }
        }
    }
}