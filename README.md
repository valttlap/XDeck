# XDeck

XDeck is an Elgato Stream Deck plugin that integrates with X-Plane 11 and X-Plane 12, allowing users to interact with the simulator through Stream Deck. It uses X-Plane’s DataRefs to send commands and receive status updates in real time.

The plugin enables users to control various aircraft functions directly from the Stream Deck, with buttons reflecting the current state of the simulator. This provides a more interactive way to manage flight controls without relying on keyboard shortcuts or mouse inputs

## Features

### Start connection

Starts listening for X-Plane DataRefs. By default, the plugin begins listening when started, so this action is only needed if the connection has been explicitly stopped using the [**Stop Connection**](#stop-connection) action.

### Stop connection

Stops listening for X-Plane DataRefs and terminates the connection to X-Plan.

### Send command

The **Send Command** feature allows the plugin to send a basic **X-Plane command DataRef**. This is the simplest function of the plugin, used to trigger specific actions in the simulator.

To configure this feature, enter the desired DataRef command in the `Command` input field. For example, entering `sim/flight_controls/flaps_down` will send the command to lower the aircraft's flaps.

### Press&Hold command

The **Press & Hold Command** feature functions similarly to **Send Command**, but is designed for actions that require the button to be held down, such as an ignition command.

Configuration is done in the same way as the **Send Command** feature by specifying the appropriate DataRef command in the `Command` input field.

### Press&hold command loop

The **Press & Hold Command Loop** feature sends the same command repeatedly while the button is held down. This can be used for actions such as simulating rotary knob adjustments in the simulator.

**Settings:**

* Command: The name of the X-Plane DataRef command to be sent.
* Wait Time (ms): The duration the button must be held before the repetition loop starts.
* Loop Time (ms): The interval between each repeated command while the button remains pressed.

### 3-way listener

The **3-Way Listener** feature monitors the status of **3-way switches** in X-Plane. It is useful for tracking switches with three distinct positions, such as the **nose wheel light** switch in the Airbus A321, where the DataRef `AirbusFBW/OHPLightSwitches[3]` provides the current state:

* 0 – Nose wheel light off
* 1 – Nose wheel taxi light
* 2 – Nose wheel takeoff light

The 3-Way Listener supports three display modes:

* **Title Mode** – Displays a user-specified title for each DataRef value (0, 1, 2).
* **Image Mode** – Displays a user-specified image for each DataRef value (0, 1, 2).
* **Both Mode** – Displays both an image and a title for each DataRef value (0, 1, 2).

**Settings**:

* **DataRef**: The X-Plane DataRef to monitor.
* **Polling Frequency (Hz)**: How often the current DataRef value is checked per second. Higher values increase responsiveness but may impact performance.
* **Mode**:
  * **Title** – Specify a title for each DataRef value.
  * **Image** – Provide an image path for each DataRef value.
  * **Both** – Specify both a title and an image path for each DataRef value.

### Listen dataref

The **Listen DataRef** feature monitors a specified X-Plane DataRef and displays its current value on the button. An optional unit can be configured to be shown alongside the value.

**Settings**:

* **DataRef**: The X-Plane DataRef to monitor.
* **Polling Frequency (Hz)**: How often the current DataRef value is checked per second. Higher values increase responsiveness but may impact performance.
* **Dataref Unit (optional)**: An optional unit to display alongside the DataRef value.

### Switch

The **Switch** feature functions similarly to the [**3-Way Listener**](#3-way-listener) but is designed for **two-state switches**. In addition to monitoring the switch state, it allows users to toggle the switch by pressing the button.

There are two modes for modifying the switch state:

* **DataRef Mode** – Pressing the button changes the monitored DataRef value between `0` and `1`, depending on the current state.
* **Command Mode** – Pressing the button sends a predefined DataRef command to perform the switch action.

**Settings**:

* **DataRef**: The X-Plane DataRef to monitor.
* **Polling Frequency (Hz)**: How often the current DataRef value is checked per second. Higher values increase responsiveness but may impact performance.
* **Mode**
  * **Dataref** – Toggle the DataRef value between `0` and `1` when pressed.
  * **Command** – Send a specified DataRef command when pressed
    * **Command**: The DataRef command to send.
* **Mode**:
  * **Title** – Specify a title for ON (value 1) and OFF (value 0) DataRef value.
  * **Image** – Provide an image path for ON (value 1) and OFF (value 0) DataRef value.
  * **Both** – Specify both a title and an image path for ON (value 1) and OFF (value 0) DataRef value.

### Modify dataref

The **Modify DataRef** feature allows modifying a specified X-Plane DataRef in three different ways:

* **Increase DataRef** – Increments the DataRef value by `1` when the button is pressed.
* **Decrease DataRef** – Decreases the DataRef value by `1` when the button is pressed.
* **Set DataRef** – Sets the DataRef to a specified value when the button is pressed.

**Settings**:

* **DataRef**: The X-Plane DataRef to monitor.
* **Polling Frequency (Hz)**: How often the current DataRef value is checked per second. Higher values increase responsiveness but may impact performance
* **Mode**
  * **Increase** – Increments the DataRef value by `1`.
    * **Max dataref value** – The maximum allowed value for the DataRef.
  * **Decrease** - Decrements the DataRef value by `1`
    * **Min dataref value** – The minimum allowed value for the DataRef.
  * **Set** – Sets the DataRef to a specific value.
    * **Value to set** – The value to assign to the DataRef when the button is pressed.

### Listen multiple datarefs:

The **Listen multiple datarefs** feature extends the functionality of the  [**3-Way Listener**](#3-way-listener) by allowing users to define titles, images, or both for multiple DataRef values.

**Settings**:

* **DataRef**: The X-Plane DataRef to monitor.
* **Polling Frequency (Hz)**: How often the current DataRef value is checked per second. Higher values increase responsiveness but may impact performance
* **Value** - The specific DataRef value for which a title and/or image is assigned.
* **Title** - The title to display for the specified DataRef value.
* **Image** - The path to a image to display for the specified DataRef value.

**Controls**:

* **Add Element** – Adds a new **Value, Title, Image** block.
* **Remove Element**– Removes the last **Value, Title, Image** block.