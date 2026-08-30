(function () {
    var versions = ["consumer1", "consumer2"];
    var base = window.location.origin;

    function createBar() {
        var bar = document.createElement("div");
        bar.id = "swagger-json-links";
        bar.style.cssText = "padding:8px 14px;background:#f7f7f7;border-bottom:1px solid #ddd;font-size:13px;font-family:sans-serif;";
        bar.innerHTML = "<strong>JSON spec:</strong> " +
            versions.map(function (v) {
                var url = base + "/swagger/docs/" + v;
                return '<a href="' + url + '" target="_blank" style="margin-right:20px;color:#547f00;">' + url + '</a>';
            }).join("");
        return bar;
    }

    function tryInject() {
        if (document.getElementById("swagger-json-links")) return;
        var header = document.getElementById("header");
        if (header) {
            header.parentNode.insertBefore(createBar(), header.nextSibling);
        }
    }

    var attempts = 0;
    var timer = setInterval(function () {
        tryInject();
        attempts++;
        if (document.getElementById("swagger-json-links") || attempts > 50) {
            clearInterval(timer);
        }
    }, 300);
})();
