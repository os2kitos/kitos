class NavigationHelper {

    getPage(destUrl: string): webdriver.promise.Promise<void> {
        return browser.getCurrentUrl()
            .then(url => {
                const navigateToUrl = browser.params.baseUrl + destUrl;
                if (navigateToUrl !== url) {
                    console.log("Not at " + navigateToUrl + " but at:" + url + ". Navigating to:" + navigateToUrl);
                    return browser.get(navigateToUrl)
                        .then(() => browser.waitForAngular());
                } else {
                    console.log("Already at " + navigateToUrl + ". Ignoring command");
                }
            });
    }

    getPageByFullUrl(fullUrl: string): webdriver.promise.Promise<void> {
        return this.getPage(fullUrl.substr(browser.params.baseUrl.length));
    }

    refreshPage(): webdriver.promise.Promise<void> {
        return browser.refresh()
            .then(() => browser.waitForAngular());
    }

    acceptAlertBox(): webdriver.promise.Promise<void> {
        return browser.switchTo().alert().accept();
    }

    findSubMenuElement(srefName: string) {
        console.log("find sub menu ", srefName);
        return element(by.css(`[data-ui-sref="${srefName}"`));
    }

    goToSubMenuElement(srefName: string) {
        console.log("Navigating to sub menu ", srefName);
        return this
            .findSubMenuElement(srefName)
            .click()
            .then(() => browser.waitForAngular());
    }
}
export = NavigationHelper;