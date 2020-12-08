using APIWithIdentity.DomainModel.Models;
using APIWithIdentity.DomainModel.Models.Auth;
using APIWithIdentity.DTOs;
using APIWithIdentity.DTOs.DTOsAuth;
using APIWithIdentity.DTOs.MusicDTOs;
using AutoMapper;

namespace APIWithIdentity.Mapping
{
    public class MappingProfile : Profile
      {
          public MappingProfile()
          {
              // Resource to Domain
              CreateMap<UserResponse, User>().ReverseMap();
              CreateMap<MusicDTO, Music>().ReverseMap();
              CreateMap<SaveMusic, Music>().ReverseMap();
              CreateMap<ArtistDTO, Artist>().ReverseMap();
              CreateMap<SaveArtist, Artist>().ReverseMap();
              CreateMap<UserSignUp, User>()
                  .ForMember(u => u.UserName, opt 
                      => opt.MapFrom(ur => ur.Email));
          }
      }
}