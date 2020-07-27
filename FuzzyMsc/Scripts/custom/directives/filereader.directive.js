
angular.module("mainfuzzy")
    .directive("fileReaderDirective", function () {
        return {
            restrict: "A",
            scope: {
                fileReaderDirective: "=",
                filePathDirective: "="
            },
            link: function (scope, element) {
                $(element).on('change', function (changeEvent) {
                    var files = changeEvent.target.files;
                    if (files.length) {
                        var r = new FileReader();
                        r.onload = function (e) {
                            var contents = e.target.result;
                            scope.$apply(function () {
                                scope.fileReaderDirective = contents;
                                scope.filePathDirective = files[0];

                            });
                        };
                        r.readAsText(files[0]);
                    }
                });
            }
        };
    });