namespace Adriva.Documents.Excel
{
    public readonly struct CellData
    {
        public static CellData Empty { get; } = new CellData(CellReference.Empty, null);

        public CellReference Cell { get; }

        public object Value { get; }

        public CellData(CellReference cellReference, object value)
        {
            this.Cell = cellReference;
            this.Value = value;
        }

        public override string ToString()
        {
            return $"{this.Value ?? "<NULL>"} ({this.Cell.Address})";
        }
    }
}