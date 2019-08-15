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



}

export = TestFixtureWrapper;