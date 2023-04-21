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

public class TurnsRepository : RepositoryBase<Turn>, ITurnRepository
{
    private readonly IMapper mapper;

    public TurnsRepository(ApplicationDbContext context, IMapper mapper) : base(context, mapper)
    {
        this.mapper = mapper;
    }



    public void Access(Turn turn)
    {
        turn.Accessed = true;
        this.Update(turn);
    }

    public async Task<Turn> GetById(Guid id)
    {
        return await this.FindByCondition(m => m.Id == id)
            .Include(m => m.Medic)
            .Include(t => t.Time).SingleOrDefaultAsync();
    }

    public async Task<TurnDTO> GetDTOById(Guid id)
    {
        return await this.FindByCondition(m => m.Id == id).ProjectTo<TurnDTO>(this.mapper.ConfigurationProvider).FirstOrDefaultAsync();
    }

    public IQueryable<TurnDTO> GetListDto() 
    {
        return this.FindAll().ProjectTo<TurnDTO>(this.mapper.ConfigurationProvider);
    }

    public async Task<List<Turn>> GetList(DateTime? date, Guid? id)
    {
        if(id != null)
        {
            if(date != null)
            {
                return await this.FindByCondition(m => m.MedicId == id && m.DateTurn.Date == date.Value.Date)
                    .Include(m => m.Medic).Include(t => t.Time)
                    .OrderBy(t => t.Time.Time)
                    .ToListAsync();
            }
            else
            {
                return await this.FindByCondition(m => m.MedicId == id && m.DateTurn == DateTime.Today)
                    .Include(m => m.Medic).Include(t => t.Time)
                    .OrderBy(t => t.Time.Time).ToListAsync();
            }
        }
        else
        {
            if (date != null)
            {
                return await this.FindByCondition(m => m.DateTurn == date)
                    .Include(m => m.Medic).Include(t => t.Time)
                    .OrderBy(t => t.Time.Time).ToListAsync();
            }
            else
            {
                return await this.FindByCondition(m => m.DateTurn == DateTime.Today)
                    .Include(m => m.Medic).Include(t => t.Time)
                    .OrderBy(t => t.Time.Time).ToListAsync();
            }
        }
    }

    public async Task<List<Turn>> ForExport(DateTime date, Guid id)
    {
        
        return await this.FindByCondition(m => m.MedicId == id && m.DateTurn.Date == date.Date)
            .Include(m => m.Medic).Include(t => t.Time)
            .OrderBy(t => t.Time.Time)
            .ToListAsync();
    }

    public bool TurnExists(Guid id)
    {
        return this.FindByCondition(m => m.Id == id).Any();
    }
    public bool CheckTurn(Guid medicId, DateTime date, Guid timeTurn)
    {
        return this.FindByCondition(e => e.MedicId == medicId && e.DateTurn.Date == date && e.TimeId == timeTurn).Any();
    }

    public void DeleteTurn(Turn turn)
    {
        this.DeleteAsync(turn);
    }

    public void UpdateTurn(Turn turn)
    {
        this.Update(turn);
    }

    public async Task CreateTurn(Turn turn)
    {
        turn.Medic = null;
        turn.Time = null;
        await this.CreateAsync(turn);
    }
}
