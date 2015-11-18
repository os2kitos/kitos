//var httpBackendMock = require('http-backend-proxy');

class ProjectEditPo {
    nameInput;

    constructor() {
        this.nameInput = element(by.model('projectEditVm.project.name'));
    }

    get(): void {
        browser.get(browser.baseUrl);
    }

    get name(): string {
        return this.nameInput.getText();
    }

    set name(value: string) {
        this.nameInput.sendKeys(value);
    }
}

describe('project edit view', () => {
    var editPage = new ProjectEditPo();

    beforeEach(() =>
        editPage.get());

    //beforeEach(() => {
    //    var myAppDev = angular.module('myAppDev', ['app', 'ngMockE2E']);
    //    myAppDev.run(($httpBackend: ng.IHttpBackendService) => {
    //        console.info("--------- devApp running");
    //        var authenticated = false;
    //        var testAccount = { "msg": "", "response": { "user": { "id": 1, "name": "Global", "lastName": "admin", "phoneNumber": null, "email": "support@kitos.dk", "defaultOrganizationUnitId": null, "defaultOrganizationUnitName": null, "isGlobalAdmin": true, "uuid": null, "adminRights": [], "objectOwnerName": null, "objectOwnerLastName": null, "lastAdvisDate": null, "lastChanged": "2015-11-13T08:37:47Z", "lastChangedByUserId": null, "fullName": "Global admin" }, "organizations": { "$type": "System.Linq.Enumerable+<ZipIterator>d__7a`3[[Core.DomainModel.Organization, Core.DomainModel],[Core.DomainModel.OrganizationUnit, Core.DomainModel],[Presentation.Web.Models.OrganizationAndDefaultUnitDTO, Presentation.Web]], System.Core", "$values": [{ "organization": { "id": 1, "name": "Fælles Kommune", "cvr": null, "type": 1, "accessModifier": 0, "config": { "id": 1, "showItProjectModule": true, "showItSystemModule": true, "showItContractModule": true, "showTabOverview": true, "showColumnTechnology": true, "showColumnUsage": true }, "root": { "id": 1, "name": "Fælles Kommune", "organizationId": 1, "organizationName": "Fælles Kommune", "qualifiedName": "Fælles Kommune, Fælles Kommune" }, "lastChanged": "2015-11-13T08:37:49Z", "lastChangedByUserId": 1, "uuid": null }, "defaultOrgUnit": null }] } } };

    //        $httpBackend.whenGET('api/authorize')
    //            .respond((method, url, data, headers) => {

    //            console.info("--------- auth: get");
    //            return authenticated ? [200, testAccount, {}] : [401, {}, {}];
    //        });

    //        $httpBackend.whenPOST('api/authorize')
    //            .respond((method, url, data, headers) => {
    //            console.info("--------- auth: post");
    //                authenticated = true;
    //                return [201, testAccount, {}];
    //            });

    //        $httpBackend.whenGET(/.*/).passThrough();
    //    });
    //});

    afterEach(() => {
        browser.manage().logs().get('browser').then(browserLogs => {
            // browserLogs is an array of objects with level and message fields
            console.log('Browser console output');
            browserLogs.forEach(log => {
                if (log.level.value > 900) { // it's an error log
                    console.log(log.message);
                }
            });
        });
    });

    it('should save when name looses focus', () => {

        // arrange
        //browser.pause();
        expect(true).toBe(true);
        var emailField = editPage.nameInput;

        // act
        editPage.name = 'some name';

        // assert
        expect(emailField.getAttribute('class')).toMatch('ng-invalid');
    });
});
