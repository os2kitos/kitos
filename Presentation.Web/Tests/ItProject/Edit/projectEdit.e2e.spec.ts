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

    it('should save when name looses focus', () => {
        // arrange
        pageObject.nameInput = 'SomeName';

        // act
        pageObject.idElement.click();

        // assert
        mock.requestsMade()
            .then((requests: Array<any>) => {
                var lastRequest = requests[requests.length - 1];

                expect(lastRequest.method).toBe('PATCH');
                expect(lastRequest.url).toMatch('api/itproject/1');
            });
    });

    afterEach(() => {
        mock.teardown();
        browserHelper.outputLog();
    });
});
