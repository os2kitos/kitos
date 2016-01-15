(function(ng, app) {
    'use strict';

    app.directive('kleFilter', [
        'taskService', function(taskService) {
            return {
                scope: {
                    // the output of filtering tasks
                    selectedGroup: "=kleFilter"
                },
                templateUrl: 'app/shared/kleFilter/kleFilter.view.html',
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
})(angular, app);
