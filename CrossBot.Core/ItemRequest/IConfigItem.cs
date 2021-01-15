using NHSE.Core;

namespace CrossBot.Core
{
    public interface IConfigItem
    {
        bool WrapAllItems { get; }
        ItemWrappingPaper WrappingPaper { get; }
    }
}
