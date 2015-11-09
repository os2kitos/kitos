﻿describe('home view', function () {
    it('should mark invalid email in field', function () {
        // arrange
        browser.get('https://127.0.0.1:80/');
        var emailField = element(by.model('email'));

        // act
        emailField.sendKeys('some invalid email');

        // assert
        expect(emailField.getAttribute('class')).toMatch('ng-invalid');
    });

    it('should mark valid email in field', function () {
        // arrange
        browser.get('https://127.0.0.1:80/');
        var emailField = element(by.model('email'));

        // act
        emailField.sendKeys('some@valid.email');

        // assert
        expect(emailField.getAttribute('class')).toMatch('ng-valid');
    });
});
