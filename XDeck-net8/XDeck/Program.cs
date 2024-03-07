using BarRaider.SdTools;
using XDeck.Backend;

namespace XDeck;
internal class Program
{
    private static void Main(string[] args)
    {
        var connector = XConnector.Instance;
        connector.Init();
        SDWrapper.Run(args);
    }
}
