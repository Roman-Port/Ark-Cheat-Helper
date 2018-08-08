
function BeginShow(backgroundImg) {
    //Called by native
    //Set the image to the background image and run the animation.
    document.getElementById('blur-bg').src = backgroundImg;
    //Play animation to open.
    document.getElementById('main').className = "main";
    //Refresh table
    RefreshTable();
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


function RefreshTable() {
    var ele = document.getElementById('dino_list');
    //Request new HTML. 
    CallNative("requestUpdatedDinoTable", "");
    //Will continue below.
}

function FinishRefreshTable(html) {
    //Called by native when done
    var ele = document.getElementById('dino_list');
    ele.innerHTML = atob(html);
}

var currentView = 0; //Dinos for 0, items for 1

function ShiftMenu() {
    var menu = document.getElementById('menu-shifter');
    if (currentView == 0) {
        currentView = 1;
        menu.className = "dino_list_search_title dino_list_search_title_down";
    } else {
        currentView = 0;
        menu.className = "dino_list_search_title";
    }
    
}