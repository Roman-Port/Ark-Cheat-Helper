
function BeginShow(backgroundImg) {
    //Called by native
    //Play animation to open.
    document.getElementById('main').className = "main";
    //Refresh table
    //RefreshTable();
    //Allow change soon
    window.setTimeout(function () {
        CallNative("setAllowedWindowToggle", "true");
    }, 150);
}

function BeginHide() {
    //Called by native
    //Play animation to close.
    document.getElementById('main').className = "main main_hidden";
    //Allow change soon
    window.setTimeout(function () {
        CallNative("setAllowedWindowToggle", "true");
    }, 150);
}


function RefreshTable(search) {
    var ele = document.getElementById('dino_list');
    //Request new HTML. 
    CallNative("requestUpdatedDinoTable", search);
    //Will continue below.
}

//Refresh now
RefreshTable('');

function FinishRefreshTable(html) {
    //Called by native when done
    var ele = document.getElementById('dino_list');
    ele.innerHTML = atob(html);
}

var readyDinoData = null;

function SpawnCharacter(rawData) {
    //"Spawn" button pressed. Open up a dialog to choose options.
    var dinoData = JSON.parse(atob(rawData));
    //Open dialog box.
    readyDinoData = dinoData;
    //Set creature name
    document.getElementById('dino-spawn-name').innerText = dinoData.name;
    //Show
    document.getElementById('dino-spawn-window').className = "mdl-rp-dialog";
    document.getElementById('darkout-dino-spawn').className = "darkout_layer";
}

function HideSpawnDinoWindow() {
    document.getElementById('dino-spawn-window').className = "mdl-rp-dialog mdl-rp-dialog-hidden";
    document.getElementById('darkout-dino-spawn').className = "darkout_layer darkout_layer_hidden";
}

function SpawnDinoWindowConfirm() {
    var id = readyDinoData.id;
    var count = parseInt(document.getElementById('creature-count').value);
    var tamedLevel = document.getElementById('creature-lvl').value;
    var entityLocationId = id.substring(0, id.length - 2);
    var entityLocation = "\"Blueprint'/Game/PrimalEarth/Dinos/" + readyDinoData.name+"/" + entityLocationId + "." + entityLocationId + "'\"";
    var spawnWithTamed = document.getElementById('tamed-switch').checked;
    var spawnWithLevel = document.getElementById('lvl-switch').checked;
    //Decide on what command to run
    var cmd = "";
    if (spawnWithLevel && !spawnWithTamed) {
        //Spawn with level
        cmd = "admincheat SpawnDino " + entityLocation + " 500 0 0 " + tamedLevel;
    } else if (!spawnWithLevel && spawnWithTamed) {
        //Spawn tamed
        cmd = "admincheat SummonTamed " + id;
    } else {
        //Spawn normal
        cmd = "admincheat summon " + id;
    }
    
    //Run the command for each time.
    var output = "";
    for (var i = 0; i < count; i += 1) {
        output += cmd;
        if (i != count - 1) {
            output += "|";
        }
    }
    RunArkCommand(output);

    //Hide the window.
    HideSpawnDinoWindow();
}

function DinoTableBtn() {
    //Load up the dino table area.
    //Hide all dynamic content
    $('.dynamic_content').css("display", "none");
    //Show dino table
    $('#dynamic_dinotable').css("display", "block");
    //Load

    //Hide menu
    ToggleMenu();
}

function ToggleMenu() {
    $(".mdl-layout__drawer-button").click();
}

function RunArkCommand(cmd) {
    var encode = btoa(cmd);
    CallNative("sendArkCommand", encode);
}

function SendSnackbar(title, time) {
    var data = {
        message: title,
        timeout: time
    };
    document.querySelector('#demo-snackbar-example').MaterialSnackbar.showSnackbar(data);
}

function ExitApp() {
    //TODO
}

function ShowArkNotInForegroundWarning() {
    var data = {
        message: 'Ark must be in the foreground!',
        timeout: 8000,
        actionHandler: function () {
            ExitApp();
        },
        actionText: 'Exit'
    };
    document.querySelector('#demo-snackbar-example').MaterialSnackbar.showSnackbar(data);
}
