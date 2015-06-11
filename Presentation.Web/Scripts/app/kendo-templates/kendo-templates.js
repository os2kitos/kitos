/**
 * This are used by kendo and they must be declared on the global scope 
 * else persisting grid state won't work.
 * 
 * Because JSONfn.parse (see gridStateService) will look for the function and fail if it can't find it.
 */

var kendoTemplate = {
    roleTemplate: function(dataItem, roleId) {
        var roles = "";

        if (dataItem.roles[roleId] === undefined)
            return roles;

        // join the first 5 username together
        if (dataItem.roles[roleId].length > 0)
            roles = dataItem.roles[roleId].slice(0, 4).join(", ");

        // if more than 5 then add an elipsis
        if (dataItem.roles[roleId].length > 5)
            roles += ", ...";

        var link = "<a data-ui-sref='it-system.usage.roles({id: " + dataItem.Id + "})'>" + roles + "</a>";

        return link;
    },
    contractTemplate: function(dataItem) {
        if (dataItem.MainContract)
            if (dataItem.MainContract.ItContract)
                if (dataItem.MainContract.ItContract.IsActive)
                    return '<a data-ui-sref="it-system.usage.contracts({id: ' + dataItem.Id + '})"><span class="glyphicon glyphicon-file text-success" aria-hidden="true"></span></a>';
                else
                    return '<a data-ui-sref="it-system.usage.contracts({id: ' + dataItem.Id + '})"><span class="glyphicon glyphicon-file text-muted" aria-hidden="true"></span></a>';

        return "";
    },
    usageButtonTemplate: function(dataItem) {
        // don't have access to user.currentOrganizationId so relying on localStorage - not ideal... :/
        var currentOrganizationId = localStorage.getItem("currentOrgId");
        // true if system is being used by system within current context, else false
        var systemHasUsages = _.find(dataItem.Usages, function (d) { return d.OrganizationId == currentOrganizationId; });

        if (systemHasUsages)
            return '<button class="btn btn-danger col-md-7" data-ng-click="removeUsage(' + dataItem.Id + ')">Fjern anv.</button>';

        return '<button class="btn btn-success col-md-7" data-ng-click="enableUsage(' + dataItem.Id + ')">Anvend</button>';
    }
};
