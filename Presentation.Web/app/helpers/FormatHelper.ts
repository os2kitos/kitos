module Kitos.Helpers {
    export class FormatHelper {
        public static formatUserWithEmail(user: any): string {
            var result = '<div>' + user.text + '</div>';
            if (user.email) {
                result += '<div class="small">' + user.email + '</div>';
            }
            return result;
        }

        public static formatOrganizationWithCvr(org: any): string {
            var result = '<div>' + org.text + '</div>';
            if (org.cvr) {
                result += '<div class="small">' + org.cvr + '</div>';
            }
            return result;
        }
    }
}
