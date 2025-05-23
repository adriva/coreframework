using ClosedXML.Excel;

namespace Adriva.Documents.Excel
{
    internal static class XLExtensions
    {
        public static CellData ResolveCellData(this IXLCell cell)
        {
            if (null == cell || cell.IsEmpty() || cell.Value.IsBlank)
            {
                return new CellData(new CellReference(cell.Address.RowNumber - 1, cell.Address.ColumnNumber - 1, cell.Address.ToString()), null);
            }

            XLCellValue cellValue = cell.Value;
            object value = null;

            switch (cellValue.Type)
            {
                case XLDataType.Boolean:
                    value = cellValue.GetBoolean();
                    break;
                case XLDataType.DateTime:
                    value = cellValue.GetDateTime();
                    break;
                case XLDataType.Text:
                    value = cellValue.GetText();
                    break;
                case XLDataType.TimeSpan:
                    value = cellValue.GetTimeSpan();
                    break;
                case XLDataType.Number:
                    value = cellValue.GetNumber();
                    break;
                case XLDataType.Error:
                    value = cellValue.GetError();
                    break;
                default:
                    value = null;
                    break;
            }

            return new CellData(new CellReference(cell.Address.RowNumber - 1, cell.Address.ColumnNumber - 1, cell.Address.ToString()), value);
        }
    }
}