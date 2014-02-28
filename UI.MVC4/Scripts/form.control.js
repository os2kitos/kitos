﻿/* form.control.js
 *
 * useful functions and default values for easier 
 * client side manipulation and validation of forms
 */

(function ($) {
    "use strict";

    $.validator.setDefaults({
        ignore: "", //don't ignore the hidden select field
        errorClass: "has-error",
        validClass: "has-success",

        messages: {
            required: "Dette felt er påkrævet", //this doesn't work???? See below
            email: "Indtast venligst en korrekt email-adresse",
            maxlength: $.validator.format("Maksimum {0} tegn."),
            minlength: $.validator.format("Minimum {0} tegn."),

            remote: "Please fix this field.",
            url: "Please enter a valid URL.",
            date: "Please enter a valid date.",
            dateISO: "Please enter a valid date (ISO).",
            number: "Please enter a valid number.",
            digits: "Please enter only digits.",
            creditcard: "Please enter a valid credit card number.",
            equalTo: "Please enter the same value again.",
            rangelength: $.validator.format("Please enter a value between {0} and {1} characters long."),
            range: $.validator.format("Please enter a value between {0} and {1}."),
            max: $.validator.format("Please enter a value less than or equal to {0}."),
            min: $.validator.format("Please enter a value greater than or equal to {0}.")
        },

        highlight: function (element, errorClass, validClass) {
            $(element).closest("div.form-group").addClass(errorClass);
        },
        unhighlight: function (elem, errorClass, validClass) {
            $(elem).closest("div.form-group").removeClass(errorClass);
        },
    });

    //For some reason, this have to be here instead of up in setDefaults() above
    $.validator.messages.required = "Dette felt er påkrævet";

    //disable all submits on a form
    $.fn.disableSubmit = function () {
        this.find(":submit").attr("disabled", "disabled");
        return this;
    };

    //enable all submits on a form
    $.fn.enableSubmit = function () {
        this.find(":submit").removeAttr("disabled");
        return this;
    };

    //validates a form and submits an ajax call on successful validation
    $.fn.simpleAjax = function (options) {
        
        var settings = $.extend({
            type: "POST", //"GET", "PUT", etc
            map: {}, //map of "JSON id": "#html-id"
            pendingMessage: "Sender data...",
            successMessage: "Success!",
            failureMessage: "Fejl!",

            onFailure: function (result, failureMsg) {
                show_message(failureMsg);
            },
            onSuccess: function (result, successMsg) {
                show_message(successMsg);
            },

            preventMultipleSubmissions: true,
            resetFormOnSuccess: true,
        }, options);

        var form = this;
        
        form.validate({
            submitHandler: function () {

                if (settings.preventMultipleSubmissions)
                    form.disableSubmit();

                var data = {};
                $.each(settings.map, function (i, v) {
                    console.log(v);
                    data[i] = $(v).val();
                });
                
                $.ajax({
                    type: settings.type,
                    url: settings.url,
                    data: data,
                }).fail(function (result) {
                    settings.onFailure(result, settings.failureMessage);

                }).done(function (result) {
                    if (settings.resetFormOnSuccess)
                        form[0].reset();

                    settings.onSuccess(result, settings.successMessage);

                }).always(function (result) {
                    if (settings.preventMultipleSubmissions)
                        form.enableSubmit();
                });
            }
        });
    };

}(jQuery));