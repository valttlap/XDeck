document.addEventListener('websocketCreate', function () {
    console.log("Websocket created!");
    showHideSettings(actionInfo.payload.settings);

    websocket.addEventListener('message', function (event) {
        console.log("Got message event!");

        // Received message from Stream Deck
        var jsonObj = JSON.parse(event.data);

        if (jsonObj.event === 'sendToPropertyInspector') {
            var payload = jsonObj.payload;
            showHideSettings(payload);
        }
        else if (jsonObj.event === 'didReceiveSettings') {
            var payload = jsonObj.payload;
            showHideSettings(payload.settings);
        }
    });
});

function showHideSettings(payload) {
    console.log("Show Hide Settings Called");

    const displayValue = value => value ? "" : "none";

    setIncreaseRefSettings(displayValue(payload.modeIncrease));
    setDecreaseRefSettings(displayValue(payload.modeDecrease));
    setSetRefSettings(displayValue(payload.modeSet));
}

function setIncreaseRefSettings(displayValue) {
    var increaseRefSettings = document.getElementById('increaseRefSettings');
    increaseRefSettings.style.display = displayValue;
}

function setDecreaseRefSettings(displayValue) {
    var decreaseRefSettings = document.getElementById('decreaseRefSettings');
    decreaseRefSettings.style.display = displayValue;
}

function setSetRefSettings(displayValue) {
    var setRefSettings = document.getElementById('setRefSettings');
    setRefSettings.style.display = displayValue;
}
