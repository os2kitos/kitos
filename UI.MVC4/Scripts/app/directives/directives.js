(function(ng, app) {
    'use strict';

    app.directive('holderFix', function() {
        return {
            link: function(scope, element, attrs) {
                Holder.run({ images: element[0], nocss: true });
            }
        };
    });

    app.directive('confirmClick', [
        function() {
            return {
                link: function(scope, element, attr) {
                    var msg = attr.confirmClick || "Er du sikker?";
                    var clickAction = attr.confirmedClick;
                    element.bind('click', function(event) {
                        if (window.confirm(msg)) {
                            scope.$eval(clickAction);
                        }
                    });
                }
            };
        }
    ]);


    app.directive('addUserButton', [
        '$http', '$modal', function($http, $modal) {
            return {
                scope: {
                    userResult: '=?addUser',
                    selectResult: '=?forSelect2'
                },
                replace: true,
                templateUrl: 'partials/directives/add-user-button.html',
                link: function(scope, element, attr) {

                    scope.open = function() {
                        var modal = $modal.open({
                            backdrop: "static", //modal can't be closed by clicking outside modal
                            templateUrl: 'partials/directives/add-user-modal.html',
                            controller: [
                                '$scope', 'notify', '$modalInstance', function($scope, notify, $modalInstance) {

                                    $scope.newUser = {};

                                    $scope.addUser = function() {

                                        if ($scope.newUser.email != $scope.newUser.repeatEmail) {
                                            notify.addErrorMessage("Email addresserne er ikke ens.");
                                        }

                                        if ($scope.newUser.form.$invalid) return;

                                        var name = $scope.newUser.name;
                                        var email = $scope.newUser.email;

                                        var data = {
                                            "name": name,
                                            "email": email
                                        };

                                        var msg = notify.addInfoMessage("Arbejder ...", false);

                                        $http.post("api/user", data, { handleBusy: true }).success(function(result) {
                                            msg.toSuccessMessage(name + " er oprettet i KITOS");

                                            $modalInstance.close(result.response);
                                        }).error(function(result) {
                                            msg.toErrorMessage("Fejl! " + name + " blev ikke oprettet i KITOS!");
                                        });
                                    };

                                    $scope.cancel = function() {
                                        $modalInstance.dismiss('cancel');
                                    };
                                }
                            ]
                        });

                        modal.result.then(function(userResult) {
                            scope.userResult = userResult;

                            scope.selectResult = {
                                id: userResult.id,
                                text: userResult.name
                            };
                        }, function() {
                            scope.userResult = null;
                            scope.selectResult = null;
                        });
                    };

                }
            };
        }
    ]);

    app.directive('selectUser', [
        '$rootScope', '$http', function($rootScope, $http) {

            //format of dropdown options
            function formatResult(obj) {
                var result = "<div>" + obj.text + "</div>";

                //obj.user might contain more info about the user
                if (obj.user) {
                    result += "<div class='small'>" + obj.user.email;

                    if (obj.user.defaultOrganizationUnitName)
                        result += ", " + obj.user.defaultOrganizationUnitName;

                    result += "</div>";
                }

                return result;
            }

            //format of the selected user
            function formatSelection(obj) {
                return obj.text;
            }

            return {
                scope: {
                    inputName: '@?name',
                    userModel: '=',
                    addUser: "@?"
                },
                replace: true,
                templateUrl: 'partials/directives/select-user.html',
                controller: [
                    '$scope', function($scope) {
                        $scope.selectUserOptions = {

                            //don't escape markup, otherwise formatResult will be bugged
                            escapeMarkup: function(m) { return m; },
                            formatResult: formatResult,
                            formatSelection: formatSelection,

                            minimumInputLength: 1,
                            initSelection: function(elem, callback) {
                            },
                            ajax: {
                                data: function(term, page) {
                                    return { query: term };
                                },
                                quietMillis: 500,
                                transport: function(queryParams) {
                                    var res = $http.get('api/user?q=' + queryParams.data.query).then(queryParams.success);
                                    res.abort = function() {
                                        return null;
                                    };

                                    return res;
                                },

                                results: function(data, page) {
                                    var results = [];

                                    _.each(data.data.response, function(user) {

                                        results.push({
                                            id: user.id, //select2 mandatory
                                            text: user.name, //select2 mandatory
                                            user: user //not mandatory, for extra info when formatting
                                        });
                                    });

                                    return { results: results };
                                }
                            }
                        };
                    }
                ]
            };
        }
    ]);

    app.directive('highlight', [
        function() {
            return {
                link: function(scope, element, attr) {

                    scope.$watch(attr.on, function(newVal, oldVal) {
                        if (!newVal) return;

                        element.addClass("highlight", 10).removeClass("highlight", 1000);

                    });

                }
            };
        }
    ]);

    app.directive('selectAccessModifier', [
        function() {
            return {
                priority: 1,
                replace: true,
                templateUrl: 'partials/directives/select-access-modifier.html'
            };
        }
    ]);

    app.directive('selectStatus', [
        function() {
            return {
                scope: {
                    model: '=selectStatus',
                    canWrite: '=',
                    onStatusChange: '&?'
                },
                replace: true,
                templateUrl: 'partials/directives/select-status.html',

                link: function(scope, element, attr) {
                    scope.setModel = function(n) {
                        if (scope.model == n) return;

                        scope.model = n;

                    };
                }
            };
        }
    ]);

    app.directive('showStatus', [
        '$timeout', function($timeout) {
            return {
                scope: {
                    status: '=showStatus'
                },
                replace: false,
                templateUrl: 'partials/directives/show-status.html',

                link: function(scope, element, attr) {
                    scope.ready = false;
                    update();

                    function update() {
                        $timeout(function() {
                            if (!scope.status) {
                                update();
                                return;
                            }
                            scope.ready = true;
                        });
                    }


                    scope.$watch("status", function(newval, oldval) {
                        if (newval === oldval) return;

                        update();
                    });
                }
            };
        }
    ]);

    app.directive('autofocus', [
        '$timeout', function($timeout) {
            return function(scope, elem, attr) {
                scope.$on('autofocus', function(e) {
                    $timeout(function() {
                        elem[0].focus();
                    });
                });
            };
        }
    ]);

    /* http://stackoverflow.com/questions/14833326/how-to-set-focus-in-angularjs */
    app.factory('autofocus', function($rootScope, $timeout) {
        return function() {
            $timeout(function() {
                $rootScope.$broadcast('autofocus');
            });
        };
    });


    /* from http://stackoverflow.com/questions/11540157/using-comma-as-list-separator-with-angularjs */
    app.filter('joinBy', function() {
        return function(input, delimiter) {
            return (input || []).join(delimiter || ',');
        };
    });

    app.directive('disabledOnBusy', [
        function() {
            return function(scope, elem, attr) {
                scope.$on('httpBusy', function(e) {
                    elem[0].disabled = true;
                });

                scope.$on('httpUnbusy', function(e) {
                    elem[0].disabled = false;
                });
            };
        }
    ]);

    app.directive('autosave', [
        '$http', 'notify', function($http, notify) {
            return {
                restrict: 'A',
                require: 'ngModel',
                link: function(scope, element, attrs, ctrl) {

                    function parseValue(value) {
                        switch (attrs.fieldtype) {
                        case "int":
                            return parseInt(value);
                        default:
                            return value;
                        }
                    }

                    var oldValue;
                    element.bind('focus', function() {
                        oldValue = parseValue(ctrl.$viewValue);
                    });

                    function saveIfNew() {
                        var newValue = parseValue(ctrl.$viewValue);
                        var payload = {};
                        payload[attrs.field] = newValue;

                        if (newValue !== oldValue) {
                            save(payload);
                        }
                    }

                    function saveCheckbox() {
                        // ctrl.$viewValue reflects the old state, so having to invert
                        var newValue = !parseValue(ctrl.$viewValue);
                        var payload = {};
                        payload[attrs.field] = newValue;
                        save(payload);
                    }

                    function save(payload) {
                        var msg = notify.addInfoMessage("Gemmer...", false);
                        $http({ method: 'PATCH', url: attrs.autosave, data: payload })
                            .success(function() {
                                msg.toSuccessMessage("Feltet er opdateret.");
                            })
                            .error(function() {
                                msg.toErrorMessage("Fejl! Feltet kunne ikke ændres!");
                            });
                    }

                    if (attrs.type === "checkbox") {
                        element.bind('change', saveCheckbox);
                    } else {
                        element.bind('blur', saveIfNew);
                    }
                }
            };
        }
    ]);


    app.directive('datewriter', ['$timeout',
        function($timeout) {
            return {
                replace: true,
                templateUrl: 'partials/directives/datewriter.html',
                link: function (scope, elem, attr) {
                    scope.opened = false;

                    elem.bind('focus',
                        function(e) {
                            scope.opened = true;
                        });

                    elem.bind('blur', function(e) {
                        scope.opened = false;
                    });
                }
            };
        }]);

})(angular, app);