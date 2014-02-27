$.validator.setDefaults({
        ignore: "", //don't ignore the hidden select field
        errorClass: "has-error",
        validClass: "has-success",

        messages: {
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

        highlight: function(element, errorClass, validClass) {
            $(element).closest("div.form-group").addClass(errorClass);
        },
        unhighlight: function(elem, errorClass, validClass) {
            $(elem).closest("div.form-group").removeClass(errorClass);
        },
});

$.validator.messages.required = "Dette felt er påkrævet";