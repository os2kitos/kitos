module Kitos.Utility {
    export class csrfToken {
        static addHeader() {
            const csrfHeader = {
                headers: {
                    "X-XSRF-Token": angular.element("input[name='__RequestVerificationToken']").val()
                }
            };
            return csrfHeader;
        }
    }
}