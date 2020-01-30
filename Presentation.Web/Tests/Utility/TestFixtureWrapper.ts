const defaultJasmineTimeout = jasmine.DEFAULT_TIMEOUT_INTERVAL;

class TestFixtureWrapper {
    public cleanupState() {
        browser.driver.manage().deleteAllCookies();
    }

    public disableAutoBrowserWaits() {
        browser.ignoreSynchronization = true;
    }

    public enableAutoBrowserWaits() {
        browser.ignoreSynchronization = false;
    }

    public enableLongRunningTest() {
        const minutes = 5;
        const mill = minutes * 60 * 1000;
        jasmine.DEFAULT_TIMEOUT_INTERVAL = mill;
        browser.manage().timeouts().setScriptTimeout(mill);
        browser.manage().timeouts().pageLoadTimeout(mill);
    }

    public disableLongRunningTest() {
        const seconds = 11;
        const mill = seconds * 1000;
        jasmine.DEFAULT_TIMEOUT_INTERVAL = defaultJasmineTimeout;
        browser.manage().timeouts().setScriptTimeout(mill);
        browser.manage().timeouts().pageLoadTimeout(mill);
    }

    public longRunningSetup() {
        const minutes = 5;
        return minutes * 60 * 1000;
    }

}

export = TestFixtureWrapper;