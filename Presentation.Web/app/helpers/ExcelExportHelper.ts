module Kitos.Helpers {

    export class ExcelExportHelper {

        static renderReferenceUrl(reference: Models.Reference.IOdataReference) {
            if (reference == null) {
                return "";
            }
            if (Utility.Validation.validateUrl(reference.URL)) {
                return reference.URL;
            }
            return reference.Title;
        }

        static renderExternalReferenceId(reference: Models.Reference.IOdataReference) {
            if (reference == null) {
                return "";
            }
            if (reference.ExternalReferenceId != null) {
                return reference.ExternalReferenceId;
            }
            return reference.Title;
        }

        static renderUrlOrFallback(url, fallback) {
            if (Utility.Validation.validateUrl(url)) {
                return url;
            }
            if (fallback != null) {
                return fallback;
            }
            return "";
        }
    }
}