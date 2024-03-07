document.addEventListener('websocketCreate', function () {
    console.log("Websocket created!");
    populateSettings(actionInfo.payload.settings.settingsJson);

    websocket.addEventListener('message', function (event) {
        // Received message from Stream Deck
        var jsonObj = JSON.parse(event.data);

        if (jsonObj.event === 'sendToPropertyInspector') {
            var payload = jsonObj.payload;
            populateSettings(payload.settings.settingsJson);
        }
        else if (jsonObj.event === 'didReceiveSettings') {
            var payload = jsonObj.payload;
            populateSettings(payload.settings.settingsJson);
        }
    });
});

const originalSetSettingsToPlugin = setSettingsToPlugin;
setSettingsToPlugin = function (payload) {
    // Your override logic
    payload['settingsJson'] = createJSON()
    originalSetSettingsToPlugin(payload);
};

document.getElementById('addElementBtn').addEventListener('click', addElement);
document.getElementById('removeElementBtn').addEventListener('click', removeElement);

function addElement() {
    const container = document.getElementById('valueElements');
    const elementHTML = `
    <div class="dataref-value" style="margin-bottom: 1rem;">
        <div class="sdpi-item">
            <div class="sdpi-item-label">Value</div>
            <input class="sdpi-item-value value-input" type="number" oninput="setSettings()" />
        </div>
        <div class="sdpi-item">
            <div class="sdpi-item-label">Title</div>
            <textarea class="sdpi-item-value title-input" type="text" oninput="setSettings()"
                rows="4"></textarea>
        </div>
        <div class="sdpi-item">
            <div class="sdpi-item-label">Image</div>
            <input class="sdpi-item-value image-path-input" type="text" oninput="setSettings()" />
        </div>
    </div>
`;

    container.insertAdjacentHTML('beforeend', elementHTML);
}

function removeElement() {
    const container = document.getElementById('valueElements');
    if (container.children.length > 1) {
        container.removeChild(container.lastChild);
    }
}

function createJSON() {
    const elements = document.querySelectorAll('.dataref-value');
    const result = {};

    elements.forEach((element) => {
        const key = element.querySelector('.value-input').value; // Use the value as the key
        const title = element.querySelector('.title-input').value;
        const imagePath = element.querySelector('.image-path-input').value

        // Only add the element to the result if a key is provided
        if (key) {
            result[key] = {
                title,
                imagePath
            };
        }
    });

    console.log(JSON.stringify(result));
    return JSON.stringify(result);
}

function populateSettings(jsonString) {
    const settings = JSON.parse(jsonString);
    const container = document.getElementById('valueElements');

    // Ensure there are enough elements
    while (container.children.length < Object.keys(settings).length) {
        addElement();
    }

    // Populate the elements with settings
    Object.keys(settings).forEach((key, index) => {
        if (index < container.children.length) {
            const element = container.children[index];
            const valueInput = element.querySelector('.value-input');
            const titleInput = element.querySelector('.title-input');
            const imagePathInput = element.querySelector('.image-path-input');

            // Set the value input to match the key
            valueInput.value = key;
            // Populate title
            titleInput.value = settings[key].title;
            // Since the actual file cannot be set due to security reasons, set the file label to show the file name
            imagePathInput.value = settings[key].imagePath;
        }
    });
}


// Optionally, add a button or trigger to call createJSON when needed
