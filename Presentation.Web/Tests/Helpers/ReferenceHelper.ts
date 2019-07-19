import RefePage = require("../PageObjects/It-system/Tabs/ItSystemReference.po");
import CSSLocator = require("../object-wrappers/CSSLocatorHelper");
import Constants = require("../Utility/Constants");
import KendoToolBar = require("../object-wrappers/KendoToolbarWrapper")
import WaitTimers = require("../Utility/WaitTimers");

var homePage = new RefePage();
var waitUpTo = new WaitTimers();
var headerButtons = homePage.kendoToolbarWrapper.headerButtons();
var inputFields = homePage.kendoToolbarWrapper.inputFields();
var objects = homePage.kendoToolbarWrapper.columnObjects();

class ReferenceHelper {

    public createReference(title: string, url: string, id: string) {
        homePage.getPage();
        browser.wait(homePage.isCreateReferenceLoaded(), waitUpTo.twentySeconds);
        headerButtons.createReference.click();
        browser.wait(homePage.isReferenceCreateFormLoaded(), waitUpTo.twentySeconds);
        inputFields.referenceDocId.sendKeys(id);
        inputFields.referenceDocTitle.sendKeys(title);
        inputFields.referenceDocUrl.sendKeys(url);
        headerButtons.editSaveReference.click();
    }

    public deleteReference(id: string) {
        homePage.getPage();
        browser.wait(homePage.isCreateReferenceLoaded(), waitUpTo.twentySeconds);

        element.all(by.id("mainGrid")).all(by.tagName("tr")).each((ele) => {

            ele.all(by.tagName("td")).each((tdele) => {

                tdele.getText().then(val => {

                         if (val === id) {
                             ele.element(by.css("[data-element-type='" + "deleteReference" + "']")).click();
                             browser.switchTo().alert().accept();
                         }
                    });

               
            });


        });


    }


}

export = ReferenceHelper;


//referenceDocTitle referenceDocId referenceDocUrl