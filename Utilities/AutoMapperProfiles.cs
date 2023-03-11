using AutoMapper;
using System;

using Turnero.Models;

namespace Turnero.Utilities {
    public class AutoMapperProfiles : Profile {
        public AutoMapperProfiles() {
            CreateMap<Medic, MedicDto>();
            CreateMap<Turn, TurnDTO>().ForMember(dto => dto.Time,
                ent => ent.MapFrom(p => p.Time.Time))
                .ForMember(dto => dto.MedicName,
                ent => ent.MapFrom(p => p.Medic.Name))
                .ForMember(dto => dto.MedicId,
                ent => ent.MapFrom(p => p.Medic.Id))
                .ForMember(dto => dto.Date, ent => ent.MapFrom(p => p.DateTurn.ToString("dd/MM/yyyy")));
        }
    }
}
