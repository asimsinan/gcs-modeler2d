angular.module('mainfuzzy')
    .factory('readFileData', function () {
    return {
        processData: function (csv_data) {
            var record = csv_data.split(/\r\n|\n/);
            var headers = record[0].split(',');
            var lines = [];
            var json = {};

            for (var i = 0; i < record.length; i++) {
                var data = record[i].split(',');
                if (data.length == headers.length) {
                    var tarr = [];
                    for (var j = 0; j < headers.length; j++) {
                        tarr.push(data[j]);
                    }
                    lines.push(tarr);
                }
            }

            for (var k = 0; k < lines.length; ++k) {
                json[k] = lines[k];
            }
            return json;
        }
    };
});