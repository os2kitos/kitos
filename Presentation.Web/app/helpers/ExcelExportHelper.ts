module Kitos.Helpers {

    export class ExcelExportHelper {

        static renderReference(reference: Models.Reference.IOdataReference) {
            if (reference == null) {
                return "";
            }
            if (Utility.Validation.validateUrl(reference.URL)) {
                return reference.URL;
            }
            return reference.Title;
        }

        static renderExternalReference(reference: Models.Reference.IOdataReference) {
            if (reference == null) {
                return "";
            }
            if (Utility.Validation.validateUrl(reference.ExternalReferenceId)) {
                return reference.ExternalReferenceId;
            }
            return reference.Title;
        }
    }
}