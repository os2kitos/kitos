angular.module("notify").directive("notify", ["$rootScope", function ($rootScope) {
    "use strict";

    return {
        restrict: 'A',
        template: '<div class="notify">' +
        '	<div class="notify-item alert" ng-repeat="message in messages" ng-class="computeClasses(message)">' +
        '		<button type="button" class="close" ng-click="deleteMessage(message)">&times;</button>' +
        '       <div data-element-type="notification-message-block" ng-bind-html="message.text"></div>' +
        '	</div>' +
        '</div>',
        replace: false,
        scope: true,
        controller: ['$scope', '$sce', '$timeout', 'notify', function ($scope, $sce, $timeout, notify) {
            var onlyUnique = notify.onlyUnique();

            $scope.messages = [];

            function addMessage(message) {
                message.closed = false;

                $scope.messages.push(message);
            }

            $rootScope.$on("notifyNewMessage", function (event, message) {
                var found;
                if (onlyUnique) {
                    angular.forEach($scope.messages, function (msg) {
                        if (message.text === msg.text && message.severity === msg.severity) {
                            found = true;
                        }
                    });

                    if (!found) {
                        addMessage(message);
                    }
                } else {
                    addMessage(message);
                }
            });

            $rootScope.$on("notifyDeleteMessage", function (event, message) {
                $scope.deleteMessage(message);
            });

            $scope.deleteMessage = function (message) {
                var index = $scope.messages.indexOf(message);
                if (index > -1) {
                    message.closed = true;
                    $scope.messages.splice(index, 1);
                }

            };

            $scope.computeClasses = function (message) {
                return {
                    'alert-success': message.severity === "success",
                    'alert-error': message.severity === "error", //bootstrap 2.3
                    'alert-danger': message.severity === "error", //bootstrap 3
                    'alert-info': message.severity === "info",
                    'alert-warning': message.severity === "warn" //bootstrap 3, no effect in bs 2.3
                };
            };
        }]
    };
}]);
