angular.module("mainfuzzy")
  .controller("visualizationcontroller", function ($http, $scope, enums, Upload, $timeout, $translate, $rootScope) {

    $scope.setList = [];
    $scope.panelExcelSec = false;
    $scope.panelSettings = false;
    $scope.panelGraphic = false;
    $scope.scale = { x: null, y: null };
    $scope.excel = {};
    $scope.excelError = false;
    $scope.count = {};
    $scope.visualizationInfo = [];
    $scope.parameters = {
      Title: "Enter Title Here",
      Subtitle: "Enter Subtitle Here",
      IsChecked: true,
      ResolutionX: 2500,
      ResolutionY: 1000,
      ScaleX: 10,
      ScaleY: 2,
      SeismicRatio: 70,
      ResistivityRatio: 40
    };
    $scope.defaultParameters = {
      Title: "Enter Title Here",
      Subtitle: "Enter Subtitle Here",
      IsChecked: true,
      ResolutionX: 2500,
      ResolutionY: 1000,
      ScaleX: 10,
      ScaleY: 2,
      SeismicRatio: 70,
      ResistivityRatio: 40
    };


    $scope.FetchSetList = function () {
      $http.get('/Graph/FetchSetList').then(function successCallback(response) {
        if (response.data.Success) {
          $scope.setList = response.data.ResultObject;
        }
        else {
          $scope.errorMessages = [];
          $scope.errorMessages.push(response.data.Message);
        }
      },
        function errorCallback(response) {
        });
    }

    $scope.FetchSetList();

    $scope.FetchRule = function (rule) {
      if (rule.RuleID == null || rule.RuleID == undefined) {
        $scope.panelSelectFile = false;
      }
      else {
        $scope.ruleID = rule.RuleID;
        $http.get('/Graph/FetchRule', { params: { ruleID: $scope.ruleID } }).then(function successCallback(response) {
          if (response.data.Success) {
            $scope.ruleList = response.data.ResultObject;
            $scope.panelExcelSec = true;
            $scope.panelSettings = false;
            $scope.panelGraphic = false;
            $scope.scale = { x: null, y: null };
            $scope.excel = {};
          }
          else {
            $scope.errorMessages = [];
            $scope.errorMessages.push(response.data.Message);
          }
        },
          function errorCallback(response) {
          });
      }
    }

    $scope.FetchRuleTextAndResistivity = function (rule) {
      if (rule.RuleID == null || rule.RuleID == undefined) {
        return null;
      }
      else {
        $scope.ruleID = rule.RuleID;
        $http.get('/Graph/FetchRuleTextAndResistivity', { params: { ruleID: $scope.ruleID } }).then(function successCallback(response) {
          if (response.data.Success) {
            $scope.ruleTextList = response.data.ResultObject.ruleTextList;
            $scope.resistivityList = response.data.ResultObject.resistivityList;
          }
          else {
            $scope.errorMessages = [];
            $scope.errorMessages.push(response.data.Message);
          }
        },
          function errorCallback(response) {
          });
      }
    }

    $scope.CheckExcel = function (excel) {
      $scope.panelExcelSec = true;
      $scope.scale = { x: null, y: null };
      $http.post('/Graph/CheckExcel', $scope.excel).then(function successCallback(response) {
        if (response.data.Success) {
          $scope.panelSettings = true;
          $scope.panelGraphic = false;
          $scope.parameters = {
            Title: "Enter Title Here",
            Subtitle: "Enter Subtitle Here",
            IsChecked: true,
            ResolutionX: 2500,
            ResolutionY: 1000,
            ScaleX: 10,
            ScaleY: 2,
            SeismicRatio: 70,
            ResistivityRatio: 40
          };
          $scope.excelError = false;
        }
        else {
          $scope.excelError = true;
          $scope.errorMessages = [];
          $scope.errorMessages.push(response.data.Message);
        }
      },
        function errorCallback(response) {
        });
    }
    $scope.DefaultSettings = function () {
      $scope.parameters = {
        Title: "Enter Title Here",
        Subtitle: "Enter Subtitle Here",
        IsChecked: true,
        ResolutionX: 2500,
        ResolutionY: 1000,
        ScaleX: 10,
        ScaleY: 2,
        SeismicRatio: 70,
        ResistivityRatio: 40
      };
    }
    $scope.Clear = function () {
      $scope.parameters = {
        Title: "",
        Subtitle: "",
        IsChecked: true,
        ResolutionX: 2500,
        ResolutionY: 1000,
        ScaleX: 10,
        ScaleY: 2,
        SeismicRatio: 70,
        ResistivityRatio: 40
      };
    }

    $scope.VisualizationSettings = function (excel, parameters, count) {
      var graph = { excel: $scope.excel, ruleID: $scope.ruleID, scale: $scope.scale, parameters: parameters, count: count };
      $http.post('/Graph/Visualize', graph).then(function successCallback(response) {
        if (response.data.Success) {
          $scope.resultValues = response.data.ResultObject;
          $scope.panelGraphic = true;
          $scope.count = $scope.resultValues.count;
          $scope.visualizationInfo = $scope.resultValues.visualizationInfo;
          $scope.Visualize($scope.resultValues);
          console.log("$scope.sonucDegerleri", $scope.resultValues);
        }
        else {
          console.log("response.data", response.data);
        }
      },
        function errorCallback(response) {
        });
    }

    $scope.uploadFiles = function (file, errFiles) {
      $scope.f = file;
      $scope.errFile = errFiles && errFiles[0];
      if (file) {
        file.upload = Upload.upload({
          url: '/Graph/UploadExcel',
          data: { file: file }
        });

        file.upload.then(function (response) {
          $timeout(function () {
            $scope.excel = { name: $scope.f.name, data: response.data.ResultObject.data, path: response.data.ResultObject.path };
          });
        }, function (response) {
          if (response.status > 0)
            $scope.errorMsg = response.status + ': ' + response.data;
        }, function (evt) {
          file.progress = Math.min(100, parseInt(100.0 *
            evt.loaded / evt.total));
        });
        $scope.excelError = false;
      }

    }

    $scope.Visualize = function (chart) {
      Highcharts.chart('container', {
        chart: {
          type: 'spline',
          zoomType: 'xy',
          panning: true,
          panKey: 'shift'
        },
        title: {
          text: chart.parameters.Title
        },
        subtitle: {
          text: 'Enter Subtitle Here'
        },
        xAxis: chart.xAxis
        ,
        yAxis: chart.yAxis
        ,

        legend: {
          align: 'right',
          verticalAlign: 'top',
          layout: 'vertical',
          x: 0,
          y: 100
        },

        plotOptions: {
          marker: {
            enabled: true
          },
        },

        annotations: chart.annotations,



        series: chart.series,

        exporting: {
          sourceWidth: chart.parameters.ResolutionX,
          sourceHeight: chart.parameters.ResolutionY,

          chartOptions: {
            subtitle: null
          }
        }

      });

      Highcharts.chart('container1', {
        chart: {
          type: 'spline',
          zoomType: 'xy',
          panning: true,
          panKey: 'shift'
        },
        title: {
          text: chart.parameters.Title
        },
        subtitle: {
          text: 'Enter Subtitle Here'
        },
        xAxis: chart.xAxis
        ,
        yAxis: chart.yAxis
        ,

        legend: {
          align: 'right',
          verticalAlign: 'top',
          layout: 'vertical',
          x: 0,
          y: 100
        },

        plotOptions: {
          marker: {
            enabled: true
          },
        },

        annotations: chart.annotations,



        series: chart.series,

        exporting: {
          sourceWidth: chart.parameters.ResolutionX,
          sourceHeight: chart.parameters.ResolutionY,

          chartOptions: {
            subtitle: null
          }
        }
      });


    }


  });