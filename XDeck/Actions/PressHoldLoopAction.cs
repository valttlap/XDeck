using BarRaider.SdTools;

using XDeck.Models;

using XPlaneConnector.Core;

namespace XDeck.Actions;

[PluginActionId("com.valtteri.pressholdloop")]
public class PressHoldLoopAction(SDConnection connection, InitialPayload payload) : CommandActionBase<PressHoldLoopSettings>(connection, payload)
{
    private bool _isPressed = false;

    public override async void KeyPressed(KeyPayload payload)
    {
        if (_settings == null) return;
        _isPressed = true;
        var command = new XPlaneCommand(_settings.Command, "Userdefined command");
        _connector.SendCommand(command);
        await ButtonPressing(command);

    }

    public override void KeyReleased(KeyPayload payload)
    {
        _isPressed = false;
    }

    private async Task ButtonPressing(XPlaneCommand command)
    {
        if (_settings == null) return;
        await Task.Delay(_settings.WaitTime); // 0.5 sec delay before the loop starts

        if (!_isPressed) // if button is released before 0.5 sec
        {
            return;
        }

        var timer = new System.Timers.Timer(_settings.LoopTime);
        timer.Elapsed += (sender, e) => _connector.SendCommand(command);
        timer.Start();

        await Task.Run(() =>
        {
            while (_isPressed)
            {
                Thread.Sleep(_settings.LoopTime);
            }

            timer.Stop();
        });
    }

}