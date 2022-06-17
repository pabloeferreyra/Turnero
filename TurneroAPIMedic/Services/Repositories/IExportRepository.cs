﻿using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;

namespace TurneroAPI.Services.Repositories
{
    public interface IExportRepository
    {
        public Task<byte[]> ExportExcelAsync(DateTime date, Guid medicId, string filename);
    }
}
