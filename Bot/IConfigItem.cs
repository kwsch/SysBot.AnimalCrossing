using NHSE.Core;

namespace SysBot.AnimalCrossing
{
    public interface IConfigItem
    {
        bool WrapAllItems { get; }
        ItemWrappingPaper WrappingPaper { get; }
    }
}
