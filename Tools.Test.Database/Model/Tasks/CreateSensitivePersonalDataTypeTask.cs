using System;
using Core.DomainModel.ItSystem;
using Infrastructure.DataAccess;
using Tools.Test.Database.Model.Extensions;

namespace Tools.Test.Database.Model.Tasks
{
    public class CreateSensitivePersonalDataTypeTask : DatabaseTask
    {
        private readonly string _name;

        public CreateSensitivePersonalDataTypeTask(string name)
        {
            _name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public override bool Execute(KitosContext context)
        {
            var globalAdmin = context.GetGlobalAdmin();

            var sensitiveDataType = new SensitivePersonalDataType()
            {
                Name = _name,
                IsLocallyAvailable = true,
                IsObligatory = true,
                IsEnabled = true,
                LastChanged = DateTime.Now,
                LastChangedByUserId = globalAdmin.Id
            };
            context.SensitivePersonalDataTypes.Add(sensitiveDataType);

            context.SaveChanges();
            return true;
        }
    }
}
