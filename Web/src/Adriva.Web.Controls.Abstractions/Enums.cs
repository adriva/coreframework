using System;

namespace Adriva.Web.Controls.Abstractions
{
    public enum HorizontalAlignment
    {
        Left,
        Center,
        Right
    }

    [Flags]
    public enum AssetDeliveryMethod : int
    {
        Unspecified = 0,
        InlineTag = 1,
        OptimizationContext = 2,
        SectionWriterTag = 3
    }
}