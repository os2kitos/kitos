using System;

namespace Core.ApplicationServices.SSO.Model
{
    public class SamlUserUuid
    {
        public Guid Value { get; }

        public SamlUserUuid(Guid value)
        {
            Value = value;
        }
    }
}
