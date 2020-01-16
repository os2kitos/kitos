module Kitos.Utility {
    export class csrfToken {
        static addHeader() {
            const tokenVal = angular.element("input[id='__RequestVerificationToken']").val();
            const csrfHeader = {
                headers: {
                    "HEADER-XSRF-TOKEN": tokenVal
                }
            };
            return csrfHeader;
        }
    }
}