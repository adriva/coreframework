namespace Adriva.Extensions.Reporting.Abstractions
{
    public struct ReportDefinitionFile
    {
        public string Name { get; set; }

        public string Path { get; set; }

        public string Base { get; set; }

        public override string ToString()
        {
            return $"ReportDefinitionFile, '{this.Name}'";
        }
    }
}
