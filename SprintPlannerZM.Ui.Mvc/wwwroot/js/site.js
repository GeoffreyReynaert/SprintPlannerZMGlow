window.onload = 
function () {
    const link = window.location.href;
    const pos = link.lastIndexOf("/");
    const pagina = link.substring(pos + 1, link.length);
    setStyle(pagina);

}

function setStyle(pagina) {
    switch (pagina) {
    case "AdminArea":
        var Index = document.getElementById("Index");
        Index.className += " sidebar-nav";
        break;
    case "LeerlingenOverzicht":
        var LeerlingenOverzicht = document.getElementById("LeerlingenOverzicht");
        LeerlingenOverzicht.className += " sidebar-nav";
        break;
    case "Klasverdeling":
        var Klasverdeling = document.getElementById("Klasverdeling");
        Klasverdeling.className += " sidebar-nav";
        break;
    case "Toezichters":
        var Toezichters = document.getElementById("Toezichters");
        Toezichters.className += " sidebar-nav";
        break;
    case "Overzichten":
        var Overzichten = document.getElementById("Overzichten");
        Overzichten.className += " sidebar-nav";
        break;
    }
}

$(function () {

    $("#importTitulEnKlas").on("click", getMessages);
    $("#ImportKlasVakLeerkrLeerlEnRelaties").on("click", getMessagesFromRelationImport);
    $("#beherenLeerlingen").on("click", getLeerlingen);

    //bij laden van de index
    //  $(document).ready(getPlayerInfo);
});

const getMessages = () => {
    $.ajax({
        type: "GET",
        url: 'ImportKlasTitularisEnKlas',
        async: true,
        success: function (response) {
            $("#berichtWeergaveDiv").html(response);
        }
    });
};

const getMessagesFromRelationImport = () => {
    $.ajax({
        type: "GET",
        url: 'ImportStudentklasLeerkrachtVak',
        success: function (response) {
            $("#berichtWeergaveDiv").html(response);
        }
    });
};

const getLeerlingen = () => {
    $.ajax({
        type: "GET",
        url: 'BeherenLeerling',
        success: function (response) {
            $("#beheerWeergave").html(response);
        }
    });
};

