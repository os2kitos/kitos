(function(ng, app) {
    var subnav = [
        { state: 'config.org', text: 'Organisation' },
        { state: 'config.project', text: 'IT Projekt' },
        { state: 'config.system', text: 'IT System' },
        { state: 'config.contract', text: 'IT Kontrakt' }
    ];

    app.config(['$stateProvider', function($stateProvider) {
        $stateProvider.state('config.org', {
            url: '/org',
            templateUrl: 'app/components/global-config/global-config-org.view.html',
            controller: 'globalConfig.OrgCtrl',
            authRoles: ['GlobalAdmin']
        });
    }]);

    app.controller('globalConfig.OrgCtrl', ['$rootScope', '$scope', function ($rootScope, $scope) {
        $rootScope.page.title = 'Global konfiguration';
        $rootScope.page.subnav = subnav;
        
       /* $("mainGrid").kendoGrid.table.kendoSortable({
            filter: ">tbody >tr",
            // hint: $.noop,
            cursor: "move",
            placeholder: function (element) {
                return element.clone().addClass("k-state-hover").css("opacity", 0.65);
            },
            container: "#grid tbody",
            change: function (e) {
                var skip = $scope.mainGrid.dataSource.skip(),
                    oldIndex = e.oldIndex + skip,
                    newIndex = e.newIndex + skip,
                    data = $scope.mainGrid.dataSource.data(),
                    dataItem = $scope.mainGrid.dataSource.getByUid(e.item.data("uid"));

                $scope.mainGrid.dataSource.remove(dataItem);
                $scope.mainGrid.dataSource.insert(newIndex, dataItem);
            }
        });



        /* $scope.mainGrid.table.kendoSortable({
             filter: ">tbody >tr",
            // hint: $.noop,
             cursor: "move",
             placeholder: function (element) {
                 return element.clone().addClass("k-state-hover").css("opacity", 0.65);
             },
             container: "#grid tbody",
             change: function (e) {
                 var skip = $scope.mainGrid.dataSource.skip(),
                     oldIndex = e.oldIndex + skip,
                     newIndex = e.newIndex + skip,
                     data = $scope.mainGrid.dataSource.data(),
                     dataItem = $scope.mainGrid.dataSource.getByUid(e.item.data("uid"));
 
                 $scope.mainGrid.dataSource.remove(dataItem);
                 $scope.mainGrid.dataSource.insert(newIndex, dataItem);
             }
        });*/
     
    }]);
})(angular, app);
