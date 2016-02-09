import IPageObject = require("../../../../Tests/object-wrappers/IPageObject.po");
import Select2Wrapper = require("../../../../Tests/object-wrappers/Select2Wrapper");
import RepeaterWrapper = require("../../../../Tests/object-wrappers/RepeaterWrapper");

class ItContractEditTabDeadlinesPo implements IPageObject {
    public getPage(): webdriver.promise.Promise<void> {
        return browser.get("https://localhost:44300/#/contract/edit/1/deadlines");
    }

    // handoverTrial selector
    public handoverTrialSelector = new Select2Wrapper("#s2id_handover-trial");

    // handoverTrialExpectedDate input
    public handoverTrialExpectedDateElement = element(by.css("#handover-trial-expected-date"));
    public handoverTrialExpectedDateInput = (value: string) => {
        this.handoverTrialExpectedDateElement.sendKeys(value);
    }

    // handoverTrialApprovedDate input
    public handoverTrialApprovedDateElement = element(by.css("#handover-trial-approved-date"));
    public handoverTrialApprovedDateInput = (value: string) => {
        this.handoverTrialApprovedDateElement.sendKeys(value);
    }

    // handoverTrialAdd button
    public handoverTrialAddButton = element(by.css("#handover-trial-add"));

    // handoverTrials repeater
    public handoverTrialsRepeater = new RepeaterWrapper("ht in handoverTrials");

    // handoverTrial locator
    public handoverTrialLocator = new Select2Wrapper(".handover-trial.select2-container");

    // handoverTrialExpectedDate locator
    public handoverTrialExpectedDateLocator = by.css("input.handover-trial-expected-date");

    // handoverTrialApprovedDate locator
    public handoverTrialApprovedDateLocator = by.css("input.handover-trial-approved-date");

    // handoverTrialDelete locator
    public handoverTrialDeleteLocator = by.css(".handover-trial-delete");

    // paymentMilestoneMilestone input
    public paymentMilestoneElement = element(by.css("#payment-milestone"));
    public paymentMilestoneInput = (value: string) => {
        this.paymentMilestoneElement.sendKeys(value);
    }

    // paymentMilestoneExpectedDate input
    public paymentMilestoneExpectedDateElement = element(by.css("#payment-milestone-expected-date"));
    public paymentMilestoneExpectedDateInput = (value: string) => {
        this.paymentMilestoneExpectedDateElement.sendKeys(value);
    }

    // paymentMilestoneApprovedDate input
    public paymentMilestoneApprovedDateElement = element(by.css("#payment-milestone-approved-date"));
    public paymentMilestoneApprovedDateInput = (value: string) => {
        this.paymentMilestoneApprovedDateElement.sendKeys(value);
    }

    // paymentMilestoneAdd button
    public paymentMilestoneAddButton = element(by.css("#payment-milestone-add"));

    // paymentMilestones repeater
    public paymentMilestonesRepeater = new RepeaterWrapper("item in paymentMilestones");

    // paymentMilestone locator
    public paymentMilestoneLocator = by.css(".payment-milestone");

    // paymentMilestoneExpectedDate locator
    public paymentMilestoneExpectedDateLocator = by.css(".payment-milestone-expected-date input");

    // paymentMilestoneApprovedDate locator
    public paymentMilestoneApprovedDateLocator = by.css(".payment-milestone-approved-date input");

    // paymentMilestoneDelete locator
    public paymentMilestoneDeleteLocator = by.css(".payment-milestone-delete");

    // agreementConcluded input
    public agreementConcludedElement = element(by.css("#agreement-concluded"));
    public agreementConcludedInput = (value: string) => {
        this.agreementConcludedElement.sendKeys(value);
    }

    // agreementDuration selector
    public agreementDurationSelector = new Select2Wrapper("#s2id_agreement-duration");

    // agreementOptionExtend selector
    public agreementOptionExtendSelector = new Select2Wrapper("#s2id_agreement-option-extend");

    // agreementOptionExtendMultiplier input
    public agreementOptionExtendMultiplierElement = element(by.css("#agreement-option-extend-multiplier"));
    public agreementOptionExtendMultiplierInput = (value: string) => {
        this.agreementOptionExtendMultiplierElement.sendKeys(value);
    }

    // agreementIrrevocable input
    public agreementIrrevocableElement = element(by.css("#agreement-irrevocable"));
    public agreementIrrevocableInput = (value: string) => {
        this.agreementIrrevocableElement.sendKeys(value);
    }

    // agreementExpiration input
    public agreementExpirationElement = element(by.css("#agreement-expiration"));
    public agreementExpirationInput = (value: string) => {
        this.agreementExpirationElement.sendKeys(value);
    }

    // agreementTerminated input
    public agreementTerminatedElement = element(by.css("#agreement-terminated"));
    public agreementTerminatedInput = (value: string) => {
        this.agreementTerminatedElement.sendKeys(value);
    }

    // agreementNotice selector
    public agreementNoticeSelector = new Select2Wrapper("#s2id_agreement-notice");
}

export = ItContractEditTabDeadlinesPo;
