function NotifiyProvider() {
    "use strict";

    var _ttl = 5000,
        _autoclose = true,
        _messagesKey = 'messages',
        _messageTextKey = 'text',
        _messageSeverityKey = 'severity',
        _onlyUniqueMessages = true;

	/**
	 * set a global timeout (time to live) after which messages will be automatically closed
	 *
	 * @param ttl in millisseconds
	 */
    this.globalTimeToLive = function (ttl) {
        _ttl = ttl;
    };

    this.onlyUniqueMessages = function (onlyUniqueMessages) {
        _onlyUniqueMessages = onlyUniqueMessages;
    };

    this.$get = ["$rootScope", "$filter", '$timeout', '$sce', function ($rootScope, $filter, $timeout, $sce) {
        var translate;

        try {
            translate = $filter("translate");
        } catch (e) {
            //
        }

        function closeMessage(message) {
            $rootScope.$broadcast("notifyDeleteMessage", message);
        }

        function setMessageTimeout(message) {
            if (message.autoclose) {

                if (message.timeout) {
                    $timeout.cancel(message.timeout);
                    delete message.timeout;
                }

                message.timeout = $timeout(function () {
                    closeMessage(message);
                }, _ttl);
            }
        }

        function broadcastMessage(message) {
            if (translate) {
                message.text = translate(message.text);
            }

            $rootScope.$broadcast("notifyNewMessage", message);
            setMessageTimeout(message);
        }

        function addMessage(text, severity, autoclose) {

            if (typeof autoclose === 'undefined') autoclose = _autoclose;

            var message: any = {
                text: $sce.trustAsHtml(text),
                severity: severity,
                autoclose: autoclose,

                update: function (text, severity, autoclose) {
                    if (typeof autoclose === 'undefined') autoclose = _autoclose;

                    this.text = $sce.trustAsHtml(text);
                    this.severity = severity;
                    this.autoclose = autoclose;

                    if (message.closed) {
                        broadcastMessage(message);
                    } else {
                        setMessageTimeout(message);
                    }

                },

                toWarnMessage: function (text, autoclose) {
                    this.update(text, "warn", autoclose);
                },
                toErrorMessage: function (text, autoclose) {
                    this.update(text, "error", autoclose);
                },
                toInfoMessage: function (text, autoclose) {
                    this.update(text, "info", autoclose);
                },
                toSuccessMessage: function (text, autoclose) {
                    this.update(text, "success", autoclose);
                },

                close: function () {
                    closeMessage(this);
                }
            };

            broadcastMessage(message);

            return message;
        }

		/**
		 * add one warn message with bootstrap class: alert
		 *
		 * @param {string} text
		 * @param {boolean} autoclose
         */
        function addWarnMessage(text, autoclose) {
            return addMessage(text, "warn", autoclose);
        }

		/**
		 * add one error message with bootstrap classes: alert, alert-error
		 *
		 * @param {string} text
		 * @param {boolean} autoclose
		 */
        function addErrorMessage(text, autoclose) {
            return addMessage(text, "error", autoclose);
        }

		/**
		 * add one info message with bootstrap classes: alert, alert-info
		 *
		 * @param {string} text
		 * @param {boolean} autoclose
		 */
        function addInfoMessage(text, autoclose) {
            return addMessage(text, "info", autoclose);
        }

		/**
		 * add one success message with bootstrap classes: alert, alert-success
		 *
		 * @param {string} text
		 * @param {boolean} autoclose
		 */
        function addSuccessMessage(text, autoclose) {
            return addMessage(text, "success", autoclose);
        }

        function onlyUnique() {
            return _onlyUniqueMessages;
        }

        return {
            addWarnMessage: addWarnMessage,
            addErrorMessage: addErrorMessage,
            addInfoMessage: addInfoMessage,
            addSuccessMessage: addSuccessMessage,
            onlyUnique: onlyUnique
        };
    }];

    return this;
}

angular.module("notify").provider("notify", NotifiyProvider);
