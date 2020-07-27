
angular.module("mainfuzzy")
    .directive("limitToMax", function () {
        return {
            link: function (scope, element, attributes) {
                element.on("keydown keyup", function (e) {
                    if (Number(element.val()) > Number(attributes.max) &&
                        e.keyCode != 46 // delete
                        &&
                        e.keyCode != 8 // backspace
                    ) {
                        e.preventDefault();
                        element.val(attributes.max);
                    }
                });
            }
        };
    });