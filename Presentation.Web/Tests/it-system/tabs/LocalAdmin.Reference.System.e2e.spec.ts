import login = require("../../Helpers/LoginHelper");
import HomePage = require("../../PageObjects/it-system/tabs/ItSystemReference.po");
import ReferenceHelper = require("../../Helpers/ReferenceHelper");
import TestFixtureWrapper = require("../../Utility/TestFixtureWrapper");
import ItSystemHelper = require("../../Helpers/SystemCatalogHelper");

var loginHelper = new login();
var homePage = new HomePage();
var refHelper = new ReferenceHelper();
var testFixture = new TestFixtureWrapper();
var headerButtons = homePage.kendoToolbarWrapper.headerButtons();
var inputFields = homePage.kendoToolbarWrapper.inputFields();


describe("Local Admin is able to create,edit and delete",
    () => {
        beforeAll(() => {
        });

        beforeEach(() => {
            testFixture.enableLongRunningTest();
            homePage.getPage();
        });

        afterAll(() => {
            testFixture.disableLongRunningTest();
            testFixture.cleanupState();
        });

        it("Can add, edit and delete a reference",
            () => {
                var itSystemName = createItSystemName();
                var referenceName = createReferenceName();
                var referenceId = createReferenceId();
                var validUrl = generateValidUrl();
                var invalidUrl = generateInvalidUrl();
                 
                loginHelper.loginAsGlobalAdmin()
                    .then(() => ItSystemHelper.createSystem(itSystemName))
                    .then(() => testFixture.cleanupState())
                    .then(() => loginHelper.loginAsLocalAdmin())
                    .then(() => refHelper.goToSpecificItSystemReferences(itSystemName))
                    .then(() => refHelper.createReference(referenceName, validUrl, referenceId))
                    .then(() => expect(getReferenceId(referenceName).getText()).toEqual(referenceId))
                    .then(() => expect(getUrlFromReference(referenceName).getAttribute("href")).toEqual(validUrl))
                    .then(() => expect(getEditButtonFromReference(referenceName).isPresent()).toBe(true))
                    .then(() => getEditButtonFromReference(referenceName).click())
                    .then(() => inputFields.referenceDocUrl.clear())
                    .then(() => inputFields.referenceDocUrl.sendKeys(invalidUrl))
                    .then(() => headerButtons.editSaveReference.click())
                    .then(() => expect(getUrlFromReference(referenceName).isPresent()).toBeFalsy())
                    .then(() => getDeleteButtonFromInvalidUrlReference(referenceName).click())
                    .then(() => browser.switchTo().alert().accept())
                    .then(() => expect(element.all(by.xpath('//*/tbody/tr'))).toBeEmptyArray());
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

function generateInvalidUrl()
{
    return `${new Date().getTime()}.dk/`;
}

function getReferenceId(refName : string) {
    return element(by.xpath('//*/tbody/*/td/a[text()="' + refName +'"]/parent::*/parent::*//td[@data-element-type="referenceIdObject"]'));
}

function getEditButtonFromReference(refName: string) {
    return element(by.xpath('//*/tbody/*/td/a[text()="'+refName+'"]/parent::*/parent::*//*/button[@data-element-type="editReference"]'));
}

function getDeleteButtonFromInvalidUrlReference(refName: string) {
    return element(by.xpath('//*/tbody/*/td[text()="' + refName + '"]/parent::*/parent::*//*/button[@data-element-type="deleteReference"]'));
}

function getUrlFromReference(refName: string) {
    return element(by.xpath('//*/tbody/*/td/a[text()="' + refName + '"]/parent::*/parent::*//td[@data-element-type="referenceObject"]/a'));
}
