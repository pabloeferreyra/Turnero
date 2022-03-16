using System;
using System.IO;
using System.Threading.Tasks;

namespace Turnero.Services.Interfaces
{
    public interface IExportService
    {
        public Task<MemoryStream> ExportExcelAsync(DateTime date, Guid medicId);
    }
}
