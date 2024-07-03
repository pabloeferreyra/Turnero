using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Turnero.Models;
using Turnero.Services.Interfaces;
using Turnero.Services.Repositories;

namespace Turnero.Services;

public class GetMedicsServices : IGetMedicsServices
{
    private readonly ILoggerServices _logger;
    private readonly IMedicRepository _medicRepository;

    public GetMedicsServices(ILoggerServices logger, IMedicRepository medicRepository)
    {
        _logger = logger;
        _medicRepository = medicRepository;
    }

    public async Task<List<MedicDto>> GetMedicsDto()
    {
        try
        {
            //_ = Task.Run(async () =>
            //{
            //_logger.Debug("Medicos traidos correctamente");
            //});
            var med = await _medicRepository.GetListDto();

            return med;
        }
        catch (Exception)
        {
            //_logger.Error(ex.Message, ex);
            return new List<MedicDto>();
        }
    }

    public async Task<List<Medic>> GetMedics()
    {
        try
        {
            //_ = Task.Run(async () =>
            //{
                //_logger.Debug("Medicos traidos correctamente");
            //});
            var med = await _medicRepository.GetList();

            return med;
        }
        catch (Exception)
        {
            ////_logger.Error(ex.Message, ex);
            return new List<Medic>();
        }
    }

    public async Task<Medic> GetMedicById(Guid id)
    {
        try
        {
            //_ = Task.Run(async () =>
            //{
                //_logger.Debug("Medico traido correctamente");
            //});
            Medic med = await _medicRepository.GetById(id);

            return med;
        }
        catch (Exception)
        {
            //_logger.Error(ex.Message, ex);
            return new Medic();
        }
    }

    public async Task<Medic> GetMedicByUserId(string id)
    {
        try
        {
            //_ = Task.Run(async () =>
            //{
                //_logger.Debug("Medico traido correctamente por usuario");
            //});
            Medic med = await _medicRepository.GetByUserId(id);

            return med;
        }
        catch (Exception)
        {
            //_logger.Error(ex.Message, ex);
            return new Medic();
        }
    }

    public bool ExistMedic(Guid id)
    {
        try
        {
            return _medicRepository.Exists(id);
        }
        catch (Exception)
        {
            //_logger.Error(ex.Message, ex);
            return false;
        }
    }

    public async Task<List<MedicDto>> GetCachedMedics()
    {
        try
        {
            return await _medicRepository.GetCachedMedics();
        }
        catch (Exception)
        {
            //_logger.Error(ex.Message, ex);
            return null;
        }
    }
}
