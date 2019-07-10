var app = angular.module('reportingApp', []);

//<editor-fold desc="global helpers">

var isValueAnArray = function (val) {
    return Array.isArray(val);
};

var getSpec = function (str) {
    var describes = str.split('|');
    return describes[describes.length - 1];
};
var checkIfShouldDisplaySpecName = function (prevItem, item) {
    if (!prevItem) {
        item.displaySpecName = true;
    } else if (getSpec(item.description) !== getSpec(prevItem.description)) {
        item.displaySpecName = true;
    }
};

var getParent = function (str) {
    var arr = str.split('|');
    str = "";
    for (var i = arr.length - 2; i > 0; i--) {
        str += arr[i] + " > ";
    }
    return str.slice(0, -3);
};

var getShortDescription = function (str) {
    return str.split('|')[0];
};

var countLogMessages = function (item) {
    if ((!item.logWarnings || !item.logErrors) && item.browserLogs && item.browserLogs.length > 0) {
        item.logWarnings = 0;
        item.logErrors = 0;
        for (var logNumber = 0; logNumber < item.browserLogs.length; logNumber++) {
            var logEntry = item.browserLogs[logNumber];
            if (logEntry.level === 'SEVERE') {
                item.logErrors++;
            }
            if (logEntry.level === 'WARNING') {
                item.logWarnings++;
            }
        }
    }
};

var defaultSortFunction = function sortFunction(a, b) {
    if (a.sessionId < b.sessionId) {
        return -1;
    }
    else if (a.sessionId > b.sessionId) {
        return 1;
    }

    if (a.timestamp < b.timestamp) {
        return -1;
    }
    else if (a.timestamp > b.timestamp) {
        return 1;
    }

    return 0;
};


//</editor-fold>

app.controller('ScreenshotReportController', function ($scope, $http) {
    var that = this;
    var clientDefaults = {};

    $scope.searchSettings = Object.assign({
        description: '',
        allselected: true,
        passed: true,
        failed: true,
        pending: true,
        withLog: true
    }, clientDefaults.searchSettings || {}); // enable customisation of search settings on first page hit

    this.warningTime = 1400;
    this.dangerTime = 1900;

    var initialColumnSettings = clientDefaults.columnSettings; // enable customisation of visible columns on first page hit
    if (initialColumnSettings) {
        if (initialColumnSettings.displayTime !== undefined) {
            // initial settings have be inverted because the html bindings are inverted (e.g. !ctrl.displayTime)
            this.displayTime = !initialColumnSettings.displayTime;
        }
        if (initialColumnSettings.displayBrowser !== undefined) {
            this.displayBrowser = !initialColumnSettings.displayBrowser; // same as above
        }
        if (initialColumnSettings.displaySessionId !== undefined) {
            this.displaySessionId = !initialColumnSettings.displaySessionId; // same as above
        }
        if (initialColumnSettings.displayOS !== undefined) {
            this.displayOS = !initialColumnSettings.displayOS; // same as above
        }
        if (initialColumnSettings.inlineScreenshots !== undefined) {
            this.inlineScreenshots = initialColumnSettings.inlineScreenshots; // this setting does not have to be inverted
        } else {
            this.inlineScreenshots = false;
        }
        if (initialColumnSettings.warningTime) {
            this.warningTime = initialColumnSettings.warningTime;
        }
        if (initialColumnSettings.dangerTime){
            this.dangerTime = initialColumnSettings.dangerTime;
        }
    }

    this.showSmartStackTraceHighlight = true;

    this.chooseAllTypes = function () {
        var value = true;
        $scope.searchSettings.allselected = !$scope.searchSettings.allselected;
        if (!$scope.searchSettings.allselected) {
            value = false;
        }

        $scope.searchSettings.passed = value;
        $scope.searchSettings.failed = value;
        $scope.searchSettings.pending = value;
        $scope.searchSettings.withLog = value;
    };

    this.isValueAnArray = function (val) {
        return isValueAnArray(val);
    };

    this.getParent = function (str) {
        return getParent(str);
    };

    this.getSpec = function (str) {
        return getSpec(str);
    };

    this.getShortDescription = function (str) {
        return getShortDescription(str);
    };

    this.convertTimestamp = function (timestamp) {
        var d = new Date(timestamp),
            yyyy = d.getFullYear(),
            mm = ('0' + (d.getMonth() + 1)).slice(-2),
            dd = ('0' + d.getDate()).slice(-2),
            hh = d.getHours(),
            h = hh,
            min = ('0' + d.getMinutes()).slice(-2),
            ampm = 'AM',
            time;

        if (hh > 12) {
            h = hh - 12;
            ampm = 'PM';
        } else if (hh === 12) {
            h = 12;
            ampm = 'PM';
        } else if (hh === 0) {
            h = 12;
        }

        // ie: 2013-02-18, 8:35 AM
        time = yyyy + '-' + mm + '-' + dd + ', ' + h + ':' + min + ' ' + ampm;

        return time;
    };


    this.round = function (number, roundVal) {
        return (parseFloat(number) / 1000).toFixed(roundVal);
    };


    this.passCount = function () {
        var passCount = 0;
        for (var i in this.results) {
            var result = this.results[i];
            if (result.passed) {
                passCount++;
            }
        }
        return passCount;
    };


    this.pendingCount = function () {
        var pendingCount = 0;
        for (var i in this.results) {
            var result = this.results[i];
            if (result.pending) {
                pendingCount++;
            }
        }
        return pendingCount;
    };


    this.failCount = function () {
        var failCount = 0;
        for (var i in this.results) {
            var result = this.results[i];
            if (!result.passed && !result.pending) {
                failCount++;
            }
        }
        return failCount;
    };

    this.passPerc = function () {
        return (this.passCount() / this.totalCount()) * 100;
    };
    this.pendingPerc = function () {
        return (this.pendingCount() / this.totalCount()) * 100;
    };
    this.failPerc = function () {
        return (this.failCount() / this.totalCount()) * 100;
    };
    this.totalCount = function () {
        return this.passCount() + this.failCount() + this.pendingCount();
    };

    this.applySmartHighlight = function (line) {
        if (this.showSmartStackTraceHighlight) {
            if (line.indexOf('node_modules') > -1) {
                return 'greyout';
            }
            if (line.indexOf('  at ') === -1) {
                return '';
            }

            return 'highlight';
        }
        return true;
    };

    var results = [
    {
        "description": "should mark invalid email in field|home view",
        "passed": true,
        "pending": false,
        "sessionId": "00fd1436faeae07c86670c5eeda61348",
        "instanceId": 9384,
        "browser": {
            "name": "chrome"
        },
        "message": "Passed.",
        "trace": "",
        "browserLogs": [
            {
                "level": "SEVERE",
                "message": "https://localhost:44300/app/utility/Constants.js 14:0 Uncaught ReferenceError: module is not defined",
                "timestamp": 1562743628879,
                "type": ""
            },
            {
                "level": "SEVERE",
                "message": "https://localhost:44300/api/authorize - Failed to load resource: the server responded with a status of 401 ()",
                "timestamp": 1562743629339,
                "type": ""
            }
        ],
        "screenShotFile": "00710031-00ad-001b-0052-000e00c6008f.png",
        "timestamp": 1562743624720,
        "duration": 6530
    },
    {
        "description": "should mark valid email in field|home view",
        "passed": true,
        "pending": false,
        "sessionId": "00fd1436faeae07c86670c5eeda61348",
        "instanceId": 9384,
        "browser": {
            "name": "chrome"
        },
        "message": "Passed.",
        "trace": "",
        "browserLogs": [
            {
                "level": "SEVERE",
                "message": "https://localhost:44300/app/utility/Constants.js 14:0 Uncaught ReferenceError: module is not defined",
                "timestamp": 1562743633045,
                "type": ""
            },
            {
                "level": "SEVERE",
                "message": "https://localhost:44300/api/authorize - Failed to load resource: the server responded with a status of 401 ()",
                "timestamp": 1562743633399,
                "type": ""
            }
        ],
        "screenShotFile": "00b40057-0023-0011-008a-00ef008f00d5.png",
        "timestamp": 1562743631641,
        "duration": 2899
    },
    {
        "description": "Is succesfully logged in|Can login succesfully",
        "passed": false,
        "pending": false,
        "sessionId": "72c8c847445c99dc7418d7c6c704de54",
        "instanceId": 13620,
        "browser": {
            "name": "chrome"
        },
        "message": [
            "Expected 'globaladmin@kitos.dk' to equal 'support@kitos.dk'."
        ],
        "trace": [
            "Error: Failed expectation\n    at UserContext.<anonymous> (C:\\STRONGMINDS Repos\\OS2KITOS\\kitos\\Presentation.Web\\Tests\\HomePage\\Login.e2e.spec.ts:18:111)\n    at C:\\STRONGMINDS Repos\\OS2KITOS\\kitos\\node_modules\\jasminewd2\\index.js:112:25\n    at new ManagedPromise (C:\\STRONGMINDS Repos\\OS2KITOS\\kitos\\node_modules\\selenium-webdriver\\lib\\promise.js:1077:7)\n    at ControlFlow.promise (C:\\STRONGMINDS Repos\\OS2KITOS\\kitos\\node_modules\\selenium-webdriver\\lib\\promise.js:2505:12)\n    at schedulerExecute (C:\\STRONGMINDS Repos\\OS2KITOS\\kitos\\node_modules\\jasminewd2\\index.js:95:18)\n    at TaskQueue.execute_ (C:\\STRONGMINDS Repos\\OS2KITOS\\kitos\\node_modules\\selenium-webdriver\\lib\\promise.js:3084:14)\n    at TaskQueue.executeNext_ (C:\\STRONGMINDS Repos\\OS2KITOS\\kitos\\node_modules\\selenium-webdriver\\lib\\promise.js:3067:27)\n    at asyncRun (C:\\STRONGMINDS Repos\\OS2KITOS\\kitos\\node_modules\\selenium-webdriver\\lib\\promise.js:2974:25)\n    at C:\\STRONGMINDS Repos\\OS2KITOS\\kitos\\node_modules\\selenium-webdriver\\lib\\promise.js:668:7"
        ],
        "browserLogs": [
            {
                "level": "SEVERE",
                "message": "https://localhost:44300/app/utility/Constants.js 14:0 Uncaught ReferenceError: module is not defined",
                "timestamp": 1562743644834,
                "type": ""
            },
            {
                "level": "SEVERE",
                "message": "https://localhost:44300/api/authorize - Failed to load resource: the server responded with a status of 401 ()",
                "timestamp": 1562743645865,
                "type": ""
            }
        ],
        "screenShotFile": "00eb0053-006c-009f-0095-0067005200a3.png",
        "timestamp": 1562743656277,
        "duration": 38
    },
    {
        "description": "Is succesfully logged in|Can login succesfully",
        "passed": false,
        "pending": false,
        "sessionId": "72c8c847445c99dc7418d7c6c704de54",
        "instanceId": 13620,
        "browser": {
            "name": "chrome"
        },
        "message": [
            "Expected 'globaladmin' to equal 'Global admin'."
        ],
        "trace": [
            "Error: Failed expectation\n    at UserContext.<anonymous> (C:\\STRONGMINDS Repos\\OS2KITOS\\kitos\\Presentation.Web\\Tests\\HomePage\\Login.e2e.spec.ts:27:64)\n    at C:\\STRONGMINDS Repos\\OS2KITOS\\kitos\\node_modules\\jasminewd2\\index.js:112:25\n    at new ManagedPromise (C:\\STRONGMINDS Repos\\OS2KITOS\\kitos\\node_modules\\selenium-webdriver\\lib\\promise.js:1077:7)\n    at ControlFlow.promise (C:\\STRONGMINDS Repos\\OS2KITOS\\kitos\\node_modules\\selenium-webdriver\\lib\\promise.js:2505:12)\n    at schedulerExecute (C:\\STRONGMINDS Repos\\OS2KITOS\\kitos\\node_modules\\jasminewd2\\index.js:95:18)\n    at TaskQueue.execute_ (C:\\STRONGMINDS Repos\\OS2KITOS\\kitos\\node_modules\\selenium-webdriver\\lib\\promise.js:3084:14)\n    at TaskQueue.executeNext_ (C:\\STRONGMINDS Repos\\OS2KITOS\\kitos\\node_modules\\selenium-webdriver\\lib\\promise.js:3067:27)\n    at asyncRun (C:\\STRONGMINDS Repos\\OS2KITOS\\kitos\\node_modules\\selenium-webdriver\\lib\\promise.js:2974:25)\n    at C:\\STRONGMINDS Repos\\OS2KITOS\\kitos\\node_modules\\selenium-webdriver\\lib\\promise.js:668:7"
        ],
        "browserLogs": [],
        "screenShotFile": "00140099-0092-00f5-00a0-006c000e0081.png",
        "timestamp": 1562743656636,
        "duration": 99
    },
    {
        "description": "Create Contract button is disabled|Regular user tests",
        "passed": true,
        "pending": false,
        "sessionId": "c22c005742b4826e47a75bb0ddde5226",
        "instanceId": 16872,
        "browser": {
            "name": "chrome"
        },
        "message": "Passed.",
        "trace": "",
        "browserLogs": [
            {
                "level": "SEVERE",
                "message": "https://localhost:44300/app/utility/Constants.js 14:0 Uncaught ReferenceError: module is not defined",
                "timestamp": 1562743666153,
                "type": ""
            },
            {
                "level": "SEVERE",
                "message": "https://localhost:44300/api/authorize - Failed to load resource: the server responded with a status of 401 ()",
                "timestamp": 1562743666471,
                "type": ""
            },
            {
                "level": "SEVERE",
                "message": "https://localhost:44300/app/utility/Constants.js 14:0 Uncaught ReferenceError: module is not defined",
                "timestamp": 1562743675578,
                "type": ""
            }
        ],
        "screenShotFile": "003e009d-0033-0076-0016-0084008c005b.png",
        "timestamp": 1562743674744,
        "duration": 3576
    },
    {
        "description": "Use filter + delete filter is disabled|Regular user tests",
        "passed": true,
        "pending": false,
        "sessionId": "c22c005742b4826e47a75bb0ddde5226",
        "instanceId": 16872,
        "browser": {
            "name": "chrome"
        },
        "message": "Passed.",
        "trace": "",
        "browserLogs": [
            {
                "level": "SEVERE",
                "message": "https://localhost:44300/app/utility/Constants.js 14:0 Uncaught ReferenceError: module is not defined",
                "timestamp": 1562743681534,
                "type": ""
            }
        ],
        "screenShotFile": "005100c7-00da-0077-0032-0066000800b8.png",
        "timestamp": 1562743678711,
        "duration": 5251
    },
    {
        "description": "Reset filter and Save filter is active|Regular user tests",
        "passed": true,
        "pending": false,
        "sessionId": "c22c005742b4826e47a75bb0ddde5226",
        "instanceId": 16872,
        "browser": {
            "name": "chrome"
        },
        "message": "Passed",
        "browserLogs": [
            {
                "level": "SEVERE",
                "message": "https://localhost:44300/app/utility/Constants.js 14:0 Uncaught ReferenceError: module is not defined",
                "timestamp": 1562743684972,
                "type": ""
            }
        ],
        "screenShotFile": "00450008-0097-00ec-0001-009500b80002.png",
        "timestamp": 1562743684266,
        "duration": 3537
    },
    {
        "description": "Apply and delete filter buttons are disabled|Regular user tests",
        "passed": true,
        "pending": false,
        "sessionId": "68ccede447b66cdb7b5b868d9c2a9d29",
        "instanceId": 19020,
        "browser": {
            "name": "chrome"
        },
        "message": "Passed.",
        "trace": "",
        "browserLogs": [
            {
                "level": "SEVERE",
                "message": "https://localhost:44300/app/utility/Constants.js 14:0 Uncaught ReferenceError: module is not defined",
                "timestamp": 1562743696955,
                "type": ""
            },
            {
                "level": "SEVERE",
                "message": "https://localhost:44300/api/authorize - Failed to load resource: the server responded with a status of 401 ()",
                "timestamp": 1562743697295,
                "type": ""
            },
            {
                "level": "SEVERE",
                "message": "https://localhost:44300/app/utility/Constants.js 14:0 Uncaught ReferenceError: module is not defined",
                "timestamp": 1562743705473,
                "type": ""
            }
        ],
        "screenShotFile": "004800ad-0034-00aa-0009-0099000c00ec.png",
        "timestamp": 1562743704716,
        "duration": 3591
    },
    {
        "description": "IT system can be opened|Regular user tests",
        "passed": false,
        "pending": false,
        "sessionId": "68ccede447b66cdb7b5b868d9c2a9d29",
        "instanceId": 19020,
        "browser": {
            "name": "chrome"
        },
        "message": [
            "Failed: Index out of bound. Trying to access element at index: 0, but there are only 0 elements that match locator By(css selector, a)"
        ],
        "trace": [
            "NoSuchElementError: Index out of bound. Trying to access element at index: 0, but there are only 0 elements that match locator By(css selector, a)\n    at selenium_webdriver_1.promise.all.then (C:\\STRONGMINDS Repos\\OS2KITOS\\kitos\\node_modules\\gulp-protractor\\node_modules\\protractor\\built\\element.js:274:27)\n    at ManagedPromise.invokeCallback_ (C:\\STRONGMINDS Repos\\OS2KITOS\\kitos\\node_modules\\selenium-webdriver\\lib\\promise.js:1376:14)\n    at TaskQueue.execute_ (C:\\STRONGMINDS Repos\\OS2KITOS\\kitos\\node_modules\\selenium-webdriver\\lib\\promise.js:3084:14)\n    at TaskQueue.executeNext_ (C:\\STRONGMINDS Repos\\OS2KITOS\\kitos\\node_modules\\selenium-webdriver\\lib\\promise.js:3067:27)\n    at asyncRun (C:\\STRONGMINDS Repos\\OS2KITOS\\kitos\\node_modules\\selenium-webdriver\\lib\\promise.js:2927:27)\n    at C:\\STRONGMINDS Repos\\OS2KITOS\\kitos\\node_modules\\selenium-webdriver\\lib\\promise.js:668:7\n    at process._tickCallback (internal/process/next_tick.js:68:7)Error\n    at ElementArrayFinder.applyAction_ (C:\\STRONGMINDS Repos\\OS2KITOS\\kitos\\node_modules\\gulp-protractor\\node_modules\\protractor\\built\\element.js:459:27)\n    at ElementArrayFinder.(anonymous function).args [as click] (C:\\STRONGMINDS Repos\\OS2KITOS\\kitos\\node_modules\\gulp-protractor\\node_modules\\protractor\\built\\element.js:91:29)\n    at ElementFinder.(anonymous function).args [as click] (C:\\STRONGMINDS Repos\\OS2KITOS\\kitos\\node_modules\\gulp-protractor\\node_modules\\protractor\\built\\element.js:831:22)\n    at UserContext.<anonymous> (C:\\STRONGMINDS Repos\\OS2KITOS\\kitos\\Presentation.Web\\Tests\\it-system\\RegularUser.System.e2e.spec.ts:41:41)\n    at C:\\STRONGMINDS Repos\\OS2KITOS\\kitos\\node_modules\\jasminewd2\\index.js:112:25\n    at new ManagedPromise (C:\\STRONGMINDS Repos\\OS2KITOS\\kitos\\node_modules\\selenium-webdriver\\lib\\promise.js:1077:7)\n    at ControlFlow.promise (C:\\STRONGMINDS Repos\\OS2KITOS\\kitos\\node_modules\\selenium-webdriver\\lib\\promise.js:2505:12)\n    at schedulerExecute (C:\\STRONGMINDS Repos\\OS2KITOS\\kitos\\node_modules\\jasminewd2\\index.js:95:18)\n    at TaskQueue.execute_ (C:\\STRONGMINDS Repos\\OS2KITOS\\kitos\\node_modules\\selenium-webdriver\\lib\\promise.js:3084:14)\n    at TaskQueue.executeNext_ (C:\\STRONGMINDS Repos\\OS2KITOS\\kitos\\node_modules\\selenium-webdriver\\lib\\promise.js:3067:27)\nFrom: Task: Run it(\"IT system can be opened\") in control flow\n    at UserContext.<anonymous> (C:\\STRONGMINDS Repos\\OS2KITOS\\kitos\\node_modules\\jasminewd2\\index.js:94:19)\n    at attempt (C:\\STRONGMINDS Repos\\OS2KITOS\\kitos\\node_modules\\jasmine\\node_modules\\jasmine-core\\lib\\jasmine-core\\jasmine.js:4297:26)\n    at QueueRunner.run (C:\\STRONGMINDS Repos\\OS2KITOS\\kitos\\node_modules\\jasmine\\node_modules\\jasmine-core\\lib\\jasmine-core\\jasmine.js:4217:20)\n    at runNext (C:\\STRONGMINDS Repos\\OS2KITOS\\kitos\\node_modules\\jasmine\\node_modules\\jasmine-core\\lib\\jasmine-core\\jasmine.js:4257:20)\n    at C:\\STRONGMINDS Repos\\OS2KITOS\\kitos\\node_modules\\jasmine\\node_modules\\jasmine-core\\lib\\jasmine-core\\jasmine.js:4264:13\n    at C:\\STRONGMINDS Repos\\OS2KITOS\\kitos\\node_modules\\jasmine\\node_modules\\jasmine-core\\lib\\jasmine-core\\jasmine.js:4172:9\n    at C:\\STRONGMINDS Repos\\OS2KITOS\\kitos\\node_modules\\jasminewd2\\index.js:64:48\n    at ControlFlow.emit (C:\\STRONGMINDS Repos\\OS2KITOS\\kitos\\node_modules\\selenium-webdriver\\lib\\events.js:62:21)\n    at ControlFlow.shutdown_ (C:\\STRONGMINDS Repos\\OS2KITOS\\kitos\\node_modules\\selenium-webdriver\\lib\\promise.js:2674:10)\n    at shutdownTask_.MicroTask (C:\\STRONGMINDS Repos\\OS2KITOS\\kitos\\node_modules\\selenium-webdriver\\lib\\promise.js:2599:53)\nFrom asynchronous test: \nError\n    at Suite.<anonymous> (C:\\STRONGMINDS Repos\\OS2KITOS\\kitos\\Presentation.Web\\Tests\\it-system\\RegularUser.System.e2e.spec.ts:39:5)\n    at addSpecsToSuite (C:\\STRONGMINDS Repos\\OS2KITOS\\kitos\\node_modules\\jasmine\\node_modules\\jasmine-core\\lib\\jasmine-core\\jasmine.js:1107:25)\n    at Env.describe (C:\\STRONGMINDS Repos\\OS2KITOS\\kitos\\node_modules\\jasmine\\node_modules\\jasmine-core\\lib\\jasmine-core\\jasmine.js:1074:7)\n    at describe (C:\\STRONGMINDS Repos\\OS2KITOS\\kitos\\node_modules\\jasmine\\node_modules\\jasmine-core\\lib\\jasmine-core\\jasmine.js:4399:18)\n    at Object.<anonymous> (C:\\STRONGMINDS Repos\\OS2KITOS\\kitos\\Presentation.Web\\Tests\\it-system\\RegularUser.System.e2e.spec.ts:4:1)\n    at Module._compile (internal/modules/cjs/loader.js:776:30)\n    at Object.Module._extensions..js (internal/modules/cjs/loader.js:787:10)\n    at Module.load (internal/modules/cjs/loader.js:653:32)\n    at tryModuleLoad (internal/modules/cjs/loader.js:593:12)"
        ],
        "browserLogs": [
            {
                "level": "SEVERE",
                "message": "https://localhost:44300/app/utility/Constants.js 14:0 Uncaught ReferenceError: module is not defined",
                "timestamp": 1562743709656,
                "type": ""
            }
        ],
        "screenShotFile": "00b900e8-0044-00cd-0024-00c900b60011.png",
        "timestamp": 1562743708958,
        "duration": 4902
    }
];

    this.sortSpecs = function () {
        this.results = results.sort(function sortFunction(a, b) {
    if (a.sessionId < b.sessionId) return -1;else if (a.sessionId > b.sessionId) return 1;

    if (a.timestamp < b.timestamp) return -1;else if (a.timestamp > b.timestamp) return 1;

    return 0;
});
    };

    this.loadResultsViaAjax = function () {

        $http({
            url: './combined.json',
            method: 'GET'
        }).then(function (response) {
                var data = null;
                if (response && response.data) {
                    if (typeof response.data === 'object') {
                        data = response.data;
                    } else if (response.data[0] === '"') { //detect super escaped file (from circular json)
                        data = CircularJSON.parse(response.data); //the file is escaped in a weird way (with circular json)
                    }
                    else {
                        data = JSON.parse(response.data);
                    }
                }
                if (data) {
                    results = data;
                    that.sortSpecs();
                }
            },
            function (error) {
                console.error(error);
            });
    };


    if (clientDefaults.useAjax) {
        this.loadResultsViaAjax();
    } else {
        this.sortSpecs();
    }


});

app.filter('bySearchSettings', function () {
    return function (items, searchSettings) {
        var filtered = [];
        if (!items) {
            return filtered; // to avoid crashing in where results might be empty
        }
        var prevItem = null;

        for (var i = 0; i < items.length; i++) {
            var item = items[i];
            item.displaySpecName = false;

            var isHit = false; //is set to true if any of the search criteria matched
            countLogMessages(item); // modifies item contents

            var hasLog = searchSettings.withLog && item.browserLogs && item.browserLogs.length > 0;
            if (searchSettings.description === '' ||
                (item.description && item.description.toLowerCase().indexOf(searchSettings.description.toLowerCase()) > -1)) {

                if (searchSettings.passed && item.passed || hasLog) {
                    isHit = true;
                } else if (searchSettings.failed && !item.passed && !item.pending || hasLog) {
                    isHit = true;
                } else if (searchSettings.pending && item.pending || hasLog) {
                    isHit = true;
                }
            }
            if (isHit) {
                checkIfShouldDisplaySpecName(prevItem, item);

                filtered.push(item);
                prevItem = item;
            }
        }

        return filtered;
    };
});

