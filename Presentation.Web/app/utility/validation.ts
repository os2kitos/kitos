module Kitos.Utility {
    export class Validation {
        static validateUrl(url: string): boolean {
            if (url == null) {
                return false;
            }
            const regexp = /(^https?):\/\/(\w+:{0,1}\w*@)?(\S+)(:[0-9]+)?(\/|\/([\w#!:.?+=&%@!\-\/])$)?/;
            return regexp.test(url.toLowerCase());
        }

    }

}