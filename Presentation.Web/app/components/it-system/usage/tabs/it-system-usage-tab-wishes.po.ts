import IPageObject = require("../../../../../Tests/object-wrappers/IPageObject.po");
import RepeaterWrapper = require("../../../../../Tests/object-wrappers/RepeaterWrapper");

class ItSystemUsageTabWishesPo implements IPageObject {
    public getPage(): webdriver.promise.Promise<void> {
        return browser.get("https://localhost:44300/#/system/usage/1/wishes");
    }

    // wish repeater
    public wishRepeater = new RepeaterWrapper("wish in wishes");
    public deleteWishLocator = by.css("tr a.delete-wish");

    // text input
    public textElement = element(by.css("#wish-text"));
    public textInput = (value: string) => {
        this.textElement.sendKeys(value);
    }

    // saveWish button
    public saveWishButton= element(by.css("#save-wish"));
}

export = ItSystemUsageTabWishesPo;
