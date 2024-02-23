using System;

namespace XDeck.Backend;

public sealed class XConnector
{
    private static readonly Lazy<XConnector> lazy = new Lazy<XConnector>(() => new XConnector());
    private static readonly object _lock = new();
    private readonly XPlaneConnector.XPlaneConnector _connector = new();

    public static XConnector Instance { get { return lazy.Value; } }

    private XConnector() { }

    public void Connect()
    {
        _connector.Start();
    }

    public void Disconnect()
    {
        _connector.Stop();
    }

    public void SendCommand(XPlaneConnector.XPlaneCommand command)
    {
        _connector.SendCommand(command);
    }

}
