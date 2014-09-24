(function(ng, app) {
    'use strict';

    app.provider('showErrorsConfig', function () {
        var _showSuccess;
        _showSuccess = false;
        this.showSuccess = function (showSuccess) {
            return _showSuccess = showSuccess;
        };
        this.$get = function () {
            return { showSuccess: _showSuccess };
        };
    });
});
