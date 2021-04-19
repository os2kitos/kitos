using System;

namespace Core.DomainModel.ItSystemUsage.GDPR
{
    public enum SensitiveDataLevel
    {
        NONE = 0,
        PERSONALDATA = 1,
        SENSITIVEDATA = 2,
        LEGALDATA = 3
    }

    public static class SensitiveDataLevelExtensions
    {
        public static string GetReadableName(this SensitiveDataLevel sensitiveDataLevel)
        {
            return sensitiveDataLevel switch
            {
                SensitiveDataLevel.NONE => "Ingen persondata",
                SensitiveDataLevel.PERSONALDATA => "Almindelige persondata",
                SensitiveDataLevel.SENSITIVEDATA => "Følsomme persondata",
                SensitiveDataLevel.LEGALDATA => "Straffedomme og lovovertrædelser",
                _ => throw new InvalidOperationException($"Invalid sensitiveDataLevel value: {sensitiveDataLevel}"),
            };
        }
    }
}
