using System.Threading;
using System;
using XPlaneConnector;

public sealed class XConnector
{
    private static readonly Lazy<XConnector> _lazy = new Lazy<XConnector>(() => new XConnector());
    private XPlaneConnector.XPlaneConnector _connector;

    public static XConnector Instance { get { return _lazy.Value; } }

    private XConnector()
    {
        _connector = new XPlaneConnector.XPlaneConnector();
    }

    public void Connect()
    {
        if (_connector == null)
        {
            _connector = new XPlaneConnector.XPlaneConnector();
        }
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

    public CancellationTokenSource StartCommand(XPlaneConnector.XPlaneCommand command)
    {
        return _connector.StartCommand(command);
    }

    public XPlaneConnector.XPlaneConnector Connector
    {
        get
        { return _connector; }
    }

    public void SetDataRefValue(DataRefElement dataref, float value)
    {
        SetDataRefValue(dataref.DataRef, value);
    }

    /// <summary>
    /// Informs X-Plane to change the value of the DataRef
    /// </summary>
    /// <param name="dataref">DataRef that will be changed</param>
    /// <param name="value">New value of the DataRef</param>
    public void SetDataRefValue(string dataref, float value)
    {
        _connector.SetDataRefValue(dataref, value);
    }

}
