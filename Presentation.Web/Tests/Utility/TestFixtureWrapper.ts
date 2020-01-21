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
        jasmine.DEFAULT_TIMEOUT_INTERVAL = minutes * 60 * 1000;
    }

    public disableLongRunningTest() {
        jasmine.DEFAULT_TIMEOUT_INTERVAL = defaultJasmineTimeout;
    }
}

export = TestFixtureWrapper;