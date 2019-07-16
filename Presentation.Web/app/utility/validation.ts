module Kitos.Utility
{
    export class Validation
    {
        static  validateUrl(url) {

            const regexp = /(http || https):\/\/(\w+:{0,1}\w*@)?(\S+)(:[0-9]+)?(\/|\/([\w#!:.?+=&%@!\-\/]))?/;
            return regexp.test(url.toLowerCase());

        }

    }

}