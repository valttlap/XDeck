using System.Net;
using XPlaneConnector.Core;

namespace XDeck.Backend;

public sealed class XConnector
{
    private static readonly Lazy<XConnector> Lazy = new(() => new XConnector());
    public static XConnector Instance => Lazy.Value;

    private CancellationTokenSource? _cts;
    private Connector? _connector;
    private IPEndPoint? _xPlaneEndPoint;

    public delegate void ConnectionStarted();
    public event ConnectionStarted? OnConnectionStarted;

    public delegate void ConnectionLost();
    public event ConnectionLost? OnConnectionLost;

    public delegate void XPlaneOnline();
    public event XPlaneOnline? OnXPlaneOnline;

    // Returns true if we have discovered an X-Plane IP/port
    public bool IsXPlaneOnline => 
        _xPlaneEndPoint is { Address: { } addr } &&
        addr != IPAddress.None &&
        _xPlaneEndPoint.Port != 0;

    // Returns true if connector is created and started
    public bool HasConnection => _connector is not null;

    private XConnector()
    {
        // By default, when XPlane is determined online, we automatically Connect.
        OnXPlaneOnline += Connect;
    }

    /// <summary>
    /// Start listening for the X-Plane beacon in a background task.
    /// When found, we store the discovered IP and port, and raise OnXPlaneOnline.
    /// </summary>
    public void Init()
    {
        Stop();

        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        Task.Run(async () =>
        {
            var endPoint = await BeaconListener.GetXPlaneClientAddressAsync(token);
            if (endPoint.Address == IPAddress.None)
                return; // Not found or was canceled

            // Store discovered IP + port
            _xPlaneEndPoint = endPoint;

            // Raise the event
            OnXPlaneOnline?.Invoke();

        }, token);
    }

    /// <summary>
    /// Connect using the discovered XPlane IP/port (if any).
    /// This is automatically called when OnXPlaneOnline is raised.
    /// </summary>
    public void Connect()
    {
        // If we have no discovered IP, do nothing
        if (!IsXPlaneOnline) return;

        // Actually create the connector now
        _connector = new Connector(_xPlaneEndPoint!);
        _connector.ServerFailed += Restart;  // if the server fails, call Restart
        _connector.Start();

        OnConnectionStarted?.Invoke();
    }

    public void Stop()
    {
        // If we had an active connector, detach the event and stop
        if (_connector != null)
        {
            _connector.ServerFailed -= Restart;
            _connector.Stop();
            _connector.Dispose();
            _connector = null;
        }

        // Cancel the beacon listener
        _cts?.Cancel();
        _cts = null;

        // We consider the connection lost, and reset the saved IP
        OnConnectionLost?.Invoke();
        _xPlaneEndPoint = new IPEndPoint(IPAddress.None, 0);
    }

    /// <summary>
    /// Restarts the entire process: stops current connector, re-listens for beacon
    /// </summary>
    public void Restart()
    {
        Stop();
        Init();
    }

    /// <summary>
    /// Overload for ServerFailed event, ignoring the Exception parameter.
    /// </summary>
    /// <param name="e">Exception from the connector</param>
    private void Restart(Exception? e)
    {
        Stop();
        Init();
    }

    public void SendCommand(XPlaneCommand command)
    {
        _connector?.SendCommand(command);
    }

    public CancellationTokenSource StartCommand(XPlaneCommand command)
    {
        return _connector is null 
            ? new CancellationTokenSource()
            : _connector.StartCommand(command);
    }

    public void SetDataRefValue(DataRefElement dataRef, float value)
    {
        _connector?.SetDataRefValue(dataRef, value);
    }

    public void SetDataRefValue(string dataRef, float value)
    {
        _connector?.SetDataRefValue(dataRef, value);
    }

    public void Subscribe(DataRefElement dataRef, Action<DataRefElement, float> callback)
    {
        _connector?.Subscribe(dataRef, dataRef.Frequency, callback);
    }

    public void Unsubscribe(DataRefElement dataRef)
    {
        _connector?.Unsubscribe(dataRef.DataRef);
    }

    public void Unsubscribe(string dataRef)
    {
        _connector?.Unsubscribe(dataRef);
    }
}
