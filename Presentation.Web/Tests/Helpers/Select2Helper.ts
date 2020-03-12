import WaitTimers = require("../Utility/WaitTimers");

class Select2Helper {
    private static waitUpTo = new WaitTimers();
    private static ec = protractor.ExpectedConditions;
    private static readonly selectInput = "select2-input";
    private static readonly selectDrop = "select2-drop";
    private static readonly selectChoice = "select2-choice";
    private static readonly selectResult = "select2-result-label";
    private static readonly disabledSelect2Class = "container-disabled";

    public static waitForDataAndSelect() {
        console.log(`waitForSelect2DataAndSelect`);
        return browser.wait(this.ec.visibilityOf(element(by.className(Select2Helper.selectResult))), this.waitUpTo.twentySeconds)
            .then(() => element(by.id(Select2Helper.selectDrop)).element(by.className(Select2Helper.selectInput)).sendKeys(protractor.Key.ENTER));
    }

    public static searchFor(name: string, elementId: string) {
        console.log(`select2SearchFor: ${name}, in element with id: ${elementId}`);
        return element(by.id(elementId)).element(by.className(Select2Helper.selectChoice))
            .click()
            .then(() => element(by.id(Select2Helper.selectDrop)).element(by.className(Select2Helper.selectInput)).click())
            .then(() => element(by.id(Select2Helper.selectDrop)).element(by.className(Select2Helper.selectInput)).sendKeys(name));
    }

    public static select(name: string, elementId: string) {
        return this.searchFor(name, elementId)
            .then(() => this.waitForDataAndSelect())
            .then(() => console.log(`Found and selected ${name}`));
    }

    public static selectWithNoSearch(name: string, elementId: string) {
        return element(by.id(elementId))
            .element(by.className(Select2Helper.selectChoice))
            .click()
            .then(() => this.findResult(name).first().click())
            .then(() => console.log(`Selected ${name}`));
    }

    public static getData(elementId: string) {
        console.log(`Finding value in ${elementId}`);
        return element(by.xpath(`//div[@id  = "${elementId}"]/child::*//span[@class = "select2-chosen"]`));
    }

    private static findResult(name: string) {
        console.log(`Finding ${name} in no search select2 result list`);
        return element(by.id("select2-drop"))
            .element(by.tagName("ul"))
            .all(by.tagName("li"))
            .filter((elem) => {
                return elem.element(by.tagName("div"))
                    .getText()
                    .then((val) => val === name);
            });
    }

    public static assertIsEnabled(findElement: () => protractor.ElementFinder, expectedState: boolean) {
        if (expectedState) {
            expect(findElement().getAttribute("class")).not.toContain(Select2Helper.disabledSelect2Class);
        } else {
            expect(findElement().getAttribute("class")).toContain(Select2Helper.disabledSelect2Class);
        }
    }
}
export = Select2Helper;