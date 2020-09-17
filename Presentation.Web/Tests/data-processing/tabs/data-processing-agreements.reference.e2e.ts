import login = require("../../Helpers/LoginHelper");
import ReferenceHelper = require("../../Helpers/ReferenceHelper");
import TestFixtureWrapper = require("../../Utility/TestFixtureWrapper");
import DataProcessingAgreementOverviewPageObject = require("../../PageObjects/Data-Processing/data-processing-agreement.overview.po");
import DataProcessingHelper = require("../../Helpers/DataProcessingAgreementHelper")
import DataProcessingAgreementEditReferencePageObject =
    require("../../PageObjects/Data-Processing/Tabs/data-processing-agreement.edit.reference.po");

describe("Data Processing agreement reference test ",
    () => {
        var loginHelper = new login();
        const pageObjectOverview = new DataProcessingAgreementOverviewPageObject();
        const pageObjectReference = new DataProcessingAgreementEditReferencePageObject();
        var refHelper = new ReferenceHelper();
        var testFixture = new TestFixtureWrapper();
        var dpaHelper = DataProcessingHelper;
        

        beforeAll(() => {
            loginHelper.loginAsLocalAdmin();
            testFixture.enableLongRunningTest();
            dpaHelper.checkAndEnableDpaModule();
        });

        afterAll(() => {
            testFixture.disableLongRunningTest();
            testFixture.cleanupState();
        });

        it("Add and delete reference to a agreement",() => {
                var referenceName = createReferenceName();
                var referenceId = createReferenceId();
                var validUrl = generateValidUrl();
                var agreementName = createAgreementName();

                dpaHelper.createDataProcessingAgreement(agreementName)
                    .then(() => pageObjectOverview.findSpecificDpaInNameColumn(agreementName))
                    .then(() => dpaHelper.goToSpecificDataProcessingAgreement(agreementName))
                    .then(() => pageObjectReference.getDpaReferenceTabButton().click())
                    .then(() => refHelper.createReference(referenceName, validUrl, referenceId))
                    .then(() => expect(refHelper.getReferenceId(referenceName).getText()).toEqual(referenceId))
                    .then(() => expect(refHelper.getUrlFromReference(referenceName).getAttribute("href")).toEqual(validUrl))
                    .then(() => refHelper.getDeleteButtonFromReference(referenceName).click())
                    .then(() => browser.switchTo().alert().accept())
                    .then(() => expect(refHelper.getReferenceId(referenceName).isPresent()).toBeFalse());
            });

        //it("Delete a reference in a agreement",
        //    () => {
        //        var referenceName = createReferenceName();
        //        var referenceId = createReferenceId();
        //        var validUrl = generateValidUrl();
        //        var agreementName = createAgreementName();

        //        dpaHelper.createDataProcessingAgreement(agreementName)
        //            .then(() => pageObjectOverview.findSpecificDpaInNameColumn(agreementName))
        //            .then(() => dpaHelper.goToSpecificDataProcessingAgreement(agreementName))
        //            .then(() => pageObjectReference.getDpaReferenceTabButton().click())
        //            .then(() => refHelper.createReference(referenceName, validUrl, referenceId))

        //        refHelper.goToSpecificItSystemReferences(itSystemName)
        //            .then(() => refHelper.createReference(referenceName, validUrl, referenceId))
        //            .then(() => getDeleteButtonFromReference(referenceName).click())
        //            .then(() => browser.switchTo().alert().accept())
        //            .then(() => expect(getReferenceId(referenceId).isPresent()).toBeFalse());
        //    });



        //it("Edit a reference in a agreement",
        //    () => {
        //        var referenceName = createReferenceName();
        //        var referenceId = createReferenceId();
        //        var validUrl = generateValidUrl();
        //        var invalidUrl = generateInvalidUrl();

        //        refHelper.goToSpecificItSystemReferences(itSystemName)
        //            .then(() => refHelper.createReference(referenceName, validUrl, referenceId))
        //            .then(() => expect(getEditButtonFromReference(referenceName).isPresent()).toBe(true))
        //            .then(() => getEditButtonFromReference(referenceName).click())
        //            .then(() => inputFields.referenceDocUrl.clear())
        //            .then(() => inputFields.referenceDocUrl.sendKeys(invalidUrl))
        //            .then(() => headerButtons.editSaveReference.click())
        //            .then(() => expect(getUrlFromReference(referenceName).isPresent()).toBeFalsy());
        //    });

    });

function createAgreementName() {
    return `DpaRefTest${new Date().getTime()}`;
}

function createReferenceName() {
    return `Reference${new Date().getTime()}`;
}

function createReferenceId() {
    return `ID${new Date().getTime()}`;
}

function generateValidUrl() {
    return `http://www.${new Date().getTime()}.dk/`;
}

function generateInvalidUrl() {
    return `${new Date().getTime()}.dk/`;
}


