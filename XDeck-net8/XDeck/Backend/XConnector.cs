using System.Net;
using XPlaneConnector.Core;

namespace XDeck.Backend;

public sealed class XConnector
{
    private static readonly Lazy<XConnector> _lazy = new Lazy<XConnector>(() => new XConnector());
    public static XConnector Instance => _lazy.Value;
    private CancellationTokenSource? _cts;
    private Connector? _connector;
    public delegate void ConnectionStarted();
    public event ConnectionStarted? OnConnectionStarted;
    public delegate void XPlaneOnline();
    public event XPlaneOnline? OnXPlaneOnline;
    private IPAddress? _address;
    private int _port;
    public bool IsXPlaneOnline { get => _address is not null && _address != IPAddress.None; }
    public bool HasConnection { get => _connector is not null; }

    private XConnector()
    {
        OnXPlaneOnline += Connect;
    }

    public void Init()
    {
        _cts = new CancellationTokenSource();
        var token = _cts.Token;
        Task.Run(async () =>
        {
            var (address, port) = await BeaconListener.GetXPlaneClientAddressAsync(token);
            if (address == IPAddress.None) return;
            _address = address;
            _port = port;
            OnXPlaneOnline?.Invoke();
        }, token);
    }

    public void Connect()
    {
        if (_address is null || _address == IPAddress.None) return;
        if (_port == 0) return;
        _connector = new Connector(_address.ToString(), _port);
        _connector.ServerFailed += Restart;
        _connector.Start();
        OnConnectionStarted?.Invoke();
    }

    public void Stop()
    {
        if (_connector is not null)
        {
            _connector.ServerFailed -= Restart;
        }
        _cts?.Cancel();
        _connector?.Stop();
        _connector?.Dispose();
        _connector = null;
        _address = IPAddress.None;
        _port = 0;
    }

    public void Restart()
    {
        Stop();
        Init();
    }
    private void Restart(Exception? e)
    {
        Stop();
        Init();
    }

    public void SendCommand(XPlaneCommand command)
    {
        if (_connector is null) return;
        _connector.SendCommand(command);
    }

    public CancellationTokenSource StartCommand(XPlaneCommand command)
    {
        if (_connector is null) return new CancellationTokenSource();
        return _connector.StartCommand(command);
    }

    public void SetDataRefValue(DataRefElement dataRef, float value)
    {
        if (_connector is null) return;
        _connector.SetDataRefValue(dataRef, value);
    }

    public void SetDataRefValue(string dataRef, float value)
    {
        if (_connector is null) return;
        _connector.SetDataRefValue(dataRef, value);
    }

    public void Subscribe(DataRefElement dataRef, Action<DataRefElement, float> callback)
    {
        if (_connector is null) return;
        _connector.Subscribe(dataRef, dataRef.Frequency, callback);
    }

    public void Unsubscribe(DataRefElement dataRef)
    {
        if (_connector is null) return;
        _connector.Unsubscribe(dataRef.DataRef);
    }
    public void Unsubscribe(string dataRef)
    {
        if (_connector is null) return;
        _connector.Unsubscribe(dataRef);
    }
}
