﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Turnero.Data;
using Turnero.Models;

namespace Turnero.Services.Repositories;

public class MedicRepository : RepositoryBase<Medic>, IMedicRepository
{
    public IMapper mapper { get; }

    public MedicRepository(ApplicationDbContext context, IMapper mapper) : base(context, mapper)
    {
        this.mapper = mapper;
    }

    public async Task<List<MedicDto>> GetListDto() {
        return await FindAll().ProjectTo<MedicDto>(this.mapper.ConfigurationProvider).ToListAsync();
    }

    public async Task<List<Medic>> GetList()
    {
        return await this.FindAll().ToListAsync();
    }

    public async Task<Medic> GetById(Guid id)
    {
        return await this.FindByCondition(m => m.Id == id).SingleOrDefaultAsync();
    }

    public async Task<Medic> GetByUserId(string id)
    {
        return await this.FindByCondition(m => m.UserGuid == id).SingleOrDefaultAsync();
    }

    public bool Exists(Guid id)
    {
        return this.FindByCondition(m => m.Id == id).Any();
    }

    public async Task NewMedic (Medic medic)
    {
        await this.CreateAsync(medic);
    }

    public async Task DeleteMedic (Medic medic)
    {
        await this.DeleteAsync(medic);
    }

    public async Task UpdateMedic(Medic medic)
    {
        await this.UpdateAsync(medic);
    }
}
