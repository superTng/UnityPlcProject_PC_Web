mergeInto(LibraryManager.library, {
    OpenExe: function (pathPtr, argsPtr) {
        var path = UTF8ToString(pathPtr);
        var args = UTF8ToString(argsPtr);

        console.log("尝试启动 EXE: " + path + " 参数: " + args);

        // 调用本地 EXE
        window.location.href = "file://" + path + "?args=" + encodeURIComponent(args);

        // 备用方式：通过本地 HTTP 服务器请求
        fetch("http://localhost:5000/start?path=" + encodeURIComponent(path) + "&args=" + encodeURIComponent(args))
            .then(function(response) { return response.text(); })
            .then(function(data) { console.log("服务器响应: " + data); })
            .catch(function(error) { console.error("请求失败: " + error); });
    }
});
