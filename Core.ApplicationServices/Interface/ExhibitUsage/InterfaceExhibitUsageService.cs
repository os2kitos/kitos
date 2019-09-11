using System;
using Core.ApplicationServices.Model.Result;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;
using Serilog;

namespace Core.ApplicationServices.Interface.ExhibitUsage
{
    public class InterfaceExhibitUsageService : IInterfaceExhibitUsageService
    {
        private readonly IGenericRepository<ItInterfaceExhibitUsage> _itInterfaceExhibitUsageRepository;
        private readonly ILogger _logger;

        public InterfaceExhibitUsageService(
            IGenericRepository<ItInterfaceExhibitUsage> itInterfaceExhibitUsageRepository,
            ILogger logger)
        {
            _itInterfaceExhibitUsageRepository = itInterfaceExhibitUsageRepository;
            _logger = logger;
        }

        public Result<OperationResult, object> DeleteByKey(object[] key)
        {
            try
            {
                _itInterfaceExhibitUsageRepository.DeleteByKey(key);
                _itInterfaceExhibitUsageRepository.Save();
                return Result<OperationResult, object>.Ok(null);
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Failed to delete interface exhibit usage with key {key}");
                return Result<OperationResult, object>.Fail(OperationResult.UnknownError);
            }
        }
    }
}
