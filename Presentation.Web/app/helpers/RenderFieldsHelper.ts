module Kitos.Helpers {

    export class UrlRenderHelper {

        private static readonly noValueFallback = "";

        static renderReferenceUrl(reference: Models.Reference.IOdataReference) {
            if (reference === null || _.isUndefined(reference)) {
                return UrlRenderHelper.noValueFallback;
            }
            if (Utility.Validation.isValidExternalReference(reference.URL)) {
                return "<a target=\"_blank\" style=\"float:left;\" href=\"" + reference.URL + "\">" + reference.Title + "</a>";
            }
            return reference.Title;
        }
    }
}