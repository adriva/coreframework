namespace Adriva.Extensions.Reporting.Abstractions
{
    public interface IDataDrivenObject
    {
        string DataSource { get; }

        string Command { get; }
    }
}