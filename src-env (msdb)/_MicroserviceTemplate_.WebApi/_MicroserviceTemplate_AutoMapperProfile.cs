using _MicroserviceTemplate_.Domain.MyTables;
using _MicroserviceTemplate_.WebApi.Models.MyTables;
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
            CreateMyTablesMaps();
        }

        private void CreateCommonMaps()
        {
            CreateMap(typeof(Page<>), typeof(Page<>));
        }

        private void CreateMyTablesMaps()
        {
            CreateMap<MyTable, MyTableViewModel>()
                            .ForMember(x => x.CreateTimeUtc, o => o.MapFrom(x => SpecifyUtc(x.CreateTimeUtc)))
                            .ForMember(x => x.UpdateTimeUtc, o => o.MapFrom(x => SpecifyUtc(x.UpdateTimeUtc)));
            CreateMap<AddMyTableViewModel, MyTable>();
            CreateMap<ChangeMyTableViewModel, MyTable>();
        }

        private static DateTime? SpecifyUtc(DateTime? dateTime)
        {
            return dateTime.HasValue ? DateTime.SpecifyKind(dateTime.Value, DateTimeKind.Utc) : (DateTime?)null;
        }
    }
}