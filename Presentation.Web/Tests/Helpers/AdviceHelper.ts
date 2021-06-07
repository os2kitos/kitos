﻿import CSSLocator = require("../object-wrappers/CSSLocatorHelper");
import localSystemNavigation = require("./SideNavigation/LocalItSystemNavigation");
import systemUsageHelper = require("./SystemUsageHelper");
import Select2 = require("./Select2Helper");
import WaitTimers = require("../Utility/WaitTimers");
import Constants = require("../Utility/Constants");

class AdviceHelper {
    private cssLocator = new CSSLocator();
    private constants = new Constants();
    private waitUpTo = new WaitTimers();

    public goToSpecificItSystemAdvice(name: string) {   
        return systemUsageHelper.openLocalSystem(name)
            .then(() => localSystemNavigation.openAdvicePage()); 
    }

    public createNewRepetitionAdvice(email: string, startDate: string, endDate: string, subject: string, interval: string ) {
        return element(this.cssLocator.byDataElementType(this.constants.adviceNewButton)).click()
            .then(() => element(this.cssLocator.byDataElementType(this.constants.adviceEmailInput)).sendKeys(email))
            .then(() => element(this.cssLocator.byDataElementType(this.constants.adviceEmailCCInput))
                .sendKeys(email))
            .then(() => Select2.selectMultipleWithNoSearch("Systemejer", this.constants.adviceSelectReceivers))
            .then(() => Select2.selectMultipleWithNoSearch("Systemejer", this.constants.adviceSelectRoleCss))
            .then(() => Select2.selectWithNoSearch("Gentagelse", this.constants.adviceSelectType))
            .then(() => Select2.selectWithNoSearch(interval, this.constants.adviceSelectRepetition))
            .then(() => element(this.cssLocator.byDataElementType(this.constants.adviceStartDateField))
                .sendKeys(startDate))
            .then(() => element(this.cssLocator.byDataElementType(this.constants.adviceEndDateField))
                .sendKeys(endDate))
            .then(() => element(this.cssLocator.byDataElementType(this.constants.adviceSubjectInput)).sendKeys(subject))
            .then(() => element(this.cssLocator.byDataElementType(this.constants.adviceSaveButton)).click());
    }

    public createNewInstantAdvice(email: string, subject: string) {
        return element(this.cssLocator.byDataElementType(this.constants.adviceNewButton)).click()
            .then(() => element(this.cssLocator.byDataElementType(this.constants.adviceEmailInput)).sendKeys(email))
            .then(() => element(this.cssLocator.byDataElementType(this.constants.adviceEmailCCInput))
                .sendKeys(email))
            .then(() => Select2.selectMultipleWithNoSearch("Systemejer", this.constants.adviceSelectReceivers))
            .then(() => Select2.selectMultipleWithNoSearch("Systemejer", this.constants.adviceSelectRoleCss))
            .then(() => Select2.selectWithNoSearch("Straks", this.constants.adviceSelectType))
            .then(() => element(this.cssLocator.byDataElementType(this.constants.adviceSubjectInput)).sendKeys(subject))
            .then(() => element(this.cssLocator.byDataElementType(this.constants.adviceSaveButton)).click());
    }

    public deleteAdvice(subjectName: string) {
        console.log(`Deleting ${subjectName}`);
        return browser.wait(this.getDeleteButton(subjectName).isPresent(), this.waitUpTo.twentySeconds)
            .then(() => this.getDeleteButton(subjectName).click())
            .then(() => browser.switchTo().alert().accept());
    }

    public deactivateAdvice(subjectName: string) {
        console.log(`Deactivating ${subjectName}`);
        return browser.wait(this.getEditAdviceButton(subjectName).isPresent(), this.waitUpTo.twentySeconds)
            .then(() => this.getEditAdviceButton(subjectName).click())
            .then(() => element(this.cssLocator.byDataElementType('adviceDeactivateButton')).click())
            .then(() => browser.switchTo().alert().accept());
    }

    private getDeleteButton(subjectName: string) {
        return element(by.xpath(`.//*[@id='mainGrid']//span[text() = '${subjectName}']/../../*//button[@data-element-type='deleteAdviceButton']`));
    }

    private getEditAdviceButton(subjectName: string) {
        return element(by.xpath(`.//*[@id='mainGrid']//span[text() = '${subjectName}']/../../*//button[@data-element-type='editAdviceButton']`));
    }
}
export = AdviceHelper;
