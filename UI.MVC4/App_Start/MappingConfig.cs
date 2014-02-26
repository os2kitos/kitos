
using System.Linq;
using AutoMapper;
using Core.DomainModel;
using UI.MVC4.Models;

[assembly: WebActivator.PreApplicationStartMethod(typeof(UI.MVC4.App_Start.MappingConfig), "Start")]

namespace UI.MVC4.App_Start
{
    public class MappingConfig
    {
         public static void Start()
         {
             Mapper.Initialize(cfg => cfg.AddProfile(new MappingProfile()));
         }
    }

    public class MappingProfile : Profile
    {
        protected override void Configure()
        {
            base.Configure();

            Mapper.CreateMap<User, UserApiModel>().ReverseMap();

            Mapper.CreateMap<Municipality, MunicipalityApiModel>().ReverseMap();
        }
    }
}