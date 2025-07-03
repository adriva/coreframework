using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Adriva.Documents.Abstractions;
using ClosedXML.Excel;

namespace Adriva.Documents.Excel
{
    public class SpreadsheetRange : IDocumentPart, IEnumerable<CellData>, INativeDocumentElement<IXLRange>
    {
        private sealed class RangeEnumerator : IEnumerator<CellData>
        {
            private readonly IXLRange Range;
            private readonly Rectangle Bounds;
            private int RowIndex = 0;
            private int ColumnIndex = 0;

            public CellData Current { get; private set; }

            object IEnumerator.Current => this.Current;

            public RangeEnumerator(IXLRange range)
            {
                this.Range = range;
                this.Bounds = new Rectangle(
                    this.Range.FirstCell()?.Address.ColumnNumber ?? 0,
                    this.Range.FirstCell()?.Address.RowNumber ?? 0,
                    this.Range.LastCell()?.Address.ColumnNumber ?? 0,
                    this.Range.LastCell()?.Address.RowNumber ?? 0
                );
            }

            public bool MoveNext()
            {
                if (this.RowIndex < this.Bounds.Height)
                {
                    if (0 == this.RowIndex)
                    {
                        ++this.RowIndex;
                    }

                    if (this.ColumnIndex < this.Bounds.Width)
                    {
                        ++this.ColumnIndex;
                    }
                    else
                    {
                        ++this.RowIndex;
                        this.ColumnIndex = 1;
                    }

                    this.Current = this.Range.Cell(this.RowIndex, this.ColumnIndex).ResolveCellData();
                    return true;
                }
                else if (this.RowIndex == this.Bounds.Height)
                {
                    if (this.ColumnIndex < this.Bounds.Width)
                    {
                        ++this.ColumnIndex;
                        this.Current = this.Range.Cell(this.RowIndex, this.ColumnIndex).ResolveCellData();
                        return true;
                    }
                }

                return false;
            }

            public void Reset()
            {
                this.RowIndex = this.ColumnIndex = 0;
            }

            public void Dispose()
            {

            }
        }

        private readonly IXLRange Range;

        public string Name => this.Range?.RangeAddress.ToString();

        public IXLRange Element => this.Range;

        public SpreadsheetRange(IXLRange range)
        {
            this.Range = range;
        }

        public override string ToString()
        {
            return this.Range.RangeAddress.ToString();
        }

        public IEnumerator<CellData> GetEnumerator()
        {
            return new SpreadsheetRange.RangeEnumerator(this.Range);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}