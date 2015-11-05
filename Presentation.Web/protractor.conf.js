exports.config = {
    // selenium server address (default)
    seleniumAddress: 'http://localhost:4444/wd/hub',

    // select all end to end tests
    specs: ['Tests/**/*.e2e.spec.js']
};
