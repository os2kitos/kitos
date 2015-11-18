// Type definitions for protractor-http-mock
// Project: https://github.com/atecarlos/protractor-http-mock
// Definitions by: Bjørn Sørensen <https://github.com/Crevil>
// Definitions: https://github.com/DefinitelyTyped/DefinitelyTyped

interface ProtractorHttpMockStatic {
    <T>(mocks?: Array<any>, skipDefaults?: boolean): ProtractorHttpMockStatic;
    teardown(): void;
    requestsMade();
}

declare var mock: ProtractorHttpMockStatic;

declare module 'protractor-http-mock' {
    export = mock;
}
