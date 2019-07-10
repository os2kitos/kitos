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
        "sessionId": "05e2175f367330ffdfba17ce5d39113f",
        "instanceId": 21304,
        "browser": {
            "name": "chrome"
        },
        "message": "Passed.",
        "trace": "",
        "browserLogs": [
            {
                "level": "SEVERE",
                "message": "https://localhost:44300/api/authorize - Failed to load resource: the server responded with a status of 401 ()",
                "timestamp": 1562745811396,
                "type": ""
            }
        ],
        "screenShotFile": "00f8009b-0074-0097-00ea-003b00ba003b.png",
        "timestamp": 1562745809075,
        "duration": 3435
    },
    {
        "description": "should mark valid email in field|home view",
        "passed": true,
        "pending": false,
        "sessionId": "05e2175f367330ffdfba17ce5d39113f",
        "instanceId": 21304,
        "browser": {
            "name": "chrome"
        },
        "message": "Passed.",
        "trace": "",
        "browserLogs": [
            {
                "level": "SEVERE",
                "message": "https://localhost:44300/api/authorize - Failed to load resource: the server responded with a status of 401 ()",
                "timestamp": 1562745814173,
                "type": ""
            }
        ],
        "screenShotFile": "00de00b2-009e-001e-000a-00dc003000bf.png",
        "timestamp": 1562745812850,
        "duration": 2395
    },
    {
        "description": "Can login succesfully|As global admin ",
        "passed": true,
        "pending": false,
        "sessionId": "d19210fa670e15205e3610c398c6dbbb",
        "instanceId": 22088,
        "browser": {
            "name": "chrome"
        },
        "message": "Passed.",
        "trace": "",
        "browserLogs": [
            {
                "level": "SEVERE",
                "message": "https://localhost:44300/api/authorize - Failed to load resource: the server responded with a status of 401 ()",
                "timestamp": 1562745821411,
                "type": ""
            },
            {
                "level": "SEVERE",
                "message": "https://localhost:44300/api/authorize - Failed to load resource: the server responded with a status of 401 ()",
                "timestamp": 1562745822558,
                "type": ""
            }
        ],
        "screenShotFile": "008100d2-0011-0090-0039-003d004c001c.png",
        "timestamp": 1562745819277,
        "duration": 11278
    },
    {
        "description": "As local admin|As global admin ",
        "passed": true,
        "pending": false,
        "sessionId": "d19210fa670e15205e3610c398c6dbbb",
        "instanceId": 22088,
        "browser": {
            "name": "chrome"
        },
        "message": "Passed.",
        "trace": "",
        "browserLogs": [
            {
                "level": "SEVERE",
                "message": "https://localhost:44300/api/authorize - Failed to load resource: the server responded with a status of 401 ()",
                "timestamp": 1562745831780,
                "type": ""
            },
            {
                "level": "SEVERE",
                "message": "https://localhost:44300/api/authorize - Failed to load resource: the server responded with a status of 401 ()",
                "timestamp": 1562745832577,
                "type": ""
            }
        ],
        "screenShotFile": "00b70004-0068-0049-00d3-001400a80056.png",
        "timestamp": 1562745830898,
        "duration": 9468
    },
    {
        "description": "As regular user|As global admin ",
        "passed": true,
        "pending": false,
        "sessionId": "d19210fa670e15205e3610c398c6dbbb",
        "instanceId": 22088,
        "browser": {
            "name": "chrome"
        },
        "message": "Passed.",
        "trace": "",
        "browserLogs": [
            {
                "level": "SEVERE",
                "message": "https://localhost:44300/api/authorize - Failed to load resource: the server responded with a status of 401 ()",
                "timestamp": 1562745841435,
                "type": ""
            },
            {
                "level": "SEVERE",
                "message": "https://localhost:44300/api/authorize - Failed to load resource: the server responded with a status of 401 ()",
                "timestamp": 1562745842156,
                "type": ""
            }
        ],
        "screenShotFile": "0023004a-002a-0020-00cb-003f001800b2.png",
        "timestamp": 1562745840654,
        "duration": 9208
    },
    {
        "description": "Apply and delete filter buttons are disabled|Regular user tests",
        "passed": true,
        "pending": false,
        "sessionId": "166073616405947991a801dcc0b25795",
        "instanceId": 24712,
        "browser": {
            "name": "chrome"
        },
        "message": "Passed.",
        "trace": "",
        "browserLogs": [
            {
                "level": "SEVERE",
                "message": "https://localhost:44300/api/authorize - Failed to load resource: the server responded with a status of 401 ()",
                "timestamp": 1562745856402,
                "type": ""
            }
        ],
        "screenShotFile": "007c00ea-008e-0030-00f8-006600c1006d.png",
        "timestamp": 1562745863776,
        "duration": 2925
    },
    {
        "description": "IT system can be opened|Regular user tests",
        "passed": true,
        "pending": false,
        "sessionId": "166073616405947991a801dcc0b25795",
        "instanceId": 24712,
        "browser": {
            "name": "chrome"
        },
        "message": "Passed.",
        "trace": "",
        "browserLogs": [],
        "screenShotFile": "00590044-00ba-00fa-009a-0093007a0023.png",
        "timestamp": 1562745867026,
        "duration": 4718
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

