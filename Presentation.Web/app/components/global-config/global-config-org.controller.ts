(function(ng, app) {

    // The class is used for testing purposes to demonstrate that the state can be reached from the controller but not through the directive
    class GlobalConfigOrgCtrl {

        private static $inject = ["$state"];
        constructor(private $state: ng.ui.IStateService) {
        }

        public showDialog = () => {
            console.log("goto config.org.edit-roles");
            this.$state.go("config.org.edit-roles", { id: 0 });
        }

    }

    app.config(['$stateProvider', $stateProvider => {
        $stateProvider.state('config.org', {
            url: '/org',
            templateUrl: 'app/components/global-config/global-config-org.view.html',
            controller: GlobalConfigOrgCtrl,
            controllerAs: "vm",
            authRoles: ['GlobalAdmin']
        });
    }]);

})(angular, app);
