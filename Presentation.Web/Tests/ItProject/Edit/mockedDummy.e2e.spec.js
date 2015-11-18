var mock = require('protractor-http-mock');

describe('project edit view', function() {

    beforeEach(function() {
        mock();
    });

    it('should save when name looses focus', function () {
        browser.get('https://localhost:44300/#/project/overview');
        expect(true).toBe(true);
    });

    afterEach(function () {
        mock.teardown();

        // Ouput console errors from browser log
        browser.manage().logs().get('browser').then(function (browserLogs) {
            if (browserLogs && browserLogs.length > 0) {
                console.error('*** Browser console output ***');

                browserLogs.forEach(function(log) {
                    if (log.level.value > 900) {
                        console.log(log.message);
                    }
                });
            }
        });
    });
});
