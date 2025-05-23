using System;
using System.Collections.Generic;
using System.Linq;
using Adriva.Documents.Abstractions;
using ClosedXML.Excel;

namespace Adriva.Documents.Excel
{
    public class SpreadsheetWorksheet : IMutableDataDocumentPart<IEnumerable<object>, CellReference>, IDocumentPartsContainer<SpreadsheetRange>, INativeDocumentElement<IXLWorksheet>
    {
        private readonly IXLWorksheet Worksheet;

        public string Name => this.Worksheet?.Name;

        public IXLWorksheet Element => this.Worksheet;

        internal SpreadsheetWorksheet(IXLWorksheet worksheet)
        {
            this.Worksheet = worksheet;
        }

        public IEnumerable<SpreadsheetRange> GetParts()
        {
            yield return new SpreadsheetRange(this.Worksheet.RangeUsed(XLCellsUsedOptions.None));
        }

        public SpreadsheetRange GetPart(CellReference fromCell, CellReference toCell)
        {
            return new SpreadsheetRange(this.Worksheet.Range(fromCell.Address, toCell.Address));
        }

        public IEnumerable<T> GetParts<T>() where T : IDocumentPart
        {
            return this.GetParts().OfType<T>();
        }

        public SpreadsheetRange AddPart()
        {
            throw new NotSupportedException();
        }

        public T AddPart<T>() where T : class, IDocumentPart
        {
            throw new NotSupportedException();
        }

        public void AppendData(IEnumerable<object> data, CellReference options)
        {
            if (null == data || !options.IsValid)
            {
                return;
            }

            this.Worksheet.Cell(options.Address).InsertData(data);
        }

        public void DeleteData(CellReference fromCell, params CellReference[] toCell)
        {
            if (!fromCell.IsValid || null == toCell || 1 != toCell.Length)
            {
                return;
            }

            this.Worksheet.Range(fromCell.Address, toCell[0].Address).Delete(XLShiftDeletedCells.ShiftCellsUp | XLShiftDeletedCells.ShiftCellsLeft);
        }

        public void UpsertData(IEnumerable<object> data, CellReference options)
        {
            if (!options.IsValid)
            {
                return;
            }

            IXLCell startingCell = this.Worksheet.Cell(options.Address);
            startingCell.InsertData(data);
        }
    }
}