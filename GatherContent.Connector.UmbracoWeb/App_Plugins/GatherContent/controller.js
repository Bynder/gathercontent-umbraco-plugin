angular.module('umbraco')
    .controller('settings.controller', [
        'gatherResource',
        'accountResource',
        'notificationsService',
        '$timeout',
        function (gatherResource, accountResource, notificationsService, $timeout) {
            this.saveButtonBusy = false;
            //this.saveButtonSuccess = false;

            var self = this;

            var _init = function () {
                accountResource.getAll().then(function (settings) {
                    if (!!settings.data)
                        self.settings = settings.data;
                }, function (error) {
                    console.error(error);
                });
            }

            var errorCallback = function (error) {
                console.error(error);
                notificationsService.error('Error', error.data.Message);
                self.saveButtonBusy = false;
            }

            this.saveSettings = function (settings) {
                self.saveButtonBusy = true;
                accountResource.save(settings).then(function (response) {
                    _init();
                    notificationsService.success('Success', 'Your credentials are saved.');
                    self.saveButtonBusy = false;
                    accountResource.testSetting().then(function (testData) {
                        if (!JSON.parse(testData.data))
                            notificationsService.error('Error', 'You have wrong credentials.');
                    }, errorCallback);
                }, errorCallback);
            }

            _init();
        }
    ])
    .controller('mappings.controller', [
        'gatherResource',
        'mappingResource',
        'navigationService',
        'notificationsService',
        function (gatherResource, mappingResource, navigationService, notificationsService) {
            this.mappings = [];
            this.errorText = '';
            this.busy = false;

            this.editMapping = function (map) {
            }

            this.selectTableHeader = function (order, field) {
                if (order === field || order.substring(1, order.length) === field) {
                    if (order[0] == '-') {
                        order = order.substring(1, order.length);
                    } else {
                        order = '-' + order;
                    }
                } else {
                    order = field;
                }
                return order;
            }

            var self = this;


            var _init = function () {
                self.busy = true;
                mappingResource.getAll().then(function (response) {
                    self.mappings = response.data;
                    //self.isError = false;
                    self.busy = false;
                }, function (error) {
                    self.errorText = "Error:" + " " + error.data.Message;
                    //self.isError = true;
                    notificationsService.error("Error", error.data.Message);
                    self.busy = false;
                });
            }

            self.deleteMapping = function (map) {
                if (confirm('Do you want to delete "' + map.MappingTitle + '" mapping?')) {
                    self.busy = true;
                    mappingResource.deleteMap(map.MappingId).then(function (response) {
                        _init();
                        self.busy = false;
                        notificationsService.success('"' + map.MappingTitle + '" has been deleted');
                    }, function (error) {
                        console.error(error);
                        self.busy = false;
                    });
                }
            }

            _init();
        }
    ])
    .controller('mapping.add.controller', [
        'gatherResource',
        'mappingResource',
        'navigationService',
        'notificationsService',
        'dialogService',
        '$scope',
        '$location',
        function (gatherResource, mappingResource, navigationService, notificationsService, dialogService, $scope, $location) {
            this.projects = [];
            this.templates = [];
            this.umbTemplates = [];
            this.tabs = [];
            this.currentProps = {};
            this.currentUmbTemplate = {};
            this.saveButtonBusy = false;
            this.busy = false;

            this.model = {
                MappingTitle: '',
                GcTemplate: { Id: '', Name: '' },
                CmsTemplate: { Id: '', Name: '' },
                DefaultLocationId: '',
                FieldMappings: []
            };


            var self = this;

            var _getTemplates = function (projectId) {
                self.busy = true;
                mappingResource.getTemplates(projectId).then(function (templates) {
                    self.templates = templates.data;
                    self.busy = false;
                }, function (error) {
                    notificationsService.error(error.data.Message);
                    self.busy = false;
                    console.error(error);
                });
            }

            var _init = function () {
                self.busy = true;
                mappingResource.getAllProjects().then(function (projects) {
                    self.projects = projects.data;
                    mappingResource.getUmbTemplates().then(function (templates) {
                        self.umbTemplates = templates.data;
                        self.busy = false;
                    }, function (error) {
                        notificationsService.error(error.data.Message);
                        console.error(error);
                        self.busy = false;
                    });
                }, function (error) {
                    notificationsService.error(error.data.Message);
                    console.error(error);
                    self.busy = false;
                });

            }

            this.openOrShow = function (event, tab) {
                var div = $(event.target).parent()[0].localName == 'div' ? $(event.target).parent()
                    : $(event.target).parent().parent()[0].localName == 'div'
                    ? $(event.target).parent().parent() : null;
                if (!!div) {
                    var element = div.find('ul');
                    if (element.css('display') == 'none') {
                        element.show();
                        tab.isOpenned = true;
                    } else {
                        element.hide();
                        tab.isOpenned = false;
                    }
                }
            }

            this.selectTemplate = function () {
                if (!!self.model.GcTemplate.Id) {
                    self.busy = true;
                    mappingResource.getFieldsByTemplate(self.model.GcTemplate.Id).then(function (fields) {
                        self.currentProps = {};
                        self.tabs = fields.data;
                        self.busy = false;
                    }, function (error) {
                        notificationsService.error(error.data.Message);
                        console.error(error);
                        self.busy = false;
                    });
                } else {
                    self.currentProps = {};
                    self.tabs = {};
                }
            }

            this.selectUmbTemplate = function () {
                angular.forEach(self.umbTemplates, function (item) {
                    if (item.Id == self.model.CmsTemplate.Id) {
                        self.currentUmbTemplate = item;
                    }
                });
            }

            this.selectProject = function () {
                if (!!self.model.GcProject.Id) {
                    _getTemplates(self.model.GcProject.Id);
                }
                self.templates = [];
                self.tabs = [];
                self.currentProps = {};
                self.model.currentTemplate = '';
            }

            this.openTreeDialog = function () {
                dialogService.contentPicker({
                    callback: function (elem) {
                        self.model.DefaultLocationId = elem.id;
                        self.model.DefaultLocationName = elem.name;
                    }
                });
            }

            this.submitForm = function () {
                self.saveButtonBusy = true;
                angular.forEach(self.currentProps, function (item, index) {
                    var gcName = '';
                    angular.forEach(self.tabs, function (tab) {
                        angular.forEach(tab.Fields, function (field) {
                            if (field.Id == index) {
                                gcName = field.Name;
                            }
                        });
                    });

                    var field = {
                        GcFieldId: index,
                        CmsTemplateId: item.Id,
                        GcFieldName: gcName
                    }
                    self.model.FieldMappings.push(field);
                });
                mappingResource.saveMapping(self.model).then(function () {
                    self.saveButtonBusy = false;
                    notificationsService.success('Mapping "' + self.model.MappingTitle + '" has been added');
                    $location.url('/GatherContent/GatherContentTree/mappings/index');
                }, function (error) {
                    self.saveButtonBusy = false;
                    notificationsService.error(error.data.Message);
                    console.error(error);
                });
            }

            _init();
        }
    ])
    .controller('mapping.edit.controller', [
        'gatherResource',
        'mappingResource',
        'navigationService',
        'notificationsService',
        'dialogService',
        'contentResource',
        '$scope',
        '$location',
        '$routeParams',
        function (gatherResource, mappingResource, navigationService, notificationsService,
            dialogService, contentResource, $scope, $location, $routeParams) {
            this.projects = [];
            this.templates = [];
            this.umbTemplates = [];
            this.tabs = [];
            this.currentProps = {};
            this.currentUmbTemplate = {};
            this.saveButtonBusy = false;
            this.busy = false;

            this.model = {
                MappingTitle: '',
                GcTemplate: { Id: '', Name: '' },
                CmsTemplate: { Id: '', Name: '' },
                DefaultLocationId: '',
                FieldMappings: []
            };

            var self = this;

            this.openOrShow = function (event, tab) {
                var div = $(event.target).parent()[0].localName == 'div' ? $(event.target).parent()
                    : $(event.target).parent().parent()[0].localName == 'div'
                    ? $(event.target).parent().parent() : null;
                if (!!div) {
                    var element = div.find('ul');
                    if (element.css('display') == 'none') {
                        element.show();
                        tab.isOpenned = true;
                    } else {
                        element.hide();
                        tab.isOpenned = false;
                    }
                }
            }

            var _getTemplates = function (projectId) {
                self.busy = true;
                mappingResource.getTemplates(projectId).then(function (templates) {
                    self.templates = templates.data;
                    self.busy = false;
                }, function (error) {
                    notificationsService.error(error.data.Message);
                    console.error(error);
                    self.busy = false;
                });
            }

            var _getSelectedProps = function () {
                self.busy = true;
                mappingResource.getFieldsByTemplate(self.model.GcTemplate.Id).then(function (fields) {
                    self.currentProps = {};
                    self.tabs = fields.data;
                    angular.forEach(self.model.FieldMappings, function (field) {
                        angular.forEach(self.currentUmbTemplate.Fields, function (item) {
                            if (item.Id == field.CmsTemplateId)
                                self.currentProps[field.GcFieldId] = item;
                        });
                    });
                    self.busy = false;
                }, function (error) {
                    notificationsService.error(error.data.Message);
                    console.error(error);
                    self.busy = false;
                });
            }

            var errorCallback = function (error) {
                notificationsService.error(error.data.Message);
                console.error(error);
                self.busy = false;
            }

            var _init = function () {
                self.busy = true;
                mappingResource.getAllProjects().then(function (projects) {
                    self.projects = projects.data;

                    mappingResource.getUmbTemplates().then(function (templates) {
                        self.umbTemplates = templates.data;

                        if (!!$routeParams.id && $routeParams.id != '') {
                            var gcId;
                            var cmsId;
                            if (!!$routeParams.gcId) {
                                gcId = $routeParams.gcId;
                                cmsId = $routeParams.id;
                            } else {
                                gcId = $routeParams.id.split('?')[1].split('=')[1];
                                cmsId = $routeParams.id.split('?')[0];
                            }
                            mappingResource.getSingle(gcId, cmsId)
                                .then(function (response) {
                                    self.model = response.data;
                                    self.selectProject();
                                    _getSelectedProps();
                                    self.selectUmbTemplate();
                                    if (!!self.model.DefaultLocationId)
                                        contentResource.getById(self.model.DefaultLocationId).then(function (node) {
                                            self.model.DefaultLocationName = node.name;
                                            self.busy = false;
                                        }, errorCallback);
                                }, errorCallback);
                        }
                    }, errorCallback);
                }, errorCallback);
            }

            this.selectTemplate = function () {
                if (!!self.model.GcTemplate.Id) {
                    self.busy = true;
                    mappingResource.getFieldsByTemplate(self.model.GcTemplate.Id).then(function (fields) {
                        self.currentProps = {};
                        self.tabs = fields.data;
                        self.busy = false;
                    }, function (error) {
                        notificationsService.error(error.data.Message);
                        console.error(error);
                        self.busy = false;
                    });
                } else {
                    self.currentProps = {};
                    self.tabs = {};
                }
            }

            this.selectUmbTemplate = function () {
                angular.forEach(self.umbTemplates, function (item) {
                    if (item.Id == self.model.CmsTemplate.Id) {
                        self.currentUmbTemplate = item;
                    }
                });
            }

            this.selectProject = function () {
                self.busy = true;
                if (!!self.model.GcProject.Id) {
                    _getTemplates(self.model.GcProject.Id);
                }
                self.templates = [];
                self.tabs = [];
                self.currentProps = {};
            }

            this.openTreeDialog = function () {
                dialogService.contentPicker({
                    callback: function (elem) {
                        self.model.DefaultLocationId = elem.id;
                        self.model.DefaultLocationName = elem.name;
                    }
                });
            }

            this.submitForm = function () {
                self.saveButtonBusy = true;
                self.model.FieldMappings = [];
                angular.forEach(self.currentProps, function (item, index) {
                    if (!item)
                        return;
                    var gcName = '';
                    angular.forEach(self.tabs, function (tab) {
                        angular.forEach(tab.Fields, function (field) {
                            if (field.Id == index) {
                                gcName = field.Name;
                            }
                        });
                    });

                    var field = {
                        GcFieldId: index,
                        CmsTemplateId: item.Id,
                        GcFieldName: gcName
                    }
                    self.model.FieldMappings.push(field);
                });
                mappingResource.saveMapping(self.model).then(function () {
                    self.saveButtonBusy = false;
                    notificationsService.success('Mapping "' + self.model.MappingTitle + '" has been saved');
                    $location.url('/GatherContent/GatherContentTree/mappings/index');
                }, function (error) {
                    self.saveButtonBusy = false;
                    notificationsService.error(error.data.Message);
                    console.error(error);
                });
            }

            _init();
        }
    ])
    .controller('import.controller', [
        '$scope',
        '$location',
        '$q',
        '$cookieStore',
        'mappingResource',
        'importResource',
        'notificationsService',
        function ($scope, $location, $q, $cookieStore, mappingResource, importResource, notificationsService) {
            var self = this;

            this.busy = false;
            this.allSelected = false;
            this.statuses = [];
            this.templates = [];
            this.projects = [];
            this.items = [];
            this.filteredItems = [];
            this.model = {
                GcTemplateId: '',
                GcProjectId: '',
                Status: ''
            }

            if (window.localStorage.getItem('gathercontent.history.back')) {
                if (window.localStorage.hasOwnProperty('GcTemplateId')) {
                    this.model.GcTemplateId = window.localStorage.getItem('GcTemplateId');
                }
                if (window.localStorage.hasOwnProperty('GcTemplates'))
                    this.templates = JSON.parse(window.localStorage.getItem('GcTemplates'));

                if (window.localStorage.hasOwnProperty('GcProjectId'))
                    this.model.GcProjectId = window.localStorage.getItem('GcProjectId');

                if (window.localStorage.hasOwnProperty('Status')) {
                    this.model.Status = window.localStorage.getItem('Status');
                }
                if (window.localStorage.hasOwnProperty('statuses')) {
                    this.statuses = JSON.parse(window.localStorage.getItem('statuses'));
                }
                if (window.localStorage.hasOwnProperty('Items'))
                    this.items = JSON.parse(window.localStorage.getItem('Items'));
            } else {
                window.localStorage.removeItem('GcTemplateId');
                window.localStorage.removeItem('GcProjectId');
                window.localStorage.removeItem('Status');
                window.localStorage.removeItem('Items');
            }

            window.localStorage.removeItem('gathercontent.history.back');

            var _init = function () {
                self.busy = true;
                importResource.getProjectsWithMapp().then(function (projects) {
                    self.projects = projects.data;
                }, function (error) {
                    notificationsService.error(error.data.Message);
                }).always(function () {
                    self.busy = false;
                });
            }
            var _getItems = function () {
                self.busy = true;
                $q.all([
                    importResource.get(self.model.GcProjectId, self.model.GcTemplateId).then(function (items) {
                        self.items = items.data;

                    }, function (error) {
                        notificationsService.error(error.data.Message);
                    }),
                    importResource.getStatuses(self.model.GcProjectId).then(function (fileters) {
                        self.statuses = fileters.data.Statuses;
                        self.templates = fileters.data.Templates;
                    }, function (error) {
                        notificationsService.error(error.data.Message);
                    })
                ]).always(function (parameters) {
                    self.busy = false;
                });
            }

            this.selectTableHeader = function (order, field) {
                if (order === field || order.substring(1, order.length) === field) {
                    if (order[0] == '-') {
                        order = order.substring(1, order.length);
                    } else {
                        order = '-' + order;
                    }
                } else {
                    order = field;
                }
                return order;
            }

            this.selectProject = function () {
                if (!!self.model.GcProjectId) {
                    //$q.all([
                    //    mappingResource.getTemplates(self.model.GcProjectId).then(function (templates) {
                    //        self.templates = templates.data;
                    //    })
                    //]).then(function () {
                    _getItems();
                    //}, function (error) {
                    //    notificationsService.error(error.data.Message);
                    //    self.busy = false;
                    //});
                } else {
                    self.items = [];
                }
            }

            this.checkItem = function () {
                var isExistNotSelected = false;
                angular.forEach(self.filteredItems, function (item) {
                    if (!item.Checked) {
                        isExistNotSelected = true;
                    }
                });
                if (isExistNotSelected) {
                    self.allSelected = false;
                } else {
                    self.allSelected = true;
                }
            }
            this.next = function () {
                var items = [];
                angular.forEach(self.filteredItems, function (item) {
                    if (item.Checked)
                        items.push(item);
                });
                if (items.length > 0) {
                    window.localStorage.setItem('ChekedItems', JSON.stringify(items));
                    window.localStorage.setItem('statuses', JSON.stringify(self.statuses));
                    window.localStorage.setItem('GcTemplates', JSON.stringify(self.templates));
                    if (!!self.model.GcProjectId)
                        window.localStorage.setItem('GcProjectId', self.model.GcProjectId);
                    if (!!self.model.GcTemplateId)
                        window.localStorage.setItem('GcTemplateId', self.model.GcTemplateId);
                    if (!!self.model.Status)
                        window.localStorage.setItem('Status', self.model.Status);
                    if (!!self.items)
                        window.localStorage.setItem('Items', JSON.stringify(self.items));
                    $location.url('/GatherContent/GatherContentTree/import.confirm/index');
                } else {
                    notificationsService.warning('No selected items.');
                }
            }

            $scope.$watch('import.allSelected', function (n, o) {

                if (typeof event != 'undefined') {

                    if (event.target.id == 'allSelected') {
                        angular.forEach(self.filteredItems, function (item) {
                            item.Checked = n;
                        });
                    }

                } else {

                    // FF hotfix
                    angular.forEach(self.filteredItems, function (item) {
                        item.Checked = n;
                    });
                }

            });

            _init();
        }
    ])
    .controller('import.confirm.controller', [
        '$scope',
        '$timeout',
        '$location',
        'importResource',
        'notificationsService',
        'dialogService',
        function ($scope, $timeout, $location, importResource, notificationsService, dialogService) {
            this.saveButtonBusy = false;
            this.items = JSON.parse(window.localStorage.getItem('ChekedItems'));
            this.GcProjectId = window.localStorage.getItem('GcProjectId');
            this.statuses = JSON.parse(window.localStorage.getItem('statuses'));;
            this.currentStatus = '';
            this.isChangeStatus = false;
            this.model = {
                TargetNodeId: null,
                TargetNodeName: ''
            }
            this.changeAllMappings = true;
            this.busy = false;
            var self = this;

            var importSuccess = function (success) {
                window.resultItem = success.data;
                var successItems = [];
                var errorItems = [];
                angular.forEach(window.resultItem, function (item) {
                    if (item.IsImportSuccessful)
                        successItems.push(item);
                    else
                        errorItems.push(item);
                });
                if (successItems.length > 0)
                    notificationsService.success(successItems.length + ' items were imported successfully.');
                if (errorItems.length > 0)
                    notificationsService.error(errorItems.length + ' items were not imported. Check errors below.');
                $location.url('/GatherContent/GatherContentTree/import.result/index');
            }

            var importError = function (error) {
                notificationsService.error(error.data.Message);
            }

            var stopBusy = function () {
                //self.saveButtonBusy = false;
                self.busy = false;
            }

            var checkForm = function () {
                if (!self.model.TargetNodeId) {
                    notificationsService.warning('Please select "Target Node".');
                    return false;
                }
                if (!self.items || self.items.length === 0) {
                    notificationsService.warning("Items for import don't selected.");
                    return false;
                }
                if (!self.GcProjectId) {
                    notificationsService.warning("Items for import don't selected.");
                    return false;
                }

                return true;
            }

            this.back = function () {
                window.history.back();
            }

            this.selectTargetNode = function () {
                dialogService.contentPicker({
                    callback: function (node) {
                        self.model.TargetNodeId = node.id;
                        self.model.TargetNodeName = node.name;
                    }
                });
            }

            this.selectMapping = function (item) {
                var counterOfElements = 0;
                if (self.changeAllMappings) {
                    angular.forEach(self.items, function (element) {
                        if (element.GcTemplate.Id == item.GcTemplate.Id) counterOfElements++;
                    });
                    if (counterOfElements > 1 && confirm('Do you want to change mappings for all items?')) {
                        angular.forEach(self.items, function (x) {
                            if (x.GcTemplate.Id == item.GcTemplate.Id) {
                                x.AvailableMappings.SelectedMappingId = item.AvailableMappings.SelectedMappingId;
                            }
                        });
                    } else {
                        self.changeAllMappings = false;
                    }
                }
            }

            this.import = function () {
                if (!checkForm())
                    return;

                //self.saveButtonBusy = true;
                self.busy = true;
                var confirmItems = [];
                angular.forEach(self.items, function (item) {
                    confirmItems.push({
                        Id: item.GcItem.Id,
                        DefaultLocation: self.model.TargetNodeId,
                        SelectedMappingId: item.AvailableMappings.SelectedMappingId
                    });
                });
                importResource.importItems(confirmItems, self.GcProjectId, self.isChangeStatus ?
                    (!!self.currentStatus ? self.currentStatus : "") : "", self.model.TargetNodeId)
                    .then(importSuccess, importError).always(stopBusy);
            }

            $scope.$on('$locationChangeSuccess', function () {
                window.localStorage.setItem('gathercontent.history.back', true);
            });
        }
    ])
    .controller('import.result.controller', [
        '$scope',
        function ($scope) {
            this.items = window.resultItem;

            var self = this;
        }
    ])
    .controller('update.controller', [
        '$scope',
        '$location',
        'notificationsService',
        'dialogService',
        'updateResource',
        function ($scope, $location, notificationsService, dialogService, updateResource) {
            var self = this;
            this.model = {
                TargetNodeId: null,
                TargetNodeName: null,
                GcTemplateId: '',
                Status: ''
            }
            this.items = [];
            this.statuses = [];
            this.templates = [];
            this.busy = false;
            this.busyUpdate = false;
            this.allSelected = false;

            var errorCallback = function (error) {
                notificationsService.error(error.data.Message);
            }

            this.selectTargetNode = function () {
                dialogService.contentPicker({
                    callback: function (node) {
                        self.model.TargetNodeId = node.id;
                        self.model.TargetNodeName = node.name;
                        if (!!self.model.TargetNodeId) {
                            self.busy = true;
                            updateResource.getItems(self.model.TargetNodeId).then(function (response) {
                                self.items = response.data.Items;
                                self.statuses = response.data.Filters.Statuses;
                                self.templates = response.data.Filters.Templates;
                            }, errorCallback)
                                .always(function () {
                                    self.busy = false;
                                });
                        }
                    }
                });
            }

            this.selectTableHeader = function (order, field) {
                if (order === field || order.substring(1, order.length) === field) {
                    if (order[0] == '-') {
                        order = order.substring(1, order.length);
                    } else {
                        order = '-' + order;
                    }
                } else {
                    order = field;
                }
                return order;
            }

            this.checkItem = function () {
                var isExistNotSelected = false;
                angular.forEach(self.filteredItems, function (item) {
                    if (!item.Checked) {
                        isExistNotSelected = true;
                    }
                });
                if (isExistNotSelected) {
                    self.allSelected = false;
                } else {
                    self.allSelected = true;
                }
            }

            this.next = function () {
                self.busyUpdate = true;
                var resultItems = [];
                angular.forEach(self.items, function (item) {
                    if (item.Checked)
                        resultItems.push({
                            GCId: item.GcItem.Id,
                            CMSId: item.CmsId
                        });
                });
                if (resultItems.length > 0)
                    updateResource.updateItems(self.model.TargetNodeId, resultItems)
                        .then(
                            function (result) {
                                window.updatedItems = result.data;
                                var successItems = [];
                                var errorItems = [];
                                angular.forEach(window.updatedItems, function (item) {
                                    if (item.IsImportSuccessful)
                                        successItems.push(item);
                                    else
                                        errorItems.push(item);
                                });
                                if (successItems.length > 0)
                                    notificationsService.success(successItems.length + ' items were updated successfully.');
                                if (errorItems.length > 0)
                                    notificationsService.error(errorItems.length + ' items were not updated. Check errors below.');
                                $location.url('/GatherContent/GatherContentTree/update.result/index');
                            }, errorCallback)
                        .always(function () {
                            self.busyUpdate = false;
                        });
                else {
                    notificationsService.warning('No selected items.');
                    self.busyUpdate = false;
                }
            }

            $scope.$watch('update.allSelected', function (n, o) {

                    if (typeof event != 'undefined') {

                        if (event.target.id == 'allSelected') {
                            angular.forEach(self.filteredItems, function (item) {
                                item.Checked = n;
                            });
                        }

                    } else {

                        // FF hotfix
                        angular.forEach(self.filteredItems, function (item) {
                            item.Checked = n;
                        });
                    }
            });
        }
    ])
    .controller('update.result.controller', [
        '$scope',
        function ($scope) {
            this.items = window.updatedItems;
        }
    ])
    .controller('import.locations.controller', [
        '$scope',
        '$location',
        '$q',
        '$cookieStore',
        'mappingResource',
        'importResource',
        'notificationsService',
        function ($scope, $location, $q, $cookieStore, mappingResource, importResource, notificationsService) {
            var self = this;

            this.busy = false;
            this.allSelected = false;
            this.statuses = [];
            this.templates = [];
            this.projects = [];
            this.items = [];
            this.filteredItems = [];
            this.model = {
                GcTemplateId: '',
                GcProjectId: '',
                Status: ''
            }

            if (window.localStorage.getItem('gathercontent.history.back')) {
                if (window.localStorage.hasOwnProperty('GcTemplateId')) {
                    this.model.GcTemplateId = window.localStorage.getItem('GcTemplateId');
                }
                if (window.localStorage.hasOwnProperty('GcTemplates'))
                    this.templates = JSON.parse(window.localStorage.getItem('GcTemplates'));

                if (window.localStorage.hasOwnProperty('GcProjectId'))
                    this.model.GcProjectId = window.localStorage.getItem('GcProjectId');

                if (window.localStorage.hasOwnProperty('Status')) {
                    this.model.Status = window.localStorage.getItem('Status');
                }
                if (window.localStorage.hasOwnProperty('statuses')) {
                    this.statuses = JSON.parse(window.localStorage.getItem('statuses'));
                }
                if (window.localStorage.hasOwnProperty('Items'))
                    this.items = JSON.parse(window.localStorage.getItem('Items'));
            } else {
                window.localStorage.removeItem('GcTemplateId');
                window.localStorage.removeItem('GcProjectId');
                window.localStorage.removeItem('GcTemplates');
                window.localStorage.removeItem('Status');
                window.localStorage.removeItem('statuses');
                window.localStorage.removeItem('Items');
            }

            window.localStorage.removeItem('gathercontent.history.back');

            var _init = function () {
                self.busy = true;
                importResource.getProjectsWithMapp().then(function (projects) {
                    self.projects = projects.data;
                }, function (error) {
                    notificationsService.error(error.data.Message);
                }).always(function () {
                    self.busy = false;
                });
            }
            var _getItems = function () {
                self.busy = true;
                $q.all([
                    importResource.get(self.model.GcProjectId, self.model.GcTemplateId).then(function (items) {
                        self.items = items.data;

                    }, function (error) {
                        notificationsService.error(error.data.Message);
                    }),
                    importResource.getStatuses(self.model.GcProjectId).then(function (fileters) {
                        self.statuses = fileters.data.Statuses;
                        self.templates = fileters.data.Templates;
                    }, function (error) {
                        notificationsService.error(error.data.Message);
                    })
                ]).always(function (parameters) {
                    self.busy = false;
                });
            }

            this.selectTableHeader = function (order, field) {
                if (order === field || order.substring(1, order.length) === field) {
                    if (order[0] == '-') {
                        order = order.substring(1, order.length);
                    } else {
                        order = '-' + order;
                    }
                } else {
                    order = field;
                }
                return order;
            }

            this.selectProject = function () {
                if (!!self.model.GcProjectId) {
                    //$q.all([
                    //    mappingResource.getTemplates(self.model.GcProjectId).then(function (templates) {
                    //        self.templates = templates.data;
                    //    })
                    //]).then(function () {
                    _getItems();
                    //}, function (error) {
                    //    notificationsService.error(error.data.Message);
                    //    self.busy = false;
                    //});
                } else {
                    self.items = [];
                }
            }

            this.checkItem = function () {
                var isExistNotSelected = false;
                angular.forEach(self.filteredItems, function (item) {
                    if (!item.Checked) {
                        isExistNotSelected = true;
                    }
                });
                if (isExistNotSelected) {
                    self.allSelected = false;
                } else {
                    self.allSelected = true;
                }
            }
            this.next = function () {
                var items = [];
                angular.forEach(self.filteredItems, function (item) {
                    if (item.Checked)
                        items.push(item);
                });
                if (items.length > 0) {
                    window.localStorage.setItem('gcItems', JSON.stringify(items));
                    window.localStorage.setItem('statuses', JSON.stringify(self.statuses));
                    window.localStorage.setItem('gcProjectId', self.model.GcProjectId);
                    window.localStorage.setItem('GcTemplates', JSON.stringify(self.templates));
                    if (!!self.model.GcProjectId)
                        window.localStorage.setItem('GcProjectId', self.model.GcProjectId);
                    if (!!self.model.GcTemplateId)
                        window.localStorage.setItem('GcTemplateId', self.model.GcTemplateId);
                    if (!!self.model.Status)
                        window.localStorage.setItem('Status', self.model.Status);
                    if (!!self.items)
                        window.localStorage.setItem('Items', JSON.stringify(self.items));

                    $location.url('/GatherContent/GatherContentTree/import.locations.confirm/index');
                } else {
                    notificationsService.warning('No selected items.');
                }
            }

            $scope.$watch('import.allSelected', function (n, o) {

                if (typeof event != 'undefined') {

                    if (event.target.id == 'allSelected') {
                        angular.forEach(self.filteredItems, function (item) {
                            item.Checked = n;
                        });
                    }

                } else {

                    // FF hotfix
                    angular.forEach(self.filteredItems, function (item) {
                        item.Checked = n;
                    });
                }

            });

            _init();
        }
    ])
    .controller('import.locations.confirm.controller', [
        '$scope',
        '$timeout',
        '$location',
        '$filter',
        'importResource',
        'notificationsService',
        'dialogService',
        function ($scope, $timeout, $location, $filter, importResource, notificationsService, dialogService) {
            var sortItemsByTemplateName = function () {
                var items = JSON.parse(window.localStorage.getItem('gcItems'));
                var result = [];

                angular.forEach(items, function (item) {
                    angular.forEach(item.AvailableMappings.Mappings, function (mapping) {

                        var found = false;
                        for (var i = 0; i < result.length; i++) {
                            if (result[i].MappingName == mapping.Title) {
                                found = true;
                                break;
                            }
                        }
                        if (!found) {
                            result.push({
                                Id: mapping.Id,
                                TemplateName: item.GcTemplate.Name,
                                ItemName: item.GcItem.Title,
                                MappingName: mapping.Title,
                                CmsTemplateName: mapping.CmsTemplateName,
                                DefaultLocationId: mapping.DefaultLocationId,
                                DefaultLocationTitle: mapping.DefaultLocationTitle
                            });
                        }

                    });
                });
                result = $filter('orderBy')(result, 'TemplateName', false);
                for (var index = 0; index < result.length; index++) {
                    var rowspan = 1;
                    for (var i = index + 1; i < result.length; i++) {
                        if (result[index].TemplateName === result[i].TemplateName) {
                            rowspan++;
                        }
                    }
                    result[index].rowspan = rowspan;
                    index += (rowspan - 1);
                }
                return result;
            }

            this.allSelected = false;
            this.saveButtonBusy = false;
            this.items = sortItemsByTemplateName();
            this.GcProjectId = window.localStorage.getItem('gcProjectId');
            this.statuses = JSON.parse(window.localStorage.getItem('statuses'));
            this.currentStatus = null;
            this.isChangeStatus = false;
            this.model = {
                TargetNodeId: null,
                TargetNodeName: ''
            }
            var self = this;

            var importError = function (error) {
                notificationsService.error(error.data.Message);
            }

            var stopBusy = function () {
                self.saveButtonBusy = false;
            }

            var checkForm = function () {
                if (!self.items || self.items.length === 0) {
                    alert("Items for import don't selected.");
                    return false;
                }
                if (!self.GcProjectId) {
                    alert("Items for import don't selected.");
                    return false;
                }

                return true;
            }

            this.back = function () {
                window.history.back();
            }

            this.selectTargetNode = function (mapping) {
                dialogService.contentPicker({
                    callback: function (node) {
                        mapping.DefaultLocationId = node.id;
                        mapping.DefaultLocationTitle = node.name;
                    }
                });
            }

            this.checkItem = function () {
                var isExistNotSelected = false;
                angular.forEach(self.items, function (item) {
                    if (!item.Checked) {
                        isExistNotSelected = true;
                    }
                });
                if (isExistNotSelected) {
                    self.allSelected = false;
                } else {
                    self.allSelected = true;
                }
            }

            $scope.$watch('confirm.allSelected', function (n, o) {

                if (typeof event != 'undefined') {

                    if (event.target.id == 'allSelected') {
                        angular.forEach(self.items, function (item) {
                            item.Checked = n;
                        });
                    }

                } else {

                    // FF hotfix
                    angular.forEach(self.items, function (item) {
                        item.Checked = n;
                    });
                }

            });

            $scope.$on('$locationChangeSuccess', function () {
                window.localStorage.setItem('gathercontent.history.back', true);
            });

            this.next = function () {
                var notFillLocation = false;
                for (var i = 0; i < self.items.length; i++) {
                    if (self.items[i].Checked && !self.items[i].DefaultLocationId) {
                        notFillLocation = true;
                        break;
                    }
                }
                if (notFillLocation) {
                    notificationsService.warning('Please select Locations');
                    return;
                }

                var result = [];
                angular.forEach(JSON.parse(window.localStorage.getItem('gcItems')), function (item) {
                    angular.forEach(self.items, function (mapItem) {
                        if (mapItem.Checked && item.GcTemplate.Name === mapItem.TemplateName) {
                            angular.forEach(item.AvailableMappings.Mappings, function (mapping) {
                                if (mapItem.Id === mapping.Id) {
                                    result.push({
                                        ItemId: item.GcItem.Id,
                                        ItemTitle: item.GcItem.Title,
                                        TemplateName: item.GcTemplate.Name,
                                        MappingId: mapping.Id,
                                        MappingTitle: mapping.Title,
                                        CmsTemplateName: mapping.CmsTemplateName,
                                        DefaultLocationId: mapItem.DefaultLocationId,
                                        DefaultLocationTitle: mapItem.DefaultLocationTitle
                                    });
                                }
                            });
                        }
                    });
                });
                if (result.length > 0) {
                    window.localStorage.setItem('itemsForImportWithLocations', JSON.stringify(result));
                    $location.url('/GatherContent/GatherContentTree/import.locations.toimport/index');
                } else {
                    notificationsService.warning('No selected items.');
                }
            }
        }
    ])
    .controller('import.locations.toimport.controller', [
        '$scope',
        '$location',
        'importResource',
        'notificationsService',
        function ($scope, $location, importResource, notificationsService) {
            var self = this;
            this.items = JSON.parse(window.localStorage.getItem('itemsForImportWithLocations'));
            this.statuses = JSON.parse(window.localStorage.getItem('statuses'));
            this.currentStatus = '';
            this.busy = false;

            var importSuccess = function (success) {
                window.resultItem = success.data;
                var successItems = [];
                var errorItems = [];
                angular.forEach(window.resultItem, function (item) {
                    if (item.IsImportSuccessful)
                        successItems.push(item);
                    else
                        errorItems.push(item);
                });
                if (successItems.length > 0)
                    notificationsService.success(successItems.length + ' items were imported successfully.');
                if (errorItems.length > 0)
                    notificationsService.error(errorItems.length + ' items were not imported. Check errors below.');
                $location.url('/GatherContent/GatherContentTree/import.result/index');
            }

            var importError = function (error) {
                notificationsService.error(error.data.Message);
            }

            var stopBusy = function () {
                self.busy = false;
            }

            this.back = function () {
                window.history.back();
            }

            this.import = function () {
                self.busy = true;
                var confirmItems = [];
                angular.forEach(self.items, function (item) {
                    confirmItems.push({
                        Id: item.ItemId,
                        SelectedLocation: item.DefaultLocationId,
                        IsImport: true,
                        SelectedMappingId: item.MappingId
                    });
                });

                if (confirmItems.length > 0)
                    importResource.importItemsLocations(confirmItems, window.localStorage.getItem('gcProjectId'), self.isChangeStatus ?
                        (!!self.currentStatus ? self.currentStatus : "") : "")
                        .then(importSuccess, importError).always(stopBusy);
                else {
                    notificationsService.warning('No items checked.');
                    stopBusy();
                }
            }
        }
    ]);
