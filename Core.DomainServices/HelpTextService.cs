using System.Linq;
using Core.DomainModel;

namespace Core.DomainServices;

public class HelpTextService : IHelpTextService
{
    private readonly IGenericRepository<HelpText> _repository;

    public HelpTextService(IGenericRepository<HelpText> repository)
    {
        _repository = repository;
    }

    public bool IsAvailableKey(string key)
    {
        return !_repository.AsQueryable().Any(ht => ht.Key == key);
    }
}