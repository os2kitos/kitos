import PageObject = require("../PageObjects/IPageObject.po");

class NavigationHelper {

    public getPage(destUrl: string): webdriver.promise.Promise<void> {
        return browser.getCurrentUrl()
            .then(url => {
                const navigateToUrl = browser.baseUrl + destUrl;
                if (navigateToUrl !== url) {
                    console.log("Not at " + navigateToUrl + " but at:" + url + ". Navigating to:" + navigateToUrl);
                    return browser.get(navigateToUrl);
                } else {
                    console.log("Already at " + navigateToUrl + ". Ignoring command");
                }
            });
    }

    public refreshPage(): webdriver.promise.Promise<void> {
        return browser.refresh();
    }
}
export = NavigationHelper;