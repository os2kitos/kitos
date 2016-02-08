import IPageObject = require("../../../../Tests/object-wrappers/IPageObject.po");
import Select2Wrapper = require("../../../../Tests/object-wrappers/Select2Wrapper");
import SelectStatus2Wrapper = require("../../../../Tests/object-wrappers/SelectStatus2Wrapper");
import RepeaterWrapper = require("../../../../Tests/object-wrappers/RepeaterWrapper");

class ItContractEditTabEconomyPo implements IPageObject {
    public getPage(): webdriver.promise.Promise<void> {
        return browser.get("https://localhost:44300/#/contract/edit/1/economy");
    }

    // extern repeater
    public externRepeater = new RepeaterWrapper("stream in externEconomyStreams");

    // externOrgUnit selector
    public externOrgUnitSelector = new Select2Wrapper(".extern-org-unit .select2-container");

    // externAcquisition locator
    public externAcquisitionLocator = by.css(".extern-acquisition");

    // externOperation locator
    public externOperationLocator = by.css(".extern-operation");

    // externOther locator
    public externOtherLocator = by.css(".extern-other");

    // externAccountingEntry locator
    public externAccountingEntryLocator = by.css(".extern-accounting-entry");

    // externAuditStatus selector
    public externAuditStatus = new SelectStatus2Wrapper(".extern-audit-status");

    // externAuditDate locator
    public externAuditDateLocator = by.css("input.extern-audit-date");

    // externNote locator
    public externNoteLocator = by.css(".extern-note");

    // externDelete locator
    public externDeleteLocator = by.css(".extern-delete");

    // new extern button
    public newExternButton = element(by.css(".new-extern"));

    // intern repeater
    public internRepeater = new RepeaterWrapper("stream in internEconomyStreams");

    // internOrgUnit selector
    public internOrgUnitSelector = new Select2Wrapper(".intern-org-unit .select2-container");

    // internAcquisition locator
    public internAcquisitionLocator = by.css(".intern-acquisition");

    // internOperation locator
    public internOperationLocator = by.css(".intern-operation");

    // internOther locator
    public internOtherLocator = by.css(".intern-other");

    // internAccountingEntry locator
    public internAccountingEntryLocator = by.css(".intern-accounting-entry");

    // internAuditStatus selector
    public internAuditStatus = new SelectStatus2Wrapper(".intern-audit-status");

    // internAuditDate locator
    public internAuditDateLocator = by.css("input.intern-audit-date");

    // internNote locator
    public internNoteLocator = by.css(".intern-note");

    // internDelete locator
    public internDeleteLocator = by.css(".intern-delete");

    // new intern button
    public newInternButton = element(by.css(".new-intern"));
}

export = ItContractEditTabEconomyPo;
