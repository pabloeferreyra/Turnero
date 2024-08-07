﻿using System;
using System.Threading.Tasks;
using Turnero.Models;
using Turnero.Services.Interfaces;
using Turnero.Services.Repositories;

namespace Turnero.Services;

public class InsertTurnsServices : IInsertTurnsServices
{
    private readonly ILoggerServices _logger;
    private readonly ITurnRepository _turnRepository;
    public InsertTurnsServices(ILoggerServices logger, ITurnRepository turnRepository)
    {
        _logger = logger;
        _turnRepository = turnRepository;
    }
    public async Task<bool> CreateTurnAsync(Turn turn)
    {
        try
        {
            await _turnRepository.CreateTurn(turn);
            //_ = Task.Run(() =>
            //{
            //    _logger.Debug("Turno agregado correctamente");
            //});
            return true;
        }
        catch (Exception)
        {
            //_logger.Error(ex.Message, ex);
            return false;
        }
    }
}
