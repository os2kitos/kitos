using Core.DomainModel;
using Core.DomainServices;

namespace Core.ApplicationServices
{
    public class MunicipalityService : IMunicipalityService
    {
        private readonly IGenericRepository<Municipality> _municipalityRepository;
        private readonly IGenericRepository<Config> _configRepository;

        public MunicipalityService(IGenericRepository<Municipality> municipalityRepository, IGenericRepository<Config> configRepository)
        {
            _municipalityRepository = municipalityRepository;
            _configRepository = configRepository;
        }

        public Municipality AddMunicipality(Municipality municipality)
        {
            municipality = _municipalityRepository.Insert(municipality);
            _municipalityRepository.Save();

            _configRepository.Insert(DefaultConfig(municipality.Id));
            _configRepository.Save();

            return municipality;
        }

        private Config DefaultConfig(int id)
        {
            return new Config()
            {
                Id = id,
                ShowItContractModule = true,
                ShowItProjectModule = true,
                ShowItSystemModule = true,
                ItSupportModuleName_Id = 1,
                ItContractModuleName_Id = 1,
                ItProjectModuleName_Id = 1,
                ItSystemModuleName_Id = 1
            };
        }
    }
}