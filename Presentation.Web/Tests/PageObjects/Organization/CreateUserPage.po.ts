﻿import IPageObject = require("../IPageObject.po");
import KendoToolbarHelper = require("../../Helpers/KendoToolbarHelper");
import KendoToolbarWrapper = require("../../object-wrappers/KendoToolbarWrapper")

class UsersPage implements IPageObject {

    public getPage(): webdriver.promise.Promise<void> {
        return browser.get(browser.baseUrl + "/#/organization/user");
    }

    public kendoToolbarHelper = new KendoToolbarHelper();
    public kendoToolbarWrapper = new KendoToolbarWrapper();

    public inputEmail = element(by.model("ctrl.vm.email"));
    public inputEmailRepeat = element(by.model("ctrl.repeatEmail"));
    public inputName = element(by.model("ctrl.vm.name"));
    public inputLastName = element(by.model("ctrl.vm.lastName"));
    public inputPhone = element(by.model("ctrl.vm.phoneNumber"));
    public boolApi = element(by.model("ctrl.vm.hasApi"));
    public boolLocalAdmin = element(by.model("ctrl.vm.isLocalAdmin"));
    public boolReadOnly = element(by.model("ctrl.vm.isReadOnly"));
    public createUserButton = element(by.buttonText("Opret bruger"));
    public editUserButton = element(by.buttonText("Gem ændringer"));
    public cancelEditUserButton = element(by.buttonText("Annuller"));
}

export = UsersPage;