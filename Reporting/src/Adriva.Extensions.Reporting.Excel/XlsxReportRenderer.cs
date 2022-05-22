using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Adriva.Extensions.Reporting.Abstractions;

namespace Adriva.Extensions.Reporting.Excel
{
    public class XlsxReportRenderer : ReportRenderer
    {
        public override async ValueTask RenderAsync(string title, ReportOutput output, Stream stream)
        {
            Dictionary<string, Func<LargeXlsx.XlsxWriter, DataRow, LargeXlsx.XlsxWriter>> columnWriters = new Dictionary<string, Func<LargeXlsx.XlsxWriter, DataRow, LargeXlsx.XlsxWriter>>();

            if (string.IsNullOrWhiteSpace(title))
            {
                title = "Report";
            }
            else
            {
                StringBuilder titleBuffer = new StringBuilder();

                foreach (var charTitle in title)
                {
                    if (char.IsLetterOrDigit(charTitle) || ' ' == charTitle)
                    {
                        titleBuffer.Append(charTitle);
                    }
                    else if (char.IsWhiteSpace(charTitle))
                    {
                        titleBuffer.Append(' ');
                    }
                }

                title = titleBuffer.ToString();
            }

            if (1 > output.DataSet.Rows.Count)
            {
                return;
            }

            var firstRow = output.DataSet.Rows[0];

            foreach (var column in output.DataSet.Columns)
            {
                var dataCell = firstRow[column.Name];

                if (dataCell is int)
                {
                    columnWriters[column.Name] = (w, r) => w.Write((int)r[column.Name]);
                }
                else if (dataCell is long || dataCell is float || dataCell is double)
                {
                    columnWriters[column.Name] = (w, r) => w.Write((double)r[column.Name]);
                }
                else if (dataCell is decimal)
                {
                    columnWriters[column.Name] = (w, r) => w.Write((decimal)r[column.Name]);
                }
                else if (dataCell is DateTime)
                {
                    columnWriters[column.Name] = (w, r) => w.Write((DateTime)r[column.Name]);
                }
                else
                {
                    columnWriters[column.Name] = (w, r) => w.Write(Convert.ToString(r[column.Name]));
                }
            }

            using (var writer = new LargeXlsx.XlsxWriter(stream, SharpCompress.Compressors.Deflate.CompressionLevel.Default, false))
            {
                writer.BeginWorksheet(title);

                writer.BeginRow();

                foreach (var column in output.DataSet.Columns)
                {
                    writer.Write(column.DisplayName);
                }

                foreach (var row in output.DataSet.Rows)
                {
                    writer.BeginRow();

                    foreach (var column in output.DataSet.Columns)
                    {
                        columnWriters[column.Name](writer, row);
                    }
                }

                writer.SetAutoFilter(1, 1, writer.CurrentRowNumber - 1, writer.CurrentColumnNumber - 1);
            }

            await stream.FlushAsync();
        }
    }
}
