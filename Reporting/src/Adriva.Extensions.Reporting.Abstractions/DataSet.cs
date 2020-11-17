using System.Collections.Generic;
using System.Linq;

namespace Adriva.Extensions.Reporting.Abstractions
{
    public class DataSet
    {
        private readonly IList<DataColumn> DataColumns = new List<DataColumn>(8);
        private readonly IList<DataRow> DataRows = new List<DataRow>(64);

        public IEnumerable<DataColumn> Columns => this.DataColumns.AsEnumerable();
        public IEnumerable<DataRow> Rows => this.DataRows.AsEnumerable();


    }
}