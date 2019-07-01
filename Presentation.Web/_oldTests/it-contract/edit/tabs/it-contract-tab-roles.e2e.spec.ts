import mock = require("protractor-http-mock");
import Helper = require("../../../helper");
import ItContractEditTabRolesPo = require("../../../../app/components/it-contract/tabs/it-contract-tab-roles.po");

describe("contract edit tab roles", () => {
    var mockHelper: Helper.Mock;
    var browserHelper: Helper.Browser;
    var pageObject: ItContractEditTabRolesPo;
    var mockDependencies: Array<string> = [
        "itContract",
        "contractType",
        "contractTemplate",
        "purchaseForm",
        "procurementStrategy",
        "agreementElement",
        "itContractRights",
        "itContractRole",
        "organizationunit",
        "organization"
    ];

    beforeEach(() => {
        browser.driver.manage().window().maximize();

        mockHelper = new Helper.Mock();
        browserHelper = new Helper.Browser(browser);
        pageObject = new ItContractEditTabRolesPo();
    });

    afterEach(() => {
        mock.teardown();
    });

    describe("with no write access", () => {
        beforeEach(done => {
            mock(["itContractNoWriteAccess"].concat(mockDependencies));
            pageObject.getPage()
                .then(() => mock.clearRequests())
                .then(() => done());
        });

        it("should hide new right fields", () => {
            // arrange

            // act

            // assert
            expect(pageObject.addRightRoleSelector.element).not.toBeVisible();
            expect(pageObject.addRightUserSelector.element).not.toBeVisible();
        });

        it("should hide edit and delete buttons on rights", () => {
            // arrange
            var editButton = element.all(pageObject.rightEditButtonLocator)
                .first();
            var deleteButton = element.all(pageObject.rightDeleteLocator)
                .first();

            // act

            // assert
            expect(editButton).not.toBeVisible();
            expect(deleteButton).not.toBeVisible();
        });
    });

    describe("with write access", () => {
        beforeEach(done => {
            mock(["itContractWriteAccess"].concat(mockDependencies));
            pageObject.getPage()
                .then(() => mock.clearRequests())
                .then(() => done());
        });

        it("should display edit field for contract signer when edit is clicked", () => {
            // arrange

            // act
            pageObject.editContractSignerButton
                .click();

            // assert
            expect(pageObject.contractSignerSelector.element).toBeVisible();
        });

        it("should save edited contract signer when save is clicked", () => {
            // arrange
            pageObject.editContractSignerButton
                .click();
            pageObject.contractSignerSelector
                .selectFirst("t");
            mock.clearRequests();

            // act
            pageObject.saveContractoSignerElement.click();

            // assert
            expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/itcontract/1" });
        });

        it("should init roles selector", () => {
            // arrange
            var element = pageObject.addRightRoleSelector;

            // act
            element.element.click();

            // assert
            expect(element.options.count()).toBeGreaterThan(0, "No options in selector");
        });

        it("should get users when typing", () => {
            // arrange
            var element = pageObject.addRightUserSelector;

            // act
            element.selectFirst("t");

            // assert
            expect(mock.requestsMade()).toMatchInRequests({ method: "GET", url: "api/organization" });
        });

        it("should save right when user is selected", () => {
            // arrange
            var element = pageObject.addRightUserSelector;

            // act
            element.selectFirst("t");

            // assert
            expect(mock.requestsMade()).toMatchInRequests({ method: "POST", url: "api/itcontractright/" });
        });

        it("should repeat rights", () => {
            // arrange

            // act

            // assert
            // two rows are created per right, one for display one for edit
            var count = pageObject.rightsRepeater.selectFirst(pageObject.rightRowLocator).count();
            expect(count).toBe(2);
        });

        it("should display edit field for rights when edit is clicked", () => {
            // arrange

            // act
            pageObject.rightsRepeater
                .selectFirst(pageObject.rightEditButtonLocator)
                .click();

            // assert
            // expect 2 as contract signer is always present
            var count = pageObject.rightsRepeater.selectFirst(pageObject.rightEditRoleInputLocator).count();
            expect(count).toBe(2);
        });

        it("should save edited field for rights when save is clicked", () => {
            // arrange
            pageObject.rightsRepeater
                .selectFirst(pageObject.rightEditButtonLocator)
                .click();
            mock.clearRequests();

            // act
            pageObject.rightsRepeater
                .selectFirst(pageObject.rightEditSaveButtonLocator)
                .click();

            // assert
            expect(mock.requestsMade()).toMatchInRequests({ method: "DELETE", url: "api/itcontractright" });
            expect(mock.requestsMade()).toMatchInRequests({ method: "POST", url: "api/itcontractright" });
        });

        it("should delete right when delete confirmed", () => {
            // arrange
            mock.clearRequests();
            pageObject.rightsRepeater
                .selectFirst(pageObject.rightDeleteLocator)
                .click();

            // act
            browserHelper.acceptAlert();

            // assert
            expect(mock.requestsMade()).toMatchInRequests({ method: "DELETE", url: "api/itcontractright" });
        });

        it("should not delete right when delete dismissed", () => {
            // arrange
            pageObject.rightsRepeater
                .selectFirst(pageObject.rightDeleteLocator)
                .click();

            // act
            browserHelper.dismissAlert();

            // assert
            expect(mock.requestsMade()).not.toMatchInRequests({ method: "DELETE", url: "api/itContractRight" });
        });
    });
});
