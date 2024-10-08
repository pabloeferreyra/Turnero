﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Turnero.Data;
using Turnero.Models;

namespace Turnero.Services.Repositories;

public class MedicRepository : RepositoryBase<Medic>, IMedicRepository
{
    public IMapper mapper { get; }

    public MedicRepository(ApplicationDbContext context, IMapper mapper, IMemoryCache cache) : base(context, mapper, cache)
    {
        this.mapper = mapper;
    }

    public async Task<List<MedicDto>> GetListDto()
    {
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

    public async Task NewMedic(Medic medic)
    {
        if (!string.IsNullOrEmpty(medic.Name))
        {
            await this.CreateAsync(medic);
        }
        
    }

    public void DeleteMedic(Medic medic)
    {
        this.DeleteAsync(medic);
    }

    public async Task UpdateMedic(Medic medic)
    {
        await this.UpdateAsync(medic);
    }

    public async Task<List<MedicDto>> GetCachedMedics()
    {
        return await GetCachedData("medics", GetListDto);
    }
}
