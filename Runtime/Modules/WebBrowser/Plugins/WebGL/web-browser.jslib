var unityWebBrowser = {

    // TAKED FROM IS.JS LIBRARY
    // https://github.com/arasatasaygin/is.js/blob/master/is.js
    $browser: {
        isTabActive: true,
        userAgent: function () {
            return (window.navigator && window.navigator.userAgent || '').toLowerCase();
        },
        platform: function () {
            return (window.navigator && window.navigator.platform || '').toLowerCase();
        },
        vendor: function () {
            return (window.navigator && window.navigator.vendor || '').toLowerCase();
        },
    },



    userAgent: function () {
        return browser.userAgent();
    },
    platform: function () {
        return browser.platform();
    },
    vendor: function () {
        return browser.vendor();
    },

    // BROWSERS 
    isIE: function () {
        return browser.userAgent().match(/(?:msie |trident.+?; rv:)(\d+)/) !== null;
    },
    isOpera: function () {
        return browser.userAgent().match(/(?:^opera.+?version|opr)\/(\d+)/) !== null;
    },
    isOperaMini: function () {
        return browser.userAgent().match(/opera mini\/(\d+)/) !== null;
    },
    isEdge: function () {
        return browser.userAgent().match(/edge\/(\d+)/) !== null;
    },
    isChrome: function () {
        var vendor = (window.navigator && window.navigator.vendor || '').toLowerCase();
        return (/google inc/.test(vendor) ? browser.userAgent().match(/(?:chrome|crios)\/(\d+)/) : null) !== null;
    },
    isSafari: function () {
        return browser.userAgent().match(/version\/(\d+).+?safari/) !== null;
    },
    isFirefox: function () {
        return browser.userAgent().match(/(?:firefox|fxios)\/(\d+)/) !== null;
    },

    // DEVICE
    isAndroid: function () {
        return /android/.test(browser.userAgent());
    },
    isAndroidPhone: function () {
        return /android/.test(browser.userAgent()) && /mobile/.test(browser.userAgent());
    },
    isAndroidTablet: function () {
        return /android/.test(browser.userAgent()) && !/mobile/.test(browser.userAgent());
    },

    isiPad: function () {
        return browser.userAgent().match(/ipad.+?os (\d+)/) !== null || (browser.userAgent().match(/macintosh/) !== null && "ontouchend" in document);
    },
    isiPod: function () {
        return browser.userAgent().match(/ipod.+?os (\d+)/) !== null;
    },
    isiPhone: function () {
        return browser.userAgent().match(/iphone(?:.+?os (\d+))?/) !== null;
    },

    isWindows: function () {
        return /win/.test(browser.platform());
    },
    isWindowsPhone: function () {
        return /phone/.test(browser.userAgent());
    },
    isWindowsTablet: function () {
        return /touch/.test(browser.userAgent());
    },

    isBlackberry: function () {
        return /blackberry/.test(browser.userAgent()) || /bb10/.test(browser.userAgent());
    },

    isOnline: function () {
        try {
            return window.navigator.onLine;
        }
        catch (e) {
            return false;
        }
    },

    sendBeacon: function (url, json) {
        url = UTF8ToString(url);
        json = UTF8ToString(json);

        console.log('[WEB] Send Beacon: url = ', url);
        console.log('[WEB] Send Beacon: body = ', json);
        window.navigator.sendBeacon(url, json);
    },

    postMessage: function (message) {
        var msg = UTF8ToString(message);
        console.log('[WEB] Post Message: ', msg);
        window.parent.postMessage(msg, '*');
    },

    setLocation: function (new_url) {
        var url = UTF8ToString(new_url);
        var iframe = false;
        try {
            iframe = (window.self !== window.top);
        } catch (e) {
            iframe = true;
        }
        if (iframe) {
            try {
                window.top.location.href = url;
            }
            catch (e) {
                const linkButton = document.createElement('a');
                linkButton.setAttribute('target', '_parent');
                linkButton.setAttribute('href', url);
                document.body.appendChild(linkButton);
                var event = new MouseEvent('click', {
                    view: window,
                    bubbles: true,
                    cancelable: true
                });
                linkButton.dispatchEvent(event);
            }
        } else {
            window.location.href = url;
        }
    },

    inIFrame: function () {
        try {
            return window.self !== window.top;
        } catch (e) {
            return true;
        }
    },

    isFullScreen: function () {
        var element = document.fullscreenElement || document.webkitFullscreenElement || document.mozFullScreenElement || document.msFullscreenElement || null;
        if (element === null) return false;
        else return true;
    },

    isTabActive: function(){
        return browser.isTabActive;
    },

    onTabVisibility: function (callback) {
        document.addEventListener("visibilitychange",
            () => {
                browser.isTabActive = document.visibilityState == "visible";
                dynCall_vi(callback, browser.isTabActive);
            });
    },

    enterFullScreen: function () {
        var element = document.body;
        try {
            if (element.requestFullscreen) element.requestFullscreen();
            else if (element.mozRequestFullScreen) element.mozRequestFullScreen();
            else if (element.webkitRequestFullscreen) element.webkitRequestFullscreen();
            else if (element.msRequestFullscreen) element.msRequestFullscreen();
        } catch (e) {
            console.log("[Error] enterFullScreen: ", e);
        }

    },

    exitFullScreen: function () {
        try {
            if (document.exitFullscreen) document.exitFullscreen();
            else if (document.mozCancelFullScreen) document.mozCancelFullScreen();
            else if (document.webkitExitFullscreen) document.webkitExitFullscreen();
            else if (document.msExitFullscreen) document.msExitFullscreen();
        } catch (e) {
            console.log("[Error] exitFullScreen: ", e);
        }
    },

    reloadPage: function () {
        var iframe = false;
        try {
            iframe = (window.self !== window.top);
        } catch (e) {
            iframe = true;
        }
        if (iframe) {
            try {
                window.top.location.reload();
            }
            catch (e) {
                window.location.reload();
                /*
                const linkButton = document.createElement('a');
                linkButton.setAttribute('target', '_parent');
                linkButton.setAttribute('href', document.referrer);
                document.body.appendChild(linkButton);
                var event = new MouseEvent('click', {
                    view: window,
                    bubbles: true,
                    cancelable: true
                });
                linkButton.dispatchEvent(event);
                */
            }
        } else {
            window.location.reload();
        }
    },


};

autoAddDeps(unityWebBrowser, '$browser');
mergeInto(LibraryManager.library, unityWebBrowser);