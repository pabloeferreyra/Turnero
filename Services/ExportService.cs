using System;
using System.IO;
using System.Threading.Tasks;
using Turnero.Services.Interfaces;
using Turnero.Services.Repositories;

namespace Turnero.Services
{
    public class ExportService : IExportService
    {
        private readonly IExportRepository _exportRepository;

        public ExportService(IExportRepository exportRepository)
        {
            _exportRepository = exportRepository;
        }

        public async Task<byte[]> ExportExcelAsync(DateTime date, Guid medicId, string filename)
        {
            return await _exportRepository.ExportExcelAsync(date, medicId, filename);
        }
    }
}
