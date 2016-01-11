import mock = require('protractor-http-mock');
import Helper = require('../../helper');
import ItProjectEditPo = require('../../../app/components/it-project/it-project-edit.po');

describe('project edit view', () => {
    var browserHelper: Helper.BrowserHelper;
    var pageObject: ItProjectEditPo;

    beforeEach(() => {
        mock(['itproject', 'itprojectrole', 'itprojecttype', 'itprojectrights']);

        browserHelper = new Helper.BrowserHelper(browser);
        pageObject = new ItProjectEditPo();
        pageObject.getPage();

        browser.driver.manage().window().maximize();
    });

    afterEach(() => {
        mock.teardown();
        browserHelper.outputLog();
    });
});
