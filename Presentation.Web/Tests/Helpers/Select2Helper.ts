import WaitTimers = require("../Utility/WaitTimers");

class Select2Helper {
    private static waitUpTo = new WaitTimers();
    private static ec = protractor.ExpectedConditions;


    public static waitForDataAndSelect() {
        console.log(`waitForSelect2DataAndSelect`);
        return browser.wait(this.ec.visibilityOf(element(by.className("select2-result-label"))), this.waitUpTo.twentySeconds)
            .then(() => element(by.id("select2-drop")).element(by.className("select2-input")).sendKeys(protractor.Key.ENTER));
    }

    public static searchFor(name: string, elementId: string) {
        console.log(`select2SearchFor: ${name}, in element with id: ${elementId}`);
        return element(by.id(elementId)).element(by.tagName('a')).click()
            .then(() => console.log("next"))
            .then(() => element(by.id("select2-drop")).element(by.className("select2-input")).click())
            .then(() => element(by.id("select2-drop")).element(by.className("select2-input")).sendKeys(name));
    }

}

export = Select2Helper;