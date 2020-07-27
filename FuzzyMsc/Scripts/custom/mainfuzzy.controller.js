angular.module("mainfuzzy", ['ngFileUpload', 'pascalprecht.translate', 'ngCookies', 'oitozero.ngSweetAlert'])
    .config(['$translateProvider', function ($translateProvider) {
        // add translation tables
        $translateProvider.translations('en', translationsEN);
        $translateProvider.translations('tr', translationsTR);
        $translateProvider.preferredLanguage('en');
        // remember language
        $translateProvider.useLocalStorage();
    }])
    //.run(function ($rootScope) {
    //    //$rootScope.globalFoo = function () {
    //    //    alert("I'm global foo!");
    //    //};
    //    $rootScope.preferredLanguage = "tr";
    //    $rootScope.setLanguage = function (langKey) {
    //        //$translate.use(langKey);
    //        $rootScope.preferredLanguage = langKey;
    //        console.log("$rootScope.preferredLanguage", $rootScope.preferredLanguage);
    //    };
    //})

    .run(appRun);
appRun.$inject = ['$rootScope', '$translate'];

function appRun($rootScope, $translate) {
    //console.log("$translateProvider", $translateProvider);
    
    //console.log("$translate.proposedLanguage()", $translate.proposedLanguage());
    //console.log("$translate.preferredLanguage()", $translate.preferredLanguage());
    //console.log("$translate.use()", $translate.use());

    //$rootScope.defaultLanguage = "tr";
    //if ($rootScope.preferredLanguage == undefined) {
    //    $rootScope.preferredLanguage = $rootScope.defaultLanguage;
    //}

    //$rootScope.preferredLanguage = $translate.preferredLanguage();
    $rootScope.preferredLanguage = $translate.use();
    
    //console.log("$rootScope.preferredLanguage", $rootScope.preferredLanguage)

    $rootScope.setLanguage = function (langKey) {
        //$translate.use(langKey);
        $rootScope.preferredLanguage = langKey;
        //console.log("$rootScope.preferredLanguage", $rootScope.preferredLanguage);
        $rootScope.changeLanguage($rootScope.preferredLanguage)
    };

    $rootScope.changeLanguage = function (langKey) {
        $translate.use(langKey);
    };

}

;
