((ng, app) => {
    app.config([
        "$stateProvider", $stateProvider => {
            $stateProvider.state("organization.structure", {
                url: "/structure",
                templateUrl: "app/components/org/structure/org-structure.view.html",
                controller: "org.StructureCtrl",
                resolve: {
                    currentOrganization: [
                        "$http", "user", ($http: ng.IHttpService, user) => $http.get<Kitos.API.Models.IApiWrapper<any>>("api/organization/" + user.currentOrganizationId).then((result) => {
                            return result.data.response;
                        })
                    ],
                    rootNodeOfOrganization: [
                        "$http", "user", ($http: ng.IHttpService, user) => $http.get<Kitos.API.Models.IApiWrapper<any>>("api/organizationunit?organization=" + user.currentOrganizationId).then((result) => {
                            return result.data.response;
                        })
                    ],
                    localOrgUnitRoles: ['localOptionServiceFactory', (localOptionServiceFactory: Kitos.Services.LocalOptions.ILocalOptionServiceFactory) =>
                        localOptionServiceFactory.create(Kitos.Services.LocalOptions.LocalOptionType.OrganizationUnitRoles).getAll()
                    ],
                    orgUnitRoles: ['$http', $http => $http.get("odata/OrganizationUnitRoles")
                        .then(result => result.data.value)],
                    user: [
                        "userService", userService => userService.getUser()
                    ],
                    userAccessRights: ["authorizationServiceFactory", "user",
                        (authorizationServiceFactory: Kitos.Services.Authorization.IAuthorizationServiceFactory, user) =>
                            authorizationServiceFactory
                                .createOrganizationAuthorization()
                                .getAuthorizationForItem(user.currentOrganizationId)
                    ],
                    hasWriteAccess: ["userAccessRights", userAccessRights => userAccessRights.canEdit
                    ]
                }
            });
        }
    ]);

    app.controller("org.StructureCtrl", [
        "$scope", "$http", "$uibModal", "$state", "notify", "rootNodeOfOrganization", "localOrgUnitRoles", "orgUnitRoles", "user", "hasWriteAccess", "authorizationServiceFactory", "select2LoadingService", "inMemoryCacheService", "organizationUnitService","currentOrganization",
        function ($scope,
            $http: ng.IHttpService,
            $modal,
            $state,
            notify,
            rootNodeOfOrganization: Kitos.Models.ViewModel.Organization.IOrganizationUnitReorderViewModel,
            localOrgUnitRoles,
            orgUnitRoles,
            user,
            hasWriteAccess,
            authorizationServiceFactory: Kitos.Services.Authorization.IAuthorizationServiceFactory,
            select2LoadingService: Kitos.Services.ISelect2LoadingService,
            inMemoryCacheService: Kitos.Shared.Caching.IInMemoryCacheService,
            organizationUnitService: Kitos.Services.Organization.IOrganizationUnitService,
            currentOrganization) {
            $scope.orgId = user.currentOrganizationId;
            $scope.pagination = {
                skip: 0,
                take: 50
            };
            $scope.rightsPagination = {
                skip: 0,
                take: 15
            };

            //flattened map of all loaded orgUnits
            $scope.orgUnits = {};
            $scope.hasWriteAccess = hasWriteAccess;
            $scope.currentOrganizationName = currentOrganization.name;
            $scope.orgUnitRoles = orgUnitRoles;
            $scope.activeOrgRoles = localOrgUnitRoles;
            $scope.orgRoles = {};
            _.each(localOrgUnitRoles, function (orgRole: { Id }) {
                $scope.orgRoles[orgRole.Id] = orgRole;
            });
            $scope.showDifferenceBetweenOrgUnitOrigin =
                // User is an admin with edit rights to the hierarchy
                (user.isGlobalAdmin || user.isLocalAdmin) &&
                // Hierarchy root has been synced from a different source than KITOS
                rootNodeOfOrganization.origin !== Kitos.Models.Api.Organization.OrganizationUnitOrigin.Kitos;


            function flattenAndSave(orgUnit, inheritWriteAccess, parentOrgunit) {
                orgUnit.parent = parentOrgunit;

                //restore previously saved settings
                if ($scope.orgUnits[orgUnit.id]) {
                    var old = $scope.orgUnits[orgUnit.id];
                    orgUnit.isOpen = old.isOpen;
                }

                checkForDefaultUnit(orgUnit);

                $scope.orgUnits[orgUnit.id] = orgUnit;

                if (!inheritWriteAccess) {

                    authorizationServiceFactory
                        .createOrganizationUnitAuthorization()
                        .getAuthorizationForItem(orgUnit.id)
                        .then(response => {
                            orgUnit.hasWriteAccess = response.canEdit;

                            _.each(orgUnit.children,
                                u => {
                                    flattenAndSave(u, response.canEdit, orgUnit);
                                });
                        });
                } else {
                    orgUnit.hasWriteAccess = true;

                    _.each(orgUnit.children, function (u) {
                        return flattenAndSave(u, true, orgUnit);
                    });
                }
            }

            function checkForDefaultUnit(unit) {
                if (!user.currentOrganizationUnitId) return;

                if (!unit || unit.id !== user.currentOrganizationUnitId) return;

                open(unit);
                $scope.chooseOrgUnit(unit);

                function open(u) {
                    u.isOpen = true;
                    if (u.parent) open(u.parent);
                }
            }

            function loadUnits() {
                var rootNode = rootNodeOfOrganization;
                $scope.nodes = [rootNode];

                flattenAndSave(rootNode, false, null);
            }

            $scope.showChildren = false;

            $scope.chosenOrgUnit = null;

            $scope.chooseOrgUnit = (node, event) => {
                if (event) {
                    var isDiv = angular.element(event.target)[0].tagName === "DIV";
                    if (!isDiv) {
                        return;
                    }
                }
                if ($scope.chosenOrgUnit === node) return;

                // reset state between selecting the current organization
                $scope.showChildren = false;

                if ($scope.chosenOrgUnit === node) return;

                // reset pagination
                $scope.rightsPagination = {
                    skip: 0,
                    take: 15
                };

                loadRights(node);
            };

            function loadRights(node) {
                if (node) {
                    //get org rights on the org unit and subtree
                    $http.get<Kitos.API.Models.IApiWrapper<any>>("api/organizationUnitRight/" + node.id + "?paged&take=" + $scope.rightsPagination.take + "&skip=" + $scope.rightsPagination.skip).then((result) => {
                        var paginationHeader = JSON.parse(result.headers("X-Pagination"));
                        $scope.totalRightsCountCopy = paginationHeader.TotalCount;
                        node.orgRights = result.data.response;

                        var count = 0;
                        _.each(node.orgRights, function (right: { userForSelect; roleForSelect; user; roleId; show; objectId; }) {
                            right.userForSelect = { id: right.user.id, text: right.user.fullName };
                            right.roleForSelect = right.roleId;
                            right.show = $scope.showChildren || belongsToChosenNode(node, right);
                            if (right.show)
                                count++;
                        });

                        $scope.totalRightsCount = ($scope.showChildren) ? $scope.totalRightsCountCopy : count;
                    });
                }

                $scope.chosenOrgUnit = node;
            }

            // don't show users in subunits if showChildren is false
            $scope.toggleChildren = function () {
                const node = $scope.chosenOrgUnit;
                var count = 0;
                _.each(node.orgRights,
                    function (right: { show; }) {
                        right.show = $scope.showChildren || belongsToChosenNode(node, right);
                        if (right.show)
                            count++;
                    });

                $scope.totalRightsCount = ($scope.showChildren) ? $scope.totalRightsCountCopy : count;

                if (!$scope.showChildren && $scope.totalRightsCount === 0 && $scope.rightsPagination.skip > 0) {
                    $scope.rightsPagination.skip = 0;
                    loadRights($scope.chosenOrgUnit);
                }
            };

            function belongsToChosenNode(node, right) {
                return node.id === right.objectId;
            }

            $scope.$watch("selectedUser", function () {
                $scope.submitRight();
            });

            $scope.submitRight = function () {
                if (!$scope.selectedUser || !$scope.newRole) return;

                var oId = $scope.chosenOrgUnit.id;
                var rId = $scope.newRole.id;
                var uId = $scope.selectedUser.id;

                if (!oId || !rId || !uId) return;
                
                var data = {
                    "roleId": rId,
                    "userId": uId
                };

                $http.post<Kitos.API.Models.IApiWrapper<any>>("api/organizationUnitRight/" + oId + "?organizationId=" + user.currentOrganizationId, data).then((result) => {
                    notify.addSuccessMessage(result.data.response.user.fullName + " er knyttet i rollen");

                    $scope.chosenOrgUnit.orgRights.push({
                        "objectId": result.data.response.objectId,
                        "roleId": result.data.response.roleId,
                        "userId": result.data.response.userId,
                        "user": result.data.response.user,
                        "userForSelect": { id: result.data.response.userId, text: result.data.response.user.fullName },
                        "roleForSelect": result.data.response.roleId,
                        show: true
                    });

                    $scope.selectedUser = "";
                }, (error) => {
                    notify.addErrorMessage("Fejl!");
                });
            };

            $scope.deleteRight = function (right) {
                var oId = right.objectId;
                var rId = right.roleId;
                var uId = right.userId;

                $http.delete<Kitos.API.Models.IApiWrapper<any>>("api/organizationUnitRight/" + oId + "?rId=" + rId + "&uId=" + uId + "&organizationId=" + user.currentOrganizationId).then((deleteResult) => {
                    right.show = false;
                    notify.addSuccessMessage("Rollen er slettet!");
                }, (error) => {
                    notify.addErrorMessage("Kunne ikke slette rollen!");
                });
            };

            $scope.updateRight = function (right) {
                if (!right.roleForSelect || !right.userForSelect) return;

                if (!$scope.checkIfRoleIsAvailable(right.roleForSelect.id)) {
                    right.edit = false;
                    return;
                }

                //old values
                var oIdOld = right.objectId;
                var rIdOld = right.roleId;
                var uIdOld = right.userId;

                //new values
                var oIdNew = right.objectId;
                var rIdNew = right.roleForSelect.id;
                var uIdNew = right.userForSelect.id;

                //if nothing was changed, just exit edit-mode
                if (oIdOld === oIdNew && rIdOld === rIdNew && uIdOld === uIdNew) {
                    right.edit = false;
                    return;
                }

                //otherwise, we should delete the old entry, then add a new one

                $http.delete<Kitos.API.Models.IApiWrapper<any>>("api/organizationUnitRight/" + oIdOld + "?rId=" + rIdOld + "&uId=" + uIdOld + "&organizationId=" + user.currentOrganizationId).then((deleteResult) => {
                    var data = {
                        "roleId": rIdNew,
                        "userId": uIdNew
                    };

                    $http.post<Kitos.API.Models.IApiWrapper<any>>("api/organizationUnitRight/" + oIdNew + "?organizationId=" + user.currentOrganizationId, data).then((result) => {
                        right.roleId = result.data.response.roleId;
                        right.user = result.data.response.user;
                        right.userId = result.data.response.userId;

                        right.edit = false;

                        notify.addSuccessMessage(right.user.fullName + " er knyttet i rollen");
                    }, (error) => {
                        // we successfully deleted the old entry, but didn't add a new one
                        right.show = false;

                        notify.addErrorMessage("Fejl!");
                    });
                }, (error) => {
                    // couldn't delete the old entry, just reset select options
                    right.userForSelect = { id: right.user.id, text: right.user.fullName };
                    right.roleForSelect = right.roleId;

                    notify.addErrorMessage("Fejl!");
                });
            };

            $scope.rightSortBy = "orgUnitName";
            $scope.rightSortReverse = false;
            $scope.rightSort = function (right) {
                switch ($scope.rightSortBy) {
                    case "orgUnitName":
                        return $scope.orgUnits[right.objectId].name;
                    case "roleName":
                        return $scope.orgRoles[right.roleId].Priority;
                    case "userName":
                        return right.user.name;
                    case "userEmail":
                        return right.user.email;
                    default:
                        return $scope.orgUnits[right.objectId].name;
                }
            };

            $scope.rightSortChange = function (val) {
                if ($scope.rightSortBy === val) {
                    $scope.rightSortReverse = !$scope.rightSortReverse;
                } else {
                    $scope.rightSortReverse = false;
                }

                $scope.rightSortBy = val;
            };

            function getSupplementaryTextForEditDialog(unit: Kitos.Models.Api.Organization.IOrganizationUnitDto): string | null {
                if (unit.origin === Kitos.Models.Api.Organization.OrganizationUnitOrigin.STS_Organisation) {
                    return "Enheden synkroniseres fra FK Organisation og nogle felter kan derfor ikke redigeres i KITOS";
                }
                return null;
            }

            $scope.editUnit = function (unit) {
                var modal = $modal.open({
                    templateUrl: "app/components/org/structure/org-structure-modal-edit.view.html",
                    windowClass: "modal fade in wide-modal",
                    controller: [
                        "$scope", "$uibModalInstance", "autofocus", "organizationUnitService", function ($modalScope, $modalInstance, autofocus, organizationUnitService: Kitos.Services.Organization.IOrganizationUnitService) {
                            autofocus();

                            // edit or create-new mode
                            $modalScope.isNew = false;

                            // holds a list of org units, which the user can select as the parent
                            const orgUnits: Kitos.Models.Api.Organization.IOrganizationUnitDto[] = [];

                            // filter out those orgunits, that are outside the organisation
                            // or is currently a subdepartment of the unit
                            function filter(node) {
                                if (node.organizationId !== unit.organizationId) return;

                                // this avoid every subdepartment
                                if (node.id === unit.id) return;

                                orgUnits.push(
                                    {
                                        id: node.id,
                                        uuid: node.uuid,
                                        name: node.name,
                                        ean: node.ean,
                                        localId: node.localId,
                                        parentId: node.parentId,
                                        organizationId: node.organizationId,
                                        externalOriginUuid: node.externalOriginUuid,
                                        origin: node.origin
                                    });

                                _.each(node.children, filter);
                            }

                            _.each($scope.nodes, filter);

                            // format the selected unit for editing
                            $modalScope.orgUnit = {
                                id: unit.id,
                                oldName: unit.name,
                                newName: unit.name,
                                oldEan: unit.ean,
                                newEan: unit.ean,
                                oldLocalId: unit.localId,
                                localId: unit.localId,
                                oldParent: unit.parentId,
                                newParent: unit.parentId,
                                orgId: unit.organizationId,
                                isRoot: unit.parentId == undefined,
                                uuid: unit.uuid,
                                orgUuid: currentOrganization.uuid,
                                isFkOrganizationUnit: unit.origin !== Kitos.Models.Api.Organization.OrganizationUnitOrigin.Kitos
                            } as Kitos.Models.ViewModel.Organization.IEditOrgUnitViewModel;

                            if ($modalScope.orgUnit.isRoot) {
                                orgUnits.push(
                                    {
                                        id: unit.id,
                                        uuid: unit.uuid,
                                        name: unit.name,
                                        ean: unit.ean,
                                        localId: unit.localId,
                                        parentId: unit.parentId,
                                        organizationId: unit.organizationId,
                                        externalOriginUuid: unit.externalOriginUuid,
                                        origin: unit.origin
                                    });
                            }

                            bindParentSelect($modalScope.orgUnit);

                            // only allow changing the parent if user is admin, and the unit isn't at the root
                            $modalScope.isAdmin = user.isGlobalAdmin || user.isLocalAdmin;
                            $modalScope.supplementaryText = getSupplementaryTextForEditDialog(unit);

                            $modalScope.canChangeParent = false;
                            $modalScope.canChangeName = false;
                            $modalScope.canEanBeModified = false;
                            $modalScope.canDeviceIdBeModified = false;
                            $modalScope.canDelete = false;

                            organizationUnitService.getUnitAccessRights(currentOrganization.uuid, unit.uuid)
                                .then(res => {
                                    $modalScope.canDelete = res.canBeDeleted;
                                    $modalScope.canChangeName = res.canNameBeModified;
                                    $modalScope.canEanBeModified = res.canEanBeModified;
                                    $modalScope.canDeviceIdBeModified = res.canDeviceIdBeModified;
                                    $modalScope.canChangeParent = res.canBeRearranged;
                                });

                            $modalScope.patch = function () {
                                // don't allow duplicate submitting
                                if ($modalScope.submitting) return;

                                var name = $modalScope.orgUnit.newName;
                                var parent = $modalScope.orgUnit.newParent;
                                var ean = $modalScope.orgUnit.newEan;
                                var localId = $modalScope.orgUnit.localId;

                                var hasChange = false;
                                if (name !== $modalScope.orgUnit.oldName ||
                                    ean !== $modalScope.orgUnit.oldEan ||
                                    localId !== $modalScope.orgUnit.oldLocalId) {
                                    hasChange = true;
                                }


                                if (!name) return;

                                var data = {
                                    "name": name,
                                    "ean": ean,
                                    "localId": localId
                                };

                                // only allow changing the parent if user is admin, and the unit isn't at the root
                                if ($modalScope.canChangeParent && parent) data["parentId"] = parent;

                                $modalScope.submitting = true;

                                var id = unit.id;

                                $http<Kitos.API.Models.IApiWrapper<any>>({
                                    method: "PATCH",
                                    url: "api/organizationUnit/" + id + "?organizationId=" + user.currentOrganizationId,
                                    data: data
                                }).then((result) => {
                                    notify.addSuccessMessage(name + " er ændret.");


                                    var resultTypes = [];

                                    if (parent !== $modalScope.orgUnit.oldParent) {
                                        resultTypes.push(Kitos.Models.ViewModel.Organization
                                            .OrganizationUnitEditResultType.UnitRelocated);
                                    } else {
                                        if (hasRegistrationChanges) {
                                            resultTypes.push(Kitos.Models.ViewModel.Organization.OrganizationUnitEditResultType.RightsChanged);
                                        }
                                        if (hasChange) {
                                            resultTypes.push(Kitos.Models.ViewModel.Organization.OrganizationUnitEditResultType.FieldsChanged);
                                        }
                                    }

                                    $modalInstance.close(createResult(resultTypes, result.data.response));
                                    inMemoryCacheService.clear();
                                },
                                    (error: ng.IHttpPromiseCallbackArg<Kitos.API.Models.IApiWrapper<any>>) => {
                                        $modalScope.submitting = false;
                                        if (error.data.msg.indexOf("Duplicate entry") > -1) {
                                            notify.addErrorMessage("Fejl! Enhed ID er allerede brugt!");
                                        } else {
                                            notify.addErrorMessage("Fejl! " + name + " kunne ikke ændres!");
                                        }
                                    });
                            };

                            $modalScope.post = function () {
                                // don't allow duplicate submitting
                                if ($modalScope.submitting) return;

                                var name = $modalScope.newOrgUnit.name;
                                if (!name) return;

                                var parent = $modalScope.newOrgUnit.parent;
                                var orgId = $modalScope.newOrgUnit.orgId;
                                var ean = $modalScope.newOrgUnit.ean;
                                var localId = $modalScope.newOrgUnit.localId;

                                var data = {
                                    "name": name,
                                    "parentId": parent,
                                    "organizationId": orgId,
                                    "ean": ean,
                                    "localId": localId,
                                    "origin": Kitos.Models.Api.Organization.OrganizationUnitOrigin.Kitos
                                };

                                $modalScope.submitting = true;

                                $http<Kitos.API.Models.IApiWrapper<any>>({
                                    method: "POST",
                                    url: "api/organizationUnit/",
                                    data: data
                                }).then((result) => {
                                    notify.addSuccessMessage(name + " er gemt.");

                                    $modalInstance.close(createResult([Kitos.Models.ViewModel.Organization.OrganizationUnitEditResultType.SubUnitCreated], result.data.response));
                                    inMemoryCacheService.clear();
                                },
                                    (error: ng.IHttpPromiseCallbackArg<Kitos.API.Models.IApiWrapper<any>>) => {
                                        $modalScope.submitting = false;
                                        if (error.data.msg.indexOf("Duplicate entry") > -1) {
                                            notify.addErrorMessage("Fejl! Enhed ID er allerede brugt!");
                                        } else {
                                            notify.addErrorMessage("Fejl! " + name + " kunne ikke oprettes!");
                                        }
                                    });
                            };

                            $modalScope.new = function () {
                                autofocus();

                                $modalScope.createNew = true;
                                $modalScope.newOrgUnit = {
                                    name: "",
                                    parent: $modalScope.orgUnit.id,
                                    orgId: $modalScope.orgUnit.orgId,
                                    origin: Kitos.Models.Api.Organization.OrganizationUnitOrigin.Kitos
                                };
                            };

                            var hasRegistrationChanges = false;

                            $modalScope.setIsBusy = (value: boolean): void => {
                                $modalScope.submitting = value;
                            }

                            $modalScope.checkIsBusy = (): boolean => {
                                return $modalScope.submitting;
                            }

                            $modalScope.registrationsChanged = () => {
                                hasRegistrationChanges = true;
                            }

                            $modalScope.stateParameters = {
                                setRootIsBusy: (value: boolean) => $modalScope.setIsBusy(value),
                                checkIsRootBusy: () => $modalScope.checkIsBusy(),
                                registrationsChanged: () => $modalScope.registrationsChanged(),
                            } as Kitos.Models.ViewModel.Organization.IRegistrationMigrationStateParameters;

                            $modalScope.delete = function () {
                                //don't allow duplicate submitting
                                if ($modalScope.submitting) return;

                                $modalScope.submitting = true;

                                organizationUnitService.deleteOrganizationUnit(currentOrganization.uuid, unit.uuid)
                                    .then((result) => {
                                        notify.addSuccessMessage(unit.name + " er slettet!");
                                        inMemoryCacheService.clear();
                                        $modalInstance.close(createResult([Kitos.Models.ViewModel.Organization.OrganizationUnitEditResultType.UnitDeleted]));
                                    }, (error) => {
                                        $modalScope.submitting = false;

                                        notify.addErrorMessage(`Fejl! ${unit.name} kunne ikke slettes!<br /><br />
                                                            Organisationsenheden bliver brugt som reference i en eller flere IT Systemer og/eller IT Kontrakter.<br /><br />
                                                            Fjern referencen for at kunne slette denne enhed.`);
                                    });

                            };

                            $modalScope.cancel = function () {
                                if (hasRegistrationChanges) {
                                    $modalInstance.close(createResult([Kitos.Models.ViewModel.Organization.OrganizationUnitEditResultType.RightsChanged], unit));
                                    return;
                                }
                                $modalInstance.close(createResult());
                            };

                            function bindParentSelect(currentUnit: Kitos.Models.ViewModel.Organization.IEditOrgUnitViewModel) {

                                const root = $scope.nodes[0];
                                const idToSkip = root.id === currentUnit.id ? null : currentUnit.id;
                                const orgUnitsOptions = Kitos.Helpers.Select2OptionsFormatHelper.addIndentationToUnitChildren(root, 0, idToSkip);

                                let existingChoice: { id: string; text: string };
                                if (currentUnit.isRoot) {
                                    const rootNodes = orgUnitsOptions.filter(x => x.id === String(currentUnit.id));
                                    if (rootNodes.length < 1)
                                        return;
                                    const rootUnit = rootNodes[0];
                                    existingChoice = rootUnit;
                                } else {
                                    const parentNodes = orgUnitsOptions.filter(x => x.id === String(currentUnit.newParent));
                                    if (parentNodes.length < 1) {
                                        return;
                                    }
                                    const parentNode = parentNodes[0];
                                    existingChoice = parentNode;
                                }

                                $modalScope.parentSelect = {
                                    selectedElement: existingChoice,
                                    select2Config: select2LoadingService.select2LocalDataFormatted(() => orgUnitsOptions, Kitos.Helpers.Select2OptionsFormatHelper.formatIndentation),
                                    elementSelected: (newElement) => {
                                        if (!!newElement) {
                                            $modalScope.orgUnit.newParent = newElement.id;
                                        }
                                    }
                                }
                            }

                            function createResult(types: Kitos.Models.ViewModel.Organization.OrganizationUnitEditResultType[] = null, unit = null): Kitos.Models.ViewModel.Organization.IOrganizationUnitEditResult {
                                let resultTypes = types;
                                if (!resultTypes) {
                                    resultTypes = [];
                                }
                                return {
                                    types: resultTypes,
                                    scopeToUnit: unit
                                };
                            }
                        }
                    ]
                });

                modal.result.then((result: Kitos.Models.ViewModel.Organization.IOrganizationUnitEditResult) => {
                    result.types.forEach(type => {
                        switch (type) {
                            case Kitos.Models.ViewModel.Organization.OrganizationUnitEditResultType.SubUnitCreated:
                            case Kitos.Models.ViewModel.Organization.OrganizationUnitEditResultType.UnitRelocated:
                            case Kitos.Models.ViewModel.Organization.OrganizationUnitEditResultType.UnitDeleted:
                                $state.go($state.current, {}, { reload: true });
                                break;
                            case Kitos.Models.ViewModel.Organization.OrganizationUnitEditResultType.FieldsChanged:
                                const currentNode = $scope.orgUnits[result.scopeToUnit.id];
                                if (!currentNode) {
                                    return;
                                }

                                currentNode.name = result.scopeToUnit.name;
                                currentNode.ean = result.scopeToUnit.ean;
                                currentNode.localId = result.scopeToUnit.localId;
                                break;
                            case Kitos.Models.ViewModel.Organization.OrganizationUnitEditResultType.RightsChanged:
                                const node = $scope.orgUnits[result.scopeToUnit.id];
                                if (!node) {
                                    return;
                                }

                                loadRights(node);
                                break;
                            default:
                                break;
                        }
                    });
                });
            };

            $scope.$watchCollection("rightsPagination", function () {
                loadRights($scope.chosenOrgUnit);
            });

            $scope.isReordering = false;
            $scope.loadingAccessRights = false;

            $scope.toggleDrag = function () {
                $scope.isReordering = !$scope.isReordering;

                if ($scope.isReordering) {
                    $scope.loadingAccessRights = true;
                    organizationUnitService.getUnitAccessRightsForOrganization(currentOrganization.uuid)
                        .then(response => {
                            const rightsMap = response.reduce((rights, next) => {
                                rights[next.unitId] = next;
                                return rights;
                            }, {})
                            applyAccessRights(rootNodeOfOrganization, rightsMap);
                            $scope.loadingAccessRights = false;
                        }, error => {
                            notify.addErrorMessage("Kunne ikke indlæse rettighederne for organisationsenheden");
                            console.log(error);
                            $scope.loadingAccessRights = false;
                            $scope.isReordering = false;
                        });
                }
            };

            function applyAccessRights(unit: Kitos.Models.ViewModel.Organization.IOrganizationUnitReorderViewModel,
                accessRights: { [key: number]: Kitos.Models.Api.Organization.UnitAccessRightsWithUnitIdDto }) {
                unit.draggable = accessRights[unit.id]?.canBeRearranged === true;
                unit.children.forEach(child => applyAccessRights(child, accessRights));
            }

            $scope.treeOptions = {
                accept: function (sourceNodeScope, destNodesScope, destIndex) {
                    return !angular.isUndefined(destNodesScope.$parentNodesScope);

                },
                dropped: function (e) {
                    var parent = e.dest.nodesScope.$nodeScope;

                    var sourceId = e.source.nodeScope.$modelValue.id;

                    var currentParentId = e.source.nodeScope.$parentNodeScope.$modelValue.id;

                    if (parent && currentParentId != parent.$modelValue.id) {
                        var parentId = parent.$modelValue.id;
                        var sourceId = e.source.nodeScope.$modelValue.id;

                        //SEND API PATCH CALL

                        var payload = {
                            parentId: parentId
                        };

                        var url = 'api/organizationunit/' + sourceId;
                        var msg = notify.addInfoMessage("Opdaterer...", false);
                        $http<Kitos.API.Models.IApiWrapper<any>>({ method: 'PATCH', url: url + '?organizationId=' + user.currentOrganizationId, data: payload }).then(() => {
                            msg.toSuccessMessage("Enheden er opdateret");
                            $scope.chooseOrgUnit(rootNodeOfOrganization);
                        }, (error) => {
                            msg.toErrorMessage("Fejl!");
                        });
                    }
                }
            };

            $scope.checkIfRoleIsAvailable = function (roleId) {
                var foundSelectedInOptions = _.find($scope.activeOrgRoles, function (option: any) { return option.Id === parseInt(roleId, 10) });
                return (foundSelectedInOptions);
            }

            $scope.getRoleName = function (roleId) {
                var role = _.find($scope.orgUnitRoles, function (role: any) { return role.Id === parseInt(roleId, 10) });
                return role.Name;
            }

            // activate
            loadUnits();
        }
    ]);
})(angular, app);
