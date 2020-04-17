module Kitos.Utility {

    export class Validation {

        static validateUrl(url: string): boolean {
            if (url == null || _.isUndefined(url)) {
                return false;
            }
            const regexp = /(^https?):\/\/(\w+:{0,1}\w*@)?(\S+)(:[0-9]+)?(\/|\/([\w#!:.?+=&%@!\-\/])$)?/;
            return regexp.test(url.toLowerCase());
        }

        static isValidExternalReference(externalRef: string): boolean {
            if (externalRef == null || _.isUndefined(externalRef)) {
                return false;
            }
            if (this.validateUrl(externalRef)) {
                return true;
            } else {
                const regexp = /^(kmdsageraabn|kmdedhvis|sbsyslauncher):.*/;
                return regexp.test(externalRef.toLowerCase());
            }
        }
    }
}
