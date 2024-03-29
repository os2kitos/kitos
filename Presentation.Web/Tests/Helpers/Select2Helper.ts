﻿import WaitTimers = require("../Utility/WaitTimers");

class Select2Helper {
    private static waitUpTo = new WaitTimers();
    private static ec = protractor.ExpectedConditions;
    private static readonly selectInput = "select2-input";
    private static readonly selectDrop = "select2-drop";
    private static readonly selectChoice = "select2-choice";
    private static readonly selectChoices = "select2-choices";
    private static readonly selectResult = "select2-result-label";
    private static readonly disabledSelect2Class = "container-disabled";

    static waitForDataAndSelect() {
        console.log(`waitForSelect2DataAndSelect`);
        return browser.wait(this.ec.visibilityOf(element(by.className(Select2Helper.selectResult))), this.waitUpTo.twentySeconds)
            .then(() => element(by.id(Select2Helper.selectDrop)).element(by.className(Select2Helper.selectInput)).sendKeys(protractor.Key.ENTER))
            .then(() => browser.waitForAngular());
    }

    static searchFor(name: string, elementId: string) {
        console.log(`select2SearchFor: ${name}, in element with id: ${elementId}`);
        return element(by.id(elementId)).element(by.className(Select2Helper.selectChoice))
            .click()
            .then(() => browser.waitForAngular())
            .then(() => element(by.id(Select2Helper.selectDrop)).element(by.className(Select2Helper.selectInput)).click())
            .then(() => browser.waitForAngular())
            .then(() => element(by.id(Select2Helper.selectDrop)).element(by.className(Select2Helper.selectInput)).sendKeys(name))
            .then(() => browser.waitForAngular());
    }

    static searchForByParent(name: string, elementId: string, parent: protractor.ElementFinder) {
        console.log(`select2SearchFor: ${name}, in element with id: ${elementId} under parent: ${parent.getTagName().toString()}`);
        parent.element(by.id(elementId)).element(by.className(Select2Helper.selectChoice))
            .click()
            .then(() => browser.waitForAngular())
            .then(() => element(by.id(Select2Helper.selectDrop)).element(by.className(Select2Helper.selectInput)).click())
            .then(() => browser.waitForAngular())
            .then(() => element(by.id(Select2Helper.selectDrop)).element(by.className(Select2Helper.selectInput)).sendKeys(name))
            .then(() => browser.waitForAngular());
    }

    static selectNoSearchByParent(name: string, elementId: string, parent: protractor.ElementFinder) {
        console.log(`selectNoSearchByParent: ${name}, in element with id: ${elementId} under parent: ${parent.getTagName().toString()}`);
        parent.element(by.id(elementId)).element(by.className(Select2Helper.selectChoice))
            .click()
            .then(() => browser.waitForAngular())
            .then(() => this.findResult(name).first().click())
            .then(() => browser.waitForAngular())
            .then(() => console.log(`Selected ${name}`));
    }

    static select(name: string, elementId: string) {
        return this.searchFor(name, elementId)
            .then(() => this.waitForDataAndSelect())
            .then(() => console.log(`Found and selected ${name}`));
    }

    static selectWithNoSearch(name: string, elementId: string) {
        return element(by.id(elementId))
            .element(by.className(Select2Helper.selectChoice))
            .click()
            .then(() => browser.waitForAngular())
            .then(() => this.findResult(name).first().click())
            .then(() => browser.waitForAngular())
            .then(() => console.log(`Selected ${name}`));
    }

    static selectMultipleWithNoSearch(name: string, elementId: string) {
        return element(by.id(elementId))
            .element(by.className(Select2Helper.selectChoices))
            .click()
            .then(() => browser.waitForAngular())
            .then(() => this.findResult(name).first().click())
            .then(() => browser.waitForAngular())
            .then(() => console.log(`Selected ${name}`));
    }

    static getData(elementId: string) {
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

    static assertIsEnabled(findElement: () => protractor.ElementFinder, expectedState: boolean) {
        if (expectedState) {
            expect(findElement().getAttribute("class")).not.toContain(Select2Helper.disabledSelect2Class);
        } else {
            expect(findElement().getAttribute("class")).toContain(Select2Helper.disabledSelect2Class);
        }
    }
}
export = Select2Helper;