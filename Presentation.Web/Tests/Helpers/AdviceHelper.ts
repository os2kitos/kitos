import ReferencePage = require("../PageObjects/It-system/Tabs/ItSystemReference.po");
import WaitTimers = require("../Utility/WaitTimers");
import CSSLocator = require("../object-wrappers/CSSLocatorHelper");
import ItSystemCatalogPage = require("../PageObjects/it-system/Catalog/ItSystemCatalog.po");
import localSystemNavigation = require("./SideNavigation/LocalItSystemNavigation");
import systemUsageHelper = require("./SystemUsageHelper");
import Select2 = require("./Select2Helper");

class AdviceHelper {
    private homePage = new ReferencePage();
    private itSystemCatalogPage = new ItSystemCatalogPage();
    private waitUpTo = new WaitTimers();
    private headerButtons = new ReferencePage().kendoToolbarWrapper.headerButtons();
    private inputFields = new ReferencePage().kendoToolbarWrapper.inputFields();
    private cssLocator = new CSSLocator();

    private static readonly selectResult = "select2-result-label";


    public goToSpecificItSystemAdvice(name: string) {   
        return systemUsageHelper.openLocalSystem(name)
            .then(() => localSystemNavigation.openAdvicePage());
    }

    public createNewRepetitionAdvice(email: string, startDate: string, endDate: string, subject: string) {
        return element(this.cssLocator.byDataElementType("NewAdviceButton")).click()
            .then(() => element(this.cssLocator.byDataElementType("adviceEmailToInput")).sendKeys(email))
            .then(() => element(this.cssLocator.byDataElementType("adviceEmailCCToInput"))
                .sendKeys(email))
            .then(() => Select2.selectMultipleWithNoSearch("Systemejer", "s2id_role-receivers"))
            .then(() => Select2.selectMultipleWithNoSearch("Systemejer", "s2id_role-ccs"))
            .then(() => Select2.selectWithNoSearch("Gentagelse", "s2id_advisType"))
            .then(() => Select2.selectWithNoSearch("Uge", "s2id_adviceRepetition"))
            .then(() => element(this.cssLocator.byDataElementType("adviceStartDate"))
                .sendKeys(startDate))
            .then(() => element(this.cssLocator.byDataElementType("adviceEndDate"))
                .sendKeys(endDate))
            .then(() => element(this.cssLocator.byDataElementType("adviceSubject")).sendKeys(subject))
            .then(() => element(this.cssLocator.byDataElementType("adviceSaveButton")).click());
    }


    public createNewInstantAdvice(email: string, subject: string) {
        return element(this.cssLocator.byDataElementType("NewAdviceButton")).click()
            .then(() => element(this.cssLocator.byDataElementType("adviceEmailToInput")).sendKeys(email))
            .then(() => element(this.cssLocator.byDataElementType("adviceEmailCCToInput"))
                .sendKeys(email))
            .then(() => Select2.selectMultipleWithNoSearch("Systemejer", "s2id_role-receivers"))
            .then(() => Select2.selectMultipleWithNoSearch("Systemejer", "s2id_role-ccs"))
            .then(() => Select2.selectWithNoSearch("Straks", "s2id_advisType"))
            .then(() => element(this.cssLocator.byDataElementType("adviceSubject")).sendKeys(subject))
            .then(() => element(this.cssLocator.byDataElementType("adviceSaveButton")).click());
    }
}

export = AdviceHelper;
