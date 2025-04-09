using System;
using ClosedXML.Excel;

namespace Adriva.Documents.Excel
{
    public readonly struct CellReference
    {
        public static CellReference Empty { get; } = new CellReference(-1, -1, string.Empty);

        public int RowIndex { get; }

        public int ColumnIndex { get; }

        public string Address { get; }

        public bool IsValid => XLHelper.IsValidA1Address(this.Address);

        public CellReference(int rowIndex, int columnIndex, string address)
        {
            this.RowIndex = rowIndex;
            this.ColumnIndex = columnIndex;
            this.Address = address;
        }

        public static implicit operator CellReference(string address)
        {
            if (!XLHelper.IsValidA1Address(address))
            {
                throw new ArgumentException($"Invalid spreadsheet address. ({address})");
            }

            int loop = address.Length - 1;

            while (char.IsDigit(address[loop]))
            {
                --loop;
            }

            int columnIndex = XLHelper.GetColumnNumberFromAddress(address);

            if (!int.TryParse(address.Substring(1 + loop), out int rowIndex))
            {
                throw new ArgumentException("Invalid row index.");
            }

            return new CellReference(rowIndex - 1, columnIndex - 1, address);
        }

        public override string ToString()
        {
            return this.Address;
        }
    }
}