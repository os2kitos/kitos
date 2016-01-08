module Kitos.ItProject.Overview.Tests {
    "use strict";

    describe("project.OverviewCtrl", () => {
        // mock object references
        var scopeMock: ng.IScope,
            httpBackendMock: ng.IHttpBackendService,
            httpMock: ng.IHttpService,
            notifyMock: any,
            projectRolesMock: any,
            userMock: any,
            $qMock: any,
            controller: OverviewController;

        // setup module
        beforeEach(() => angular.module("app"));

        // setup mocks
        beforeEach(inject(($injector: angular.auto.IInjectorService) => {
            scopeMock = $injector.get<ng.IScope>("$rootScope").$new();

            httpBackendMock = $injector.get<ng.IHttpBackendService>("$httpBackend");
            // setup HTTP path responses here
            // http://dotnetspeak.com/2014/03/testing-controllers-in-angular-with-jasmine

            userMock = {
                currentOrganizationId: 1
            };
        }));

        // TODO
    });
}
