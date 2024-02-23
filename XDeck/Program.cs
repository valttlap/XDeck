using BarRaider.SdTools;

namespace XDeck
{
    internal class Program
    {
        static void Main(string[] args)
        {
#if DEBUG
            System.Diagnostics.Debugger.Launch();
#endif
            SDWrapper.Run(args);
        }
    }
}
