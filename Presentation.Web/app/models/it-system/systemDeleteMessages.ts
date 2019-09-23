module Kitos.Models.ItSystem {
    export class SystemDeleteMessages {
        public static errorMessagesSystemDelete509Status = [
            "Systemet kan ikke slettes! \nDa Systemet er i brug",
            "Systemet kan ikke slettes! \nDa andre systemer afhænger af dette system",
            "Systemet kan ikke slettes! \nDa en snitflade afhænger af dette system",
            "Systemet kan ikke slettes!"
        ];
    }
}