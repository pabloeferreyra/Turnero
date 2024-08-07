﻿using System;
using System.Threading.Tasks;
using Turnero.Models;
using Turnero.Services.Interfaces;
using Turnero.Services.Repositories;

namespace Turnero.Services;

public class InsertTimeTurnServices : IInsertTimeTurnServices
{
    private readonly ILoggerServices _logger;
    private readonly ITimeTurnRepository _timeTurnRepository;

    public InsertTimeTurnServices(ILoggerServices logger,
                                  ITimeTurnRepository timeTurnRepository)
    {
        _logger = logger;
        _timeTurnRepository = timeTurnRepository;
    }

    public async Task Create(TimeTurn timeTurnViewModel)
    {
        try
        {
            //_ = Task.Run(async () =>
            //{
                //_logger.Debug($"Horario {timeTurnViewModel.Id} creado");
            //});
            await _timeTurnRepository.CreateTT(timeTurnViewModel);
        }
        catch (Exception)
        {
            //_logger.Error(ex.Message, ex);
        }
    }
}
