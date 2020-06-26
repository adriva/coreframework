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
        Inline = 0,
        OptimizationContext = 1,
        SectionWriter = 2
    }
}