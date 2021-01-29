using NHSE.Core;

namespace SysBot.AnimalCrossing
{
    /// <summary>
    /// Tracks the state of the Drop Bot
    /// </summary>
    public class DropBotState
    {
        public int DropCount ;

        public void AfterDrop(int count)
        {
            DropCount += count;
        }
    }
}