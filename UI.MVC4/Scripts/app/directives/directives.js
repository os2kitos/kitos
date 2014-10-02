(function(ng, app) {
    'use strict';

    app.directive('unique', [
            '$http', 'userService', function ($http, userService) {
                return {
                    require: 'ngModel',
                    link: function (scope, element, attrs, ctrl) {
                        var user;
                        userService.getUser().then(function(result) {
                            user = result;
                        });
                        var validateAsync = _.debounce(function (viewValue) {
                            $http.get(attrs.unique + '?checkname=' + viewValue + '&orgId=' + user.currentOrganizationId)
                                .success(function() {
                                    ctrl.$setValidity('available', true);
                                    ctrl.$setValidity('lookup', true);
                                })
                                .error(function(data, status) {
                                    // conflict
                                    if (status == 409) {
                                        ctrl.$setValidity('available', false);
                                    } else {
                                        // something went wrong
                                        ctrl.$setValidity('lookup', false);
                                    }
                                });
                        }, 500);

                        ctrl.$parsers.unshift(function(viewValue) {
                            validateAsync(viewValue);
                            // async returns breaks the setting of $modelValue so just returning
                            return viewValue;
                        });
                    }
                };
            }
        ]
    );

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
                            resolve: {
                                user: [
                                    'userService', function(userService) {
                                        return userService.getUser();
                                    }
                                ]
                            },
                            controller: [
                                '$scope', 'notify', '$modalInstance', 'user', 'autofocus', function($scope, notify, $modalInstance, user, autofocus) {
                                    autofocus();
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
                                            "email": email,
                                            "createdInId": user.currentOrganizationId
                                        };

                                        var msg = notify.addInfoMessage("Opretter bruger, sender email...", false);

                                        $http.post("api/user", data, { handleBusy: true }).success(function(result, status) {
                                            var userResult = result.response;
                                            if (status == 201) {
                                                msg.toSuccessMessage(userResult.name + " er oprettet i KITOS");
                                            } else {
                                                msg.toInfoMessage("En bruger med den email-adresse fandtes allerede i systemet.");
                                            }

                                            $modalInstance.close(userResult);
                                        }).error(function(result) {
                                            msg.toErrorMessage("Fejl! Noget gik galt ved oprettelsen af " + name + "!");
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

    app.directive('selectUser', function() {

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
                    addUser: '@?',
                    allowClear: '@?',
                    onSelect: '&?',
                    ngDisabled: '=?',
                    orgId: '@?'
                },
                replace: true,
                templateUrl: 'partials/directives/select-user.html',
                controller: [
                    '$scope', '$http', '$timeout', function($scope, $http, $timeout) {
                        $scope.onChange = function() {

                            //for some reason (probably a bugger in select2)
                            //this is called 2 times, once with the original select value (like 1 or "")
                            //and once with the object value of select2 {id, text}.
                            //we only need the last one
                            if (typeof $scope.userModel !== 'object') return;

                            //timeout, otherwise we get the bad version of the model.
                            $timeout($scope.onSelect);
                        };

                        var userSrc = typeof $scope.orgId !== 'undefined' ? 'api/organization/' + $scope.orgId + '?users&q=' : 'api/user?q=';

                        $scope.selectUserOptions = {
                            //don't escape markup, otherwise formatResult will be bugged
                            escapeMarkup: function(m) { return m; },
                            formatResult: formatResult,
                            formatSelection: formatSelection,

                            allowClear: !!$scope.allowClear,

                            minimumInputLength: 1,
                            initSelection: function(elem, callback) {
                            },
                            ajax: {
                                data: function(term, page) {
                                    return { query: term };
                                },
                                quietMillis: 500,
                                transport: function(queryParams) {
                                    var res = $http.get(userSrc + queryParams.data.query).then(queryParams.success);
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
    );

    app.directive('selectAccessModifier', [
        function() {
            return {
                priority: 1,
                replace: true,
                templateUrl: 'partials/directives/select-access-modifier.html',
                controller: [
                    '$scope', 'userService', function($scope, userService) {
                        userService.getUser().then(function(user) {
                            $scope.isGlobalAdmin = user.isGlobalAdmin;
                        });
                    }
                ]
            };
        }
    ]);

    app.directive('selectStatus2', [
        '$timeout',
        function($timeout) {
            return {
                scope: {
                    canWrite: '=',
                },
                require: 'ngModel',
                templateUrl: 'partials/directives/select-status2.html',

                link: function(scope, element, attr, ngModel) {
                    scope.setModel = function(n) {
                        //only update on change
                        if (scope.model == n) return;

                        //save new value
                        scope.model = n;

                        $timeout(function() {
                            //then trigger event
                            ngModel.$setViewValue(scope.model);

                            //this triggers the autosave directive
                            element.triggerHandler("blur");
                        });
                    };

                    //read value from ngModel
                    ngModel.$render = function() {
                        scope.model = ngModel.$viewValue;
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

    app.directive('squareTrafficLight', [
        function() {
            return {
                template: '<progressbar class="status-bar" data-value="value" data-type="{{type}}"></progressbar>',
                scope: {
                    status: '=squareTrafficLight'
                },
                link: function(scope) {
                    switch (scope.status) {
                    case 1:
                        scope.type = 'danger';
                        scope.value = 100;
                        break;
                    case 2:
                        scope.type = 'warning';
                        scope.value = 100;
                        break;
                    case 3:
                        scope.type = 'success';
                        scope.value = 100;
                        break;
                    default:
                        scope.value = 0;
                    }
                }
            };
        }
    ]);

    app.directive('dateToString', [
        'dateFilter', function(dateFilter) {
            return {
                restrict: 'A',
                require: 'ngModel',
                link: function(scope, element, attr, ctrl) {
                    ctrl.$parsers.push(function(value) {
                        if (value instanceof Date)
                            return dateFilter(value, 'yyyy-MM-dd');
                        return value;
                    });
                }
            };
        }
    ]);

    app.directive('autosave', [
        '$http', '$timeout', 'notify', function($http, $timeout, notify) {
            return {
                restrict: 'A',
                require: 'ngModel',
                priority: 0,
                link: function(scope, element, attrs, ctrl) {
                    var oldValue;
                    $timeout(function() {
                        oldValue = ctrl.$modelValue; // get initial value
                    });

                    function saveIfNew() {
                        var newValue = ctrl.$modelValue;
                        if (attrs.pluck)
                            newValue = _.pluck(newValue, attrs.pluck);

                        var payload = {};
                        payload[attrs.field] = newValue;

                        if (newValue !== oldValue) {
                            if (ctrl.$valid) {
                                save(payload);
                            }
                        }
                    }

                    function saveCheckbox() {
                        // ctrl.$viewValue reflects the old state.
                        // using timeout to wait for the value to update
                        $timeout(function() {
                            var newValue = ctrl.$modelValue;
                            var payload = {};
                            payload[attrs.field] = newValue;
                            save(payload);
                        });
                    }

                    function saveSelect2() {
                        // ctrl.$viewValue reflects the old state.
                        // using timeout to wait for the value to update
                        $timeout(function() {
                            var newValue;

                            var viewValue = ctrl.$viewValue;
                            if (angular.isArray(viewValue)) {
                                newValue = _.pluck(viewValue, 'id');
                            } else if (angular.isObject(viewValue)) {
                                newValue = viewValue.id;
                            } else {
                                newValue = viewValue;
                            }

                            var payload = {};
                            payload[attrs.field] = newValue;

                            save(payload);
                        });
                    }

                    function saveMultipleSelect2(e) {
                        var id, msg = notify.addInfoMessage("Gemmer...", false);
                        if (e.added) {
                            id = e.added.id;
                            $http.post(attrs.autosave + '?' + attrs.field + '=' + id)
                                .success(function() {
                                    msg.toSuccessMessage("Feltet er opdateret.");
                                })
                                .error(function() {
                                    msg.toErrorMessage("Fejl! Feltet kunne ikke ændres!");
                                });
                        } else if (e.removed) {
                            id = e.removed.id;
                            $http.delete(attrs.autosave + '?' + attrs.field + '=' + id)
                                .success(function() {
                                    msg.toSuccessMessage("Feltet er opdateret.");
                                })
                                .error(function() {
                                    msg.toErrorMessage("Fejl! Feltet kunne ikke ændres!");
                                });
                        }
                    }

                    function save(payload) {
                        var msg = notify.addInfoMessage("Gemmer...", false);
                        $http({ method: 'PATCH', url: attrs.autosave, data: payload })
                            .success(function() {
                                msg.toSuccessMessage("Feltet er opdateret.");
                                oldValue = ctrl.$modelValue;
                            })
                            .error(function() {
                                msg.toErrorMessage("Fejl! Feltet kunne ikke ændres!");
                            });
                    }

                    // select2 fields trigger the change event
                    if (!angular.isUndefined(attrs.uiSelect2)) {
                        if (attrs.multiple) {
                            element.bind('change', saveMultipleSelect2);
                        } else {
                            element.bind('change', saveSelect2);
                        }
                    } else if (attrs.type === 'checkbox') {
                        element.bind('change', saveCheckbox);
                    } else {
                        element.bind('blur', saveIfNew);
                    }
                }
            };
        }
    ]);

    app.directive('datereader', [
        function() {
            return {
                scope: true,
                template: '<span>{{dateStr}}</span>',
                require: 'ngModel',
                link: function(scope, element, attr, ctrl) {

                    scope.date = {};

                    function read() {
                        if (angular.isUndefined(ctrl.$modelValue) || ctrl.$modelValue == null) scope.dateStr = "";
                        else scope.dateStr = moment(ctrl.$modelValue).format("DD-MM-YY", "da", true);
                    }

                    read();
                    ctrl.$render = read;
                }
            };
        }
    ]);

    app.directive('simpleComment', [
        function() {
            return {
                scope: true,
                require: 'ngModel',
                template: '<button class="btn btn-link btn-sm" data-ng-disabled="disabled" data-popover="{{comment}}"><i class="glyphicon glyphicon-comment small" data-ng-class="ngClassObj"></i></button>',
                link: function(scope, element, attr, ctrl) {

                    function setDisabled(disabled) {
                        scope.disabled = disabled;
                        scope.ngClassObj = { 'faded': disabled };
                    }

                    setDisabled(true);

                    ctrl.$render = function() {
                        setDisabled(!ctrl.$viewValue);

                        scope.comment = ctrl.$viewValue;
                    };
                }
            };
        }
    ]);

    app.directive('selectOrgUnit', [
        '$http', '$timeout', '$sce', 'userService',
        function($http, $timeout, $sce, userService) {
            return {
                scope: {
                    extraOptions: '=?',
                    allowClear: '@?',
                    ngDisabled: '=?'
                },
                //we require ngModel to be available on the outer tag
                require: 'ngModel',
                priority: 0,
                templateUrl: 'partials/directives/select-org-unit.html',
                link: function(scope, element, attrs, ctrl) {
                    //this is called when the user selects something from select2
                    element.bind('change', function() {
                        $timeout(function() {
                            //update the view value
                            ctrl.$setViewValue(scope.select.selected);

                            //this triggers the autosave directive
                            element.triggerHandler("blur");
                        });
                    });

                    //when the outer ngModel is changed, update the inner model
                    ctrl.$render = function() {
                        scope.select.selected = ctrl.$viewValue;
                    };

                    //stores the <options> for the select
                    var options = [];

                    //settings for select2
                    var settings = {
                        allowClear: !!scope.allowClear,

                        //don't format markup in result
                        escapeMarkup: function(m) { return m; },

                        //when an option has been selected, print the no-html version
                        formatSelection: function(item) {

                            var option;
                            if (item.id) {
                                option = _.findWhere(options, { id: parseInt(item.id) });
                            } else {
                                option = _.findWhere(options, { id: parseInt(item) });
                            }

                            if (option) {
                                return option.selectedText;
                            } else return null;
                        }
                    };

                    if (scope.extraOptions) {
                        _.each(scope.extraOptions, function(extraOption) {
                            var option = {
                                id: extraOption.id,
                                text: extraOption.text,
                                selectedText: extraOption.text
                            };
                            options.push(option);
                        });
                    }

                    //loads the org unit roots
                    userService.getUser().then(function(user) {

                        $http.get('api/organizationUnit?organization=' + user.currentOrganizationId, { cache: true }).success(function(result) {

                            //recursive function for added indentation, 
                            //and pushing org units to the list in the right order (depth-first)
                            function visit(orgUnit, indentation) {
                                var option = {
                                    id: orgUnit.id,
                                    text: $sce.trustAsHtml(indentation + orgUnit.name),
                                    selectedText: orgUnit.name
                                };

                                options.push(option);

                                _.each(orgUnit.children, function(child) {
                                    //indentation is non breaking spaces (alt+255)
                                    return visit(child, indentation + "    ");
                                });
                            }

                            visit(result.response, "");

                            scope.select.isReady = true;
                        });

                    });

                    scope.select = {
                        settings: settings,
                        options: options,
                        isReady: false,
                        selected: null
                    };
                }

            };
        }
    ]);

    app.directive('suggestNew', [
        '$http', 'notify', function($http, notify) {
            return {
                scope: {
                    url: '@'
                },
                templateUrl: 'partials/local-config/suggest-new.html',
                link: function(scope, element, attrs) {
                    scope.suggest = function() {
                        var data = {
                            "isSuggestion": true,
                            "name": scope.suggestion
                        };

                        $http.post(scope.url, data).success(function(result) {
                            notify.addSuccessMessage('Foreslag sendt!');
                            scope.suggestion = "";
                        }).error(function(result) {
                            notify.addErrorMessage('Kunne ikke sende foreslag!');
                        });
                    };
                }
            };
        }
    ]);

    app.directive('suggestNewRole', [
        '$http', 'notify', function($http, notify) {
            return {
                scope: {
                    url: '@'
                },
                templateUrl: 'partials/local-config/suggest-new-role.html',
                link: function(scope, element, attrs) {
                    scope.suggest = function() {

                        var data = {
                            "isSuggestion": true,
                            "name": scope.suggestion,
                            "hasReadAccess": true,
                            "hasWriteAccess": scope.writeAccess
                        };

                        $http.post(scope.url, data).success(function(result) {
                            notify.addSuccessMessage('Foreslag sendt!');
                            scope.suggestion = "";
                        }).error(function(result) {
                            notify.addErrorMessage('Kunne ikke sende foreslag!');
                        });
                    };
                }
            };
        }
    ]);

    app.directive('optionList', [
        '$http', function($http) {
            return {
                scope: {
                    optionsUrl: '@',
                    title: '@',
                },
                templateUrl: 'partials/local-config/optionlist.html',
                link: function(scope, element, attrs) {

                    scope.list = [];

                    $http.get(scope.optionsUrl + '?nonsuggestions').success(function(result) {
                        _.each(result.response, function(v) {
                            scope.list.push({
                                id: v.id,
                                name: v.name,
                                note: v.note
                            });
                        });
                    });
                }
            };
        }
    ]);

    app.directive('globalOptionList', [
        '$http', '$timeout', '$state', '$stateParams', 'notify', function($http, $timeout, $state, $stateParams, notify) {
            return {
                scope: {
                    optionsUrl: '@',
                    title: '@',
                },
                templateUrl: 'partials/global-config/optionlist.html',
                link: function(scope, element, attrs) {
                    scope.list = [];
                    $http.get(scope.optionsUrl + '?nonsuggestions').success(function(result) {
                        _.each(result.response, function(v) {
                            scope.list.push({
                                id: v.id,
                                name: v.name,
                                note: v.note,
                                isActive: v.isActive
                            });
                        });
                    });

                    scope.suggestions = [];
                    $http.get(scope.optionsUrl + '?suggestions').success(function(result) {
                        _.each(result.response, function(v) {
                            scope.suggestions.push({
                                id: v.id,
                                name: v.name,
                                note: v.note
                            });
                        });
                    });

                    scope.approve = function(id) {
                        var msg = notify.addInfoMessage("Gemmer...", false);
                        $http({ method: 'PATCH', url: scope.optionsUrl + '/' + id, data: { isSuggestion: false } })
                            .success(function() {
                                msg.toSuccessMessage("Valgmuligheden er opdateret.");
                                // reload page to show changes
                                reload();
                            })
                            .error(function() {
                                msg.toErrorMessage("Fejl! Valgmuligheden kunne ikke ændres!");
                            });
                    };

                    // work around for $state.reload() not updating scope
                    // https://github.com/angular-ui/ui-router/issues/582
                    function reload() {
                        return $state.transitionTo($state.current, $stateParams, {
                            reload: true
                        }).then(function() {
                            scope.hideContent = true;
                            return $timeout(function() {
                                return scope.hideContent = false;
                            }, 1);
                        });
                    };
                }
            };
        }
    ]);

    app.directive('globalOptionRoleList', [
        '$http', '$timeout', '$state', '$stateParams', 'notify', function($http, $timeout, $state, $stateParams, notify) {
            return {
                scope: {
                    optionsUrl: '@',
                    title: '@',
                },
                templateUrl: 'partials/global-config/optionrolelist.html',
                link: function(scope, element, attrs) {
                    scope.list = [];
                    $http.get(scope.optionsUrl + '?nonsuggestions').success(function(result) {
                        _.each(result.response, function(v) {
                            scope.list.push({
                                id: v.id,
                                name: v.name,
                                note: v.note,
                                isActive: v.isActive,
                                hasWriteAccess: v.hasWriteAccess
                            });
                        });
                    });

                    scope.suggestions = [];
                    $http.get(scope.optionsUrl + '?suggestions').success(function(result) {
                        _.each(result.response, function(v) {
                            scope.suggestions.push({
                                id: v.id,
                                name: v.name,
                                note: v.note
                            });
                        });
                    });

                    scope.approve = function(id) {
                        var msg = notify.addInfoMessage("Gemmer...", false);
                        $http({ method: 'PATCH', url: scope.optionsUrl + '/' + id, data: { isSuggestion: false } })
                            .success(function() {
                                msg.toSuccessMessage("Rollen er opdateret.");
                                // reload page to show changes
                                reload();
                            })
                            .error(function() {
                                msg.toErrorMessage("Fejl! Rollen kunne ikke ændres!");
                            });
                    };

                    // work around for $state.reload() not updating scope
                    // https://github.com/angular-ui/ui-router/issues/582
                    function reload() {
                        return $state.transitionTo($state.current, $stateParams, {
                            reload: true
                        }).then(function() {
                            scope.hideContent = true;
                            return $timeout(function() {
                                return scope.hideContent = false;
                            }, 1);
                        });
                    };
                }
            };
        }
    ]);

    app.directive('kleFilter', [
        'taskService', function(taskService) {
            return {
                scope: {
                    // the output of filtering tasks
                    selectedGroup: "=kleFilter"
                },
                templateUrl: 'partials/directives/kle-filter.html',
                link: function(scope, element, attrs) {

                    // loading main groups
                    taskService.getRoots().then(function(roots) {
                        scope.maingroups = roots;
                    });

                    // called when selected a main group
                    scope.maingroupChanged = function() {
                        scope.taskList = [];
                        scope.selectedSubgroup = null;
                        scope.groupChanged();

                        if (!scope.selectedMaingroup) return;

                        // load groups
                        taskService.getChildren(scope.selectedMaingroup).then(function(groups) {
                            scope.groups = groups;
                        });
                    };

                    scope.groupChanged = function() {
                        if (scope.selectedSubgroup)
                            scope.selectedGroup = scope.selectedSubgroup;
                        else if (scope.selectedMaingroup)
                            scope.selectedGroup = scope.selectedMaingroup;
                        else
                            scope.selectedGroup = '';
                    };
                }
            };
        }
    ]);


    app.directive('paginationButtons', [
        function() {
            return {
                scope: {
                    //the output of filtering tasks
                    pagination: "=paginationButtons"
                },
                templateUrl: 'partials/directives/pagination.html',
                link: function(scope, element, attrs) {
                    scope.less = function() {
                        scope.pagination.skip -= scope.pagination.take;
                        if (scope.pagination.skip < 0) scope.pagination.skip = 0;
                    };

                    scope.more = function() {
                        scope.pagination.skip += scope.pagination.take;
                    };
                }
            };
        }
    ]);

    app.directive('orderBy', [
        function() {
            return {
                scope: {
                    orderBy: '=orderBy',
                    pagination: '=paging',
                },
                replace: true,
                templateUrl: 'partials/directives/order-by.html',
                link: function(scope, element, attrs) {
                    scope.order = function() {
                        scope.pagination.skip = 0;

                        if (scope.pagination.orderBy == scope.orderBy) {
                            scope.pagination.descending = !scope.pagination.descending;
                        } else {
                            scope.pagination.orderBy = scope.orderBy;
                            scope.pagination.descending = false;
                        }
                    };
                }

            };

        }
    ]);

    app.directive('searchBox', [
        '$timeout', function($timeout) {
            return {
                scope: {
                    pagination: '=paging'
                },
                replace: true,
                templateUrl: 'partials/directives/search-box.html',
                link: function(scope, element, attrs) {
                    var updatePromise = null;

                    function doUpdate() {
                        scope.pagination.skip = 0;
                        scope.pagination.search = scope.search;

                        updatePromise = null;
                    }


                    scope.update = function() {
                        if (updatePromise) $timeout.cancel(updatePromise);

                        updatePromise = $timeout(doUpdate, 200);
                    };
                }
            };
        }
    ]);

    app.directive('showErrors', [
            '$timeout', function($timeout) {
                var linkFn;
                linkFn = function(scope, el, attrs, formCtrl) {
                    var blurred, inputEl, inputName, inputNgEl, showSuccess, toggleClasses;
                    blurred = false;
                    showSuccess = true;
                    inputEl = el[0].querySelector('[name]');
                    inputNgEl = angular.element(inputEl);
                    inputName = inputNgEl.attr('name');
                    if (!inputName) {
                        throw 'show-errors element has no child input elements with a \'name\' attribute';
                    }
                    inputNgEl.bind('blur', function() {
                        blurred = true;
                        return toggleClasses(formCtrl[inputName].$invalid);
                    });
                    scope.$watch(function() {
                        return formCtrl[inputName] && formCtrl[inputName].$invalid;
                    }, function(invalid) {
                        if (!blurred) {
                            return;
                        }
                        return toggleClasses(invalid);
                    });
                    scope.$on('show-errors-check-validity', function() {
                        return toggleClasses(formCtrl[inputName].$invalid);
                    });
                    scope.$on('show-errors-reset', function() {
                        return $timeout(function() {
                            el.removeClass('has-error');
                            el.removeClass('has-success');
                            return blurred = false;
                        }, 0, false);
                    });
                    return toggleClasses = function(invalid) {
                        el.toggleClass('has-error', invalid);
                        if (showSuccess) {
                            return el.toggleClass('has-success', !invalid);
                        }
                    };
                };
                return {
                    restrict: 'A',
                    require: '^form',
                    compile: function(elem, attrs) {
                        if (!elem.hasClass('form-group')) {
                            throw 'show-errors element does not have the \'form-group\' class';
                        }
                        return linkFn;
                    }
                };
            }
        ]
    );
})(angular, app);
