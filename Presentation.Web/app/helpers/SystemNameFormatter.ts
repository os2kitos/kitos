module Kitos.Helpers {

    export class SystemNameFormat {

        private static readonly noValueFallback = "";
        private static readonly inactiveSystemNameSuffix = " (Ikke aktivt)"; // the space is intentional as it is a suffix to an existing name

        static apply(name: String, disabled : boolean) {
            if (!name) {
                return SystemNameFormat.noValueFallback;
            }
            return name + (disabled ? SystemNameFormat.inactiveSystemNameSuffix : "");
        }
    }

    export class InterfaceNameFormat {

        private static readonly noValueFallback = "";
        private static readonly inactiveSystemNameSuffix = " (Ikke aktiv)";

        static apply(name: String, disabled: boolean) {
            if (!name) {
                return InterfaceNameFormat.noValueFallback;
            }
            return name + (disabled ? InterfaceNameFormat.inactiveSystemNameSuffix : "");
        }
    }
}