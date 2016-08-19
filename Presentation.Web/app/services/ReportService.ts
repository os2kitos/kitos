/// <reference path="../index.d.ts" />

// https://docs.angularjs.org/api/ngResource/service/$resource
// https://chsakell.com/2015/04/04/asp-net-web-api-feat-odata/

// (function (ng, app) {
//     angular.module('app')
//         .factory('ReportService', ["$http", function ($http) {
//             var odataUrl = '/odata/reports';
//             return $resource('', {},
//                 {
//                     'getAll': { method: 'GET', url: odataUrl },
//                     'get': { method: 'GET', params: { key: '@key' }, url: odataUrl + '(:key)' },
//                     'create': { method: 'POST', url: odataUrl },
//                     'patch': { method: 'PATCH', params: { key: '@key' }, url: odataUrl + '(:key)' },
//                     'delete': { method: 'DELETE', params: { key: '@key' }, url: odataUrl + '(:key)' }
//                 });
//         }]);
// })(angular, app);

// (function (ng, app) {
//     angular.module('app')
//         .factory('ReportService', ["$resource", function ($resource) {
//             var odataUrl = '/odata/reports';
//             return $resource('', {},
//                 {
//                     'getAll': { method: 'GET', url: odataUrl },
//                     'get': { method: 'GET', params: { key: '@key' }, url: odataUrl + '(:key)' },
//                     'create': { method: 'POST', url: odataUrl },
//                     'patch': { method: 'PATCH', params: { key: '@key' }, url: odataUrl + '(:key)' },
//                     'delete': { method: 'DELETE', params: { key: '@key' }, url: odataUrl + '(:key)' }
//                 });
//         }]);
// })(angular, app);