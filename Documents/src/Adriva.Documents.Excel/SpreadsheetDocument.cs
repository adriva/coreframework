using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Adriva.Documents.Abstractions;
using ClosedXML.Excel;

namespace Adriva.Documents.Excel
{
    public class SpreadsheetDocument : Document<SpreadsheetWorksheet>
    {
        protected IXLWorkbook Workbook { get; private set; }

        public override bool CanSave => true;

        public int WorksheetCount => null == this.Workbook.Worksheets ? 0 : this.Workbook.Worksheets.Count;

        internal SpreadsheetDocument()
        {
            this.Workbook = new XLWorkbook();
        }

        internal SpreadsheetDocument(Stream stream)
        {
            this.Workbook = new XLWorkbook(stream);
        }

        private IXLWorksheet GetWorksheet(int worksheetIndex)
        {
            if (0 == this.WorksheetCount)
            {
                throw new InvalidOperationException("Workbook does not have any worksheets.");
            }

            ++worksheetIndex;

            if (1 > worksheetIndex || worksheetIndex > this.WorksheetCount)
            {
                if (1 == this.WorksheetCount)
                {
                    throw new ArgumentException($"worksheetIndex for this workbook should be 0.");
                }
                else
                {
                    throw new ArgumentException($"worksheetIndex for this workbook should be between 0 and {this.WorksheetCount - 1}.");
                }
            }

            return this.Workbook.Worksheet(worksheetIndex);
        }

        protected override Task SaveDocumentAsync(Stream stream)
        {
            this.Workbook.SaveAs(stream);
            return Task.CompletedTask;
        }

        public override IEnumerable<SpreadsheetWorksheet> GetParts()
        {
            if (null == this.Workbook?.Worksheets || 0 == this.Workbook.Worksheets.Count)
            {
                yield break;
            }

            for (int loop = 0; loop < this.Workbook.Worksheets.Count; loop++)
            {
                yield return new SpreadsheetWorksheet(this.Workbook.Worksheet(1 + loop));
            }
        }

        public override IEnumerable<T> GetParts<T>()
        {
            return this.GetParts().OfType<T>();
        }

        public override SpreadsheetWorksheet AddPart()
        {
            return this.AddPart<SpreadsheetWorksheet>();
        }

        public override T AddPart<T>()
        {
            if (typeof(T).IsAssignableFrom(typeof(SpreadsheetWorksheet)))
            {
                return new SpreadsheetWorksheet(this.Workbook.AddWorksheet()) as T;
            }

            throw new NotSupportedException("Spreadsheet document only accepts worksheet parts.");
        }
    }
}