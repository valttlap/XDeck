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
    setTitleSettings(displayValue(payload.modeTitle || payload.modeTitleImage));
    setImageSettings(displayValue(payload.modeImage || payload.modeTitleImage));
}

function setTitleSettings(displayValue) {
    var titleSettings = document.getElementById('titleSettings');
    titleSettings.style.display = displayValue;
}

function setImageSettings(displayValue) {
    var imageSettings = document.getElementById('imageSettings');
    imageSettings.style.display = displayValue;
}
