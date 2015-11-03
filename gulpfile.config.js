'use strict';
var GulpConfig = (function () {
    function gulpConfig() {
        this.source = 'Presentation.Web/Scripts/';
        this.sourceApp = this.source + 'app';

        this.allJavaScript = [this.sourceApp + '/**/*.js'];
        this.allTypeScript = this.sourceApp + '/**/*.ts';
    }
    return gulpConfig;
})();
module.exports = GulpConfig;