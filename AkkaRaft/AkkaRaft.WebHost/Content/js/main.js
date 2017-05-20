var uri = "ws://" + window.location.host + "/ws";

var PROGRESS_COLOR_NORMAL = '#87CEEB';
var PROGRESS_COLOR_ELECTION = '#F4425C';
var PROGRESS_COLOR_LEADER = '#A442F4';
var Circles = {};

function connect() {

    console.log("connecting to " + uri);

    socket = new WebSocket(uri);
    socket.onopen = function (event) {
        console.log("opened connection to " + uri);
    };
    socket.onclose = function (event) {
        console.log("closed connection from " + uri);
    };
    socket.onmessage = function (event) {
        var jsonObj = JSON.parse(event.data);
        onMessage(jsonObj);
    };

    socket.onerror = function (event) {
        console.log("error: " + event.data);
    };
}

function onMessage(obj) {
    if (obj.NodeId in Circles) {
        if (obj.Terminated)
        {
            terminateNode(obj.NodeId);
        }
        else
        {
            updateNode(obj);
        }
    }
    else {
        createNode(obj);
    }
}

function createNode(obj) {
    
    var template = $("#item_tmpl").html();

    template = $(template).attr("id", obj.NodeId.toString());

    $(".loadingcontainer").remove();
    $("#nodesArea").append(template);

    var circle = $("#" + obj.NodeId.toString() + " .circle").attr("id", "circle_" + obj.NodeId.toString());
    $("#" + obj.NodeId.toString() + " .vote").attr("id", "vote_" + obj.NodeId.toString());
    
    circle.radialIndicator({
        barColor: PROGRESS_COLOR_NORMAL,
        barWidth: 10,
        initValue: 0,
        roundCorner: true,
        minValue: 0,
        maxValue: obj.ElectionDuration,
        frameNum: 100
    });
    
    circle.duration = obj.ElectionDuration;

    $("#" + obj.NodeId.toString() + " .btn").click(function () {
        $.ajax({
            type: "POST",
            data: obj.ProcessId.toString(),
            url: "/Node/Terminate",
            complete: function (data) {
                // success, do work
                if (data.status == 200) {
                    terminateNode(obj.NodeId);
                    console.log("terminated");
                }
            },
            dataType: 'json'
        });
    });

    var radialObj = circle.data("radialIndicator");
    Circles[obj.NodeId] = radialObj;
}

function terminateNode(nodeId) {
    $("#" + nodeId.toString()).remove();
    delete Circles[nodeId];
}

function updateNode(obj) {

    if (obj.NodeId in Circles) {
        var radialObj = Circles[obj.NodeId];

        if (radialObj.duration != obj.ElectionDuration) {
            radialObj.duration = obj.ElectionDuration;
            radialObj.option('maxValue', obj.ElectionDuration);
        }

        var circleid = "#circle_" + obj.NodeId.toString();
        var voteid = "#vote_" + obj.NodeId.toString();

        if (obj.Role != radialObj.role) {
            radialObj.role = obj.Role;

            if (obj.Role == "Leader")
            {
                radialObj.option('displayNumber', false);
                radialObj.option('maxValue', 100);
                radialObj.animate(100);
                $(circleid + " .message .label").html("Leader").addClass("label-success").removeClass("label-warning label-default");
                radialObj.option('barColor', PROGRESS_COLOR_LEADER);
                $(voteid).hide();
            }
            else if (obj.Role == "Candidate")
            {
                $(circleid + " .message .label").html("Candidate").addClass("label-warning").removeClass("label-success label-default");
                radialObj.option('barColor', PROGRESS_COLOR_ELECTION);
                $(voteid).show();
            }
            else if (obj.Role == "Follower")
            {
                radialObj.option('barColor', PROGRESS_COLOR_NORMAL);
                $(circleid + " .message .label").html(obj.ElectionDuration.toString() + "ms").addClass("label-default").removeClass("label-warning label-success");
                $(voteid).hide();
            }

            if (obj.IsLeader != true) {
                radialObj.option('displayNumber', true);
            }
        }

        if (obj.Role == "Candidate")
        {
            $(voteid + " .voteValue").html(obj.Votes.toString() + " / " + obj.Majority.toString());
            radialObj.animate(obj.ElectionElapsed);
        }
        else if (obj.Role == "Follower")
        {
            radialObj.animate(obj.ElectionElapsed);
        }
    }
}

$(function () {    
    
    connect();

    $("#btnStartNew").click(function () {
        var loading = $("#loading_tmpl").html();
        $("#nodesArea").append(loading);
        $.ajax({
            type: "POST",
            url: "/Node/Start",
            success: function (data) {
                // success, do work
                console.log("created");
            },
            dataType: 'json'
        });
    });
});