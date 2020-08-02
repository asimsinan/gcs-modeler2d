angular.module("mainfuzzy")
    .controller("fuzzycontroller", function ($http, $scope, enums, $translate, $rootScope) {

        $scope.enums = enums;
        $scope.SetName = "";
        $scope.resistivityButtonCancel = false;
        $scope.resistivityButtonUpdate = false;
        $scope.resistivityButtonSave = true;
        $scope.groundButtonCancel = false;
        $scope.groundButtonUpdate = false;
        $scope.groundButtonSave = true;
        $scope.ruleButtonCancel = false;
        $scope.ruleButtonUpdate = false;
        $scope.ruleButtonSave = true;
        $scope.panelGround = false;
        $scope.panelRules = false;

        $scope.groundList = [];
        $scope.resistivityList = [{
            name: "Low",
            minValue: 0,
            maxValue: 30
        }, {
            name: "Medium",
            minValue: 30,
            maxValue: 50
        }, {
            name: "High",
            minValue: 50,
            maxValue: 70
        }, {
            name: "VeryHigh",
            minValue: 70,
            maxValue: 1000
        }];
        $scope.groundList = [{
            name: "Clay",
            minValue: 0,
            maxValue: 30
        }, {
            name: "Silt",
            minValue: 30,
            maxValue: 50
        }, {
            name: "Sand",
            minValue: 50,
            maxValue: 70
        }, {
            name: "Gravel",
            minValue: 70,
            maxValue: 1000
        }];

        $scope.ruleList = [];
        $scope.resultValues = [];
        $scope.preRuleList = [];

        $scope.Test = function () {
            $http.get('/Fuzzy/FuzzySet').then(function successCallback(response) {
                if (response.data.Success) {
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

        $scope.Add = function (item) {
            $scope.groundList.push({
                resistivity: item.resistivity,
                resistance: item.resistance,
                saturation: item.saturation
            });
        }

        $scope.Results = function (groundList) {
            $http.post('/Fuzzy/Results', groundList).then(function successCallback(response) {
                if (response.data.Success) {
                    $scope.resultValues = response.data.ResultObject;
                  
                }
                else {
                    $scope.errorMessages = [];
                    $scope.errorMessages.push(response.data.Message);
                }
            },
                function errorCallback(response) {
                });
        }

      
        $scope.AddResistivity = function (item) {
            $scope.resistivityList.push({
                name: item.name,
                minValue: item.minValue,
                maxValue: item.maxValue
            });
            $scope.resistivity = {};
        }

        $scope.UpdateResistivity = function (item) {
            $scope.resistivityList[item.$index] = item;
            $scope.resistivity = {};
            $scope.resistivityButtonCancel = false;
            $scope.resistivityButtonUpdate = false;
            $scope.resistivityButtonSave = true;
 
        }

        $scope.DeleteResistivity = function ($index) {
            $scope.resistivityList.splice($index, 1);
        }

        $scope.EditResistivity = function (item, $index) {
        
            $scope.resistivityButtonCancel = true;
            $scope.resistivityButtonUpdate = true;
            $scope.resistivityButtonSave = false;
            $scope.resistivity = angular.copy(item);
            $scope.resistivity.$index = $index;
          
        }
   
        $scope.CancelResistivity = function () {
            $scope.resistivity = {};
            $scope.resistivityButtonCancel = false;
            $scope.resistivityButtonUpdate = false;
            $scope.resistivityButtonSave = true;
        }

        $scope.SaveResistivity = function (resistivityList) {
            $scope.panelGround = true;
        }
     
        $scope.AddGround = function (item) {
            $scope.groundList.push({
                name: item.name,
                minValue: item.minValue,
                maxValue: item.maxValue
            });
            $scope.ground= {};
        }

        $scope.UpdateGround = function (item) {
            $scope.groundList[item.$index] = item;
            $scope.ground = {};
            $scope.groundButtonCancel = false;
            $scope.groundButtonUpdate = false;
            $scope.groundButtonSave = true;
        }

        $scope.DeleteGround = function ($index) {
            $scope.groundList.splice($index, 1);
        }

        $scope.EditGround = function (item, $index) {
            $scope.groundButtonCancel = true;
            $scope.groundButtonUpdate = true;
            $scope.groundButtonSave = false;
            $scope.ground = angular.copy(item);
            $scope.ground.$index = $index;
           
        }

        $scope.CancelGround = function () {
            $scope.ground = {};
            $scope.groundButtonCancel = false;
            $scope.groundButtonUpdate = false;
            $scope.groundButtonUpdate = true;
        }

        $scope.SaveGround = function (groundList) {
            $scope.panelRules = true;
        }
    
        $scope.AddRule = function (rule) {
            $scope.ruleList.push({
                text: "If The Resistivity Is " + rule.resistivity + " Then Ground Is " + rule.ground + ".",
                rule: rule
            });
            $scope.rule = {};
        }

        $scope.UpdateRule = function (rule) {
            debugger;
            var ruleItem = {
                text: "If The Resistivity Is " + rule.resistivity + " Then Ground Is " + rule.ground + ".",
                rule: rule
            };
            $scope.ruleList[rule.$index] = ruleItem;
            $scope.rule = {};
            $scope.ruleButtonCancel = false;
            $scope.ruleButtonUpdate = false;
            $scope.ruleButtonSave = true;
        }

        $scope.DeleteRule = function ($index) {
            $scope.ruleList.splice($index, 1);
        }

        $scope.EditRule = function (item, $index) {
            $scope.ruleButtonCancel = true;
            $scope.ruleButtonUpdate = true;
            $scope.ruleButtonSave = false;
            $scope.rule = angular.copy(item.rule);
            $scope.rule.$index = $index;
        }

        $scope.CancelRule = function () {
            $scope.rule = {};
            $scope.ruleButtonCancel = false;
            $scope.ruleButtonUpdate = false;
            $scope.ruleButtonSave = true;
        }

        $scope.SaveRule = function (ruleList) {
            $scope.panelRules = true;
        }

        $scope.AutomaticDefinition = function () {
            if ($scope.resistivityList.length == $scope.groundList.length) {
                for (var i = 0; i < $scope.resistivityList.length; i++) {
                    var rule = { resistivity: $scope.resistivityList[i].name, ground: $scope.groundList[i].name };
                    $scope.AddRule(rule);
                }
            }
        }
      

        $scope.SaveSet = function (ruleList) {
            
            var ruleSet = { SetName: $scope.SetName, RuleList: ruleList, ResistivityList: $scope.resistivityList, GroundList: $scope.groundList };
            console.log(ruleSet);
            $http.post('/Fuzzy/SaveSet', ruleSet).then(function successCallback(response) {
             
                if (response.data.Success) {
                    
                    $scope.resultValues = response.data.ResultObject;
                    window.location.href = '/Graph/Visualize';
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
