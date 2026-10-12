using Core.Abstractions.Types;
using Core.DomainModel.Shared;
using Presentation.Web.Models.API.V2.Types.SystemUsage;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Web.Controllers.API.V2.External.ItSystemUsages.Mapping
{
    public static class MainContractStateMappingExtensions
    {
        private static readonly EnumMap<MainContractStateChoice, MainContractState> Mapping;

        static MainContractStateMappingExtensions()
        {
            Mapping = new EnumMap<MainContractStateChoice, MainContractState>
            (
                (MainContractStateChoice.NoContract, MainContractState.NoContract),
                (MainContractStateChoice.Active, MainContractState.Active),
                (MainContractStateChoice.Inactive, MainContractState.Inactive)
            );
        }

        public static MainContractState ToMainContractState(this MainContractStateChoice value)
        {
            return Mapping.FromLeftToRight(value);
        }

        public static MainContractStateChoice ToMainContractStateChoice(this MainContractState value)
        {
            return Mapping.FromRightToLeft(value);
        }
    }
}
