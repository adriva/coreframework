namespace Adriva.Extensions.Reporting.Abstractions
{
    public enum FilterProperties : long
    {
        Default = 0,
        Constant = 1,
        Context = 2,
        Required = 3
    }

    public enum FieldProperties : long
    {
        None = 0,
        KeyField = 1,
        DisplayField = 2
    }
}