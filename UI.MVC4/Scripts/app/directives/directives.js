(function (ng, app) {
    app.directive('holderFix', function () {
        return {
            link: function (scope, element, attrs) {
                Holder.run({ images: element[0], nocss: true });
            }
        };
    });

    app.directive('confirmClick', [
        function () {
            return {
                link: function (scope, element, attr) {
                    var msg = attr.confirmClick || "Er du sikker?";
                    var clickAction = attr.confirmedClick;
                    element.bind('click', function (event) {
                        if (window.confirm(msg)) {
                            scope.$eval(clickAction);
                        }
                    });
                }
            };
        }]);


    app.directive('addUser', ['$http', '$modal', function($http, $modal) {
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
                        controller: ['$scope', 'notify', '$modalInstance', function($scope, notify, $modalInstance) {

                            $scope.newUser = {};

                            $scope.addUser = function () {

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
                        }]
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
    }]);
    
    app.directive('selectUser', ['$rootScope', '$http', function ($rootScope, $http) {
        function userToString(user) {
            var result = user.name;

            if (user.defaultOrganizationUnitName)
                result += " <span class='pull-right'>" + user.defaultOrganizationUnitName + "</span>";

            result += "</small>";

            return result;
        }
        
        function formatUser(obj) {
            var result = "<div>" + obj.text + "</div>";

            if (obj.user) {
                result += "<div class='small'>" + obj.user.email;

                if(obj.user.defaultOrganizationUnitName)
                result += ", " + obj.user.defaultOrganizationUnitName;

                result += "</div>";

            }

            return result;
        }

        return {
            scope: {
                inputName: '@?name',
                userModel: '=',
                addUser: "="
            },
            

            replace: true,
            templateUrl: 'partials/directives/select-user.html',
            controller: ['$scope', function($scope) {
                $scope.selectUserOptions = {
                    
                    //don't escape markup
                    escapeMarkup: function (m) { return m; },
                    
                    formatResultCssClass: function(object) {
                        return "";
                    },
                    formatResult: formatUser,
                    formatSelection: formatUser,
                    
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
                                    id: user.id,
                                    text: user.name,
                                    user: user
                                });
                            });

                            return { results: results };
                        }
                    }
                };
            }]
        };
    }]);

    app.directive('highlight', [function () {
        return {
            link: function(scope, element, attr) {

                scope.$watch(attr.on, function(newVal, oldVal) {
                    if (!newVal) return;

                    element.addClass("highlight", 10).removeClass("highlight", 1000);

                });

            }
        };
    }]);

    app.directive('selectStatus', [function() {
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
    }]);
    
    app.directive('showStatus', ['$timeout', function ($timeout) {
        return {
            scope: {
                status: '=showStatus'
            },
            replace: false,
            templateUrl: 'partials/directives/show-status.html',
            
            link: function (scope, element, attr) {
                scope.ready = false;
                update();

                function update() {
                    $timeout(function () {
                        if (!scope.status) {
                            update();
                            return;
                        }
                        scope.ready = true;
                    });
                }


                scope.$watch("status", function (newval, oldval) {
                    if (newval === oldval) return;

                    update();
                });
            }
        };
    }]);
    
    //// http://stackoverflow.com/questions/14833326/how-to-set-focus-in-angularjs
    //// use autofocus instead
    //app.directive('focusOn', ['$timeout', function ($timeout) {
    //    return function (scope, elem, attr) {
    //        scope.$on('focusOn', function (e, name) {
    //            if (name === attr.focusOn) {
    //                $timeout(function() {
    //                    elem[0].focus();
    //                });
    //            }
    //        });
    //    };
    //}]);

    //// http://stackoverflow.com/questions/14833326/how-to-set-focus-in-angularjs
    //app.factory('focus', function ($rootScope, $timeout) {
    //    return function(name) {
    //        $timeout(function() {
    //            $rootScope.$broadcast('focusOn', name);
    //        });
    //    };
    //});


    app.directive('autofocus', ['$timeout', function($timeout) {
        return function (scope, elem, attr) {
            scope.$on('autofocus', function (e) {
                $timeout(function () {
                    elem[0].focus();
                });
            });
        };
    }]);
    
    /* http://stackoverflow.com/questions/14833326/how-to-set-focus-in-angularjs */
    app.factory('autofocus', function ($rootScope, $timeout) {
        return function () {
            $timeout(function () {
                $rootScope.$broadcast('autofocus');
            });
        };
    });


    /* from http://stackoverflow.com/questions/11540157/using-comma-as-list-separator-with-angularjs */
    app.filter('joinBy', function () {
        return function (input, delimiter) {
            return (input || []).join(delimiter || ',');
        };
    });

    app.directive('disabledOnBusy', [function() {
        return function(scope, elem, attr) {
            scope.$on('httpBusy', function (e) {
                elem[0].disabled = true;
            });

            scope.$on('httpUnbusy', function(e) {
                elem[0].disabled = false;
            });
        };
    }]);

})(angular, app);