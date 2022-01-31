using AutoMapper;
using src.Areas.Identity.Data;
using src.Areas.Profile.ViewModels;
using src.Models;
using System.Collections.Generic;

namespace src.Helpers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<srcUser, ProfileViewModel>().ReverseMap();
            CreateMap<AanmeldingClient, AanmeldingClient>();
        }
    }
}
