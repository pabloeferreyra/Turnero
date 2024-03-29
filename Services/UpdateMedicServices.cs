﻿using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Turnero.Models;
using Turnero.Services.Interfaces;
using Turnero.Services.Repositories;

namespace Turnero.Services;

public class UpdateMedicServices : IUpdateMedicServices
{
    private readonly ILoggerServices _logger;
    private readonly IMedicRepository _medicRepository;

    public UpdateMedicServices(ILoggerServices logger, IMedicRepository medicRepository)
    {
        _logger = logger;
        _medicRepository = medicRepository;
    }

    public async Task<bool> Update(Medic medic)
    {
        try
        {
            await _medicRepository.UpdateMedic(medic);
            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.Error(ex.Message, ex);
            return false;
        }
    }

    public void Delete(Medic medic)
    {
        try
        {
            _medicRepository.DeleteMedic(medic);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.Error(ex.Message, ex);
        }
    }
}
