angular.module("mainfuzzy")
    .controller("indexcontroller", function ($http, $scope) {
        $scope.Selam = function () {
            $http.get('/Home/Save').then(function successCallback(response) {
                if (response.data.Success) {
                        debugger;
                        alert(response.data);
                     
                    }
                    else {
                        $scope.errorMessages = [];
                        $scope.errorMessages.push(response.data.Message);
                    }
                },
                    function errorCallback(response) {
                    });
           
        }
    });
