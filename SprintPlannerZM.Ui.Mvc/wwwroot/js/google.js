window.onload = function () {
    var GoogleUser = {}
    gapi.load('auth2', function () {
        var auth2 = gapi.auth2.init({
            client_id: '300704695807-qqhvmrt48k8v91lbob96g0u277c5clei.apps.googleusercontent.com',
            cookiepolicy: 'single_host_origin',
            scope: 'profile'
        });

        auth2.attachClickHandler(document.getElementById('googleSignup'), {},
            function (googleUser) {
                var profile = googleUser.getBasicProfile();
                console.log("ID: " + profile.getId());
                console.log('Full Name: ' + profile.getName());
                console.log('Given Name: ' + profile.getGivenName());
                console.log('Family Name: ' + profile.getFamilyName());
                console.log("Image URL: " + profile.getImageUrl());
                console.log("Email: " + profile.getEmail());

                var id_token = googleUser.getAuthResponse().id_token;
                console.log("ID Token: " + id_token);
            }, function (error) {
                console.log('Sign-in error', error);
            }
        );
    });
}