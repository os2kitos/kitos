using System.Text.RegularExpressions;

namespace Core.DomainServices.Repositories.KLE
{
    public class KLEParentHelper : IKLEParentHelper
    {
        // "12" returns false and sets parentidStr = "" (no parent)
        // "12.13.14" returns true and sets parentidStr, "12.13"
        public bool TryDeduceParentTaskKey(string kleChangeTaskKey, out string parentIdStr)
        {
            parentIdStr = string.Empty;
            var regex = new Regex(@"(\d{1,6})");
            var match = regex.Match(kleChangeTaskKey);
            while (match.Success)
            {
                var thisTaskLevel = match.Value;
                match = match.NextMatch();
                if (match.Success)
                {
                    parentIdStr = string.IsNullOrEmpty(parentIdStr) ? thisTaskLevel : parentIdStr + "." + thisTaskLevel;
                }
            }
            return !string.IsNullOrEmpty(parentIdStr);
        }
    }
}