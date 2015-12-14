export class BrowserHelper {
    browser: protractor.IBrowser;

    constructor(browser: protractor.IBrowser) {
        this.browser = browser;
    }

    outputLog() {
        this.browser
            .manage()
            .logs()
            .get('browser')
            .then(browserLogs => {
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
    }
}
