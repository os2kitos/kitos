using Core.DomainModel;

namespace Core.ApplicationServices
{
    public interface IFeatureChecker
    {
        bool CanExecute(User user, Feature feature);
    }
}