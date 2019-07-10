import login = require("../Helpers/LoginHelper");
import homePage = require("../PageObjects/HomePage/HomePage.po")

var pageObject = new homePage();
var navigationDropdown = pageObject.navigationDropdownMenu();
var loginHelper = new login();

describe("Can login succesfully", () => { 

    beforeEach(() => {
        pageObject.getPage();
    });

    afterEach(() => {
        navigationDropdown.logOut.click();
    })

    it("As global admin", () => {
        loginHelper.loginAsGlobalAdmin();
        navigationDropdown.dropDownElement.click();
        expect(navigationDropdown.myProfile.getText()).toEqual("Min profil");
    });

    it("As local admin", () => {
        loginHelper.loginAsLocalAdmin();
        navigationDropdown.dropDownElement.click();
        expect(navigationDropdown.myProfile.getText()).toEqual("Min profil");
    });

    it("As regular user", () => {
        loginHelper.loginAsRegularUser();
        navigationDropdown.dropDownElement.click();
        expect(navigationDropdown.myProfile.getText()).toEqual("Min profil");
    });

});