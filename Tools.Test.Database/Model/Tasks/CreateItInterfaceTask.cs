using Core.DomainModel.ItSystem;
using Infrastructure.DataAccess;
using System;
using Core.DomainModel;
using Tools.Test.Database.Model.Extensions;

namespace Tools.Test.Database.Model.Tasks;

public class CreateItInterfaceTask : DatabaseTask
{
    private readonly string _name;
    private readonly string _organizationName;
    public CreateItInterfaceTask(string name, string organizationName)
    {
        _name = name;
        _organizationName = organizationName;
    }

    public override bool Execute(KitosContext context)
    {
        var organization = context.GetOrganization(_organizationName);
        var globalAdmin = context.GetGlobalAdmin();

        var itInterface = new ItInterface
        {
            Name = _name,
            OrganizationId = organization.Id,
            Uuid = Guid.NewGuid(),
            ObjectOwnerId = globalAdmin.Id,
            LastChangedByUserId = globalAdmin.Id,
            ItInterfaceId = "DefaultInterfaceId",
            AccessModifier = AccessModifier.Public
        };

        context.ItInterfaces.Add(itInterface);


        context.SaveChanges();

        return true;
    }
}