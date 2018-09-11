(function () {
    function addApiKeyAuthorization() {
        var key = $('#input_apiKey')[0].value;
        if (key && key.trim() != "") {
            var apiKeyAuth = new SwaggerClient.ApiKeyAuthorization(swashbuckleConfig.apiKeyName, key, swashbuckleConfig.apiKeyIn);
            window.swaggerUi.api.clientAuthorizations.add("api_key", apiKeyAuth);
            log("added key " + key);
        }
    }
    $('#input_apiKey').change(addApiKeyAuthorization);
})();