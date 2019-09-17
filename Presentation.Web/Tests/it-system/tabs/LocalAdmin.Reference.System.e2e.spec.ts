import login = require("../../Helpers/LoginHelper");
import ItSystemReferenceHelper = require("../../PageObjects/it-system/tabs/ItSystemReference.po");
import ReferenceHelper = require("../../Helpers/ReferenceHelper");
import TestFixtureWrapper = require("../../Utility/TestFixtureWrapper");
import ItSystemHelper = require("../../Helpers/SystemCatalogHelper");

var loginHelper = new login();
var itSystemReference = new ItSystemReferenceHelper();
var refHelper = new ReferenceHelper();
var testFixture = new TestFixtureWrapper();
var headerButtons = itSystemReference.kendoToolbarWrapper.headerButtons();
var inputFields = itSystemReference.kendoToolbarWrapper.inputFields();

describe("Local Admin is able to create,edit and delete",
    () => {
        var itSystemName = createItSystemName();

        beforeAll(() => {
            loginHelper.loginAsGlobalAdmin()
                .then(() => ItSystemHelper.createSystem(itSystemName))
                .then(() => testFixture.cleanupState());
        });

        beforeEach(() => {
            testFixture.cleanupState();
            testFixture.enableLongRunningTest();
        });

        afterAll(() => {
            testFixture.disableLongRunningTest();
            testFixture.cleanupState();
        });

        it("Can add reference to IT-System",
            () => {
                var referenceName = createReferenceName();
                var referenceId = createReferenceId();
                var validUrl = generateValidUrl();

                loginHelper.loginAsLocalAdmin()
                    .then(() => refHelper.goToSpecificItSystemReferences(itSystemName))
                    .then(() => refHelper.createReference(referenceName, validUrl, referenceId))
                    .then(() => expect(getReferenceId(referenceName).getText()).toEqual(referenceId))
                    .then(() => expect(getUrlFromReference(referenceName).getAttribute("href")).toEqual(validUrl));
            });

        it("Can edit a reference in a IT-System",
            () => {
                var referenceName = createReferenceName();
                var referenceId = createReferenceId();
                var validUrl = generateValidUrl();
                var invalidUrl = generateInvalidUrl();

                loginHelper.loginAsLocalAdmin()
                    .then(() => refHelper.goToSpecificItSystemReferences(itSystemName))
                    .then(() => refHelper.createReference(referenceName, validUrl, referenceId))
                    .then(() => expect(getEditButtonFromReference(referenceName).isPresent()).toBe(true))
                    .then(() => getEditButtonFromReference(referenceName).click())
                    .then(() => inputFields.referenceDocUrl.clear())
                    .then(() => inputFields.referenceDocUrl.sendKeys(invalidUrl))
                    .then(() => headerButtons.editSaveReference.click())
                    .then(() => expect(getUrlFromReference(referenceName).isPresent()).toBeFalsy());
            });

        it("Can delete a reference in a IT-system",
            () => {
                var referenceName = createReferenceName();
                var referenceId = createReferenceId();
                var validUrl = generateValidUrl();

                loginHelper.loginAsLocalAdmin()
                    .then(() => refHelper.goToSpecificItSystemReferences(itSystemName))
                    .then(() => refHelper.createReference(referenceName, validUrl, referenceId))
                    .then(() => getDeleteButtonFromReference(referenceName).click())
                    .then(() => browser.switchTo().alert().accept())
                    .then(() => expect(getReferenceId(referenceId).isPresent()).toBeFalse());
            });
    });

function createItSystemName() {
    return `ItSystemRefTest${new Date().getTime()}`;
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

function generateInvalidUrl(){
    return `${new Date().getTime()}.dk/`;
}

function getReferenceId(refName : string) {
    return element(by.xpath('//*/tbody/*/td/a[text()="' + refName +'"]/parent::*/parent::*//td[@data-element-type="referenceIdObject"]'));
}

function getEditButtonFromReference(refName: string) {
    return element(by.xpath('//*/tbody/*/td/a[text()="'+refName+'"]/parent::*/parent::*//*/button[@data-element-type="editReference"]'));
}

function getDeleteButtonFromReference(refName: string) {
    return element(by.xpath('//*/tbody/*/td/a[text()="' + refName + '"]/parent::*/parent::*//*/button[@data-element-type="deleteReference"]'));
}

function getUrlFromReference(refName: string) {
    return element(by.xpath('//*/tbody/*/td/a[text()="' + refName + '"]/parent::*/parent::*//td[@data-element-type="referenceObject"]/a'));
}
