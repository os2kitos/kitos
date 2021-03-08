(function (ng, app) {
    'use strict';

    app.directive('selectUserOrgUnit', [
        '$http', '$timeout', '$sce', 'userService',
        function ($http, $timeout, $sce, userService) {
            return {
                scope: {
                    extraOptions: '=?',
                    allowClear: '@?',
                    ngDisabled: '=?'
                },
                //we require ngModel to be available on the outer tag
                require: 'ngModel',
                priority: 0,
                templateUrl: 'app/components/home/selectUserOrgUnit/selectUserOrgUnit.view.html',
                link: function (scope, element, attrs, ctrl) {
                    //this is called when the user selects something from select2
                    element.bind('change', function () {
                        $timeout(function () {
                            //update the view value
                            ctrl.$setViewValue(scope.select.selected);

                            //this triggers the autosave directive
                            element.triggerHandler("blur");
                        });
                    });

                    //when the outer ngModel is changed, update the inner model
                    ctrl.$render = function () {
                        scope.select.selected = ctrl.$viewValue;
                    };

                    //stores the <options> for the select
                    var options = [];

                    //settings for select2
                    var settings = {
                        allowClear: !!scope.allowClear,

                        //don't format markup in result
                        escapeMarkup: function (m) { return m; },

                        //when an option has been selected, print the no-html version
                        formatSelection: function (item) {

                            var option;
                            if (item.id) {
                                option = _.find(options, { id: parseInt(item.id) });
                            } else {
                                option = _.find(options, { id: parseInt(item) });
                            }

                            if (option) {
                                return option.selectedText;
                            } else return null;
                        }
                    };

                    if (scope.extraOptions) {
                        _.each(scope.extraOptions, function (extraOption) {
                            var option = {
                                id: extraOption.id,
                                text: extraOption.text,
                                selectedText: extraOption.text
                            };
                            options.push(option);
                        });
                    }

                    //loads the org unit roots
                    userService.getUser().then(function (user) {

                        $http.get('api/organizationUnit?organization=' + user.currentOrganizationId, { cache: true })
                            .then(function onSuccess(result) {

                                //recursive function for added indentation,
                                //and pushing org units to the list in the right order (depth-first)
                                function visit(orgUnit, indentation) {
                                    var option = {
                                        id: orgUnit.id,
                                        text: $sce.trustAsHtml(indentation + orgUnit.name),
                                        selectedText: orgUnit.name
                                    };

                                    options.push(option);

                                    _.each(orgUnit.children, function (child) {
                                        //indentation is non breaking spaces (alt+255)
                                        return visit(child, indentation + "    ");
                                    });
                                }

                                visit(result.data.response, "");

                                scope.select.isReady = true;
                            });

                    });

                    scope.select = {
                        settings: settings,
                        options: options,
                        isReady: false,
                        selected: userService.getUser().defaultOrganizationUnitId
                    };
                }
            };
        }
    ]);
})(angular, app);
