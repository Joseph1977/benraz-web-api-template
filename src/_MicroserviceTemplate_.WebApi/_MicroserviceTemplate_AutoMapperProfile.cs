using _MicroserviceTemplate_.Domain.Settings;
using _MicroserviceTemplate_.WebApi.Models.Settings;
using AutoMapper;
using Benraz.Infrastructure.Common.Paging;
using System;

namespace _MicroserviceTemplate_.WebApi
{
    class _MicroserviceTemplate_AutoMapperProfile : Profile
    {
        public _MicroserviceTemplate_AutoMapperProfile()
        {
            CreateCommonMaps();
            CreateSettingsMaps();
        }

        private void CreateCommonMaps()
        {
            CreateMap(typeof(Page<>), typeof(Page<>));
        }

        private void CreateSettingsMaps()
        {
            CreateMap<SettingsEntry, SettingsEntryViewModel>()
                .ForMember(x => x.CreateTimeUtc, o => o.MapFrom(x => SpecifyUtc(x.CreateTimeUtc)))
                .ForMember(x => x.UpdateTimeUtc, o => o.MapFrom(x => SpecifyUtc(x.UpdateTimeUtc)));
            CreateMap<AddSettingsEntryViewModel, SettingsEntry>();
            CreateMap<ChangeSettingsEntryViewModel, SettingsEntry>();
        }

        private static DateTime? SpecifyUtc(DateTime? dateTime)
        {
            return dateTime.HasValue ? DateTime.SpecifyKind(dateTime.Value, DateTimeKind.Utc) : (DateTime?)null;
        }
    }
}