using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Turnero.Services.Repositories
{
    public interface IExportRepository
    {
        public Task<MemoryStream> ExportExcelAsync(DateTime date, Guid medicId);
    }
}
