using System;
using System.Threading.Tasks;

namespace Turnero.Services.Repositories;

public interface IExportRepository
{
    public Task<byte[]> ExportExcelAsync(DateTime date, Guid medicId, string filename);
}
