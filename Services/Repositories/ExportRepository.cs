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
        public ExportRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<MemoryStream> ExportExcelAsync(DateTime date, Guid medicId)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var turns = await GetList(date, medicId);
            var stream = new MemoryStream();
            using (var xlPackage = new ExcelPackage(stream))
            {
                // Define a worksheet
                var worksheet = xlPackage.Workbook.Worksheets.Add("Users");

                var customStyle = xlPackage.Workbook.Styles.CreateNamedStyle("CustomStyle");
                customStyle.Style.Font.UnderLine = true;
                customStyle.Style.Font.Color.SetColor(Color.Red);

                // First row
                var startRow = 5;
                var row = startRow;

                worksheet.Cells["A1"].Value = "Turnos del dia "+ date.ToShortDateString();
                using (var r = worksheet.Cells["A1:C1"])
                { 
                    r.Merge = true;
                    r.Style.Font.Color.SetColor(Color.Green);
                    r.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    r.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(23, 55, 93));
                }

                worksheet.Cells["A4"].Value = "Nombre";
                worksheet.Cells["B4"].Value = "Hora";
                worksheet.Cells["C4"].Value = "Obra Social";
                worksheet.Cells["D4"].Value = "Motivo";
                worksheet.Cells["A4:D4"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells["A4:D4"].Style.Fill.BackgroundColor.SetColor(Color.Yellow);

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

                xlPackage.Save();
            }

            stream.Position = 0;
            return stream;
        }
    }
}
