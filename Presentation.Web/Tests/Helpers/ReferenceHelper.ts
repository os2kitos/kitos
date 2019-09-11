import RefePage = require("../PageObjects/It-system/Tabs/ItSystemReference.po");
import WaitTimers = require("../Utility/WaitTimers");
import CSSLocator = require("../object-wrappers/CSSLocatorHelper");

class ReferenceHelper {
    private homePage = new RefePage();
    private waitUpTo = new WaitTimers();
    private headerButtons = new RefePage().kendoToolbarWrapper.headerButtons();
    private inputFields = new RefePage().kendoToolbarWrapper.inputFields();
    private cssLocator = new CSSLocator();

    public createReference(title: string, url: string, id: string) {
        return this.homePage.getPage()
            .then(() => browser.wait(this.homePage.isCreateReferenceLoaded(), this.waitUpTo.twentySeconds))
            .then(() => this.headerButtons.createReference.click())
            .then(() => browser.wait(this.homePage.isReferenceCreateFormLoaded(), this.waitUpTo.twentySeconds))
            .then(() => this.inputFields.referenceDocId.sendKeys(id))
            .then(() => this.inputFields.referenceDocTitle.sendKeys(title))
            .then(() => this.inputFields.referenceDocUrl.sendKeys(url))
            .then(() => this.headerButtons.editSaveReference.click());
    }

    public deleteReference(id: string) {
        return this.homePage.getPage()
            .then(() => browser.wait(this.homePage.isCreateReferenceLoaded(), this.waitUpTo.twentySeconds))
            .then(() => {
                element.all(by.id("mainGrid")).all(by.tagName("tr")).each((ele) => {
                    ele.all(by.tagName("td")).each((tdele) => {

                        tdele.getText().then(val => {

                            if (val === id) {
                                return ele.element(this.cssLocator.byDataElementType("deleteReference")).click()
                                    .then(() => browser.switchTo().alert().accept());
                            }
                        });
                    });
                });
            });
    }


}

export = ReferenceHelper;