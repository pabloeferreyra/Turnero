using AutoMapper;
using System;
using System.Globalization;
using Turnero.Models;

namespace Turnero.Utilities {
    public class AutoMapperProfiles : Profile {
        public AutoMapperProfiles() {
            CreateMap<Medic, MedicDto>();
            CreateMap<Turn, TurnDTO>().ForMember(dto => dto.Time,
                ent => ent.MapFrom(p => p.Time.Time))
                .ForMember(dto => dto.TimeId,
                ent => ent.MapFrom(p => p.Time.Id))
                .ForMember(dto => dto.MedicName,
                ent => ent.MapFrom(p => p.Medic.Name))
                .ForMember(dto => dto.MedicId,
                ent => ent.MapFrom(p => p.Medic.Id))
                .ForMember(dto => dto.Date, ent => ent.MapFrom(p => p.DateTurn.ToString("dd/MM/yyyy")));
            CreateMap<TurnDTO, Turn>().ForMember(ent => ent.TimeId,
                dto => dto.MapFrom(p => p.Time))
                .ForMember(ent => ent.Time, 
                prop => prop.Ignore())
                .ForMember(ent => ent.MedicId,
                dto => dto.MapFrom(p => p.MedicId))
                .ForMember(ent => ent.Medic,
                prop => prop.Ignore())
                .ForMember(ent => ent.DateTurn,
                dto => dto.MapFrom(p => DateTime.Parse(p.Date)));
        }
    }
}
