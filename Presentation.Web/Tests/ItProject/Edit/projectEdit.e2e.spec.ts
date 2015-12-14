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

    it('should save when name looses focus', () => {
        // arrange
        pageObject.nameInput = 'SomeName';

        // act
        //pageObject.idElement.click();

        // assert
        mock.requestsMade()
            .then((requests: Array<any>) => {
                var lastRequest = requests[requests.length - 1];

                expect(lastRequest.method).toBe('PATCH');
                expect(lastRequest.url).toMatch('api/itproject/1');
            });
    });

    it('should save when type is changed', () => {
        // arrange

        // act
        pageObject.typeSelect.selectFirst("lo");

        // assert
        mock.requestsMade()
            .then((requests: Array<any>) => {
                var lastRequest = requests[requests.length - 1];
                console.log(lastRequest);
                expect(lastRequest.method).toBe('PATCH');
                expect(lastRequest.url).toMatch('api/itproject/1');
            });
    });

    /*
    it('should save when projectId looses focus', () => {
        // arrange
        pageObject.idInput = 'SomeId';

        // act
        pageObject.nameElement.click();

        // assert
        mock.requestsMade()
            .then((requests: Array<any>) => {
                var lastRequest = requests[requests.length - 1];

                expect(lastRequest.method).toBe('PATCH');
                expect(lastRequest.url).toMatch('api/itproject/1');
            });
    });

    it('should save when cmdb looses focus', () => {
        // arrange
        pageObject.cmdbInput = 'SomeCmdb';

        // act
        pageObject.nameElement.click();

        // assert
        mock.requestsMade()
            .then((requests: Array<any>) => {
                var lastRequest = requests[requests.length - 1];

                expect(lastRequest.method).toBe('PATCH');
                expect(lastRequest.url).toMatch('api/itproject/1');
            });
    });

    it('should save when projectEsdh looses focus', () => {
        // arrange
        pageObject.esdhInput = 'SomeEsdh';

        // act
        pageObject.nameElement.click();

        // assert
        mock.requestsMade()
            .then((requests: Array<any>) => {
                var lastRequest = requests[requests.length - 1];

                expect(lastRequest.method).toBe('PATCH');
                expect(lastRequest.url).toMatch('api/itproject/1');
            });
    });

    it('should save when projectFolder looses focus', () => {
        // arrange
        pageObject.folderInput = 'SomeFolder';

        // act
        pageObject.nameElement.click();

        // assert
        mock.requestsMade()
            .then((requests: Array<any>) => {
                var lastRequest = requests[requests.length - 1];

                expect(lastRequest.method).toBe('PATCH');
                expect(lastRequest.url).toMatch('api/itproject/1');
            });
    });

    it('should save when background looses focus', () => {
        // arrange
        pageObject.backgroundInput = 'SomeBackground';

        // act
        pageObject.nameElement.click();

        // assert
        mock.requestsMade()
            .then((requests: Array<any>) => {
                var lastRequest = requests[requests.length - 1];

                expect(lastRequest.method).toBe('PATCH');
                expect(lastRequest.url).toMatch('api/itproject/1');
            });
    });

    it('should save when note looses focus', () => {
        // arrange
        pageObject.noteInput = 'SomeNote';

        // act
        pageObject.nameElement.click();

        // assert
        mock.requestsMade()
            .then((requests: Array<any>) => {
                var lastRequest = requests[requests.length - 1];

                expect(lastRequest.method).toBe('PATCH');
                expect(lastRequest.url).toMatch('api/itproject/1');
            });
    });*/
});
