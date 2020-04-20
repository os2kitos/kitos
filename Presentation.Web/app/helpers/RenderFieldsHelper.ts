﻿module Kitos.Helpers {

    export class RenderFieldsHelper {

        private static readonly noValueFallback = "";

        static renderReferenceUrl(reference: Models.Reference.IOdataReference) {
            if (reference === null || _.isUndefined(reference)) {
                return RenderFieldsHelper.noValueFallback;
            }
            if (Utility.Validation.isValidExternalReference(reference.URL)) {
                return "<a target=\"_blank\" style=\"float:left;\" href=\"" + reference.URL + "\">" + reference.Title + "</a>";
            }
            if (reference.Title === null || _.isUndefined(reference.Title)) {
                return RenderFieldsHelper.noValueFallback;
            }
            return reference.Title;
        }

        static renderReferenceId(reference: Models.Reference.IOdataReference) {
            if (reference === null || _.isUndefined(reference)) {
                return RenderFieldsHelper.noValueFallback;
            }
            if (reference.ExternalReferenceId === null || _.isUndefined(reference.ExternalReferenceId)) {
                return RenderFieldsHelper.noValueFallback;
            }
            return reference.ExternalReferenceId;
        }
    }
}