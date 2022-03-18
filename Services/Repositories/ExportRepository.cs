using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using Turnero.Data;

namespace Turnero.Services.Repositories
{
    public class ExportRepository : TurnsRepository, IExportRepository
    {
        private IMedicRepository _medic;
        public ExportRepository(ApplicationDbContext context, IMedicRepository medic) : base(context)
        {
            _medic = medic;
        }

        public async Task<byte[]> ExportExcelAsync(DateTime date, Guid medicId, string filename)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var turns = await GetList(date, medicId);
            using (var xlPackage = new ExcelPackage())
            {
                // Define a worksheet
                var worksheet = xlPackage.Workbook.Worksheets.Add(filename);

                var customStyle = xlPackage.Workbook.Styles.CreateNamedStyle("CustomStyle");
                customStyle.Style.Font.UnderLine = true;
                customStyle.Style.Font.Color.SetColor(Color.Red);

                // First row
                var startRow = 5;
                var row = startRow;
                var medic = await this._medic.GetById(medicId);
                worksheet.Cells["A1"].Value = "Turnos del dia "+ date.ToShortDateString() + " " + medic.Name;
                using (var r = worksheet.Cells["A1:D1"])
                { 
                    r.Merge = true;
                }

                worksheet.Cells["A3"].Value = "Nombre";
                worksheet.Cells["B3"].Value = "Hora";
                worksheet.Cells["C3"].Value = "Obra Social";
                worksheet.Cells["D3"].Value = "Motivo";
                worksheet.Cells["A3:D3"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells["A3:D3"].Style.Fill.BackgroundColor.SetColor(Color.Green);

                row = 5;
                foreach (var turn in turns)
                {
                    worksheet.Cells[row, 1].Value = turn.Name;
                    worksheet.Cells[row, 2].Value = turn.Time.Time;
                    worksheet.Cells[row, 3].Value = turn.SocialWork;
                    worksheet.Cells[row, 4].Value = turn.Reason;

                    row++; // row = row + 1;
                }

                xlPackage.Workbook.Properties.Title = "Turnos";
                xlPackage.Workbook.Properties.Author = "PF Software";

                return xlPackage.GetAsByteArray();
            }   
        }
    }
}
