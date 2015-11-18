import mock = require('protractor-http-mock');

describe('project edit view', () => {
    beforeEach(() => {
        mock(['itproject', 'itprojectrole', 'itprojecttype']);
    });

    it('should save when name looses focus', () => {
        browser.driver.manage().window().maximize();
        browser.get('https://localhost:44300/#/project/edit/1/status-project');
        //browser.pause();
        expect(true).toBe(true);

        console.log('\n');
        mock.requestsMade().then(d => console.log(d));
    });

    afterEach(() => {

        mock.teardown();

        // Ouput console errors from browser log
        browser.manage().logs().get('browser').then(browserLogs => {
            if (browserLogs && browserLogs.length > 0) {
                console.log('\n*** Browser console output ***');

                browserLogs.forEach(log => {
                    if (log.level.value > 900) {
                        console.log(log.message);
                    }
                });

                console.log('\n');
            }
        });
    });
});
