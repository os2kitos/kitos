namespace Core.DomainServices.Repositories.KLE
{
    public interface IKLEParentHelper
    {
        bool TryDeduceParentTaskKey(string kleChangeTaskKey, out string s);
    }
}