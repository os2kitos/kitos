(function (ng, app) {
    app.config(["$stateProvider", function ($stateProvider) {
        $stateProvider.state("it-project.edit.references", {
            url: "/reference/",
            templateUrl: "app/components/it-reference.view.html",
            controller: "project.EditReference",
            controllerAs: "Vm"
        });
    }]);

    app.controller("project.EditReference",
        ["$scope", "$http", "$timeout", "$state", "$stateParams","project","$confirm","notify","$",
            function ($scope, $http, $timeout, $state, $stateParams,project,$confirm,notify,$) {
                console.log(project);

                //$scope.objectId = project.id;

                //$scope.objectReference = 'it-project.edit.references.create';
                
                
               // $scope.references = project.externalReferences;

                $scope.deleteReference = function (event) {
                    var msg = notify.addInfoMessage("Sletter...");

                    event.preventDefault();
                    var dataItem = $scope.mainGrid.dataItem($(event.currentTarget).closest("tr"));
                    var id = dataItem["id"];
                    
                    $http.delete('api/Reference/' + id + '?organizationId=' + project.organizationId).success(() => {
                        msg.toSuccessMessage("Slettet!");
                    }).error(() => {
                        msg.toErrorMessage("Fejl! Kunne ikke slette!");
                        });
                    reload();
                };

                $scope.isValidUrl = function (url) {
                    console.log(url);
                    var regexp = /(http):\/\/(\w+:{0,1}\w*@)?(\S+)(:[0-9]+)?(\/|\/([\w#!:.?+=&%@!\-\/]))?/;
                    return regexp.test(url);
                };

                $scope.edit = function (event) {
                    event.preventDefault();
                    var dataItem = $scope.mainGrid.dataItem($(event.currentTarget).closest("tr"));
                    $state.go(".edit", { refId: dataItem["id"], orgId: project.organizationId });
                };

                function reload() {
                    $state.go(".", null, { reload: true });
                };

                $scope.mainGridOptions  = {
                dataSource: {
                        data: project.externalReferences,
                        pageSize: 10
                    },
                    sortable: true,
                    pageable: {
                        refresh: true,
                        pageSizes: true,
                        buttonCount: 5
                    },
                    columns: [{
                        field: "title",
                        title: "Dokumenttitel",
                        template: function (data) {
                            if ($scope.isValidUrl(data.url)) {
                                return "<a href=\"" + data.url + "\">" + data.title + "</a>"; 
                            } else {
                                return data.title;
                            }
                        },
                        width: 240
                    }, {
                        field: "externalReferenceId",
                        title: "Evt. dokumentID/Sagsnr./anden referenceContact"
                    }, {
                        field: "created",
                        title: "Oprettet",
                        template: "#= kendo.toString(kendo.parseDate(created, 'yyyy-MM-dd'), 'dd. MMMM yyyy') #"
                        
                    }, {
                        field: "objectOwner.fullName",
                        title: "Oprettet af",
                        width: 150
                    }, {
                        title: "Rediger",
                        command: [
                            { text: "Redigér", click: $scope.edit, imageClass: "k-edit", className: "k-custom-edit", iconClass: "k-icon"} as any,
                            { text: "Slet", click: $scope.deleteReference, imageClass: "k-delete", className: "k-custom-delete", iconClass: "k-icon"} as any,
                        ]
                    }],
                    toolbar: [
                        {
                            name: "addReference",
                            text: "Tilføj reference",
                            template: "<a id=\"addReferenceasdasd\" class=\"btn btn-success btn-sm\" href=\"\\#/project/edit/" + project.id + "/reference//createReference/" + project.id +"\"'>#=text#</a>"
                        }]
                };
            }]);
})(angular, app);
