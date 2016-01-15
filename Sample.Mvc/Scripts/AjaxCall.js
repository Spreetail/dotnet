var ajaxCall = function (call) {

    call.HandleAllErrors = call.HandleAllErrors || true;
    call.Async = call.Async || true;

    if (typeof call.Endpoint === 'undefined') {
        alert("[ajaxCall] call.Endpoint is undefined.");
        return false;
    }

    if (typeof call.OnFail === 'undefined' || call.OnFail == null) {
        call.OnFail = function (e) {
            console.log(e);

            if (call.Silent)
                return;

            if (typeof addErrorMsg === 'undefined') {
                alert("Unable to complete call: " + call.Endpoint + "\n\n" + e.statusText);
            } else {
                addErrorMsg("Unable to complete call: " + call.Endpoint, e.statusText);
            }
        }
    }

    var endpoint = "http://localhost:54025";
    if (call.Endpoint[0] != "/")
        endpoint += "/" + call.Endpoint;
    else
        endpoint += call.Endpoint;

    var callData = (typeof call.Data === "string" ? call.Data : JSON.stringify(call.Data));

    var handleSuccess = function (result) {
        //if (!call.Silent) console.log("Handling Success");

        var EndedWithSuccess = true;

        $.each(result, function (k, v) {

            if (v != null && v.ResponseAck != undefined && (v.ResponseAck == 3 || (v.ResponseAck != 0 && call.HandleAllErrors))) {
                if (typeof call.OnFail === 'function') {
                    // Don't display anything, the caller obviously wants to handle this one.
                } else if (v.Errors != undefined && v.Errors != null && typeof displayError !== "undefined") {
                    displayError(v.Errors, "Callback Failed [" + k + "]");
                } else {
                    displayErrorBasic(v.Errors, "---Callback Failed [" + k + "]---");
                }

                EndedWithSuccess = false;
            }

            //if (!call.Silent) console.log(v);
        });

        if (EndedWithSuccess)
            call.OnSuccess(result);
        else
            call.OnFail(result);

        if (call.Finally != null)
            call.Finally();
    }
    var miniProfiler = $("#mini-profiler").data("current-id");
    var hasMini = miniProfiler != null;

    var theCall = $.ajax({
        url: endpoint,
        type: "POST",
        dataType: "json",
        contentType: "application/json;charset=utf-8",
        beforeSend: function (xhr) {
            if (hasMini)
                xhr.setRequestHeader("MiniProfilerRequestHeader", miniProfiler + "&::1&n");
        },
        data: callData,
        success: handleSuccess,
        async: call.Async,
        error: function (e) {
            call.OnFail(e);
            if (call.Finally != null)
                call.Finally();
        }
    });

    function displayErrorBasic(errors, title) {

        if (errors == null) {
            errors = [];
            errors.push({ ShortMessage: "Unspecified Error" });
        }

        if (typeof (errors) === "string") {
            var msg = errors;
            errors = [];
            errors.push({ ShortMessage: msg, FullMessage: "" });
        }

        if (errors.length > 0) {

            if (title == null)
                title = "System Error";


            for (var i = 0; i < errors.length; i++) {

                var err = errors[i];

                var t = "Error:";
                if (err.ShortMessage != null)
                    t = err.ShortMessage;

                alert(title + "\r\r" + t);

            }

        }

    }

    return theCall;

}

var ajaxCallType = function () {
    this.Endpoint = "";
    this.Data = "";
    this.OnSuccess = null;
    this.OnFail = null;
    this.Finally = null;
}