using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Turnero.Models;
using Turnero.Services.Interfaces;
using Turnero.Services.Repositories;

namespace Turnero.Services;

public class UpdateTurnsServices : IUpdateTurnsServices
{
    private readonly ILoggerServices _logger;
    private readonly ITurnRepository _turnRepository;

    public UpdateTurnsServices(ILoggerServices logger, ITurnRepository turnRepository)
    {
        _logger = logger;
        _turnRepository = turnRepository;
    }

    public void Accessed(Turn turn)
    {
        try
        {
            if(turn.DateTurn <= DateTime.Today)
            {
                _turnRepository.Access(turn);
                //_ = Task.Run(() =>
                //{
                    //_logger.Debug($"Turno {turn.Id} ingresado");
                //});
            }
        }
        catch (Exception ex)
        {
            //_logger.Error(ex.Message, ex);
        }
    }

    public void Update(Turn turn)
    {
        try
        {
            _turnRepository.UpdateTurn(turn);
            //_ = Task.Run(() =>
            //  {
            //      _logger.Debug($"Turno {turn.Id} Actualizado");
            //  });
            
        }
        catch (DbUpdateConcurrencyException ex)
        {
            //_logger.Error(ex.Message, ex);
        }
    }

    public void Delete(Turn turn)
    {
        try
        {
            _turnRepository.DeleteTurn(turn);
            //_ = Task.Run(() =>
            //{
            //    _logger.Debug($"Turno {turn.Id} Eliminado");
            //});
        }
        catch (DbUpdateConcurrencyException ex)
        {
            //_logger.Error(ex.Message, ex);
        }
    }
}
