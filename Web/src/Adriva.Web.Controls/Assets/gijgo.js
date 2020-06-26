
var gj = {};

gj.widget = function () {
    var self = this;

    self.xhr = null;

    self.generateGUID = function () {
        function s4() {
            return Math.floor((1 + Math.random()) * 0x10000).toString(16).substring(1);
        }
        return s4() + s4() + '-' + s4() + '-' + s4() + '-' + s4() + '-' + s4() + s4() + s4();
    };

    self.mouseX = function (e) {
        if (e) {
            if (e.pageX) {
                return e.pageX;
            } else if (e.clientX) {
                return e.clientX + (document.documentElement.scrollLeft ? document.documentElement.scrollLeft : document.body.scrollLeft);
            } else if (e.touches && e.touches.length) {
                return e.touches[0].pageX;
            } else if (e.changedTouches && e.changedTouches.length) {
                return e.changedTouches[0].pageX;
            } else if (e.originalEvent && e.originalEvent.touches && e.originalEvent.touches.length) {
                return e.originalEvent.touches[0].pageX;
            } else if (e.originalEvent && e.originalEvent.changedTouches && e.originalEvent.changedTouches.length) {
                return e.originalEvent.touches[0].pageX;
            }
        }
        return null;
    };

    self.mouseY = function (e) {
        if (e) {
            if (e.pageY) {
                return e.pageY;
            } else if (e.clientY) {
                return e.clientY + (document.documentElement.scrollTop ? document.documentElement.scrollTop : document.body.scrollTop);
            } else if (e.touches && e.touches.length) {
                return e.touches[0].pageY;
            } else if (e.changedTouches && e.changedTouches.length) {
                return e.changedTouches[0].pageY;
            } else if (e.originalEvent && e.originalEvent.touches && e.originalEvent.touches.length) {
                return e.originalEvent.touches[0].pageY;
            } else if (e.originalEvent && e.originalEvent.changedTouches && e.originalEvent.changedTouches.length) {
                return e.originalEvent.touches[0].pageY;
            }
        }
        return null;
    };
};

gj.widget.prototype.getFullConfig = function () {
    return this.data();
};

gj.widget.prototype.setFullConfig = function (config) {
    return this.data(config);
};

gj.widget.prototype.init = function (jsConfig, type) {
    var option, clientConfig, fullConfig;

    this.attr('data-type', type);
    clientConfig = $.extend(true, {}, this.getHTMLConfig() || {});
    $.extend(true, clientConfig, jsConfig || {});
    fullConfig = this.getConfig(clientConfig, type);
    this.attr('data-guid', fullConfig.guid);
    this.data(fullConfig);
    for (option in fullConfig) {
        if (gj[type].events.hasOwnProperty(option)) {
            this.on(option, fullConfig[option]);
            delete fullConfig[option];
        }
    }
    for (plugin in gj[type].plugins) {
        if (gj[type].plugins.hasOwnProperty(plugin)) {
            gj[type].plugins[plugin].configure(this, fullConfig, clientConfig);
        }
    }

    return this;
};

gj.widget.prototype.getConfig = function (clientConfig, type) {
    var config, uiLibrary, iconsLibrary, plugin;

    config = $.extend(true, {}, gj[type].config.base);

    uiLibrary = clientConfig.hasOwnProperty('uiLibrary') ? clientConfig.uiLibrary : config.uiLibrary;
    if (gj[type].config[uiLibrary]) {
        $.extend(true, config, gj[type].config[uiLibrary]);
    }

    iconsLibrary = clientConfig.hasOwnProperty('iconsLibrary') ? clientConfig.iconsLibrary : config.iconsLibrary;
    if (gj[type].config[iconsLibrary]) {
        $.extend(true, config, gj[type].config[iconsLibrary]);
    }

    for (plugin in gj[type].plugins) {
        if (gj[type].plugins.hasOwnProperty(plugin)) {
            $.extend(true, config, gj[type].plugins[plugin].config.base);
            if (gj[type].plugins[plugin].config[uiLibrary]) {
                $.extend(true, config, gj[type].plugins[plugin].config[uiLibrary]);
            }
            if (gj[type].plugins[plugin].config[iconsLibrary]) {
                $.extend(true, config, gj[type].plugins[plugin].config[iconsLibrary]);
            }
        }
    }

    $.extend(true, config, clientConfig);

    if (!config.guid) {
        config.guid = this.generateGUID();
    }

    return config;
}

gj.widget.prototype.getHTMLConfig = function () {
    var result = this.data(),
        attrs = this[0].attributes;
    if (attrs['width']) {
        result.width = attrs['width'].value;
    }
    if (attrs['height']) {
        result.height = attrs['height'].value;
    }
    if (attrs['value']) {
        result.value = attrs['value'].value;
    }
    if (attrs['align']) {
        result.align = attrs['align'].value;
    }
    if (result && result.source) {
        result.dataSource = result.source;
        delete result.source;
    }
    return result;
};

gj.widget.prototype.createDoneHandler = function () {
    var $widget = this;
    return function (response) {
        if (typeof (response) === 'string' && JSON) {
            response = JSON.parse(response);
        }
        gj[$widget.data('type')].methods.render($widget, response);
    };
};

gj.widget.prototype.createErrorHandler = function () {
    var $widget = this;
    return function (response) {
        if (response && response.statusText && response.statusText !== 'abort') {
            alert(response.statusText);
        }
    };
};

gj.widget.prototype.reload = function (params) {
    var ajaxOptions, result, data = this.data(), type = this.data('type');
    if (data.dataSource === undefined) {
        gj[type].methods.useHtmlDataSource(this, data);
    }
    $.extend(data.params, params);
    if ($.isArray(data.dataSource)) {
        result = gj[type].methods.filter(this);
        gj[type].methods.render(this, result);
    } else if (typeof (data.dataSource) === 'string') {
        ajaxOptions = { url: data.dataSource, data: data.params };
        if (this.xhr) {
            this.xhr.abort();
        }
        this.xhr = $.ajax(ajaxOptions).done(this.createDoneHandler()).fail(this.createErrorHandler());
    } else if (typeof (data.dataSource) === 'object') {
        if (!data.dataSource.data) {
            data.dataSource.data = {};
        }
        $.extend(data.dataSource.data, data.params);
        ajaxOptions = $.extend(true, {}, data.dataSource); //clone dataSource object
        if (ajaxOptions.dataType === 'json' && typeof (ajaxOptions.data) === 'object') {
            ajaxOptions.data = JSON.stringify(ajaxOptions.data);
        }
        if (!ajaxOptions.success) {
            ajaxOptions.success = this.createDoneHandler();
        }
        if (!ajaxOptions.error) {
            ajaxOptions.error = this.createErrorHandler();
        }
        if (this.xhr) {
            this.xhr.abort();
        }
        this.xhr = $.ajax(ajaxOptions);
    }
    return this;
}

gj.documentManager = {
    events: {},

    subscribeForEvent: function (eventName, widgetId, callback) {
        if (!gj.documentManager.events[eventName] || gj.documentManager.events[eventName].length === 0) {
            gj.documentManager.events[eventName] = [{ widgetId: widgetId, callback: callback }];
            $(document).on(eventName, gj.documentManager.executeCallbacks);
        } else if (!gj.documentManager.events[eventName][widgetId]) {
            gj.documentManager.events[eventName].push({ widgetId: widgetId, callback: callback });
        } else {
            throw 'Event ' + eventName + ' for widget with guid="' + widgetId + '" is already attached.';
        }
    },

    executeCallbacks: function (e) {
        var callbacks = gj.documentManager.events[e.type];
        if (callbacks) {
            for (var i = 0; i < callbacks.length; i++) {
                callbacks[i].callback(e);
            }
        }
    },

    unsubscribeForEvent: function (eventName, widgetId) {
        var success = false,
            events = gj.documentManager.events[eventName];
        if (events) {
            for (var i = 0; i < events.length; i++) {
                if (events[i].widgetId === widgetId) {
                    events.splice(i, 1);
                    success = true;
                    if (events.length === 0) {
                        $(document).off(eventName);
                        delete gj.documentManager.events[eventName];
                    }
                }
            }
        }
        if (!success) {
            throw 'The "' + eventName + '" for widget with guid="' + widgetId + '" can\'t be removed.';
        }
    }
};
gj.core = {
    messages: {
        'en-us': {
            monthNames: ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'],
            monthShortNames: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'],
            weekDaysMin: ['S', 'M', 'T', 'W', 'T', 'F', 'S'],
            weekDaysShort: ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"],
            weekDays: ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"],
            am: 'AM',
            pm: 'PM',
            ok: 'Ok',
            cancel: 'Cancel',
            titleFormat: 'mmmm yyyy'
        },
        'tr-tr': {
            monthNames: ['Ocak', 'Şubat', 'Mart', 'Nisan', 'Mayıs', 'Haziran', 'Temmuz', 'Ağustos', 'Eylül', 'Ekim', 'Kasım', 'Aralık'],
            monthShortNames: ['Oca', 'Şub', 'Mar', 'Nis', 'May', 'Haz', 'Tem', 'Ağu', 'Eyl', 'Eki', 'Kas', 'Ara'],
            weekDaysMin: ['P', 'P', 'S', 'Ç', 'P', 'C', 'C'],
            weekDaysShort: ['Pz', 'Pzt', 'Sal', 'Çrş', 'Prş', 'Cu', 'Cts'],
            weekDays: ['Pazar', 'Pazartesi', 'Salı', 'Çarşamba', 'Perşembe', 'Cuma', 'Cumartesi'],
            am: 'AM',
            pm: 'PM',
            ok: 'Tamam',
            cancel: 'İptal',
            titleFormat: 'mmmm yyyy'
        }
    },
    parseDate: function (value, format, locale) {
        var i, year = 0, month = 0, date = 1, hour = 0, minute = 0, dateParts, formatParts, result;

        if (value && typeof value === 'string') {
            if (/^\d+$/.test(value)) {
                result = new Date(value);
            } else if (value.indexOf('/Date(') > -1) {
                result = new Date(parseInt(value.substr(6), 10));
            } else if (value) {
                formatParts = format.split(/[\s,-\.//\:]+/);
                dateParts = value.split(/[\s]+/);
                if (dateParts.length != formatParts.length) {
                    dateParts = value.split(/[\s,-\.//\:]+/);
                }
                for (i = 0; i < formatParts.length; i++) {
                    if (['d', 'dd'].indexOf(formatParts[i]) > -1) {
                        date = parseInt(dateParts[i], 10);
                    } else if (['m', 'mm'].indexOf(formatParts[i]) > -1) {
                        month = parseInt(dateParts[i], 10) - 1;
                    } else if ('mmm' === formatParts[i]) {
                        month = gj.core.messages[locale || 'en-us'].monthShortNames.indexOf(dateParts[i]);
                    } else if ('mmmm' === formatParts[i]) {
                        month = gj.core.messages[locale || 'en-us'].monthNames.indexOf(dateParts[i]);
                    } else if (['yy', 'yyyy'].indexOf(formatParts[i]) > -1) {
                        year = parseInt(dateParts[i], 10);
                        if (formatParts[i] === 'yy') {
                            year += 2000;
                        }
                    } else if (['h', 'hh', 'H', 'HH'].indexOf(formatParts[i]) > -1) {
                        hour = parseInt(dateParts[i], 10);
                    } else if (['M', 'MM'].indexOf(formatParts[i]) > -1) {
                        minute = parseInt(dateParts[i], 10);
                    }
                }
                result = new Date(year, month, date, hour, minute);
            }
        } else if (typeof value === 'number') {
            result = new Date(value);
        } else if (value instanceof Date) {
            result = value;
        }

        return result;
    },
    formatDate: function (date, format, locale) {
        var result = '', separator, tmp,
            formatParts = format.split(/[\s,-\.//\:]+/),
            separators = format.split(/s+|M+|H+|h+|t+|T+|d+|m+|y+/);

        separators = separators.splice(1, separators.length - 2);

        for (i = 0; i < formatParts.length; i++) {
            separator = (separators[i] || '');
            switch (formatParts[i]) {
                case 's':
                    result += date.getSeconds() + separator;
                    break;
                case 'ss':
                    result += gj.core.pad(date.getSeconds()) + separator;
                    break;
                case 'M':
                    result += date.getMinutes() + separator;
                    break;
                case 'MM':
                    result += gj.core.pad(date.getMinutes()) + separator;
                    break;
                case 'H':
                    result += date.getHours() + separator;
                    break;
                case 'HH':
                    result += gj.core.pad(date.getHours()) + separator;
                    break;
                case 'h':
                    tmp = date.getHours() > 12 ? date.getHours() % 12 : date.getHours();
                    result += tmp + separator;
                    break;
                case 'hh':
                    tmp = date.getHours() > 12 ? date.getHours() % 12 : date.getHours();
                    result += gj.core.pad(tmp) + separator;
                    break;
                case 'tt':
                    result += (date.getHours() >= 12 ? 'pm' : 'am') + separator;
                    break;
                case 'TT':
                    result += (date.getHours() >= 12 ? 'PM' : 'AM') + separator;
                    break;
                case 'd':
                    result += date.getDate() + separator;
                    break;
                case 'dd':
                    result += gj.core.pad(date.getDate()) + separator;
                    break;
                case 'ddd':
                    result += gj.core.messages[locale || 'en-us'].weekDaysShort[date.getDay()] + separator;
                    break;
                case 'dddd':
                    result += gj.core.messages[locale || 'en-us'].weekDays[date.getDay()] + separator;
                    break;
                case 'm':
                    result += (date.getMonth() + 1) + separator;
                    break;
                case 'mm':
                    result += gj.core.pad(date.getMonth() + 1) + separator;
                    break;
                case 'mmm':
                    result += gj.core.messages[locale || 'en-us'].monthShortNames[date.getMonth()] + separator;
                    break;
                case 'mmmm':
                    result += gj.core.messages[locale || 'en-us'].monthNames[date.getMonth()] + separator;
                    break;
                case 'yy':
                    result += date.getFullYear().toString().substr(2) + separator;
                    break;
                case 'yyyy':
                    result += date.getFullYear() + separator;
                    break;
            }
        }

        return result;
    },

    pad: function (val, len) {
        val = String(val);
        len = len || 2;
        while (val.length < len) {
            val = '0' + val;
        }
        return val;
    },

    center: function ($dialog) {
        var left = ($(window).width() / 2) - ($dialog.width() / 2),
            top = ($(window).height() / 2) - ($dialog.height() / 2);
        $dialog.css('position', 'absolute');
        $dialog.css('left', left > 0 ? left : 0);
        $dialog.css('top', top > 0 ? top : 0);
    },

    isIE: function () {
        return !!navigator.userAgent.match(/Trident/g) || !!navigator.userAgent.match(/MSIE/g);
    },

    setChildPosition: function (mainEl, childEl) {
        var mainElRect = mainEl.getBoundingClientRect(),
            mainElHeight = gj.core.height(mainEl, true),
            childElHeight = gj.core.height(childEl, true),
            mainElWidth = gj.core.width(mainEl, true),
            childElWidth = gj.core.width(childEl, true),
            scrollY = window.scrollY || window.pageYOffset || 0,
            scrollX = window.scrollX || window.pageXOffset || 0;

        if ((mainElRect.top + mainElHeight + childElHeight) > window.innerHeight && mainElRect.top > childElHeight) {
            childEl.style.top = Math.round(mainElRect.top + scrollY - childElHeight - 3) + 'px';
        } else {
            childEl.style.top = Math.round(mainElRect.top + scrollY + mainElHeight + 3) + 'px';
        }

        if (mainElRect.left + childElWidth > document.body.clientWidth) {
            childEl.style.left = Math.round(mainElRect.left + scrollX + mainElWidth - childElWidth) + 'px';
        } else {
            childEl.style.left = Math.round(mainElRect.left + scrollX) + 'px';
        }
    },

    height: function (el, margin) {
        var result, style = window.getComputedStyle(el);

        if (style.boxSizing === 'border-box') { // border-box include padding and border within the height
            result = parseInt(style.height, 10);
            if (gj.core.isIE()) {
                result += parseInt(style.paddingTop || 0, 10) + parseInt(style.paddingBottom || 0, 10);
                result += parseInt(style.borderTopWidth || 0, 10) + parseInt(style.borderBottomWidth || 0, 10);
            }
        } else {
            result = parseInt(style.height, 10);
            result += parseInt(style.paddingTop || 0, 10) + parseInt(style.paddingBottom || 0, 10);
            result += parseInt(style.borderTopWidth || 0, 10) + parseInt(style.borderBottomWidth || 0, 10);
        }

        if (margin) {
            result += parseInt(style.marginTop || 0, 10) + parseInt(style.marginBottom || 0, 10);
        }

        return result;
    },

    width: function (el, margin) {
        var result, style = window.getComputedStyle(el);

        if (style.boxSizing === 'border-box') { // border-box include padding and border within the width
            result = parseInt(style.width, 10);
        } else {
            result = parseInt(style.width, 10);
            result += parseInt(style.paddingLeft || 0, 10) + parseInt(style.paddingRight || 0, 10);
            result += parseInt(style.borderLeftWidth || 0, 10) + parseInt(style.borderRightWidth || 0, 10);
        }

        if (margin) {
            result += parseInt(style.marginLeft || 0, 10) + parseInt(style.marginRight || 0, 10);
        }

        return result;
    },

    addClasses: function (el, classes) {
        var i, arr;
        if (classes) {
            arr = classes.split(' ');
            for (i = 0; i < arr.length; i++) {
                el.classList.add(arr[i]);
            }
        }
    },

    position: function (el) {
        var xScroll, yScroll, left = 0, top = 0,
            height = gj.core.height(el),
            width = gj.core.width(el);

        while (el) {
            if (el.tagName == "BODY") {
                xScroll = el.scrollLeft || document.documentElement.scrollLeft;
                yScroll = el.scrollTop || document.documentElement.scrollTop;
                left += el.offsetLeft - xScroll; // + el.clientLeft);
                top += el.offsetTop - yScroll; // + el.clientTop);
            } else {
                left += el.offsetLeft - el.scrollLeft; // + el.clientLeft;
                top += el.offsetTop - el.scrollTop; // + el.clientTop;
            }

            el = el.offsetParent;
        }

        return { top: top, left: left, bottom: top + height, right: left + width };
    },

    setCaretAtEnd: function (elem) {
        var elemLen;
        if (elem) {
            elemLen = elem.value.length;
            if (document.selection) { // For IE Only
                elem.focus();
                var oSel = document.selection.createRange();
                oSel.moveStart('character', -elemLen);
                oSel.moveStart('character', elemLen);
                oSel.moveEnd('character', 0);
                oSel.select();
            } else if (elem.selectionStart || elem.selectionStart == '0') { // Firefox/Chrome                
                elem.selectionStart = elemLen;
                elem.selectionEnd = elemLen;
                elem.focus();
            }
        }
    },
    getScrollParent: function (node) {
        if (node == null) {
            return null;
        } else if (node.scrollHeight > node.clientHeight) {
            return node;
        } else {
            return gj.core.getScrollParent(node.parentNode);
        }
    }
};
gj.picker = {
    messages: {
        'en-us': {
        }
    }
};

gj.picker.methods = {

    initialize: function ($input, data, methods) {
        var $calendar, $rightIcon,
            $picker = methods.createPicker($input, data),
            $wrapper = $input.parent('div[role="wrapper"]');

        if (data.uiLibrary === 'bootstrap') {
            $rightIcon = $('<span class="input-group-addon">' + data.icons.rightIcon + '</span>');
        } else if (data.uiLibrary === 'bootstrap4') {
            $rightIcon = $('<span class="input-group-append"><button class="btn btn-outline-secondary border-left-0" type="button">' + data.icons.rightIcon + '</button></span>');
        } else {
            $rightIcon = $(data.icons.rightIcon);
        }
        $rightIcon.attr('role', 'right-icon');

        if ($wrapper.length === 0) {
            $wrapper = $('<div role="wrapper" />').addClass(data.style.wrapper); // The css class needs to be added before the wrapping, otherwise doesn't work.
            $input.wrap($wrapper);
        } else {
            $wrapper.addClass(data.style.wrapper);
        }
        $wrapper = $input.parent('div[role="wrapper"]');

        data.width && $wrapper.css('width', data.width);

        $input.val(data.value).addClass(data.style.input).attr('role', 'input');

        data.fontSize && $input.css('font-size', data.fontSize);

        if (data.uiLibrary === 'bootstrap' || data.uiLibrary === 'bootstrap4') {
            if (data.size === 'small') {
                $wrapper.addClass('input-group-sm');
                $input.addClass('form-control-sm');
            } else if (data.size === 'large') {
                $wrapper.addClass('input-group-lg');
                $input.addClass('form-control-lg');
            }
        } else {
            if (data.size === 'small') {
                $wrapper.addClass('small');
            } else if (data.size === 'large') {
                $wrapper.addClass('large');
            }
        }

        $rightIcon.on('click', function (e) {
            if ($picker.is(':visible')) {
                $input.close();
            } else {
                $input.open();
            }
        });
        $wrapper.append($rightIcon);

        if (data.footer !== true) {
            $input.on('blur', function () {
                $input.timeout = setTimeout(function () {
                    $input.close();
                }, 500);
            });
            $picker.mousedown(function () {
                clearTimeout($input.timeout);
                $input.focus();
                return false;
            });
            $picker.on('click', function () {
                clearTimeout($input.timeout);
                $input.focus();
            });
        }
    }
};


gj.picker.widget = function ($element, jsConfig) {
    var self = this,
        methods = gj.picker.methods;

    self.destroy = function () {
        return methods.destroy(this);
    };

    return $element;
};

gj.picker.widget.prototype = new gj.widget();
gj.picker.widget.constructor = gj.picker.widget;

gj.picker.widget.prototype.init = function (jsConfig, type, methods) {
    gj.widget.prototype.init.call(this, jsConfig, type);
    this.attr('data-' + type, 'true');
    gj.picker.methods.initialize(this, this.data(), gj[type].methods);
    return this;
};

gj.picker.widget.prototype.open = function (type) {
    var data = this.data(),
        $picker = $('body').find('[role="picker"][guid="' + this.attr('data-guid') + '"]');

    $picker.show();
    $picker.closest('div[role="modal"]').show();
    if (data.modal) {
        gj.core.center($picker);
    } else {
        gj.core.setChildPosition(this[0], $picker[0]);
        this.focus();
    }
    clearTimeout(this.timeout);

    gj[type].events.open(this);

    return this;
};

gj.picker.widget.prototype.close = function (type) {
    var $picker = $('body').find('[role="picker"][guid="' + this.attr('data-guid') + '"]');
    $picker.hide();
    $picker.closest('div[role="modal"]').hide();
    gj[type].events.close(this);
    return this;
};

gj.picker.widget.prototype.destroy = function (type) {
    var data = this.data(),
        $parent = this.parent(),
        $picker = $('body').find('[role="picker"][guid="' + this.attr('data-guid') + '"]');
    if (data) {
        this.off();
        if ($picker.parent('[role="modal"]').length > 0) {
            $picker.unwrap();
        }
        $picker.remove();
        this.removeData();
        this.removeAttr('data-type').removeAttr('data-guid').removeAttr('data-' + type);
        this.removeClass();
        $parent.children('[role="right-icon"]').remove();
        this.unwrap();
    }
    return this;
};
/** 
 * @widget Dialog 
 * @plugin Base
 */
gj.dialog = {
    plugins: {},
    messages: {}
};

gj.dialog.config = {
    base: {
        autoOpen: true,
        closeButtonInHeader: true,
        closeOnEscape: true,
        draggable: true,
        height: 'auto',
        locale: 'en-us',
        maxHeight: undefined,
        maxWidth: undefined,
        minHeight: undefined,
        minWidth: undefined,
        modal: false,
        resizable: false,
        scrollable: false,
        title: undefined,
        uiLibrary: undefined,
        width: 300,

        style: {
            modal: 'gj-modal',
            content: 'gj-dialog-md',
            header: 'gj-dialog-md-header gj-unselectable',
            headerTitle: 'gj-dialog-md-title',
            headerCloseButton: 'gj-dialog-md-close',
            body: 'gj-dialog-md-body',
            footer: 'gj-dialog-footer gj-dialog-md-footer'
        }
    },

    bootstrap: {
        style: {
            modal: 'modal',
            content: 'modal-content gj-dialog-bootstrap',
            header: 'modal-header',
            headerTitle: 'modal-title',
            headerCloseButton: 'close',
            body: 'modal-body',
            footer: 'gj-dialog-footer modal-footer'
        }
    },

    bootstrap4: {
        style: {
            modal: 'modal',
            content: 'modal-content gj-dialog-bootstrap4',
            header: 'modal-header',
            headerTitle: 'modal-title',
            headerCloseButton: 'close',
            body: 'modal-body',
            footer: 'gj-dialog-footer modal-footer'
        }
    }
};
gj.dialog.events = {
    initialized: function ($dialog) {
        $dialog.trigger("initialized");
    },
    opening: function ($dialog) {
        $dialog.trigger("opening");
    },
    opened: function ($dialog) {
        $dialog.trigger("opened");
    },
    closing: function ($dialog) {
        $dialog.trigger("closing");
    },
    closed: function ($dialog) {
        $dialog.trigger("closed");
    },
    drag: function ($dialog) {
        $dialog.trigger("drag");
    },
    dragStart: function ($dialog) {
        $dialog.trigger("dragStart");
    },
    dragStop: function ($dialog) {
        $dialog.trigger("dragStop");
    },
    resize: function ($dialog) {
        $dialog.trigger("resize");
    },
    resizeStart: function ($dialog) {
        $dialog.trigger("resizeStart");
    },
    resizeStop: function ($dialog) {
        $dialog.trigger("resizeStop");
    }
};

gj.dialog.methods = {

    init: function (jsConfig) {
        gj.widget.prototype.init.call(this, jsConfig, 'dialog');

        gj.dialog.methods.localization(this);
        gj.dialog.methods.initialize(this);
        gj.dialog.events.initialized(this);
        return this;
    },

    localization: function ($dialog) {
        var data = $dialog.data();
        if (typeof (data.title) === 'undefined') {
            data.title = gj.dialog.messages[data.locale].DefaultTitle;
        }
    },

    getHTMLConfig: function () {
        var result = gj.widget.prototype.getHTMLConfig.call(this),
            attrs = this[0].attributes;
        if (attrs['title']) {
            result.title = attrs['title'].value;
        }
        return result;
    },

    initialize: function ($dialog) {
        var data = $dialog.data(),
            $header, $body, $footer;

        $dialog.addClass(data.style.content);

        gj.dialog.methods.setSize($dialog);

        if (data.closeOnEscape) {
            $(document).keyup(function (e) {
                if (e.keyCode === 27) {
                    $dialog.close();
                }
            });
        }

        $body = $dialog.children('div[data-role="body"]');
        if ($body.length === 0) {
            $body = $('<div data-role="body"/>').addClass(data.style.body);
            $dialog.wrapInner($body);
        } else {
            $body.addClass(data.style.body);
        }

        $header = gj.dialog.methods.renderHeader($dialog);

        $footer = $dialog.children('div[data-role="footer"]').addClass(data.style.footer);

        $dialog.find('[data-role="close"]').on('click', function () {
            $dialog.close();
        });

        if (gj.draggable) {
            if (data.draggable) {
                gj.dialog.methods.draggable($dialog, $header);
            }
            if (data.resizable) {
                gj.dialog.methods.resizable($dialog);
            }
        }

        if (data.scrollable && data.height) {
            $dialog.addClass('gj-dialog-scrollable');
            $dialog.on('opened', function () {
                var $body = $dialog.children('div[data-role="body"]');
                $body.css('height', data.height - $header.outerHeight() - ($footer.length ? $footer.outerHeight() : 0));
            });
        }

        gj.core.center($dialog);

        if (data.modal) {
            $dialog.wrapAll('<div data-role="modal" class="' + data.style.modal + '"/>');
        }

        if (data.autoOpen) {
            $dialog.open();
        }
    },

    setSize: function ($dialog) {
        var data = $dialog.data();
        if (data.width) {
            $dialog.css("width", data.width);
        }
        if (data.height) {
            $dialog.css("height", data.height);
        }
    },

    renderHeader: function ($dialog) {
        var $header, $title, $closeButton, data = $dialog.data();
        $header = $dialog.children('div[data-role="header"]');
        if ($header.length === 0) {
            $header = $('<div data-role="header" />');
            $dialog.prepend($header);
        }
        $header.addClass(data.style.header);

        $title = $header.find('[data-role="title"]');
        if ($title.length === 0) {
            $title = $('<h4 data-role="title">' + data.title + '</h4>');
            $header.append($title);
        }
        $title.addClass(data.style.headerTitle);

        $closeButton = $header.find('[data-role="close"]');
        if ($closeButton.length === 0 && data.closeButtonInHeader) {
            $closeButton = $('<button type="button" data-role="close" title="' + gj.dialog.messages[data.locale].Close + '"><span>×</span></button>');
            $closeButton.addClass(data.style.headerCloseButton);
            $header.append($closeButton);
        } else if ($closeButton.length > 0 && data.closeButtonInHeader === false) {
            $closeButton.hide();
        } else {
            $closeButton.addClass(data.style.headerCloseButton);
        }

        return $header;
    },

    draggable: function ($dialog, $header) {
        $dialog.appendTo('body');
        $header.addClass('gj-draggable');
        $dialog.draggable({
            handle: $header,
            start: function () {
                $dialog.addClass('gj-unselectable');
                gj.dialog.events.dragStart($dialog);
            },
            stop: function () {
                $dialog.removeClass('gj-unselectable');
                gj.dialog.events.dragStop($dialog);
            }
        });
    },

    resizable: function ($dialog) {
        var config = {
            'drag': gj.dialog.methods.resize,
            'start': function () {
                $dialog.addClass('gj-unselectable');
                gj.dialog.events.resizeStart($dialog);
            },
            'stop': function () {
                this.removeAttribute('style');
                $dialog.removeClass('gj-unselectable');
                gj.dialog.events.resizeStop($dialog);
            }
        };
        $dialog.append($('<div class="gj-resizable-handle gj-resizable-n"></div>').draggable($.extend(true, { horizontal: false }, config)));
        $dialog.append($('<div class="gj-resizable-handle gj-resizable-e"></div>').draggable($.extend(true, { vertical: false }, config)));
        $dialog.append($('<div class="gj-resizable-handle gj-resizable-s"></div>').draggable($.extend(true, { horizontal: false }, config)));
        $dialog.append($('<div class="gj-resizable-handle gj-resizable-w"></div>').draggable($.extend(true, { vertical: false }, config)));
        $dialog.append($('<div class="gj-resizable-handle gj-resizable-ne"></div>').draggable($.extend(true, {}, config)));
        $dialog.append($('<div class="gj-resizable-handle gj-resizable-nw"></div>').draggable($.extend(true, {}, config)));
        $dialog.append($('<div class="gj-resizable-handle gj-resizable-sw"></div>').draggable($.extend(true, {}, config)));
        $dialog.append($('<div class="gj-resizable-handle gj-resizable-se"></div>').draggable($.extend(true, {}, config)));
    },

    resize: function (e, newPosition) {
        var $el, $dialog, position, data, height, width, top, left, result = false;

        $el = $(this);
        $dialog = $el.parent();
        position = gj.core.position(this);
        offset = { top: newPosition.top - position.top, left: newPosition.left - position.left };
        data = $dialog.data();
        if ($el.hasClass('gj-resizable-n')) {
            height = $dialog.height() - offset.top;
            top = $dialog.offset().top + offset.top;
        } else if ($el.hasClass('gj-resizable-e')) {
            width = $dialog.width() + offset.left;
        } else if ($el.hasClass('gj-resizable-s')) {
            height = $dialog.height() + offset.top;
        } else if ($el.hasClass('gj-resizable-w')) {
            width = $dialog.width() - offset.left;
            left = $dialog.offset().left + offset.left;
        } else if ($el.hasClass('gj-resizable-ne')) {
            height = $dialog.height() - offset.top;
            top = $dialog.offset().top + offset.top;
            width = $dialog.width() + offset.left;
        } else if ($el.hasClass('gj-resizable-nw')) {
            height = $dialog.height() - offset.top;
            top = $dialog.offset().top + offset.top;
            width = $dialog.width() - offset.left;
            left = $dialog.offset().left + offset.left;
        } else if ($el.hasClass('gj-resizable-se')) {
            height = $dialog.height() + offset.top;
            width = $dialog.width() + offset.left;
        } else if ($el.hasClass('gj-resizable-sw')) {
            height = $dialog.height() + offset.top;
            width = $dialog.width() - offset.left;
            left = $dialog.offset().left + offset.left;
        }

        if (height && (!data.minHeight || height >= data.minHeight) && (!data.maxHeight || height <= data.maxHeight)) {
            $dialog.height(height);
            if (top) {
                $dialog.css('top', top);
            }
            result = true;
        }

        if (width && (!data.minWidth || width >= data.minWidth) && (!data.maxWidth || width <= data.maxWidth)) {
            $dialog.width(width);
            if (left) {
                $dialog.css('left', left);
            }
            result = true;
        }

        if (result) {
            gj.dialog.events.resize($dialog);
        }

        return result;
    },

    open: function ($dialog, title) {
        var $footer;
        gj.dialog.events.opening($dialog);
        $dialog.css('display', 'block');
        $dialog.closest('div[data-role="modal"]').css('display', 'block');
        $footer = $dialog.children('div[data-role="footer"]');
        if ($footer.length && $footer.outerHeight()) {
            $dialog.children('div[data-role="body"]').css('margin-bottom', $footer.outerHeight());
        }
        if (title !== undefined) {
            $dialog.find('[data-role="title"]').html(title);
        }
        gj.dialog.events.opened($dialog);
        return $dialog;
    },

    close: function ($dialog) {
        if ($dialog.is(':visible')) {
            gj.dialog.events.closing($dialog);
            $dialog.css('display', 'none');
            $dialog.closest('div[data-role="modal"]').css('display', 'none');
            gj.dialog.events.closed($dialog);
        }
        return $dialog;
    },

    isOpen: function ($dialog) {
        return $dialog.is(':visible');
    },

    content: function ($dialog, html) {
        var $body = $dialog.children('div[data-role="body"]');
        if (typeof (html) === "undefined") {
            return $body.html();
        } else {
            return $body.html(html);
        }
    },

    destroy: function ($dialog, keepHtml) {
        var data = $dialog.data();
        if (data) {
            if (keepHtml === false) {
                $dialog.remove();
            } else {
                $dialog.close();
                $dialog.off();
                $dialog.removeData();
                $dialog.removeAttr('data-type');
                $dialog.removeClass(data.style.content);
                $dialog.find('[data-role="header"]').removeClass(data.style.header);
                $dialog.find('[data-role="title"]').removeClass(data.style.headerTitle);
                $dialog.find('[data-role="close"]').remove();
                $dialog.find('[data-role="body"]').removeClass(data.style.body);
                $dialog.find('[data-role="footer"]').removeClass(data.style.footer);
            }

        }
        return $dialog;
    }
};
gj.dialog.widget = function ($element, jsConfig) {
    var self = this,
        methods = gj.dialog.methods;
    self.open = function (title) {
        return methods.open(this, title);
    }
    self.close = function () {
        return methods.close(this);
    }
    self.isOpen = function () {
        return methods.isOpen(this);
    }
    self.content = function (content) {
        return methods.content(this, content);
    }
    self.destroy = function (keepHtml) {
        return methods.destroy(this, keepHtml);
    }

    $.extend($element, self);
    if ('dialog' !== $element.attr('data-type')) {
        methods.init.call($element, jsConfig);
    }

    return $element;
};

gj.dialog.widget.prototype = new gj.widget();
gj.dialog.widget.constructor = gj.dialog.widget;

gj.dialog.widget.prototype.getHTMLConfig = gj.dialog.methods.getHTMLConfig;

(function ($) {
    $.fn.dialog = function (method) {
        var $widget;
        if (this && this.length) {
            if (typeof method === 'object' || !method) {
                return new gj.dialog.widget(this, method);
            } else {
                $widget = new gj.dialog.widget(this, null);
                if ($widget[method]) {
                    return $widget[method].apply(this, Array.prototype.slice.call(arguments, 1));
                } else {
                    throw 'Method ' + method + ' does not exist.';
                }
            }
        }
    };
})(jQuery);
gj.dialog.messages['en-us'] = {
    Close: 'Close',
    DefaultTitle: 'Dialog'
};
/** 
 * @widget Draggable 
 * @plugin Base
 */
gj.draggable = {
    plugins: {}
};

gj.draggable.config = {
    base: {
        handle: undefined,
        vertical: true,
        horizontal: true,
        containment: undefined
    }
};

gj.draggable.methods = {
    init: function (jsConfig) {
        var $handleEl, data, $dragEl = this;

        gj.widget.prototype.init.call(this, jsConfig, 'draggable');
        data = this.data();
        $dragEl.attr('data-draggable', 'true');

        $handleEl = gj.draggable.methods.getHandleElement($dragEl);

        $handleEl.on('touchstart mousedown', function (e) {
            var position = gj.core.position($dragEl[0]);
            $dragEl[0].style.top = position.top + 'px';
            $dragEl[0].style.left = position.left + 'px';
            $dragEl[0].style.position = 'fixed';

            $dragEl.attr('draggable-dragging', true);
            $dragEl.removeAttr('draggable-x').removeAttr('draggable-y');
            gj.documentManager.subscribeForEvent('touchmove', $dragEl.data('guid'), gj.draggable.methods.createMoveHandler($dragEl, $handleEl, data));
            gj.documentManager.subscribeForEvent('mousemove', $dragEl.data('guid'), gj.draggable.methods.createMoveHandler($dragEl, $handleEl, data));
        });

        gj.documentManager.subscribeForEvent('mouseup', $dragEl.data('guid'), gj.draggable.methods.createUpHandler($dragEl));
        gj.documentManager.subscribeForEvent('touchend', $dragEl.data('guid'), gj.draggable.methods.createUpHandler($dragEl));
        gj.documentManager.subscribeForEvent('touchcancel', $dragEl.data('guid'), gj.draggable.methods.createUpHandler($dragEl));

        return $dragEl;
    },

    getHandleElement: function ($dragEl) {
        var $handle = $dragEl.data('handle');
        return ($handle && $handle.length) ? $handle : $dragEl;
    },

    createUpHandler: function ($dragEl) {
        return function (e) {
            if ($dragEl.attr('draggable-dragging') === 'true') {
                $dragEl.attr('draggable-dragging', false);
                gj.documentManager.unsubscribeForEvent('mousemove', $dragEl.data('guid'));
                gj.documentManager.unsubscribeForEvent('touchmove', $dragEl.data('guid'));
                gj.draggable.events.stop($dragEl, { x: $dragEl.mouseX(e), y: $dragEl.mouseY(e) });
            }
        };
    },

    createMoveHandler: function ($dragEl, $handleEl, data) {
        return function (e) {
            var mouseX, mouseY, offsetX, offsetY, prevX, prevY;
            if ($dragEl.attr('draggable-dragging') === 'true') {
                mouseX = Math.round($dragEl.mouseX(e));
                mouseY = Math.round($dragEl.mouseY(e));
                prevX = $dragEl.attr('draggable-x');
                prevY = $dragEl.attr('draggable-y');
                if (prevX && prevY) {
                    offsetX = data.horizontal ? mouseX - parseInt(prevX, 10) : 0;
                    offsetY = data.vertical ? mouseY - parseInt(prevY, 10) : 0;
                    gj.draggable.methods.move($dragEl[0], data, offsetX, offsetY, mouseX, mouseY);
                } else {
                    gj.draggable.events.start($dragEl, mouseX, mouseY);
                }
                $dragEl.attr('draggable-x', mouseX);
                $dragEl.attr('draggable-y', mouseY);
            }
        }
    },

    move: function (dragEl, data, offsetX, offsetY, mouseX, mouseY) {
        var contPosition, maxTop, maxLeft,
            position = gj.core.position(dragEl),
            newTop = position.top + offsetY,
            newLeft = position.left + offsetX;

        if (data.containment) {
            contPosition = gj.core.position(data.containment);
            maxTop = contPosition.top + gj.core.height(data.containment) - gj.core.height(dragEl);
            maxLeft = contPosition.left + gj.core.width(data.containment) - gj.core.width(dragEl);
            if (newTop > contPosition.top && newTop < maxTop) {
                if (contPosition.top >= mouseY || contPosition.bottom <= mouseY) {
                    newTop = position.top;
                }
            } else {
                if (newTop <= contPosition.top) {
                    newTop = contPosition.top + 1;
                } else {
                    newTop = maxTop - 1;
                }
            }
            if (newLeft > contPosition.left && newLeft < maxLeft) {
                if (contPosition.left >= mouseX || contPosition.right <= mouseX) {
                    newLeft = position.left;
                }
            } else {
                if (newLeft <= contPosition.left) {
                    newLeft = contPosition.left + 1;
                } else {
                    newLeft = maxLeft - 1;
                }
            }
        }

        if (false !== gj.draggable.events.drag($(dragEl), newLeft, newTop, mouseX, mouseY)) {
            dragEl.style.top = newTop + 'px';
            dragEl.style.left = newLeft + 'px';
        }
    },

    destroy: function ($dragEl) {
        if ($dragEl.attr('data-draggable') === 'true') {
            gj.documentManager.unsubscribeForEvent('mouseup', $dragEl.data('guid'));
            $dragEl.removeData();
            $dragEl.removeAttr('data-guid').removeAttr('data-type').removeAttr('data-draggable');
            $dragEl.removeAttr('draggable-x').removeAttr('draggable-y').removeAttr('draggable-dragging');
            $dragEl[0].style.top = '';
            $dragEl[0].style.left = '';
            $dragEl[0].style.position = '';
            $dragEl.off('drag').off('start').off('stop');
            gj.draggable.methods.getHandleElement($dragEl).off('mousedown');
        }
        return $dragEl;
    }
};

gj.draggable.events = {
    drag: function ($dragEl, newLeft, newTop, mouseX, mouseY) {
        return $dragEl.triggerHandler('drag', [{ left: newLeft, top: newTop }, { x: mouseX, y: mouseY }]);
    },
    start: function ($dragEl, mouseX, mouseY) {
        $dragEl.triggerHandler('start', [{ x: mouseX, y: mouseY }]);
    },
    stop: function ($dragEl, mousePosition) {
        $dragEl.triggerHandler('stop', [mousePosition]);
    }
};

gj.draggable.widget = function ($element, jsConfig) {
    var self = this,
        methods = gj.draggable.methods;

    if (!$element.destroy) {
        self.destroy = function () {
            return methods.destroy(this);
        };
    }

    $.extend($element, self);
    if ('true' !== $element.attr('data-draggable')) {
        methods.init.call($element, jsConfig);
    }

    return $element;
};

gj.draggable.widget.prototype = new gj.widget();
gj.draggable.widget.constructor = gj.draggable.widget;

(function ($) {
    $.fn.draggable = function (method) {
        var $widget;
        if (this && this.length) {
            if (typeof method === 'object' || !method) {
                return new gj.draggable.widget(this, method);
            } else {
                $widget = new gj.draggable.widget(this, null);
                if ($widget[method]) {
                    return $widget[method].apply(this, Array.prototype.slice.call(arguments, 1));
                } else {
                    throw 'Method ' + method + ' does not exist.';
                }
            }
        }
    };
})(jQuery);
/** 
 * @widget Droppable 
 * @plugin Base
 */
gj.droppable = {
    plugins: {}
};

gj.droppable.config = {
    hoverClass: undefined
};

gj.droppable.methods = {
    init: function (jsConfig) {
        var $dropEl = this;

        gj.widget.prototype.init.call(this, jsConfig, 'droppable');
        $dropEl.attr('data-droppable', 'true');

        gj.documentManager.subscribeForEvent('mousedown', $dropEl.data('guid'), gj.droppable.methods.createMouseDownHandler($dropEl));
        gj.documentManager.subscribeForEvent('mousemove', $dropEl.data('guid'), gj.droppable.methods.createMouseMoveHandler($dropEl));
        gj.documentManager.subscribeForEvent('mouseup', $dropEl.data('guid'), gj.droppable.methods.createMouseUpHandler($dropEl));

        return $dropEl;
    },

    createMouseDownHandler: function ($dropEl) {
        return function (e) {
            $dropEl.isDragging = true;
        }
    },

    createMouseMoveHandler: function ($dropEl) {
        return function (e) {
            if ($dropEl.isDragging) {
                var hoverClass = $dropEl.data('hoverClass'),
                    mousePosition = {
                        x: $dropEl.mouseX(e),
                        y: $dropEl.mouseY(e)
                    },
                    newIsOver = gj.droppable.methods.isOver($dropEl, mousePosition);
                if (newIsOver != $dropEl.isOver) {
                    if (newIsOver) {
                        if (hoverClass) {
                            $dropEl.addClass(hoverClass);
                        }
                        gj.droppable.events.over($dropEl, mousePosition);
                    } else {
                        if (hoverClass) {
                            $dropEl.removeClass(hoverClass);
                        }
                        gj.droppable.events.out($dropEl);
                    }
                }
                $dropEl.isOver = newIsOver;
            }
        }
    },

    createMouseUpHandler: function ($dropEl) {
        return function (e) {
            var mousePosition = {
                left: $dropEl.mouseX(e),
                top: $dropEl.mouseY(e)
            };
            $dropEl.isDragging = false;
            if (gj.droppable.methods.isOver($dropEl, mousePosition)) {
                gj.droppable.events.drop($dropEl);
            }
        }
    },

    isOver: function ($dropEl, mousePosition) {
        var offsetTop = $dropEl.offset().top,
            offsetLeft = $dropEl.offset().left;
        return mousePosition.x > offsetLeft && mousePosition.x < (offsetLeft + $dropEl.outerWidth(true))
            && mousePosition.y > offsetTop && mousePosition.y < (offsetTop + $dropEl.outerHeight(true));
    },

    destroy: function ($dropEl) {
        if ($dropEl.attr('data-droppable') === 'true') {
            gj.documentManager.unsubscribeForEvent('mousedown', $dropEl.data('guid'));
            gj.documentManager.unsubscribeForEvent('mousemove', $dropEl.data('guid'));
            gj.documentManager.unsubscribeForEvent('mouseup', $dropEl.data('guid'));
            $dropEl.removeData();
            $dropEl.removeAttr('data-guid');
            $dropEl.removeAttr('data-droppable');
            $dropEl.off('drop').off('over').off('out');
        }
        return $dropEl;
    }
};

gj.droppable.events = {
    drop: function ($dropEl, offsetX, offsetY) {
        $dropEl.trigger('drop', [{ top: offsetY, left: offsetX }]);
    },
    over: function ($dropEl, mousePosition) {
        $dropEl.trigger('over', [mousePosition]);
    },
    out: function ($dropEl) {
        $dropEl.trigger('out');
    }
};

gj.droppable.widget = function ($element, jsConfig) {
    var self = this,
        methods = gj.droppable.methods;

    self.isOver = false;
    self.isDragging = false;
    self.destroy = function () {
        return methods.destroy(this);
    }

    self.isOver = function (mousePosition) {
        return methods.isOver(this, mousePosition);
    }

    $.extend($element, self);
    if ('true' !== $element.attr('data-droppable')) {
        methods.init.call($element, jsConfig);
    }

    return $element;
};

gj.droppable.widget.prototype = new gj.widget();
gj.droppable.widget.constructor = gj.droppable.widget;

(function ($) {
    $.fn.droppable = function (method) {
        var $widget;
        if (this && this.length) {
            if (typeof method === 'object' || !method) {
                return new gj.droppable.widget(this, method);
            } else {
                $widget = new gj.droppable.widget(this, null);
                if ($widget[method]) {
                    return $widget[method].apply(this, Array.prototype.slice.call(arguments, 1));
                } else {
                    throw 'Method ' + method + ' does not exist.';
                }
            }
        }
    };
})(jQuery);
/**
  * @widget Grid
  * @plugin Base
  */
gj.grid = {
    plugins: {},
    messages: {}
};

gj.grid.config = {
    base: {
        dataSource: undefined,
        addNewAction: undefined,
        columns: [],
        autoGenerateColumns: false,
        defaultColumnSettings: {
            hidden: false,
            width: undefined,
            sortable: false,
            type: 'text',
            title: undefined,
            field: undefined,
            align: undefined,
            cssClass: undefined,
            headerCssClass: undefined,
            tooltip: undefined,
            icon: undefined,
            events: undefined,
            format: 'mm/dd/yyyy',
            decimalDigits: undefined,
            tmpl: undefined,
            stopPropagation: false,
            renderer: undefined,
            filter: undefined
        },

        mapping: {
            dataField: 'items',
            totalRecordsField: 'recordCount'
        },

        params: {},

        paramNames: {
            sortBy: 'sortBy',
            direction: 'direction'
        },
        uiLibrary: 'materialdesign',
        iconsLibrary: 'materialicons',
        selectionType: 'single',
        selectionMethod: 'basic',
        autoLoad: true,
        notFoundText: undefined,
        width: undefined,
        minWidth: undefined,
        headerRowHeight: 'fixed',
        bodyRowHeight: 'autogrow',
        fontSize: undefined,
        primaryKey: undefined,
        locale: 'en-us',

        defaultIconColumnWidth: 70,
        defaultCheckBoxColumnWidth: 70,

        style: {
            wrapper: 'gj-grid-wrapper',
            table: 'gj-grid gj-grid-md',
            loadingCover: 'gj-grid-loading-cover',
            loadingText: 'gj-grid-loading-text',
            header: {
                cell: undefined,
                sortable: 'gj-cursor-pointer gj-unselectable'
            },
            content: {
                rowSelected: 'gj-grid-md-select'
            }
        },

        icons: {
            asc: '▲',
            desc: '▼'
        }
    },

    bootstrap: {
        style: {
            wrapper: 'gj-grid-wrapper',
            table: 'gj-grid gj-grid-bootstrap gj-grid-bootstrap-3 table table-bordered table-hover',
            content: {
                rowSelected: 'active'
            }
        },

        iconsLibrary: 'glyphicons',

        defaultIconColumnWidth: 34,
        defaultCheckBoxColumnWidth: 36
    },

    bootstrap4: {
        style: {
            wrapper: 'gj-grid-wrapper',
            table: 'gj-grid gj-grid-bootstrap gj-grid-bootstrap-4 table table-bordered table-hover',
            content: {
                rowSelected: 'active'
            }
        },

        defaultIconColumnWidth: 42,
        defaultCheckBoxColumnWidth: 44
    },

    materialicons: {
        icons: {
            asc: '<i class="gj-icon arrow-upward" />',
            desc: '<i class="gj-icon arrow-downward" />'
        }
    },

    fontawesome: {
        icons: {
            asc: '<i class="fa fa-sort-amount-asc" aria-hidden="true"></i>',
            desc: '<i class="fa fa-sort-amount-desc" aria-hidden="true"></i>'
        }
    },

    glyphicons: {
        icons: {
            asc: '<span class="glyphicon glyphicon-sort-by-alphabet" />',
            desc: '<span class="glyphicon glyphicon-sort-by-alphabet-alt" />'
        }
    }
};
gj.grid.events = {
    beforeEmptyRowInsert: function ($grid, $row) {
        return $grid.triggerHandler('beforeEmptyRowInsert', [$row]);
    },
    dataBinding: function ($grid, records) {
        return $grid.triggerHandler('dataBinding', [records]);
    },
    dataBound: function ($grid, records, totalRecords) {
        return $grid.triggerHandler('dataBound', [records, totalRecords]);
    },
    rowDataBound: function ($grid, $row, id, record) {
        return $grid.triggerHandler('rowDataBound', [$row, id, record]);
    },
    cellDataBound: function ($grid, $displayEl, id, column, record) {
        return $grid.triggerHandler('cellDataBound', [$displayEl, id, column, record]);
    },
    rowSelect: function ($grid, $row, id, record) {
        return $grid.triggerHandler('rowSelect', [$row, id, record]);
    },
    rowUnselect: function ($grid, $row, id, record) {
        return $grid.triggerHandler('rowUnselect', [$row, id, record]);
    },
    rowRemoving: function ($grid, $row, id, record) {
        return $grid.triggerHandler('rowRemoving', [$row, id, record]);
    },
    destroying: function ($grid) {
        return $grid.triggerHandler('destroying');
    },
    columnHide: function ($grid, column) {
        return $grid.triggerHandler('columnHide', [column]);
    },
    columnShow: function ($grid, column) {
        return $grid.triggerHandler('columnShow', [column]);
    },
    initialized: function ($grid) {
        return $grid.triggerHandler('initialized');
    },
    dataFiltered: function ($grid, records) {
        return $grid.triggerHandler('dataFiltered', [records]);
    }
};
gj.grid.methods = {

    init: function (jsConfig) {
        gj.widget.prototype.init.call(this, jsConfig, 'grid');

        gj.grid.methods.initialize(this);

        if (this.data('autoLoad')) {
            this.reload();
        }
        return this;
    },

    getConfig: function (jsConfig, type) {
        var config = gj.widget.prototype.getConfig.call(this, jsConfig, type);
        gj.grid.methods.setDefaultColumnConfig(config.columns, config.defaultColumnSettings);
        return config;
    },

    setDefaultColumnConfig: function (columns, defaultColumnSettings) {
        var column, i;
        if (columns && columns.length) {
            for (i = 0; i < columns.length; i++) {
                column = $.extend(true, {}, defaultColumnSettings);
                $.extend(true, column, columns[i]);
                columns[i] = column;
            }
        }
    },

    getHTMLConfig: function () {
        var result = gj.widget.prototype.getHTMLConfig.call(this);
        result.columns = [];
        this.find('thead > tr > th').each(function () {
            var $el = $(this),
                title = $el.text(),
                config = gj.widget.prototype.getHTMLConfig.call($el);
            config.title = title;
            if (!config.field) {
                config.field = title;
            }
            if (config.events) {
                config.events = gj.grid.methods.eventsParser(config.events);
            }
            result.columns.push(config);
        });
        return result;
    },

    eventsParser: function (events) {
        var result = {}, list, i, key, func, position;
        list = events.split(',');
        for (i = 0; i < list.length; i++) {
            position = list[i].indexOf(':');
            if (position > 0) {
                key = $.trim(list[i].substr(0, position));
                func = $.trim(list[i].substr(position + 1, list[i].length));
                result[key] = eval('window.' + func); //window[func]; //TODO: eveluate functions from string
            }
        }
        return result;
    },

    initialize: function ($grid) {
        var data = $grid.data(),
            $wrapper = $grid.parent('div[data-role="wrapper"]');

        gj.grid.methods.localization(data);

        if ($wrapper.length === 0) {
            $wrapper = $('<div data-role="wrapper" />').addClass(data.style.wrapper); //The css class needs to be added before the wrapping, otherwise doesn't work.
            $grid.wrap($wrapper);
        } else {
            $wrapper.addClass(data.style.wrapper);
        }

        if (data.width) {
            $grid.parent().css('width', data.width);
        }
        if (data.minWidth) {
            $grid.css('min-width', data.minWidth);
        }
        if (data.fontSize) {
            $grid.css('font-size', data.fontSize);
        }
        if (data.headerRowHeight === 'autogrow') {
            $grid.addClass('autogrow-header-row');
        }
        if (data.bodyRowHeight === 'fixed') {
            $grid.addClass('fixed-body-rows');
        }
        $grid.addClass(data.style.table);
        if ('checkbox' === data.selectionMethod) {
            data.columns.splice(gj.grid.methods.getColumnPositionNotInRole($grid), 0, {
                title: '',
                width: data.defaultCheckBoxColumnWidth,
                align: 'center',
                type: 'checkbox',
                role: 'selectRow',
                events: {
                    click: function (e) {
                        gj.grid.methods.setSelected($grid, e.data.id, $(this).closest('tr'));
                    }
                },
                headerCssClass: 'gj-grid-select-all',
                stopPropagation: true
            });
        }

        if ($grid.children('tbody').length === 0) {
            $grid.append($('<tbody/>'));
        }

        gj.grid.methods.renderHeader($grid);
        gj.grid.methods.appendEmptyRow($grid, '&nbsp;');
        gj.grid.events.initialized($grid);
    },

    localization: function (data) {
        if (!data.notFoundText) {
            data.notFoundText = gj.grid.messages[data.locale].NoRecordsFound;
        }
    },

    renderHeader: function ($grid) {
        var data, columns, style, $thead, $row, $cell, $title, i, $checkAllBoxes;

        data = $grid.data();
        columns = data.columns;
        style = data.style.header;

        $thead = $grid.children('thead');
        if ($thead.length === 0) {
            $thead = $('<thead />');
            $grid.prepend($thead);
        }

        $row = $('<tr data-role="caption" />');
        for (i = 0; i < columns.length; i += 1) {
            $cell = $('<th data-field="' + (columns[i].field || '') + '" />');
            if (columns[i].width) {
                $cell.attr('width', columns[i].width);
            } else if (columns[i].type === 'checkbox') {
                $cell.attr('width', data.defaultIconColumnWidth);
            }
            $cell.addClass(style.cell);
            if (columns[i].headerCssClass) {
                $cell.addClass(columns[i].headerCssClass);
            }
            $cell.css('text-align', columns[i].align || 'left');
            if ('checkbox' === data.selectionMethod && 'multiple' === data.selectionType &&
                'checkbox' === columns[i].type && 'selectRow' === columns[i].role) {
                $checkAllBoxes = $cell.find('input[data-role="selectAll"]');
                if ($checkAllBoxes.length === 0) {
                    $checkAllBoxes = $('<input type="checkbox" data-role="selectAll" />');
                    $cell.append($checkAllBoxes);
                    $checkAllBoxes.checkbox({ uiLibrary: data.uiLibrary });
                }
                $checkAllBoxes.off('click').on('click', function () {
                    if (this.checked) {
                        $grid.selectAll();
                    } else {
                        $grid.unSelectAll();
                    }
                });
            } else {
                $title = $('<div data-role="title"/>').html(typeof (columns[i].title) === 'undefined' ? columns[i].field : columns[i].title);
                $cell.append($title);
                if (columns[i].sortable) {
                    $title.addClass(style.sortable);
                    $title.on('click', gj.grid.methods.createSortHandler($grid, columns[i]));
                }
            }
            if (columns[i].hidden) {
                $cell.hide();
            }
            $row.append($cell);
        }

        $thead.empty().append($row);
    },

    createSortHandler: function ($grid, column) {
        return function () {
            var data, params = {};
            if ($grid.count() > 0) {
                data = $grid.data();
                params[data.paramNames.sortBy] = column.field;
                column.direction = (column.direction === 'asc' ? 'desc' : 'asc');
                params[data.paramNames.direction] = column.direction;
                $grid.reload(params);
            }
        };
    },

    updateHeader: function ($grid) {
        var $sortIcon, $cellTitle,
            data = $grid.data(),
            sortBy = data.params[data.paramNames.sortBy],
            direction = data.params[data.paramNames.direction];

        $grid.find('thead tr th [data-role="sorticon"]').remove();

        if (sortBy) {
            position = gj.grid.methods.getColumnPosition($grid.data('columns'), sortBy);
            if (position > -1) {
                $cellTitle = $grid.find('thead tr th:eq(' + position + ') div[data-role="title"]');
                $sortIcon = $('<div data-role="sorticon" class="gj-unselectable" />').append(('desc' === direction) ? data.icons.desc : data.icons.asc);
                $cellTitle.after($sortIcon);
            }
        }
    },

    useHtmlDataSource: function ($grid, data) {
        var dataSource = [], i, j, $cells, record,
            $rows = $grid.find('tbody tr[data-role != "empty"]');
        for (i = 0; i < $rows.length; i++) {
            $cells = $($rows[i]).find('td');
            record = {};
            for (j = 0; j < $cells.length; j++) {
                record[data.columns[j].field] = $($cells[j]).html();
            }
            dataSource.push(record);
        }
        data.dataSource = dataSource;
    },

    startLoading: function ($grid) {
        var $tbody, $cover, $loading, width, height, top, data;
        gj.grid.methods.stopLoading($grid);
        data = $grid.data();
        if (0 === $grid.outerHeight()) {
            return;
        }
        $tbody = $grid.children('tbody');
        width = $tbody.outerWidth(false);
        height = $tbody.outerHeight(false);
        top = Math.abs($grid.parent().offset().top - $tbody.offset().top);
        $cover = $('<div data-role="loading-cover" />').addClass(data.style.loadingCover).css({
            width: width,
            height: height,
            top: top
        });
        $loading = $('<div data-role="loading-text">' + gj.grid.messages[data.locale].Loading + '</div>').addClass(data.style.loadingText);
        $loading.insertAfter($grid);
        $cover.insertAfter($grid);
        $loading.css({
            top: top + (height / 2) - ($loading.outerHeight(false) / 2),
            left: (width / 2) - ($loading.outerWidth(false) / 2)
        });
    },

    stopLoading: function ($grid) {
        $grid.parent().find('div[data-role="loading-cover"]').remove();
        $grid.parent().find('div[data-role="loading-text"]').remove();
    },

    appendEmptyRow: function ($grid, caption) {
        var data, $row, $cell, $wrapper;
        data = $grid.data();
        $row = $('<tr data-role="empty"/>');
        $cell = $('<td/>').css({ width: '100%', 'text-align': 'center' });
        $cell.attr('colspan', gj.grid.methods.countVisibleColumns($grid));
        $wrapper = $('<div />').html(caption || data.notFoundText);
        $cell.append($wrapper);
        $row.append($cell);

        gj.grid.events.beforeEmptyRowInsert($grid, $row);

        $grid.append($row);
    },

    autoGenerateColumns: function ($grid, records) {
        var names, value, type, i, data = $grid.data();
        data.columns = [];
        if (records.length > 0) {
            names = Object.getOwnPropertyNames(records[0]);
            for (i = 0; i < names.length; i++) {
                value = records[0][names[i]];
                type = 'text';
                if (value) {
                    if (typeof value === 'number') {
                        type = 'number';
                    } else if (value.indexOf('/Date(') > -1) {
                        type = 'date';
                    }
                }
                data.columns.push({ field: names[i], type: type });
            }
            gj.grid.methods.setDefaultColumnConfig(data.columns, data.defaultColumnSettings);
        }
        gj.grid.methods.renderHeader($grid);
    },

    loadData: function ($grid) {
        var data, records, i, recLen, rowCount, $tbody, $rows, $row;

        data = $grid.data();
        records = $grid.getAll();
        gj.grid.events.dataBinding($grid, records);
        recLen = records.length;
        gj.grid.methods.stopLoading($grid);

        if (data.autoGenerateColumns) {
            gj.grid.methods.autoGenerateColumns($grid, records);
        }

        $tbody = $grid.children('tbody');
        if ('checkbox' === data.selectionMethod && 'multiple' === data.selectionType) {
            $grid.find('thead input[data-role="selectAll"]').prop('checked', false);
        }
        $tbody.children('tr').not('[data-role="row"]').remove();
        if (0 === recLen) {
            $tbody.empty();
            gj.grid.methods.appendEmptyRow($grid);
        }

        $rows = $tbody.children('tr');

        rowCount = $rows.length;

        for (i = 0; i < rowCount; i++) {
            if (i < recLen) {
                $row = $rows.eq(i);
                gj.grid.methods.renderRow($grid, $row, records[i], i);
            } else {
                $tbody.find('tr[data-role="row"]:gt(' + (i - 1) + ')').remove();
                break;
            }
        }

        for (i = rowCount; i < recLen; i++) {
            gj.grid.methods.renderRow($grid, null, records[i], i);
        }
        gj.grid.events.dataBound($grid, records, data.totalRecords);
    },

    getId: function (record, primaryKey, position) {
        return (primaryKey && record[primaryKey]) ? record[primaryKey] : position;
    },

    renderRow: function ($grid, $row, record, position) {
        var id, $cell, i, data, mode;
        data = $grid.data();
        if (!$row || $row.length === 0) {
            mode = 'create';
            $row = $('<tr data-role="row"/>');
            $grid.children('tbody').append($row);
        } else {
            mode = 'update';
            $row.removeClass(data.style.content.rowSelected).removeAttr('data-selected').off('click');
        }
        id = gj.grid.methods.getId(record, data.primaryKey, (position + 1));
        $row.attr('data-position', position + 1);
        if (data.selectionMethod !== 'checkbox') {
            $row.on('click', gj.grid.methods.createRowClickHandler($grid, id));
        }
        for (i = 0; i < data.columns.length; i++) {
            if (mode === 'update') {
                $cell = $row.find('td:eq(' + i + ')');
                gj.grid.methods.renderCell($grid, $cell, data.columns[i], record, id);
            } else {
                $cell = gj.grid.methods.renderCell($grid, null, data.columns[i], record, id);
                $row.append($cell);
            }
        }
        gj.grid.events.rowDataBound($grid, $row, id, record);
    },

    renderCell: function ($grid, $cell, column, record, id, mode) {
        var $displayEl, key;

        if (!$cell || $cell.length === 0) {
            $cell = $('<td/>');
            $displayEl = $('<div data-role="display" />');
            column.align && $cell.css('text-align', column.align);
            column.cssClass && $cell.addClass(column.cssClass);
            $cell.append($displayEl);
            mode = 'create';
        } else {
            $displayEl = $cell.find('div[data-role="display"]');
            mode = 'update';
        }

        gj.grid.methods.renderDisplayElement($grid, $displayEl, column, record, id, mode);
        if ('update' === mode) {
            $cell.off();
            $displayEl.off();
        }
        if (column.events) {
            for (key in column.events) {
                if (column.events.hasOwnProperty(key)) {
                    $cell.on(key, { id: id, field: column.field, record: record, grid: $grid }, gj.grid.methods.createCellEventHandler(column, column.events[key]));
                }
            }
        }
        if (column.hidden) {
            $cell.hide();
        }

        gj.grid.events.cellDataBound($grid, $displayEl, id, column, record);

        return $cell;
    },

    createCellEventHandler: function (column, func) {
        return function (e) {
            if (column.stopPropagation) {
                e.stopPropagation();
            }
            func.call(this, e);
        };
    },

    renderDisplayElement: function ($grid, $displayEl, column, record, id, mode) {
        var text, $checkbox;

        if ('checkbox' === column.type && gj.checkbox) {
            if ('create' === mode) {
                $checkbox = $('<input type="checkbox" />').val(id).prop('checked', (record[column.field] ? true : false));
                column.role && $checkbox.attr('data-role', column.role);
                $displayEl.append($checkbox);
                $checkbox.checkbox({ uiLibrary: $grid.data('uiLibrary') });
                if (column.role === 'selectRow') {
                    $checkbox.on('click', function () { return false; });
                } else {
                    $checkbox.prop('disabled', true);
                }
            } else {
                $displayEl.find('input[type="checkbox"]').val(id).prop('checked', (record[column.field] ? true : false));
            }
        } else if ('icon' === column.type) {
            if ('create' === mode) {
                $displayEl.append($('<span/>').addClass(column.icon).css({ cursor: 'pointer' }));
                $grid.data().uiLibrary === 'bootstrap' && $displayEl.children('span').addClass('glyphicon');
                column.stopPropagation = true;
            }
        } else if (column.tmpl) {
            text = column.tmpl;
            column.tmpl.replace(/\{(.+?)\}/g, function ($0, $1) {
                text = text.replace($0, gj.grid.methods.formatText(record[$1], column, $grid));
            });
            $displayEl.html(text);
        } else if (column.renderer && typeof (column.renderer) === 'function') {
            text = column.renderer(record[column.field], record, $displayEl.parent(), $displayEl, id, $grid);
            if (text) {
                $displayEl.html(text);
            }
        } else {
            text = gj.grid.methods.formatText(record[column.field], column, $grid);
            if (!column.tooltip && record[column.field]) {
                $displayEl.attr('title', record[column.field]);
            }
            $displayEl.html(text);
        }
        if (column.tooltip && 'create' === mode) {
            $displayEl.attr('title', column.tooltip);
        }
    },

    formatText: function (text, column, $grid) {
        if (text && ['date', 'time', 'datetime'].indexOf(column.type) > -1) {
            text = gj.core.formatDate(gj.core.parseDate(text, column.format), column.format);
        } else {
            text = (typeof (text) === 'undefined' || text === null) ? '' : text.toString();
        }
        if (column.decimalDigits && text) {
            text = parseFloat(text).toFixed(column.decimalDigits);
        }
        if ('undefined' !== typeof (column.format) && 0 < column.format.length) {
            var output = /(^(\C|\N|\D))([a-zA-Z]{3})*(\d*)$/g.exec(column.format);

            if (output && 5 === output.length) {
                var style = output[2];
                var code = output[3];
                var digits = parseInt(output[4]);
                var cultureCode = $grid.getFullConfig().culture;
                var dateFormat = $grid.getFullConfig().dateFormat || 'dd/mm/yyyy';
                var formatOptions = {};

                if ('C' === style) /* Currency */ {
                    if (!code || 3 != code.length) {
                        throw 'Currency formatting requires 3 letter currency code such as UDS, EUR, TRY etc. Format provided was = ' + column.format;
                    }
                    if (isNaN(digits)) digits = 2;
                    digits = Math.max(digits, 2);
                    formatOptions['currency'] = code;
                    formatOptions['currencyDisplay'] = 'symbol';
                    formatOptions['style'] = 'currency';
                }
                else if ('N' === style) /* Number */ {
                    if (isNaN(digits)) digits = 0;
                    formatOptions['style'] = 'decimal';
                }
                else if ('d' === style) /* Short date */ {
                    if (0 < text.length) {

                        text = gj.core.formatDate(new Date(text), dateFormat);
                        return text;
                    }
                    else {
                        return text;
                    }
                }

                if ('undefined' === typeof (cultureCode) || 2 > cultureCode.length) {
                    cultureCode = 'default';
                }

                formatOptions['minimumFractionDigits'] = digits;
                formatOptions['maximumFractionDigits'] = digits;

                text = parseFloat(text).toLocaleString(cultureCode, formatOptions)
            }
        }
        return text;
    },

    setRecordsData: function ($grid, response) {
        var records = [],
            totalRecords = 0,
            data = $grid.data();
        if ($.isArray(response)) {
            records = response;
            totalRecords = response.length;
        } else if (data && data.mapping && $.isArray(response[data.mapping.dataField])) {
            records = response[data.mapping.dataField];
            totalRecords = response[data.mapping.totalRecordsField];
            if (!totalRecords || isNaN(totalRecords)) {
                totalRecords = 0;
            }
        }
        $grid.data('records', records);
        $grid.data('totalRecords', totalRecords);
        return records;
    },

    createRowClickHandler: function ($grid, id) {
        return function () {
            gj.grid.methods.setSelected($grid, id, $(this));
        };
    },

    selectRow: function ($grid, data, $row, id) {
        var $checkbox;
        $row.addClass(data.style.content.rowSelected);
        $row.attr('data-selected', 'true');
        if ('checkbox' === data.selectionMethod) {
            $checkbox = $row.find('input[type="checkbox"][data-role="selectRow"]');
            $checkbox.length && !$checkbox.prop('checked') && $checkbox.prop('checked', true);
            if ('multiple' === data.selectionType && $grid.getSelections().length === $grid.count(false)) {
                $grid.find('thead input[data-role="selectAll"]').prop('checked', true);
            }
        }
        return gj.grid.events.rowSelect($grid, $row, id, $grid.getById(id));
    },

    unselectRow: function ($grid, data, $row, id) {
        var $checkbox;
        if ($row.attr('data-selected') === 'true') {
            $row.removeClass(data.style.content.rowSelected);
            if ('checkbox' === data.selectionMethod) {
                $checkbox = $row.find('td input[type="checkbox"][data-role="selectRow"]');
                $checkbox.length && $checkbox.prop('checked') && $checkbox.prop('checked', false);
                if ('multiple' === data.selectionType) {
                    $grid.find('thead input[data-role="selectAll"]').prop('checked', false);
                }
            }
            $row.removeAttr('data-selected');
            return gj.grid.events.rowUnselect($grid, $row, id, $grid.getById(id));
        }
    },

    setSelected: function ($grid, id, $row) {
        var data = $grid.data();
        if (!$row || !$row.length) {
            $row = gj.grid.methods.getRowById($grid, id);
        }
        if ($row) {
            if ($row.attr('data-selected') === 'true') {
                gj.grid.methods.unselectRow($grid, data, $row, id);
            } else {
                if ('single' === data.selectionType) {
                    $row.siblings('[data-selected="true"]').each(function () {
                        var $row = $(this),
                            id = gj.grid.methods.getId($row, data.primaryKey, $row.data('position'));
                        gj.grid.methods.unselectRow($grid, data, $row, id);
                    });
                }
                gj.grid.methods.selectRow($grid, data, $row, id);
            }
        }
        return $grid;
    },

    selectAll: function ($grid) {
        var data = $grid.data();
        $grid.find('tbody tr[data-role="row"]').each(function () {
            var $row = $(this),
                position = $row.data('position'),
                record = $grid.get(position),
                id = gj.grid.methods.getId(record, data.primaryKey, position);
            gj.grid.methods.selectRow($grid, data, $row, id);
        });
        $grid.find('thead input[data-role="selectAll"]').prop('checked', true);
        return $grid;
    },

    unSelectAll: function ($grid) {
        var data = $grid.data();
        $grid.find('tbody tr').each(function () {
            var $row = $(this),
                position = $row.data('position'),
                record = $grid.get(position),
                id = gj.grid.methods.getId(record, data.primaryKey, position);
            gj.grid.methods.unselectRow($grid, data, $row, id);
            $row.find('input[type="checkbox"][data-role="selectRow"]').prop('checked', false);
        });
        $grid.find('thead input[data-role="selectAll"]').prop('checked', false);
        return $grid;
    },

    getSelected: function ($grid) {
        var result = null, selections, record, position;
        selections = $grid.find('tbody>tr[data-selected="true"]');
        if (selections.length > 0) {
            position = $(selections[0]).data('position');
            record = $grid.get(position);
            result = gj.grid.methods.getId(record, $grid.data().primaryKey, position);
        }
        return result;
    },

    getSelectedRows: function ($grid) {
        var data = $grid.data();
        return $grid.find('tbody>tr[data-selected="true"]');
    },

    getSelections: function ($grid) {
        var result = [], position, record,
            data = $grid.data(),
            $selections = gj.grid.methods.getSelectedRows($grid);
        if (0 < $selections.length) {
            $selections.each(function () {
                position = $(this).data('position');
                record = $grid.get(position);
                result.push(gj.grid.methods.getId(record, data.primaryKey, position));
            });
        }
        return result;
    },

    getById: function ($grid, id) {
        var result = null, i, primaryKey = $grid.data('primaryKey'), records = $grid.data('records');
        if (primaryKey) {
            for (i = 0; i < records.length; i++) {
                if (records[i][primaryKey] == id) {
                    result = records[i];
                    break;
                }
            }
        } else {
            result = $grid.get(id);
        }
        return result;
    },

    getRecVPosById: function ($grid, id) {
        var result = id, i, data = $grid.data();
        if (data.primaryKey) {
            for (i = 0; i < data.dataSource.length; i++) {
                if (data.dataSource[i][data.primaryKey] == id) {
                    result = i;
                    break;
                }
            }
        }
        return result;
    },

    getRowById: function ($grid, id) {
        var records = $grid.getAll(false),
            primaryKey = $grid.data('primaryKey'),
            $result = undefined,
            position,
            i;
        if (primaryKey) {
            for (i = 0; i < records.length; i++) {
                if (records[i][primaryKey] == id) {
                    position = i + 1;
                    break;
                }
            }
        } else {
            position = id;
        }
        if (position) {
            $result = $grid.children('tbody').children('tr[data-position="' + position + '"]');
        }
        return $result;
    },

    getByPosition: function ($grid, position) {
        return $grid.getAll(false)[position - 1];
    },

    getColumnPosition: function (columns, field) {
        var position = -1, i;
        for (i = 0; i < columns.length; i++) {
            if (columns[i].field === field) {
                position = i;
                break;
            }
        }
        return position;
    },

    getColumnInfo: function ($grid, field) {
        var i, result = {}, data = $grid.data();
        for (i = 0; i < data.columns.length; i += 1) {
            if (data.columns[i].field === field) {
                result = data.columns[i];
                break;
            }
        }
        return result;
    },

    getCell: function ($grid, id, field) {
        var position, $row, $result = null;
        position = gj.grid.methods.getColumnPosition($grid.data('columns'), field);
        if (position > -1) {
            $row = gj.grid.methods.getRowById($grid, id);
            $result = $row.find('td:eq(' + position + ') div[data-role="display"]');
        }
        return $result;
    },

    setCellContent: function ($grid, id, field, value) {
        var column, $displayEl = gj.grid.methods.getCell($grid, id, field);
        if ($displayEl) {
            $displayEl.empty();
            if (typeof (value) === 'object') {
                $displayEl.append(value);
            } else {
                column = gj.grid.methods.getColumnInfo($grid, field);
                gj.grid.methods.renderDisplayElement($grid, $displayEl, column, $grid.getById(id), id, 'update');
            }
        }
    },

    clone: function (source) {
        var target = [];
        $.each(source, function () {
            target.push(this.clone());
        });
        return target;
    },

    getAll: function ($grid) {
        return $grid.data('records');
    },

    countVisibleColumns: function ($grid) {
        var columns, count, i;
        columns = $grid.data().columns;
        count = 0;
        for (i = 0; i < columns.length; i++) {
            if (columns[i].hidden !== true) {
                count++;
            }
        }
        return count;
    },

    clear: function ($grid, showNotFoundText) {
        var data = $grid.data();
        $grid.xhr && $grid.xhr.abort();
        $grid.children('tbody').empty();
        data.records = [];
        gj.grid.methods.stopLoading($grid);
        gj.grid.methods.appendEmptyRow($grid, showNotFoundText ? data.notFoundText : '&nbsp;');
        gj.grid.events.dataBound($grid, [], 0);
        return $grid;
    },

    render: function ($grid, response) {
        if (response) {
            gj.grid.methods.setRecordsData($grid, response);
            gj.grid.methods.updateHeader($grid);
            gj.grid.methods.loadData($grid);
        }
        return $grid;
    },

    filter: function ($grid) {
        var field, column,
            data = $grid.data(),
            records = data.dataSource.slice();

        if (data.params[data.paramNames.sortBy]) {
            column = gj.grid.methods.getColumnInfo($grid, data.params[data.paramNames.sortBy]);
            records.sort(column.sortable.sorter ? column.sortable.sorter(column.direction, column) : gj.grid.methods.createDefaultSorter(column.direction, column.field));
        }

        for (field in data.params) {
            if (data.params[field] && !data.paramNames[field]) {
                column = gj.grid.methods.getColumnInfo($grid, field);
                if (!$.isEmptyObject({})) {
                    records = $.grep(records, function (record) {
                        var value = record[field] || '',
                            searchStr = data.params[field] || '';
                        return column && typeof (column.filter) === 'function' ? column.filter(value, searchStr) : (value.toUpperCase().indexOf(searchStr.toUpperCase()) > -1);
                    });
                }
            }
        }

        gj.grid.events.dataFiltered($grid, records);

        return records;
    },

    createDefaultSorter: function (direction, field) {
        return function (recordA, recordB) {
            var a = (recordA[field] || '').toString(),
                b = (recordB[field] || '').toString();
            return (direction === 'asc') ? a.localeCompare(b) : b.localeCompare(a);
        };
    },

    destroy: function ($grid, keepTableTag, keepWrapperTag) {
        var data = $grid.data();
        if (data) {
            gj.grid.events.destroying($grid);
            gj.grid.methods.stopLoading($grid);
            $grid.xhr && $grid.xhr.abort();
            $grid.off();
            if (keepWrapperTag === false && $grid.parent('div[data-role="wrapper"]').length > 0) {
                $grid.unwrap();
            }
            $grid.removeData();
            if (keepTableTag === false) {
                $grid.remove();
            } else {
                $grid.removeClass().empty();
            }
            $grid.removeAttr('data-type');
        }
        return $grid;
    },

    showColumn: function ($grid, field) {
        var data = $grid.data(),
            position = gj.grid.methods.getColumnPosition(data.columns, field),
            $cells;

        if (position > -1) {
            $grid.find('thead>tr').each(function () {
                $(this).children('th').eq(position).show();
            });
            $.each($grid.find('tbody>tr'), function () {
                $(this).children('td').eq(position).show();
            });
            data.columns[position].hidden = false;

            $cells = $grid.find('tbody > tr[data-role="empty"] > td');
            if ($cells && $cells.length) {
                $cells.attr('colspan', gj.grid.methods.countVisibleColumns($grid));
            }

            gj.grid.events.columnShow($grid, data.columns[position]);
        }

        return $grid;
    },

    hideColumn: function ($grid, field) {
        var data = $grid.data(),
            position = gj.grid.methods.getColumnPosition(data.columns, field),
            $cells;

        if (position > -1) {
            $grid.find('thead>tr').each(function () {
                $(this).children('th').eq(position).hide();
            });
            $.each($grid.find('tbody>tr'), function () {
                $(this).children('td').eq(position).hide();
            });
            data.columns[position].hidden = true;

            $cells = $grid.find('tbody > tr[data-role="empty"] > td');
            if ($cells && $cells.length) {
                $cells.attr('colspan', gj.grid.methods.countVisibleColumns($grid));
            }

            gj.grid.events.columnHide($grid, data.columns[position]);
        }

        return $grid;
    },

    isLastRecordVisible: function () {
        return true;
    },

    addRow: function ($grid, record) {
        var data = $grid.data();
        data.totalRecords = $grid.data('totalRecords') + 1;
        gj.grid.events.dataBinding($grid, [record]);
        data.records.push(record);
        if ($.isArray(data.dataSource)) {
            data.dataSource.push(record);
        }
        if (data.totalRecords === 1) {
            $grid.children('tbody').empty();
        }
        if (true || gj.grid.methods.isLastRecordVisible($grid)) {
            gj.grid.methods.renderRow($grid, null, record, $grid.count() - 1);
        }
        gj.grid.events.dataBound($grid, [record], data.totalRecords);
        return $grid;
    },

    updateRow: function ($grid, id, record) {
        var $row = gj.grid.methods.getRowById($grid, id),
            data = $grid.data(), position;
        data.records[$row.data('position') - 1] = record;
        if ($.isArray(data.dataSource)) {
            position = gj.grid.methods.getRecVPosById($grid, id);
            data.dataSource[position] = record;
        }
        gj.grid.methods.renderRow($grid, $row, record, $row.index());
        return $grid;
    },

    removeRow: function ($grid, id) {
        var position,
            data = $grid.data(),
            $row = gj.grid.methods.getRowById($grid, id);

        gj.grid.events.rowRemoving($grid, $row, id, $grid.getById(id));
        if ($.isArray(data.dataSource)) {
            position = gj.grid.methods.getRecVPosById($grid, id);
            data.dataSource.splice(position, 1);
        }
        $grid.reload();
        return $grid;
    },

    count: function ($grid, includeAllRecords) {
        return includeAllRecords ? $grid.data().totalRecords : $grid.getAll().length;
    },

    getColumnPositionByRole: function ($grid, role) {
        var i, result, columns = $grid.data('columns');
        for (i = 0; i < columns.length; i++) {
            if (columns[i].role === role) {
                result = i;
                break;
            }
        }
        return result;
    },

    getColumnPositionNotInRole: function ($grid) {
        var i, result = 0, columns = $grid.data('columns');
        for (i = 0; i < columns.length; i++) {
            if (!columns[i].role) {
                result = i;
                break;
            }
        }
        return result;
    }
};
gj.grid.widget = function ($grid, jsConfig) {
    var self = this,
        methods = gj.grid.methods;
    self.reload = function (params) {
        methods.startLoading(this);
        return gj.widget.prototype.reload.call(this, params);
    };
    self.clear = function (showNotFoundText) {
        return methods.clear(this, showNotFoundText);
    };
    self.count = function (includeAllRecords) {
        return methods.count(this, includeAllRecords);
    };
    self.render = function (response) {
        return methods.render($grid, response);
    };
    self.destroy = function (keepTableTag, keepWrapperTag) {
        return methods.destroy(this, keepTableTag, keepWrapperTag);
    };
    self.setSelected = function (id) {
        return methods.setSelected(this, id);
    };
    self.getSelected = function () {
        return methods.getSelected(this);
    };
    self.getSelections = function () {
        return methods.getSelections(this);
    };
    self.selectAll = function () {
        return methods.selectAll(this);
    };
    self.unSelectAll = function () {
        return methods.unSelectAll(this);
    };
    self.getById = function (id) {
        return methods.getById(this, id);
    };
    self.get = function (position) {
        return methods.getByPosition(this, position);
    };
    self.getAll = function (includeAllRecords) {
        return methods.getAll(this, includeAllRecords);
    };
    self.showColumn = function (field) {
        return methods.showColumn(this, field);
    };
    self.hideColumn = function (field) {
        return methods.hideColumn(this, field);
    };
    self.addRow = function (record) {
        return methods.addRow(this, record);
    };
    self.updateRow = function (id, record) {
        return methods.updateRow(this, id, record);
    };
    self.setCellContent = function (id, index, value) {
        methods.setCellContent(this, id, index, value);
    };
    self.removeRow = function (id) {
        return methods.removeRow(this, id);
    };
    self.getRowById = function (id) {
        return methods.getRowById(this, id);
    };
    self.scrollToRow = function (id) {
        let row = methods.getRowById(this, id);
        if (row) {
            row.closest('tbody').animate({ scrollTop: row.offset().top });
        }
    };

    $.extend($grid, self);
    if ('grid' !== $grid.attr('data-type')) {
        methods.init.call($grid, jsConfig);
    }

    return $grid;
}

gj.grid.widget.prototype = new gj.widget();
gj.grid.widget.constructor = gj.grid.widget;

gj.grid.widget.prototype.getConfig = gj.grid.methods.getConfig;
gj.grid.widget.prototype.getHTMLConfig = gj.grid.methods.getHTMLConfig;

(function ($) {
    $.fn.grid = function (method) {
        var $widget;
        if (this && this.length) {
            if (typeof method === 'object' || !method) {
                return new gj.grid.widget(this, method);
            } else {
                $widget = new gj.grid.widget(this, null);
                if ($widget[method]) {
                    return $widget[method].apply(this, Array.prototype.slice.call(arguments, 1));
                } else {
                    throw 'Method ' + method + ' does not exist.';
                }
            }
        }
    };
})(jQuery);
gj.grid.plugins.fixedHeader = {
    config: {
        base: {
            fixedHeader: false,

            height: 300
        }
    },

    private: {
        init: function ($grid) {
            var data = $grid.data(),
                $tbody = $grid.children('tbody'),
                $thead = $grid.children('thead'),
                bodyHeight = data.height - $thead.outerHeight() - ($grid.children('tfoot').outerHeight() || 0);
            $grid.addClass('gj-grid-scrollable');
            $tbody.css('width', $thead.outerWidth());
            $tbody.height(bodyHeight);
        },

        refresh: function ($grid) {
            var i, width,
                data = $grid.data(),
                $tbody = $grid.children('tbody'),
                $thead = $grid.children('thead'),
                $tbodyCells = $grid.find('tbody tr[data-role="row"] td'),
                $theadCells = $grid.find('thead tr[data-role="caption"] th');

            if ($grid.children('tbody').height() < gj.grid.plugins.fixedHeader.private.getRowsHeight($grid)) {
                $tbody.css('width', $thead.outerWidth() + gj.grid.plugins.fixedHeader.private.getScrollBarWidth() + (navigator.userAgent.toLowerCase().indexOf('firefox') > -1 ? 1 : 0));
            } else {
                $tbody.css('width', $thead.outerWidth());
            }

            for (i = 0; i < $theadCells.length; i++) {
                width = $($theadCells[i]).outerWidth();
                if (i === 0 && gj.core.isIE()) {
                    width = width - 1;
                }
                $($tbodyCells[i]).attr('width', width);
            }
        },

        getRowsHeight: function ($grid) {
            var total = 0;
            $grid.find('tbody tr').each(function () {
                total += $(this).height();
            });
            return total;
        },

        getScrollBarWidth: function () {
            var inner = document.createElement('p');
            inner.style.width = "100%";
            inner.style.height = "200px";

            var outer = document.createElement('div');
            outer.style.position = "absolute";
            outer.style.top = "0px";
            outer.style.left = "0px";
            outer.style.visibility = "hidden";
            outer.style.width = "200px";
            outer.style.height = "150px";
            outer.style.overflow = "hidden";
            outer.appendChild(inner);

            document.body.appendChild(outer);
            var w1 = inner.offsetWidth;
            outer.style.overflow = 'scroll';
            var w2 = inner.offsetWidth;
            if (w1 == w2) w2 = outer.clientWidth;

            document.body.removeChild(outer);

            return (w1 - w2);
        }
    },

    public: {
    },

    events: {
    },

    configure: function ($grid, fullConfig, clientConfig) {
        $.extend(true, $grid, gj.grid.plugins.fixedHeader.public);
        var data = $grid.data();
        if (clientConfig.fixedHeader) {
            $grid.on('initialized', function () {
                gj.grid.plugins.fixedHeader.private.init($grid);
            });
            $grid.on('dataBound', function () {
                gj.grid.plugins.fixedHeader.private.refresh($grid);
            });
            $grid.on('resize', function () {
                gj.grid.plugins.fixedHeader.private.refresh($grid);
            });
        }
    }
};
gj.grid.plugins.expandCollapseRows = {
    config: {
        base: {
            detailTemplate: undefined,
            keepExpandedRows: true,

            expandedRows: [],

            icons: {
                expandRow: '<i class="gj-icon chevron-right" />',
                collapseRow: '<i class="gj-icon chevron-down" />'
            }
        },

        fontawesome: {
            icons: {
                expandRow: '<i class="fa fa-angle-right" aria-hidden="true"></i>',
                collapseRow: '<i class="fa fa-angle-down" aria-hidden="true"></i>'
            }
        },

        glyphicons: {
            icons: {
                expandRow: '<span class="glyphicon glyphicon-chevron-right" />',
                collapseRow: '<span class="glyphicon glyphicon-chevron-down" />'
            }
        }
    },

    'private': {
        expandDetail: function ($grid, $cell, id) {
            var $contentRow = $cell.closest('tr'),
                $detailsRow = $('<tr data-role="details" />'),
                $detailsCell = $('<td colspan="' + gj.grid.methods.countVisibleColumns($grid) + '" />'),
                $detailsWrapper = $('<div data-role="display" />'),
                data = $grid.data(),
                position = $contentRow.data('position'),
                record = $grid.get(position),
                plugin = gj.grid.plugins.expandCollapseRows;

            if (typeof (id) === undefined) {
                id = gj.grid.methods.getId(record, data.primaryKey, record);
            }
            $detailsRow.append($detailsCell.append($detailsWrapper.append($contentRow.data('details'))));
            $detailsRow.insertAfter($contentRow);
            $cell.children('div[data-role="display"]').empty().append(data.icons.collapseRow);
            $grid.updateDetails($contentRow);
            plugin.private.keepSelection($grid, id);
            plugin.events.detailExpand($grid, $detailsRow.find('td>div'), id);
        },

        collapseDetail: function ($grid, $cell, id) {
            var $contentRow = $cell.closest('tr'),
                $detailsRow = $contentRow.next('tr[data-role="details"]'),
                data = $grid.data(),
                plugin = gj.grid.plugins.expandCollapseRows;

            if (typeof (id) === undefined) {
                id = gj.grid.methods.getId(record, data.primaryKey, record);
            }
            $detailsRow.remove();
            $cell.children('div[data-role="display"]').empty().append(data.icons.expandRow);
            plugin.private.removeSelection($grid, id);
            plugin.events.detailCollapse($grid, $detailsRow.find('td>div'), id);
        },

        keepSelection: function ($grid, id) {
            var data = $grid.data();
            if (data.keepExpandedRows) {
                if ($.isArray(data.expandedRows)) {
                    if (data.expandedRows.indexOf(id) == -1) {
                        data.expandedRows.push(id);
                    }
                } else {
                    data.expandedRows = [id];
                }
            }
        },

        removeSelection: function ($grid, id) {
            var data = $grid.data();
            if (data.keepExpandedRows && $.isArray(data.expandedRows) && data.expandedRows.indexOf(id) > -1) {
                data.expandedRows.splice(data.expandedRows.indexOf(id), 1);
            }
        },

        updateDetailsColSpan: function ($grid) {
            var $cells = $grid.find('tbody > tr[data-role="details"] > td');
            if ($cells && $cells.length) {
                $cells.attr('colspan', gj.grid.methods.countVisibleColumns($grid));
            }
        }
    },

    'public': {
        collapseAll: function () {
            var $grid = this, data = $grid.data(), position;


            if (typeof (data.detailTemplate) !== 'undefined') {
                position = gj.grid.methods.getColumnPositionByRole($grid, 'expander');
                $grid.find('tbody tr[data-role="row"]').each(function () {
                    gj.grid.plugins.expandCollapseRows.private.collapseDetail($grid, $(this).find('td:eq(' + position + ')'));
                });
            }

            if (typeof (data.grouping) !== 'undefined') {
                $grid.find('tbody tr[role="group"]').each(function () {
                    gj.grid.plugins.grouping.private.collapseGroup(data, $(this).find('td:eq(0)'));
                });
            }
            return $grid;
        },
        expandAll: function () {
            var $grid = this, data = $grid.data(), position;

            if (typeof (data.detailTemplate) !== 'undefined') {
                position = gj.grid.methods.getColumnPositionByRole($grid, 'expander');
                $grid.find('tbody tr[data-role="row"]').each(function () {
                    gj.grid.plugins.expandCollapseRows.private.expandDetail($grid, $(this).find('td:eq(' + position + ')'));
                });
            }

            if (typeof (data.grouping) !== 'undefined') {
                $grid.find('tbody tr[role="group"]').each(function () {
                    gj.grid.plugins.grouping.private.expandGroup(data, $(this).find('td:eq(0)'));
                });
            }
            return $grid;
        },
        updateDetails: function ($contentRow) {
            var $grid = this,
                $detailWrapper = $contentRow.data('details'),
                content = $detailWrapper.html(),
                record = $grid.get($contentRow.data('position'));
            var hasDetailContent = false;

            if (record && content) {
                $detailWrapper.html().replace(/\{(.+?)\}/g, function ($0, $1) {
                    var column = gj.grid.methods.getColumnInfo($grid, $1);

                    if ('function' === typeof column.renderer && 'undefined' !== typeof record[$1]) {
                        content = content.replace($0, column.renderer(record[$1], record));
                    }
                    else if ('undefined' !== typeof column.tmpl) {
                        column.tmpl.replace(/\{(.+?)\}/g, function (first, second) {
                            content = content.replace(first, gj.grid.methods.formatText(record[second], column, $grid));
                        });
                    }
                    else {
                        content = content.replace($0, gj.grid.methods.formatText(record[$1], column, $grid));
                    }
                });
                $detailWrapper.html(content);
                var contentText = $(content).text();

                hasDetailContent = contentText && 0 < contentText.length;
            }

            if (!hasDetailContent) {
                $('td.gj-unselectable', $contentRow).children().css('display', 'none')
                gj.grid.plugins.expandCollapseRows.private.collapseDetail($grid, $('td.gj-unselectable'));
            }
            else {
                $('td.gj-unselectable', $contentRow).children().css('display', 'block')
                $('td.gj-unselectable', $contentRow).css('visibility', 'visible');
            }
            return $grid;
        }
    },

    'events': {
        detailExpand: function ($grid, $detailWrapper, id) {
            $grid.triggerHandler('detailExpand', [$detailWrapper, id]);
        },
        detailCollapse: function ($grid, $detailWrapper, id) {
            $grid.triggerHandler('detailCollapse', [$detailWrapper, id]);
        }
    },

    'configure': function ($grid) {
        var column, data = $grid.data();

        $.extend(true, $grid, gj.grid.plugins.expandCollapseRows.public);

        if (typeof (data.detailTemplate) !== 'undefined') {
            column = {
                title: '',
                width: data.defaultIconColumnWidth,
                align: 'center',
                stopPropagation: true,
                cssClass: 'gj-cursor-pointer gj-unselectable',
                tmpl: data.icons.expandRow,
                role: 'expander',
                events: {
                    'click': function (e) {
                        var $cell = $(this), methods = gj.grid.plugins.expandCollapseRows.private;
                        if ($cell.closest('tr').next().attr('data-role') === 'details') {
                            methods.collapseDetail($grid, $cell, e.data.id);
                        } else {
                            methods.expandDetail($grid, $(this), e.data.id);
                        }
                    }
                }
            };
            data.columns = [column].concat(data.columns);

            $grid.on('rowDataBound', function (e, $row, id, record) {
                $row.data('details', $(data.detailTemplate));
            });
            $grid.on('columnShow', function (e, column) {
                gj.grid.plugins.expandCollapseRows.private.updateDetailsColSpan($grid);
            });
            $grid.on('columnHide', function (e, column) {
                gj.grid.plugins.expandCollapseRows.private.updateDetailsColSpan($grid);
            });
            $grid.on('rowRemoving', function (e, $row, id, record) {
                gj.grid.plugins.expandCollapseRows.private.collapseDetail($grid, $row.children('td').first(), id);
            });
            $grid.on('dataBinding', function () {
                $grid.collapseAll();
            });
            $grid.on('pageChanging', function () {
                $grid.collapseAll();
            });
            $grid.on('dataBound', function () {
                var i, $cell, $row, position, data = $grid.data();
                if (data.keepExpandedRows && $.isArray(data.expandedRows)) {
                    for (i = 0; i < data.expandedRows.length; i++) {
                        $row = gj.grid.methods.getRowById($grid, data.expandedRows[i]);
                        if ($row && $row.length) {
                            position = gj.grid.methods.getColumnPositionByRole($grid, 'expander');
                            $cell = $row.children('td:eq(' + position + ')');
                            if ($cell && $cell.length) {
                                gj.grid.plugins.expandCollapseRows.private.expandDetail($grid, $cell);
                            }
                        }
                    }
                }
            });
        }
    }
};
gj.grid.plugins.inlineEditing = {
    renderers: {
        editManager: function (value, record, $cell, $displayEl, id, $grid) {
            var data = $grid.data(),
                $edit = $(data.inlineEditing.editButton).attr('key', id),
                $delete = $(data.inlineEditing.deleteButton).attr('key', id),
                $update = $(data.inlineEditing.updateButton).attr('key', id).hide(),
                $cancel = $(data.inlineEditing.cancelButton).attr('key', id).hide();
            $edit.on('click', function (e) {
                $grid.edit($(this).attr('key'));
            });
            $delete.on('click', function (e) {
                $grid.removeRow($(this).attr('key'));
            });
            $update.on('click', function (e) {
                $grid.update($(this).attr('key'));
            });
            $cancel.on('click', function (e) {
                $grid.cancel($(this).attr('key'));
            });
            $displayEl.empty().append($edit).append($delete).append($update).append($cancel);
        }
    }
};

gj.grid.plugins.inlineEditing.config = {
    base: {
        defaultColumnSettings: {
            editor: undefined,
            editField: undefined,
            mode: 'readEdit'
        },
        inlineEditing: {
            mode: 'click',
            managementColumn: true,

            managementColumnConfig: { width: 300, role: 'managementColumn', align: 'center', renderer: gj.grid.plugins.inlineEditing.renderers.editManager, cssClass: 'gj-grid-management-column' }
        }
    },

    bootstrap: {
        inlineEditing: {
            managementColumnConfig: { width: 200, role: 'managementColumn', align: 'center', renderer: gj.grid.plugins.inlineEditing.renderers.editManager, cssClass: 'gj-grid-management-column' }
        }
    },

    bootstrap4: {
        inlineEditing: {
            managementColumnConfig: { width: 280, role: 'managementColumn', align: 'center', renderer: gj.grid.plugins.inlineEditing.renderers.editManager, cssClass: 'gj-grid-management-column' }
        }
    }
};

gj.grid.plugins.inlineEditing.private = {
    localization: function (data) {
        if (data.uiLibrary === 'bootstrap') {
            data.inlineEditing.editButton = '<button role="edit" class="btn btn-default btn-sm"><span class="glyphicon glyphicon-pencil" aria-hidden="true"></span> ' + gj.grid.messages[data.locale].Edit + '</button>';
            data.inlineEditing.deleteButton = '<button role="delete" class="btn btn-default btn-sm gj-margin-left-10"><span class="glyphicon glyphicon-remove" aria-hidden="true"></span> ' + gj.grid.messages[data.locale].Delete + '</button>';
            data.inlineEditing.updateButton = '<button role="update" class="btn btn-default btn-sm"><span class="glyphicon glyphicon-ok" aria-hidden="true"></span> ' + gj.grid.messages[data.locale].Update + '</button>';
            data.inlineEditing.cancelButton = '<button role="cancel" class="btn btn-default btn-sm gj-margin-left-10"><span class="glyphicon glyphicon-ban-circle" aria-hidden="true"></span> ' + gj.grid.messages[data.locale].Cancel + '</button>';
        } else {
            data.inlineEditing.editButton = '<button role="edit" class="gj-button-md"><i class="gj-icon pencil" /> ' + gj.grid.messages[data.locale].Edit.toUpperCase() + '</button>';
            data.inlineEditing.deleteButton = '<button role="delete" class="gj-button-md"><i class="gj-icon delete" /> ' + gj.grid.messages[data.locale].Delete.toUpperCase() + '</button>';
            data.inlineEditing.updateButton = '<button role="update" class="gj-button-md"><i class="gj-icon check-circle" /> ' + gj.grid.messages[data.locale].Update.toUpperCase() + '</button>';
            data.inlineEditing.cancelButton = '<button role="cancel" class="gj-button-md"><i class="gj-icon cancel" /> ' + gj.grid.messages[data.locale].Cancel.toUpperCase() + '</button>';
        }
    },

    editMode: function ($grid, $cell, column, record) {
        var $displayContainer, $editorContainer, $editorField, value, config, data = $grid.data();
        if ($cell.attr('data-mode') !== 'edit') {
            if (column.editor) {
                gj.grid.plugins.inlineEditing.private.updateOtherCells($grid, column.mode);
                $displayContainer = $cell.find('div[data-role="display"]').hide();
                $editorContainer = $cell.find('div[data-role="edit"]').show();
                if ($editorContainer.length === 0) {
                    $editorContainer = $('<div data-role="edit" />');
                    $cell.append($editorContainer);
                }
                value = record[column.editField || column.field];
                $editorField = $editorContainer.find('input, select, textarea').first();
                if ($editorField.length) {
                    switch (column.type) {
                        case 'checkbox':
                            $editorField.prop('checked', value);
                            break;
                        case 'dropdown':
                            $editorField = $editorField.dropdown('value', value);
                            break;
                        default:
                            $editorField.val(value);
                    }
                } else {
                    if (typeof (column.editor) === 'function') {
                        column.editor($editorContainer, value, record);
                        $editorField = $editorContainer.find('input, select, textarea').first();
                    } else {
                        config = typeof column.editor === "object" ? column.editor : {};
                        config.uiLibrary = data.uiLibrary;
                        config.iconsLibrary = data.iconsLibrary;
                        config.fontSize = $grid.css('font-size');
                        config.showOnFocus = false;
                        if ('checkbox' === column.type && gj.checkbox) {
                            $editorField = $('<input type="checkbox" />').prop('checked', value);
                            $editorContainer.append($editorField);
                            $editorField.checkbox(config);
                        } else if (('date' === column.type && gj.datepicker) || ('time' === column.type && gj.timepicker) || ('datetime' === column.type && gj.datetimepicker)) {
                            $editorField = $('<input type="text" width="100%"/>');
                            $editorContainer.append($editorField);
                            if (column.format) {
                                config.format = column.format;
                            }
                            switch (column.type) {
                                case 'date':
                                    $editorField = $editorField.datepicker(config);
                                    break;
                                case 'time':
                                    $editorField = $editorField.timepicker(config);
                                    break;
                                case 'datetime':
                                    $editorField = $editorField.datetimepicker(config);
                                    break;
                            }
                            if ($editorField.value) {
                                $editorField.value($displayContainer.html());
                            }
                        } else if ('dropdown' === column.type && gj.dropdown) {
                            $editorField = $('<select type="text" width="100%"/>');
                            $editorContainer.append($editorField);
                            config.dataBound = function (e) {
                                var $dropdown = $(this).dropdown();
                                if (column.editField) {
                                    $dropdown.value(record[column.editField]);
                                } else {
                                    $dropdown.value(record[column.field]);
                                }
                            };
                            $editorField = $editorField.dropdown(config);
                        } else {
                            $editorField = $('<input type="text" value="' + value + '" class="gj-width-full"/>');
                            if (data.uiLibrary === 'materialdesign') {
                                $editorField.addClass('gj-textbox-md').css('font-size', $grid.css('font-size'));
                            }
                            $editorContainer.append($editorField);
                        }
                    }
                    if (data.inlineEditing.mode !== 'command' && column.mode !== 'editOnly') {
                        $editorField = $editorContainer.find('input, select, textarea').first();
                        $editorField.on('keyup', function (e) {
                            if (e.keyCode === 13 || e.keyCode === 27) {
                                gj.grid.plugins.inlineEditing.private.displayMode($grid, $cell, column);
                            }
                        });
                    }
                }
                if ($editorField.prop('tagName').toUpperCase() === "INPUT" && $editorField.prop('type').toUpperCase() === 'TEXT') {
                    gj.core.setCaretAtEnd($editorField[0]);
                } else {
                    $editorField.focus();
                }
                $cell.attr('data-mode', 'edit');
            } else if (column.role === 'managementColumn') {
                $cell.find('[role="edit"]').hide();
                $cell.find('[role="delete"]').hide();
                $cell.find('[role="update"]').show();
                $cell.find('[role="cancel"]').show();
            }
        }
    },

    displayMode: function ($grid, $cell, column, cancel) {
        var $editorContainer, $displayContainer, $ele, newValue, newEditFieldValue, record, position, style = '';
        if (column.mode !== 'editOnly') {
            if ($cell.attr('data-mode') === 'edit') {
                $editorContainer = $cell.find('div[data-role="edit"]');
                $displayContainer = $cell.find('div[data-role="display"]');
                $ele = $editorContainer.find('input, select, textarea').first();
                if ($ele[0].tagName.toUpperCase() === "SELECT" && $ele[0].selectedIndex > -1) {
                    newValue = $ele[0].options[$ele[0].selectedIndex].innerHTML;
                    newEditFieldValue = $ele[0].value;
                } else if ($ele[0].tagName.toUpperCase() === "INPUT" && $ele[0].type.toUpperCase() === "CHECKBOX") {
                    newValue = $ele[0].checked;
                } else {
                    newValue = $ele.val();
                }
                position = $cell.parent().data('position');
                record = $grid.get(position);
                if (cancel !== true && newValue !== record[column.field]) {
                    record[column.field] = column.type === 'date' ? gj.core.parseDate(newValue, column.format) : newValue;
                    if (column.editField) {
                        record[column.editField] = newEditFieldValue || newValue;
                    }
                    if (column.mode !== 'editOnly') {
                        gj.grid.methods.renderDisplayElement($grid, $displayContainer, column, record, gj.grid.methods.getId(record, $grid.data('primaryKey'), position), 'update');
                        if ($cell.find('span.gj-dirty').length === 0) {
                            $cell.prepend($('<span class="gj-dirty" />'));
                        }
                    }
                    gj.grid.plugins.inlineEditing.events.cellDataChanged($grid, $cell, column, record, newValue);
                    gj.grid.plugins.inlineEditing.private.updateChanges($grid, column, record, newValue);
                }
                $editorContainer.hide();
                $displayContainer.show();
                $cell.attr('data-mode', 'display');
            }
            if (column.role === 'managementColumn') {
                $cell.find('[role="update"]').hide();
                $cell.find('[role="cancel"]').hide();
                $cell.find('[role="edit"]').show();
                $cell.find('[role="delete"]').show();
            }
        }
    },

    updateOtherCells: function ($grid, mode) {
        var data = $grid.data();
        if (data.inlineEditing.mode !== 'command' && mode !== 'editOnly') {
            $grid.find('div[data-role="edit"]:visible').parent('td').each(function () {
                var $cell = $(this),
                    column = data.columns[$cell.index()];
                gj.grid.plugins.inlineEditing.private.displayMode($grid, $cell, column);
            });
        }
    },

    updateChanges: function ($grid, column, sourceRecord, newValue) {
        var targetRecords, filterResult, newRecord, data = $grid.data();
        if (!data.guid) {
            data.guid = gj.grid.plugins.inlineEditing.private.generateGUID();
        }
        if (data.primaryKey) {
            targetRecords = JSON.parse(sessionStorage.getItem('gj.grid.' + data.guid));
            if (targetRecords) {
                filterResult = targetRecords.filter(function (record) {
                    return record[data.primaryKey] === sourceRecord[data.primaryKey];
                });
            } else {
                targetRecords = [];
            }
            if (filterResult && filterResult.length === 1) {
                filterResult[0][column.field] = newValue;
            } else {
                newRecord = {};
                newRecord[data.primaryKey] = sourceRecord[data.primaryKey];
                if (data.primaryKey !== column.field) {
                    newRecord[column.field] = newValue;
                }
                targetRecords.push(newRecord);
            }
            sessionStorage.setItem('gj.grid.' + data.guid, JSON.stringify(targetRecords));
        }
    },

    generateGUID: function () {
        function s4() {
            return Math.floor((1 + Math.random()) * 0x10000)
                .toString(16)
                .substring(1);
        }
        return s4() + s4() + '-' + s4() + '-' + s4() + '-' + s4() + '-' + s4() + s4() + s4();
    }
};

gj.grid.plugins.inlineEditing.public = {
    getChanges: function () {
        return JSON.parse(sessionStorage.getItem('gj.grid.' + this.data().guid));
    },
    edit: function (id) {
        var i, record = this.getById(id),
            $cells = gj.grid.methods.getRowById(this, id).children('td'),
            columns = this.data('columns');

        for (i = 0; i < $cells.length; i++) {
            gj.grid.plugins.inlineEditing.private.editMode(this, $($cells[i]), columns[i], record);
        }

        return this;
    },
    update: function (id) {
        var i, record = this.getById(id),
            $cells = gj.grid.methods.getRowById(this, id).children('td'),
            columns = this.data('columns');

        for (i = 0; i < $cells.length; i++) {
            gj.grid.plugins.inlineEditing.private.displayMode(this, $($cells[i]), columns[i], false);
        }

        gj.grid.plugins.inlineEditing.events.rowDataChanged(this, id, record);

        return this;
    },
    cancel: function (id) {
        var i, record = this.getById(id),
            $cells = gj.grid.methods.getRowById(this, id).children('td'),
            columns = this.data('columns');

        for (i = 0; i < $cells.length; i++) {
            gj.grid.plugins.inlineEditing.private.displayMode(this, $($cells[i]), columns[i], true);
        }

        return this;
    }
};

gj.grid.plugins.inlineEditing.events = {
    cellDataChanged: function ($grid, $cell, column, record, oldValue, newValue) {
        $grid.triggerHandler('cellDataChanged', [$cell, column, record, oldValue, newValue]);
    },
    rowDataChanged: function ($grid, id, record) {
        $grid.triggerHandler('rowDataChanged', [id, record]);
    }
};

gj.grid.plugins.inlineEditing.configure = function ($grid, fullConfig, clientConfig) {
    var data = $grid.data();
    $.extend(true, $grid, gj.grid.plugins.inlineEditing.public);
    if (clientConfig.inlineEditing) {
        $grid.on('dataBound', function () {
            $grid.find('span.gj-dirty').remove();
        });
        $grid.on('rowDataBound', function (e, $row, id, record) {
            $grid.cancel(id);
        });
    }
    if (data.inlineEditing.mode === 'command') {
        gj.grid.plugins.inlineEditing.private.localization(data);
        if (fullConfig.inlineEditing.managementColumn) {
            data.columns.push(fullConfig.inlineEditing.managementColumnConfig);
        }
    } else {
        $grid.on('cellDataBound', function (e, $displayEl, id, column, record) {
            if (column.editor) {
                if (column.mode === 'editOnly') {
                    gj.grid.plugins.inlineEditing.private.editMode($grid, $displayEl.parent(), column, record);
                } else {
                    $displayEl.parent('td').on(data.inlineEditing.mode === 'dblclick' ? 'dblclick' : 'click', function () {
                        gj.grid.plugins.inlineEditing.private.editMode($grid, $displayEl.parent(), column, record);
                    });
                }
            }
        });
    }
};
gj.grid.plugins.optimisticPersistence = {

    config: {
        base: {
            optimisticPersistence: {
                localStorage: undefined,
                sessionStorage: undefined
            }
        }
    },

    private: {
        applyParams: function ($grid) {
            var data = $grid.data(),
                params = {}, storage;
            storage = JSON.parse(sessionStorage.getItem('gj.grid.' + data.guid));
            if (storage && storage.optimisticPersistence) {
                $.extend(params, storage.optimisticPersistence);
            }
            storage = JSON.parse(localStorage.getItem('gj.grid.' + data.guid));
            if (storage && storage.optimisticPersistence) {
                $.extend(params, storage.optimisticPersistence);
            }
            $.extend(data.params, params);
        },

        saveParams: function ($grid) {
            var i, param,
                data = $grid.data(),
                storage = { optimisticPersistence: {} };

            if (data.optimisticPersistence.sessionStorage) {
                for (i = 0; i < data.optimisticPersistence.sessionStorage.length; i++) {
                    param = data.optimisticPersistence.sessionStorage[i];
                    storage.optimisticPersistence[param] = data.params[param];
                }
                storage = $.extend(true, JSON.parse(sessionStorage.getItem('gj.grid.' + data.guid)), storage);
                sessionStorage.setItem('gj.grid.' + data.guid, JSON.stringify(storage));
            }

            if (data.optimisticPersistence.localStorage) {
                storage = { optimisticPersistence: {} };
                for (i = 0; i < data.optimisticPersistence.localStorage.length; i++) {
                    param = data.optimisticPersistence.localStorage[i];
                    storage.optimisticPersistence[param] = data.params[param];
                }
                storage = $.extend(true, JSON.parse(localStorage.getItem('gj.grid.' + data.guid)), storage);
                localStorage.setItem('gj.grid.' + data.guid, JSON.stringify(storage));
            }
        }
    },

    configure: function ($grid, fullConfig, clientConfig) {
        if (fullConfig.guid) {
            if (fullConfig.optimisticPersistence.localStorage || fullConfig.optimisticPersistence.sessionStorage) {
                gj.grid.plugins.optimisticPersistence.private.applyParams($grid);
                $grid.on('dataBound', function (e) {
                    gj.grid.plugins.optimisticPersistence.private.saveParams($grid);
                });
            }
        }
    }
};
gj.grid.plugins.pagination = {
    config: {
        base: {
            style: {
                pager: {
                    panel: '',
                    stateDisabled: '',
                    activeButton: ''
                }
            },

            paramNames: {
                page: 'pageIndex',
                limit: 'pageSize'
            },

            pager: {
                limit: 50,
                sizes: [

                ],
                leftControls: undefined,
                rightControls: undefined
            }
        },

        bootstrap: {
            style: {
                pager: {
                    panel: '',
                    stateDisabled: ''
                }
            }
        },

        bootstrap4: {
            style: {
                pager: {
                    panel: 'btn-toolbar',
                    stateDisabled: ''
                }
            }
        },

        glyphicons: {
            icons: {
                first: '<span class="glyphicon glyphicon-step-backward"></span>',
                previous: '<span class="glyphicon glyphicon-backward"></span>',
                next: '<span class="glyphicon glyphicon-forward"></span>',
                last: '<span class="glyphicon glyphicon-step-forward"></span>',
                refresh: '<span class="glyphicon glyphicon-refresh"></span>'
            }
        },

        materialicons: {
            icons: {
                first: '<i class="gj-icon first-page" />',
                previous: '<i class="gj-icon chevron-left" />',
                next: '<i class="gj-icon chevron-right" />',
                last: '<i class="gj-icon last-page" />',
                refresh: '<i class="gj-icon refresh" />'
            }
        },

        fontawesome: {
            icons: {
                first: '<i class="fa fa-fast-backward" aria-hidden="true"></i>',
                previous: '<i class="fa fa-backward" aria-hidden="true"></i>',
                next: '<i class="fa fa-forward" aria-hidden="true"></i>',
                last: '<i class="fa fa-fast-forward" aria-hidden="true"></i>',
                refresh: '<i class="fa fa-refresh" aria-hidden="true"></i>'
            }
        }
    },

    private: {
        init: function ($grid) {
            var $row, $cell, data, controls, $leftPanel, $rightPanel, $tfoot, leftControls, rightControls, i;

            data = $grid.data();

            if (data.pager) {
                if (!data.params[data.paramNames.page]) {
                    data.params[data.paramNames.page] = 1;
                }
                if (!data.params[data.paramNames.limit]) {
                    data.params[data.paramNames.limit] = data.pager.limit;
                }

                gj.grid.plugins.pagination.private.localization(data);

                $row = $('<tr data-role="pager"/>');
                $cell = $('<th/>');
                $row.append($cell);

                $leftPanel = $('<div data-role="display" />').addClass(data.style.pager.panel).css({ 'float': 'left' });
                $rightPanel = $('<div data-role="display" />').addClass(data.style.pager.panel).css({ 'float': 'right' });

                $cell.append($leftPanel).append($rightPanel);

                $tfoot = $('<tfoot />').append($row);
                $grid.append($tfoot);
                gj.grid.plugins.pagination.private.updatePagerColSpan($grid);

                leftControls = gj.grid.methods.clone(data.pager.leftControls); //clone array
                $.each(leftControls, function () {
                    $leftPanel.append(this);
                });

                rightControls = gj.grid.methods.clone(data.pager.rightControls); //clone array
                $.each(rightControls, function () {
                    $rightPanel.append(this);
                });

                controls = $grid.find('tfoot [data-role]');
                for (i = 0; i < controls.length; i++) {
                    gj.grid.plugins.pagination.private.initPagerControl($(controls[i]), $grid);
                }
            }
        },

        localization: function (data) {
            if (data.uiLibrary === 'bootstrap') {
                gj.grid.plugins.pagination.private.localizationBootstrap(data);
            } else if (data.uiLibrary === 'bootstrap4') {
                gj.grid.plugins.pagination.private.localizationBootstrap4(data);
            } else {
                gj.grid.plugins.pagination.private.localizationMaterialDesign(data);
            }
        },

        localizationBootstrap: function (data) {
            var msg = gj.grid.messages[data.locale];
            if (typeof (data.pager.leftControls) === 'undefined') {
                data.pager.leftControls = [
                    $('<button type="button" class="btn btn-default btn-sm">' + (data.icons.first || msg.First) + '</button>').attr('title', msg.FirstPageTooltip).attr('data-role', 'page-first'),
                    $('<button type="button" class="btn btn-default btn-sm">' + (data.icons.previous || msg.Previous) + '</button>').attr('title', msg.PreviousPageTooltip).attr('data-role', 'page-previous'),
                    $('<div>' + msg.Page + '</div>'),
                    $('<input data-role="page-number" class="form-control input-sm" type="text" value="0">'),
                    $('<div>' + msg.Of + '</div>'),
                    $('<div data-role="page-label-last">0</div>'),
                    $('<button type="button" class="btn btn-default btn-sm">' + (data.icons.next || msg.Next) + '</button>').attr('title', msg.NextPageTooltip).attr('data-role', 'page-next'),
                    $('<button type="button" class="btn btn-default btn-sm">' + (data.icons.last || msg.Last) + '</button>').attr('title', msg.LastPageTooltip).attr('data-role', 'page-last'),
                    $('<button type="button" class="btn btn-default btn-sm">' + (data.icons.refresh || msg.Refresh) + '</button>').attr('title', msg.Refresh).attr('data-role', 'page-refresh'),
                    $('<select data-role="page-size" class="form-control input-sm" width="60"></select>')
                ];
            }
            if (typeof (data.pager.rightControls) === 'undefined') {
                data.pager.rightControls = [
                    $('<div>' + msg.DisplayingRecords + '</div>'),
                    $('<div data-role="record-first">0</div>'),
                    $('<div>-</div>'),
                    $('<div data-role="record-last">0</div>'),
                    $('<div>' + msg.Of + '</div>'),
                    $('<div data-role="record-total">0</div>')
                ];
            }
        },

        localizationBootstrap4: function (data) {
            var msg = gj.grid.messages[data.locale];
            if (typeof (data.pager.leftControls) === 'undefined') {
                data.pager.leftControls = [
                    $('<button class="btn btn-default btn-sm gj-cursor-pointer">' + (data.icons.first || msg.First) + '</button>').attr('title', msg.FirstPageTooltip).attr('data-role', 'page-first'),
                    $('<button class="btn btn-default btn-sm gj-cursor-pointer">' + (data.icons.previous || msg.Previous) + '</button>').attr('title', msg.PreviousPageTooltip).attr('data-role', 'page-previous'),
                    $('<div>' + msg.Page + '</div>'),
                    $('<div class="input-group"><input data-role="page-number" class="form-control form-control-sm" type="text" value="0"></div>'),
                    $('<div>' + msg.Of + '</div>'),
                    $('<div data-role="page-label-last">0</div>'),
                    $('<button class="btn btn-default btn-sm gj-cursor-pointer">' + (data.icons.next || msg.Next) + '</button>').attr('title', msg.NextPageTooltip).attr('data-role', 'page-next'),
                    $('<button class="btn btn-default btn-sm gj-cursor-pointer">' + (data.icons.last || msg.Last) + '</button>').attr('title', msg.LastPageTooltip).attr('data-role', 'page-last'),
                    $('<button class="btn btn-default btn-sm gj-cursor-pointer">' + (data.icons.refresh || msg.Refresh) + '</button>').attr('title', msg.Refresh).attr('data-role', 'page-refresh'),
                    $('<select data-role="page-size" class="form-control input-sm" width="60"></select>')
                ];
            }
            if (typeof (data.pager.rightControls) === 'undefined') {
                data.pager.rightControls = [
                    $('<div>' + msg.DisplayingRecords + '&nbsp;</div>'),
                    $('<div data-role="record-first">0</div>'),
                    $('<div>-</div>'),
                    $('<div data-role="record-last">0</div>'),
                    $('<div>' + msg.Of + '</div>'),
                    $('<div data-role="record-total">0</div>')
                ];
            }
        },

        localizationMaterialDesign: function (data) {
            var msg = gj.grid.messages[data.locale];
            if (typeof (data.pager.leftControls) === 'undefined') {
                data.pager.leftControls = [];
            }
            if (typeof (data.pager.rightControls) === 'undefined') {
                data.pager.rightControls = [
                    $('<span class="">' + msg.RowsPerPage + '</span>'),
                    $('<select data-role="page-size" class="gj-grid-md-limit-select" width="52"></select></div>'),
                    $('<span class="gj-md-spacer-32">&nbsp;</span>'),
                    $('<span data-role="record-first" class="">0</span>'),
                    $('<span class="">-</span>'),
                    $('<span data-role="record-last" class="">0</span>'),
                    $('<span class="gj-grid-mdl-pager-label">' + msg.Of + '</span>'),
                    $('<span data-role="record-total" class="">0</span>'),
                    $('<span class="gj-md-spacer-32">&nbsp;</span>'),
                    $('<button class="gj-button-md">' + (data.icons.previous || msg.Previous) + '</button>').attr('title', msg.PreviousPageTooltip).attr('data-role', 'page-previous').addClass(data.icons.first ? 'gj-button-md-icon' : ''),
                    $('<span class="gj-md-spacer-24">&nbsp;</span>'),
                    $('<button class="gj-button-md">' + (data.icons.next || msg.Next) + '</button>').attr('title', msg.NextPageTooltip).attr('data-role', 'page-next').addClass(data.icons.first ? 'gj-button-md-icon' : '')
                ];
            }
        },

        initPagerControl: function ($control, $grid) {
            var data = $grid.data();
            switch ($control.data('role')) {
                case 'page-size':
                    var fullConfig = $grid.getFullConfig();
                    if (data.pager.sizes && 0 < data.pager.sizes.length) {
                        $control.show();
                        $.each(data.pager.sizes, function () {
                            var desiredPageSize = parseInt(this);
                            if (0 >= fullConfig.maxRowLimit || !fullConfig.showHiddenColumnsAsDetails || (fullConfig.showHiddenColumnsAsDetails && fullConfig.maxRowLimit >= desiredPageSize)) {
                                $control.append($('<option/>').attr('value', this.toString()).text(this.toString()));
                            }
                        });
                        $control.change(function () {
                            var newSize = parseInt(this.value, 10);
                            data.params[data.paramNames.limit] = newSize;
                            gj.grid.plugins.pagination.private.changePage($grid, 1);
                            gj.grid.plugins.pagination.events.pageSizeChange($grid, newSize);
                        });
                        $control.val(data.params[data.paramNames.limit]);
                        if (gj.dropdown) {
                            $control.dropdown({
                                uiLibrary: data.uiLibrary,
                                iconsLibrary: data.iconsLibrary,
                                fontSize: $control.css('font-size'),
                                style: {
                                    presenter: 'btn btn-default btn-sm'
                                }
                            });
                        }
                    } else {
                        $control.hide();
                    }
                    break;
                case 'page-refresh':
                    $control.on('click', function () { $grid.reload(); });
                    break;
            }

        },

        reloadPager: function ($grid, totalRecords) {
            var page, limit, lastPage, firstRecord, lastRecord, data, controls, i;

            data = $grid.data();

            if (data.pager) {
                page = (0 === totalRecords) ? 0 : parseInt(data.params[data.paramNames.page], 10);
                limit = parseInt(data.params[data.paramNames.limit], 10);
                lastPage = Math.ceil(totalRecords / limit);
                firstRecord = (0 === page) ? 0 : (limit * (page - 1)) + 1;
                lastRecord = (firstRecord + limit) > totalRecords ? totalRecords : (firstRecord + limit) - 1;

                controls = $grid.find('TFOOT [data-role]');
                for (i = 0; i < controls.length; i++) {
                    gj.grid.plugins.pagination.private.reloadPagerControl($(controls[i]), $grid, page, lastPage, firstRecord, lastRecord, totalRecords);
                }

                gj.grid.plugins.pagination.private.updatePagerColSpan($grid);
            }
        },

        reloadPagerControl: function ($control, $grid, page, lastPage, firstRecord, lastRecord, totalRecords) {
            var newPage;

            switch ($control.data('role')) {
                case 'page-first':
                    gj.grid.plugins.pagination.private.assignPageHandler($grid, $control, 1, page < 2);
                    break;
                case 'page-previous':
                    gj.grid.plugins.pagination.private.assignPageHandler($grid, $control, page - 1, page < 2);
                    break;
                case 'page-number':
                    $control.val(page).off('change').on('change', gj.grid.plugins.pagination.private.createChangePageHandler($grid, page));
                    break;
                case 'page-label-last':
                    $control.text(lastPage);
                    break;
                case 'page-next':
                    gj.grid.plugins.pagination.private.assignPageHandler($grid, $control, page + 1, lastPage === page);
                    break;
                case 'page-last':
                    gj.grid.plugins.pagination.private.assignPageHandler($grid, $control, lastPage, lastPage === page);
                    break;
                case 'page-button-one':
                    newPage = (page === 1) ? 1 : ((page == lastPage) ? (page - 2) : (page - 1));
                    gj.grid.plugins.pagination.private.assignButtonHandler($grid, $control, page, newPage, lastPage);
                    break;
                case 'page-button-two':
                    newPage = (page === 1) ? 2 : ((page == lastPage) ? lastPage - 1 : page);
                    gj.grid.plugins.pagination.private.assignButtonHandler($grid, $control, page, newPage, lastPage);
                    break;
                case 'page-button-three':
                    newPage = (page === 1) ? page + 2 : ((page == lastPage) ? page : (page + 1));
                    gj.grid.plugins.pagination.private.assignButtonHandler($grid, $control, page, newPage, lastPage);
                    break;
                case 'record-first':
                    $control.text(firstRecord);
                    break;
                case 'record-last':
                    $control.text(lastRecord);
                    break;
                case 'record-total':
                    $control.text(totalRecords);
                    break;
                case 'add-new':
                    gj.grid.plugins.pagination.private.assignAddNewHandler($grid, $control, page);
                    break;
            }
        },

        assignPageHandler: function ($grid, $control, newPage, disabled) {
            var gridData = $grid.data();
            var style = gridData.style.pager;

            if (disabled) {
                $control.addClass(style.stateDisabled).prop('disabled', true).off('click');
            } else {
                $control.removeClass(style.stateDisabled).prop('disabled', false).off('click').on('click', function () {
                    gj.grid.plugins.pagination.private.changePage($grid, newPage);
                });
            }
        },

        assignButtonHandler: function ($grid, $control, page, newPage, lastPage) {
            var style = $grid.data().style.pager;
            if (newPage < 1 || newPage > lastPage) {
                $control.hide();
            } else {
                $control.show().off('click').text(newPage);
                if (newPage === page) {
                    $control.addClass(style.activeButton);
                } else {
                    $control.removeClass(style.activeButton).on('click', function () {
                        gj.grid.plugins.pagination.private.changePage($grid, newPage);
                    });
                }
            }
        },

        assignAddNewHandler: function ($grid, $control, page) {
            var action = $grid.data().addNewAction;

            if ('function' === typeof action) {
                $control.off('click').on('click', function () {
                    action.call(this, $grid, $control, page);
                });
            }
        },

        createChangePageHandler: function ($grid, currentPage) {
            return function () {
                var data = $grid.data(),
                    newPage = parseInt(this.value, 10);
                gj.grid.plugins.pagination.private.changePage($grid, newPage);
            };
        },

        changePage: function ($grid, newPage) {
            var data = $grid.data();

            if (gj.grid.plugins.pagination.events.pageChanging($grid, newPage) !== false && !isNaN(newPage)) {
                $grid.find('TFOOT [data-role="page-number"]').val(newPage);
                data.params[data.paramNames.page] = newPage;
            }
            $grid.reload();
        },

        updatePagerColSpan: function ($grid) {
            var $cell = $grid.find('tfoot > tr[data-role="pager"] > th');
            if ($cell && $cell.length) {
                $cell.attr('colspan', gj.grid.methods.countVisibleColumns($grid));
            }
        },

        isLastRecordVisible: function ($grid) {
            var result = true,
                data = $grid.data(),
                limit = parseInt(data.params[data.paramNames.limit], 10),
                page = parseInt(data.params[data.paramNames.page], 10),
                count = $grid.count();
            if (limit && page) {
                result = ((page - 1) * limit) + count === data.totalRecords;
            }
            return result;
        }
    },

    public: {
        getAll: function (includeAllRecords) {
            var limit, page, start, data = this.data();
            if ($.isArray(data.dataSource)) {
                if (includeAllRecords) {
                    return data.dataSource;
                } else if (data.params[data.paramNames.limit] && data.params[data.paramNames.page]) {
                    limit = parseInt(data.params[data.paramNames.limit], 10);
                    page = parseInt(data.params[data.paramNames.page], 10);
                    start = (page - 1) * limit;
                    return data.records.slice(start, start + limit);
                } else {
                    return data.records;
                }
            } else {
                return data.records;
            }
        }
    },

    events: {
        pageSizeChange: function ($grid, newSize) {
            $grid.triggerHandler('pageSizeChange', [newSize]);
        },
        pageChanging: function ($grid, newSize) {
            $grid.triggerHandler('pageChanging', [newSize]);
        }
    },

    configure: function ($grid, fullConfig, clientConfig) {
        $.extend(true, $grid, gj.grid.plugins.pagination.public);
        var data = $grid.data();
        if (clientConfig.pager) {
            gj.grid.methods.isLastRecordVisible = gj.grid.plugins.pagination.private.isLastRecordVisible;

            $grid.on('initialized', function () {
                gj.grid.plugins.pagination.private.init($grid);
            });
            $grid.on('dataBound', function (e, records, totalRecords) {
                gj.grid.plugins.pagination.private.reloadPager($grid, totalRecords);
            });
            $grid.on('columnShow', function () {
                gj.grid.plugins.pagination.private.updatePagerColSpan($grid);
            });
            $grid.on('columnHide', function () {
                gj.grid.plugins.pagination.private.updatePagerColSpan($grid);
            });
        }
    }
};
gj.grid.plugins.responsiveDesign = {
    config: {
        base: {
            resizeCheckInterval: 500,
            responsive: false,
            showHiddenColumnsAsDetails: false,
            maxRowLimit: 0,

            defaultColumn: {
                priority: undefined,
                minWidth: 250
            },
            style: {
                rowDetailItem: ''
            }
        },

        bootstrap: {
            style: {
                rowDetailItem: 'col-lg-4'
            }
        }
    },

    'private': {

        orderColumns: function (config) {
            var result = [];
            if (config.columns && config.columns.length) {
                for (i = 0; i < config.columns.length; i++) {
                    result.push({
                        position: i,
                        field: config.columns[i].field,
                        minWidth: config.columns[i].width || config.columns[i].minWidth || config.defaultColumn.minWidth,
                        priority: config.columns[i].priority || 0
                    });
                }
                result.sort(function (a, b) {
                    var result = 0;
                    if (a.priority < b.priority) {
                        result = -1;
                    } else if (a.priority > b.priority) {
                        result = 1;
                    }
                    return result;
                });
            }
            return result;
        },

        updateDetails: function ($grid) {
            var rows, data, i, j, $row, details, $placeholder, column, tmp;
            rows = $grid.find('tbody > tr[data-role="row"]');
            data = $grid.data();
            for (i = 0; i < rows.length; i++) {
                $row = $(rows[i]);
                details = $row.data('details');
                for (j = 0; j < data.columns.length; j++) {
                    column = data.columns[j];
                    $placeholder = details && details.find('div[data-id="' + column.field + '"]');
                    if (data.columns[j].hidden) {
                        tmp = '<b>' + (column.title || column.field) + '</b>: {' + column.field + '}';
                        if (!$placeholder || !$placeholder.length) {
                            $placeholder = $('<div data-id="' + column.field + '"/>').html(tmp);
                            $placeholder.addClass(data.style.rowDetailItem);
                            if (!details || !details.length) {
                                details = $('<div class="row"/>');
                            }
                            details.append($placeholder);
                        } else {
                            $placeholder.empty().html(tmp);
                        }
                    } else if ($placeholder && $placeholder.length) {
                        $placeholder.remove();
                    }
                }
                $grid.updateDetails($row);
            }
        }
    },

    'public': {

        oldWidth: undefined,

        resizeCheckIntervalId: undefined,
        makeResponsive: function () {
            var i, $column,
                extraWidth = 0,
                config = this.data(),
                columns = gj.grid.plugins.responsiveDesign.private.orderColumns(config);
            for (i = 0; i < columns.length; i++) {
                $column = this.find('thead>tr>th:eq(' + columns[i].position + ')');
                if ($column.is(':visible') && columns[i].minWidth < $column.width()) {
                    extraWidth += $column.width() - columns[i].minWidth;
                }
            }
            if (extraWidth) {
                for (i = 0; i < columns.length; i++) {
                    $column = this.find('thead>tr>th:eq(' + columns[i].position + ')');
                    if (!$column.is(':visible') && columns[i].minWidth <= extraWidth) {
                        this.showColumn(columns[i].field);
                        extraWidth -= $column.width();
                    }
                }
            }
            for (i = (columns.length - 1); i >= 0; i--) {
                $column = this.find('thead>tr>th:eq(' + columns[i].position + ')');
                if ($column.is(':visible') && columns[i].priority && columns[i].minWidth > $column.outerWidth()) {
                    this.hideColumn(columns[i].field);
                }
            }

            return this;
        },
    },

    'events': {
        resize: function ($grid, newWidth, oldWidth) {
            $grid.triggerHandler('resize', [newWidth, oldWidth]);
        }
    },

    'configure': function ($grid, fullConfig, clientConfig) {
        $.extend(true, $grid, gj.grid.plugins.responsiveDesign.public);
        if (fullConfig.responsive) {
            $grid.on('initialized', function () {
                $grid.makeResponsive();
                $grid.oldWidth = $grid.width();
                $grid.resizeCheckIntervalId = setInterval(function () {
                    var newWidth = $grid.width();
                    if (newWidth !== $grid.oldWidth) {
                        gj.grid.plugins.responsiveDesign.events.resize($grid, newWidth, $grid.oldWidth);
                    }
                    $grid.oldWidth = newWidth;
                }, fullConfig.resizeCheckInterval);
            });
            $grid.on('destroy', function () {
                if ($grid.resizeCheckIntervalId) {
                    clearInterval($grid.resizeCheckIntervalId);
                }
            });
            $grid.on('resize', function () {
                var existingRecords = $grid.data().records;
                $grid.makeResponsive();
                $grid.render(existingRecords);
            });
        }
        if (fullConfig.showHiddenColumnsAsDetails && gj.grid.plugins.expandCollapseRows) {
            $grid.on('dataBound', function () {
                gj.grid.plugins.responsiveDesign.private.updateDetails($grid);
            });
            $grid.on('columnHide', function () {
                gj.grid.plugins.responsiveDesign.private.updateDetails($grid);
            });
            $grid.on('columnShow', function () {
                gj.grid.plugins.responsiveDesign.private.updateDetails($grid);
            });
            $grid.on('rowDataBound', function () {
                gj.grid.plugins.responsiveDesign.private.updateDetails($grid);
            });
        }
    }
};
gj.grid.plugins.toolbar = {
    config: {
        base: {
            toolbarTemplate: undefined,
            title: undefined,

            style: {
                toolbar: 'gj-grid-md-toolbar'
            }
        },

        bootstrap: {
            style: {
                toolbar: 'gj-grid-bootstrap-toolbar'
            }
        },

        bootstrap4: {
            style: {
                toolbar: 'gj-grid-bootstrap-4-toolbar'
            }
        }
    },

    private: {
        init: function ($grid) {
            var data, $toolbar, $title;
            data = $grid.data();
            $toolbar = $grid.prev('div[data-role="toolbar"]');
            if (typeof (data.toolbarTemplate) !== 'undefined' || typeof (data.title) !== 'undefined' || $toolbar.length > 0) {
                if ($toolbar.length === 0) {
                    $toolbar = $('<div data-role="toolbar"></div>');
                    $grid.before($toolbar);
                }
                $toolbar.addClass(data.style.toolbar);

                if ($toolbar.children().length === 0 && data.toolbarTemplate) {
                    $toolbar.append(data.toolbarTemplate);
                }

                $title = $toolbar.find('[data-role="title"]');
                if ($title.length === 0) {
                    $title = $('<div data-role="title"/>');
                    $toolbar.prepend($title);
                }
                if (data.title) {
                    $title.text(data.title);
                }

                if (data.minWidth) {
                    $toolbar.css('min-width', data.minWidth);
                }
            }
        }
    },

    public: {
        title: function (text) {
            var $titleEl = this.parent().find('div[data-role="toolbar"] [data-role="title"]');
            if (typeof (text) !== 'undefined') {
                $titleEl.text(text);
                return this;
            } else {
                return $titleEl.text();
            }
        }
    },

    configure: function ($grid) {
        $.extend(true, $grid, gj.grid.plugins.toolbar.public);
        $grid.on('initialized', function () {
            gj.grid.plugins.toolbar.private.init($grid);
        });
        $grid.on('destroying', function () {
            $grid.prev('[data-role="toolbar"]').remove();
        });
    }
};
gj.grid.plugins.resizableColumns = {
    config: {
        base: {
            resizableColumns: false
        }
    },

    private: {
        init: function ($grid, config) {
            var $columns, $column, i, $wrapper, $resizer, marginRight;
            $columns = $grid.find('thead tr[data-role="caption"] th');
            if ($columns.length) {
                for (i = 0; i < $columns.length - 1; i++) {
                    $column = $($columns[i]);
                    $wrapper = $('<div class="gj-grid-column-resizer-wrapper" />');
                    marginRight = parseInt($column.css('padding-right'), 10) + 3;
                    $resizer = $('<span class="gj-grid-column-resizer" />').css('margin-right', '-' + marginRight + 'px');
                    $resizer.draggable({
                        start: function () {
                            $grid.addClass('gj-unselectable');
                            $grid.addClass('gj-grid-resize-cursor');
                        },
                        stop: function () {
                            $grid.removeClass('gj-unselectable');
                            $grid.removeClass('gj-grid-resize-cursor');
                            this.style.removeProperty('top');
                            this.style.removeProperty('left');
                            this.style.removeProperty('position');
                        },
                        drag: gj.grid.plugins.resizableColumns.private.createResizeHandle($grid, $column, config.columns[i])
                    });
                    $column.append($wrapper.append($resizer));
                }
                for (i = 0; i < $columns.length; i++) {
                    $column = $($columns[i]);
                    if (!$column.attr('width')) {
                        $column.attr('width', $column.outerWidth());
                    }
                }
            }
        },

        createResizeHandle: function ($grid, $column, column) {
            var data = $grid.data();
            return function (e, newPosition) {
                var i, index, rows, cell, newWidth, nextWidth,
                    currentWidth = parseInt($column.attr('width'), 10),
                    position = gj.core.position(this),
                    offset = { top: newPosition.top - position.top, left: newPosition.left - position.left };
                if (!currentWidth) {
                    currentWidth = $column.outerWidth();
                }
                if (offset.left) {
                    newWidth = currentWidth + offset.left;
                    column.width = newWidth;
                    $column.attr('width', newWidth);
                    index = $column[0].cellIndex;
                    cell = $column[0].parentElement.children[index + 1];
                    nextWidth = parseInt($(cell).attr('width'), 10) - offset.left;
                    cell.setAttribute('width', nextWidth);
                    if (data.resizableColumns) {
                        rows = $grid[0].tBodies[0].children;
                        for (i = 0; i < rows.length; i++) {
                            rows[i].cells[index].setAttribute('width', newWidth);
                            cell = rows[i].cells[index + 1];
                            cell.setAttribute('width', nextWidth);
                        }
                    }
                }
            };
        }
    },

    public: {
    },

    configure: function ($grid, fullConfig, clientConfig) {
        $.extend(true, $grid, gj.grid.plugins.resizableColumns.public);
        if (fullConfig.resizableColumns && gj.draggable) {
            $grid.on('initialized', function () {
                gj.grid.plugins.resizableColumns.private.init($grid, fullConfig);
            });
        }
    }
};
gj.grid.plugins.rowReorder = {
    config: {
        base: {
            rowReorder: false,
            rowReorderColumn: undefined,
            orderNumberField: undefined,

            style: {
                targetRowIndicatorTop: 'gj-grid-row-reorder-indicator-top',
                targetRowIndicatorBottom: 'gj-grid-row-reorder-indicator-bottom'
            }
        }
    },

    private: {
        init: function ($grid) {
            var i, columnPosition, $row,
                $rows = $grid.find('tbody tr[data-role="row"]');
            if ($grid.data('rowReorderColumn')) {
                columnPosition = gj.grid.methods.getColumnPosition($grid.data('columns'), $grid.data('rowReorderColumn'));
            }
            for (i = 0; i < $rows.length; i++) {
                $row = $($rows[i]);
                if (typeof (columnPosition) !== 'undefined') {
                    $row.find('td:eq(' + columnPosition + ')').on('mousedown', gj.grid.plugins.rowReorder.private.createRowMouseDownHandler($grid, $row));
                } else {
                    $row.on('mousedown', gj.grid.plugins.rowReorder.private.createRowMouseDownHandler($grid, $row));
                }
            }
        },

        createRowMouseDownHandler: function ($grid, $trSource) {
            return function (e) {
                var $dragEl = $grid.clone(),
                    columns = $grid.data('columns'),
                    i, $cells;
                $grid.addClass('gj-unselectable');
                $('body').append($dragEl);
                $dragEl.attr('data-role', 'draggable-clone').css('cursor', 'move');
                $dragEl.children('thead').remove().children('tfoot').remove();
                $dragEl.find('tbody tr:not([data-position="' + $trSource.data('position') + '"])').remove();
                $cells = $dragEl.find('tbody tr td');
                for (i = 0; i < $cells.length; i++) {
                    if (columns[i].width) {
                        $cells[i].setAttribute('width', columns[i].width);
                    }
                }
                $dragEl.draggable({
                    stop: gj.grid.plugins.rowReorder.private.createDragStopHandler($grid, $trSource)
                });
                $dragEl.css({
                    position: 'absolute', top: $trSource.offset().top, left: $trSource.offset().left, width: $trSource.width(), zIndex: 1
                });
                if ($trSource.attr('data-droppable') === 'true') {
                    $trSource.droppable('destroy');
                }
                $trSource.siblings('tr[data-role="row"]').each(function () {
                    var $dropEl = $(this);
                    if ($dropEl.attr('data-droppable') === 'true') {
                        $dropEl.droppable('destroy');
                    }
                    $dropEl.droppable({
                        over: gj.grid.plugins.rowReorder.private.createDroppableOverHandler($trSource),
                        out: gj.grid.plugins.rowReorder.private.droppableOut
                    });
                });
                $dragEl.trigger('mousedown');
            };
        },

        createDragStopHandler: function ($grid, $trSource) {
            return function (e, mousePosition) {
                $('table[data-role="draggable-clone"]').draggable('destroy').remove();
                $grid.removeClass('gj-unselectable');
                $trSource.siblings('tr[data-role="row"]').each(function () {
                    var $trTarget = $(this),
                        targetPosition = $trTarget.data('position'),
                        sourcePosition = $trSource.data('position'),
                        data = $grid.data(),
                        $rows, $row, i, record, id;

                    if ($trTarget.droppable('isOver', mousePosition)) {
                        if (targetPosition < sourcePosition) {
                            $trTarget.before($trSource);
                        } else {
                            $trTarget.after($trSource);
                        }
                        data.records.splice(targetPosition - 1, 0, data.records.splice(sourcePosition - 1, 1)[0]);
                        $rows = $trTarget.parent().find('tr[data-role="row"]');
                        for (i = 0; i < $rows.length; i++) {
                            $($rows[i]).attr('data-position', i + 1);
                        }
                        if (data.orderNumberField) {
                            for (i = 0; i < data.records.length; i++) {
                                data.records[i][data.orderNumberField] = i + 1;
                            }
                            for (i = 0; i < $rows.length; i++) {
                                $row = $($rows[i]);
                                id = gj.grid.methods.getId($row, data.primaryKey, $row.attr('data-position'));
                                record = gj.grid.methods.getByPosition($grid, $row.attr('data-position'));
                                $grid.setCellContent(id, data.orderNumberField, record[data.orderNumberField]);
                            }
                        }
                    }
                    $trTarget.removeClass('gj-grid-top-border');
                    $trTarget.removeClass('gj-grid-bottom-border');
                    $trTarget.droppable('destroy');
                });
            }
        },

        createDroppableOverHandler: function ($trSource) {
            return function (e) {
                var $trTarget = $(this),
                    targetPosition = $trTarget.data('position'),
                    sourcePosition = $trSource.data('position');
                if (targetPosition < sourcePosition) {
                    $trTarget.addClass('gj-grid-top-border');
                } else {
                    $trTarget.addClass('gj-grid-bottom-border');
                }
            };
        },

        droppableOut: function () {
            $(this).removeClass('gj-grid-top-border');
            $(this).removeClass('gj-grid-bottom-border');
        }
    },

    public: {
    },

    configure: function ($grid, fullConfig, clientConfig) {
        $.extend(true, $grid, gj.grid.plugins.rowReorder.public);
        if (fullConfig.rowReorder && gj.draggable && gj.droppable) {
            $grid.on('dataBound', function () {
                gj.grid.plugins.rowReorder.private.init($grid);
            });
        }
    }
};
gj.grid.plugins.export = {
    config: { base: {} },

    public: {
        getCSV: function (includeAllRecords) {
            var i, j, line = '', str = '',
                columns = this.data().columns,
                records = this.getAll(includeAllRecords);

            if (records.length) {

                for (i = 0; i < columns.length; i++) {
                    if (gj.grid.plugins.export.public.isColumnApplicable(columns[i])) {
                        line += '"' + (columns[i].title || columns[i].field).replace(/<[^>]+>/g, ' ') + '",';
                    }
                }
                str += line.slice(0, line.length - 1) + '\r\n';

                for (i = 0; i < records.length; i++) {
                    line = '';

                    for (j = 0; j < columns.length; j++) {
                        if (gj.grid.plugins.export.public.isColumnApplicable(columns[j])) {
                            line += '"' + records[i][columns[j].field] + '",';
                        }
                    }
                    str += line.slice(0, line.length - 1) + '\r\n';
                }
            }

            return str;
        },
        downloadCSV: function (filename, includeAllRecords) {
            var link = document.createElement('a');
            document.body.appendChild(link);
            link.download = filename || 'griddata.csv';
            if (window.navigator.userAgent.indexOf("Edge") > -1) {
                link.href = URL.createObjectURL(new Blob([this.getCSV(includeAllRecords)], { type: 'text/csv;charset=utf-8;' }));
            } else {
                link.href = 'data:text/csv;charset=utf-8,' + escape(this.getCSV(includeAllRecords));
            }
            link.click();
            document.body.removeChild(link);
            return this;
        },

        isColumnApplicable: function (column) {
            return column.hidden !== true && !column.role;
        }
    },

    configure: function ($grid) {
        $.extend(true, $grid, gj.grid.plugins.export.public);
    }
};
gj.grid.plugins.columnReorder = {
    config: {
        base: {
            columnReorder: false,

            dragReady: false,

            style: {
                targetRowIndicatorTop: 'gj-grid-row-reorder-indicator-top',
                targetRowIndicatorBottom: 'gj-grid-row-reorder-indicator-bottom'
            }
        }
    },

    private: {
        init: function ($grid) {
            var i, $cell,
                $cells = $grid.find('thead tr th');
            for (i = 0; i < $cells.length; i++) {
                $cell = $($cells[i]);
                $cell.on('mousedown', gj.grid.plugins.columnReorder.private.createMouseDownHandler($grid, $cell));
                $cell.on('mousemove', gj.grid.plugins.columnReorder.private.createMouseMoveHandler($grid, $cell));
                $cell.on('mouseup', gj.grid.plugins.columnReorder.private.createMouseUpHandler($grid, $cell));
            }
        },

        createMouseDownHandler: function ($grid) {
            return function (e) {
                $grid.timeout = setTimeout(function () {
                    $grid.data('dragReady', true);
                }, 100);
            }
        },

        createMouseUpHandler: function ($grid) {
            return function (e) {
                clearTimeout($grid.timeout);
                $grid.data('dragReady', false);
            }
        },

        createMouseMoveHandler: function ($grid, $thSource) {
            return function (e) {
                var $dragEl, srcIndex;
                if ($grid.data('dragReady')) {
                    $grid.data('dragReady', false);
                    $dragEl = $grid.clone();
                    srcIndex = $thSource.index();
                    $grid.addClass('gj-unselectable');
                    $('body').append($dragEl);
                    $dragEl.attr('data-role', 'draggable-clone').css('cursor', 'move');
                    $dragEl.find('thead tr th:eq(' + srcIndex + ')').siblings().remove();
                    $dragEl.find('tbody tr[data-role != "row"]').remove();
                    $dragEl.find('tbody tr td:nth-child(' + (srcIndex + 1) + ')').siblings().remove();
                    $dragEl.find('tfoot').remove();
                    $dragEl.draggable({
                        stop: gj.grid.plugins.columnReorder.private.createDragStopHandler($grid, $thSource)
                    });
                    $dragEl.css({
                        position: 'absolute', top: $thSource.offset().top, left: $thSource.offset().left, width: $thSource.width(), zIndex: 1
                    });
                    if ($thSource.attr('data-droppable') === 'true') {
                        $thSource.droppable('destroy');
                    }
                    $thSource.siblings('th').each(function () {
                        var $dropEl = $(this);
                        if ($dropEl.attr('data-droppable') === 'true') {
                            $dropEl.droppable('destroy');
                        }
                        $dropEl.droppable({
                            over: gj.grid.plugins.columnReorder.private.createDroppableOverHandler($grid, $thSource),
                            out: gj.grid.plugins.columnReorder.private.droppableOut
                        });
                    });
                    $dragEl.trigger('mousedown');
                }
            };
        },

        createDragStopHandler: function ($grid, $thSource) {
            return function (e, mousePosition) {
                $('table[data-role="draggable-clone"]').draggable('destroy').remove();
                $grid.removeClass('gj-unselectable');
                $thSource.siblings('th').each(function () {
                    var $thTarget = $(this),
                        data = $grid.data(),
                        targetPosition = gj.grid.methods.getColumnPosition(data.columns, $thTarget.data('field')),
                        sourcePosition = gj.grid.methods.getColumnPosition(data.columns, $thSource.data('field'));

                    $thTarget.removeClass('gj-grid-left-border').removeClass('gj-grid-right-border');
                    $thTarget.closest('table').find('tbody tr[data-role="row"] td:nth-child(' + ($thTarget.index() + 1) + ')').removeClass('gj-grid-left-border').removeClass('gj-grid-right-border');
                    if ($thTarget.droppable('isOver', mousePosition)) {
                        if (targetPosition < sourcePosition) {
                            $thTarget.before($thSource);
                        } else {
                            $thTarget.after($thSource);
                        }
                        gj.grid.plugins.columnReorder.private.moveRowCells($grid, sourcePosition, targetPosition);
                        data.columns.splice(targetPosition, 0, data.columns.splice(sourcePosition, 1)[0]);
                    }
                    $thTarget.droppable('destroy');
                });
            }
        },

        moveRowCells: function ($grid, sourcePosition, targetPosition) {
            var i, $row, $rows = $grid.find('tbody tr[data-role="row"]');
            for (i = 0; i < $rows.length; i++) {
                $row = $($rows[i]);
                if (targetPosition < sourcePosition) {
                    $row.find('td:eq(' + targetPosition + ')').before($row.find('td:eq(' + sourcePosition + ')'));
                } else {
                    $row.find('td:eq(' + targetPosition + ')').after($row.find('td:eq(' + sourcePosition + ')'));
                }
            }
        },

        createDroppableOverHandler: function ($grid, $thSource) {
            return function (e) {
                var $thTarget = $(this),
                    data = $grid.data(),
                    targetPosition = gj.grid.methods.getColumnPosition(data.columns, $thTarget.data('field')),
                    sourcePosition = gj.grid.methods.getColumnPosition(data.columns, $thSource.data('field'));
                if (targetPosition < sourcePosition) {
                    $thTarget.addClass('gj-grid-left-border');
                    $grid.find('tbody tr[data-role="row"] td:nth-child(' + ($thTarget.index() + 1) + ')').addClass('gj-grid-left-border');
                } else {
                    $thTarget.addClass('gj-grid-right-border');
                    $grid.find('tbody tr[data-role="row"] td:nth-child(' + ($thTarget.index() + 1) + ')').addClass('gj-grid-right-border');
                }
            };
        },

        droppableOut: function () {
            var $thTarget = $(this);
            $thTarget.removeClass('gj-grid-left-border').removeClass('gj-grid-right-border');
            $thTarget.closest('table').find('tbody tr[data-role="row"] td:nth-child(' + ($thTarget.index() + 1) + ')').removeClass('gj-grid-left-border').removeClass('gj-grid-right-border');
        }
    },

    public: {
    },

    configure: function ($grid, fullConfig, clientConfig) {
        $.extend(true, $grid, gj.grid.plugins.columnReorder.public);
        if (fullConfig.columnReorder) {
            $grid.on('initialized', function () {
                gj.grid.plugins.columnReorder.private.init($grid);
            });
        }
    }
};
gj.grid.plugins.headerFilter = {
    config: {
        base: {
            defaultColumnSettings: {
                filterable: true
            },
            headerFilter: {
                type: 'onenterkeypress'
            }
        }
    },

    private: {
        init: function ($grid) {
            var i, $th, $ctrl, data = $grid.data(),
                $filterTr = $('<tr data-role="filter"/>');

            for (i = 0; i < data.columns.length; i++) {
                $th = $('<th/>');
                if (data.columns[i].filterable) {
                    $ctrl = $('<input data-field="' + data.columns[i].field + '" class="gj-width-full" />');
                    if ('onchange' === data.headerFilter.type) {
                        $ctrl.on('input propertychange', function (e) {
                            gj.grid.plugins.headerFilter.private.reload($grid, $(this));
                        });
                    } else {
                        $ctrl.on('keypress', function (e) {
                            if (e.which == 13) {
                                gj.grid.plugins.headerFilter.private.reload($grid, $(this));
                            }
                        });
                        $ctrl.on('blur', function (e) {
                            gj.grid.plugins.headerFilter.private.reload($grid, $(this));
                        });
                    }
                    $th.append($ctrl);
                }
                if (data.columns[i].hidden) {
                    $th.hide();
                }
                $filterTr.append($th);
            }

            $grid.children('thead').append($filterTr);
        },

        reload: function ($grid, $ctrl) {
            var params = {};
            params[$ctrl.data('field')] = $ctrl.val();
            $grid.reload(params);
        }
    },

    public: {
    },

    events: {
    },

    configure: function ($grid, fullConfig, clientConfig) {
        $.extend(true, $grid, gj.grid.plugins.headerFilter.public);
        var data = $grid.data();
        if (clientConfig.headerFilter) {
            $grid.on('initialized', function () {
                gj.grid.plugins.headerFilter.private.init($grid);
            });
        }
    }
};
gj.grid.plugins.grouping = {
    config: {
        base: {
            paramNames: {
                groupBy: 'groupBy',

                groupByTitle: undefined,
                groupByDirection: 'groupByDirection'
            },

            grouping: {
                groupBy: undefined,

                direction: 'asc'
            },

            icons: {
                expandGroup: '<i class="gj-icon plus" />',
                collapseGroup: '<i class="gj-icon minus" />'
            }
        },

        fontawesome: {
            icons: {
                expandGroup: '<i class="fa fa-plus" aria-hidden="true"></i>',
                collapseGroup: '<i class="fa fa-minus" aria-hidden="true"></i>'
            }
        },

        glyphicons: {
            icons: {
                expandGroup: '<span class="glyphicon glyphicon-plus" />',
                collapseGroup: '<span class="glyphicon glyphicon-minus" />'
            }
        }
    },

    private: {
        init: function ($grid) {
            var previousValue, data = $grid.data();

            previousValue = undefined;
            $grid.on('rowDataBound', function (e, $row, id, record) {
                if (previousValue !== record[data.grouping.groupBy] || $row[0].rowIndex === 1) {
                    var colspan = gj.grid.methods.countVisibleColumns($grid) - 1,
                        $groupRow = $('<tr role="group" />'),
                        $expandCollapseCell = $('<td class="gj-text-align-center gj-unselectable gj-cursor-pointer" />');

                    $expandCollapseCell.append('<div data-role="display">' + data.icons.collapseGroup + '</div>');
                    $expandCollapseCell.on('click', gj.grid.plugins.grouping.private.createExpandCollapseHandler(data));
                    $groupRow.append($expandCollapseCell);
                    let groupByTitle = data.grouping.groupByTitle;
                    if (!groupByTitle) groupByTitle = data.grouping.groupBy;
                    $groupRow.append('<td colspan="' + colspan + '"><div data-role="display">' + groupByTitle + ': ' + record[data.grouping.groupBy] + '</div></td>');
                    $groupRow.insertBefore($row);
                    previousValue = record[data.grouping.groupBy];
                }
                $row.show();
            });

            data.params[data.paramNames.groupBy] = data.grouping.groupBy;
            data.params[data.paramNames.groupByDirection] = data.grouping.direction;
        },

        grouping: function ($grid, records) {
            var data = $grid.data();
            records.sort(gj.grid.methods.createDefaultSorter(data.grouping.direction, data.grouping.groupBy));
        },

        createExpandCollapseHandler: function (data) {
            return function (e) {
                var $cell = $(this),
                    methods = gj.grid.plugins.grouping.private;
                if ($cell.closest('tr').next(':visible').data('role') === 'row') {
                    methods.collapseGroup(data, $cell);
                } else {
                    methods.expandGroup(data, $cell);
                }
            };
        },

        collapseGroup: function (data, $cell) {
            var $display = $cell.children('div[data-role="display"]'),
                $groupRow = $cell.closest('tr');

            $groupRow.nextUntil('[role="group"]').hide();
            $display.empty().append(data.icons.expandGroup);
        },

        expandGroup: function (data, $cell) {
            var $display = $cell.children('div[data-role="display"]'),
                $groupRow = $cell.closest('tr');

            $groupRow.nextUntil('[role="group"]').show();
            $display.empty().append(data.icons.collapseGroup);
        }
    },

    public: {},

    configure: function ($grid) {
        var column, data = $grid.data();
        $.extend(true, $grid, gj.grid.plugins.grouping.public);
        if (data.grouping && data.grouping.groupBy) {
            column = {
                title: '',
                width: data.defaultIconColumnWidth,
                align: 'center',
                stopPropagation: true,
                cssClass: 'gj-cursor-pointer gj-unselectable'
            };
            data.columns = [column].concat(data.columns);

            $grid.on('initialized', function () {
                gj.grid.plugins.grouping.private.init($grid);
            });

            $grid.on('dataFiltered', function (e, records) {
                gj.grid.plugins.grouping.private.grouping($grid, records);
            });
        }
    }
};

gj.grid.messages['en-us'] = {
    First: 'İlk',
    Previous: 'Önceki',
    Next: 'Sonraki',
    Last: 'Son',
    Page: 'Sayfa',
    FirstPageTooltip: 'İlk Sayfa',
    PreviousPageTooltip: 'Önceki Sayfa',
    NextPageTooltip: 'Sonraki Sayfa',
    LastPageTooltip: 'Son Sayfa',
    Refresh: 'Yenile',
    Of: '/',
    DisplayingRecords: 'Kayıtlar Gösteriliyor',
    RowsPerPage: 'Sayfadaki Kayıt Sayısı:',
    Edit: 'Düzenle',
    Delete: 'Sil',
    Update: 'Güncelle',
    Cancel: 'İptal',
    NoRecordsFound: 'Kayıt bulunamadı',
    Loading: 'Yükleniyor...'
};
/**
  * @widget Tree
  * @plugin Base
  */
gj.tree = {
    plugins: {}
};

gj.tree.config = {
    base: {

        params: {},
        autoLoad: true,
        selectionType: 'single',
        cascadeSelection: false,
        dataSource: undefined,
        primaryKey: undefined,
        textField: 'text',
        childrenField: 'children',
        hasChildrenField: 'hasChildren',
        imageCssClassField: 'imageCssClass',
        imageUrlField: 'imageUrl',
        imageHtmlField: 'imageHtml',
        disabledField: 'disabled',
        width: undefined,
        border: false,
        uiLibrary: 'materialdesign',
        iconsLibrary: 'materialicons',

        autoGenId: 1,

        autoGenFieldName: 'autoId_b5497cc5-7ef3-49f5-a7dc-4a932e1aee4a',

        indentation: 24,

        style: {
            wrapper: 'gj-unselectable',
            list: 'gj-list gj-list-md',
            item: undefined,
            active: 'gj-list-md-active',
            leafIcon: undefined,
            border: 'gj-tree-md-border'
        },

        icons: {
            expand: '<i class="gj-icon chevron-right" />',
            collapse: '<i class="gj-icon chevron-down" />'
        }
    },

    bootstrap: {
        style: {
            wrapper: 'gj-unselectable gj-tree-bootstrap-3',
            list: 'gj-list gj-list-bootstrap list-group',
            item: 'list-group-item',
            active: 'active',
            border: 'gj-tree-bootstrap-border'
        },
        iconsLibrary: 'glyphicons'
    },

    bootstrap4: {
        style: {
            wrapper: 'gj-unselectable gj-tree-bootstrap-4',
            list: 'gj-list gj-list-bootstrap',
            item: 'list-group-item',
            active: 'active',
            border: 'gj-tree-bootstrap-border'
        },
        icons: {
            expand: '<i class="gj-icon plus" />',
            collapse: '<i class="gj-icon minus" />'
        }
    },

    materialicons: {
        style: {
            expander: 'gj-tree-material-icons-expander'
        }
    },

    fontawesome: {
        style: {
            expander: 'gj-tree-font-awesome-expander'
        },
        icons: {
            expand: '<i class="fa fa-plus" aria-hidden="true"></i>',
            collapse: '<i class="fa fa-minus" aria-hidden="true"></i>'
        }
    },

    glyphicons: {
        style: {
            expander: 'gj-tree-glyphicons-expander'
        },
        icons: {
            expand: '<span class="glyphicon glyphicon-plus" />',
            collapse: '<span class="glyphicon glyphicon-minus" />'
        }
    }
};
gj.tree.events = {
    initialized: function ($tree) {
        $tree.triggerHandler('initialized');
    },
    dataBinding: function ($tree) {
        $tree.triggerHandler('dataBinding');
    },
    dataBound: function ($tree) {
        $tree.triggerHandler('dataBound');
    },
    select: function ($tree, $node, id, record) {
        return $tree.triggerHandler('select', [$node, id, record]);
    },
    unselect: function ($tree, $node, id) {
        return $tree.triggerHandler('unselect', [$node, id]);
    },
    expand: function ($tree, $node, id) {
        return $tree.triggerHandler('expand', [$node, id]);
    },
    collapse: function ($tree, $node, id) {
        return $tree.triggerHandler('collapse', [$node, id]);
    },
    enable: function ($tree, $node) {
        return $tree.triggerHandler('enable', [$node]);
    },
    disable: function ($tree, $node) {
        return $tree.triggerHandler('disable', [$node]);
    },
    destroying: function ($tree) {
        return $tree.triggerHandler('destroying');
    },
    nodeDataBound: function ($tree, $node, id, record) {
        return $tree.triggerHandler('nodeDataBound', [$node, id, record]);
    }
}
gj.tree.methods = {

    init: function (jsConfig) {
        gj.widget.prototype.init.call(this, jsConfig, 'tree');

        gj.tree.methods.initialize.call(this);

        if (this.data('autoLoad')) {
            this.reload();
        }
        return this;
    },

    initialize: function () {
        var data = this.data(),
            $root = $('<ul class="' + data.style.list + '"/>');
        this.empty().addClass(data.style.wrapper).append($root);
        if (data.width) {
            this.width(data.width);
        }
        if (data.border) {
            this.addClass(data.style.border);
        }
        gj.tree.events.initialized(this);
    },

    useHtmlDataSource: function ($tree, data) {
        data.dataSource = [];
    },

    render: function ($tree, response) {
        var data;
        if (response) {
            if (typeof (response) === 'string' && JSON) {
                response = JSON.parse(response);
            }
            data = $tree.data();
            data.records = response;
            if (!data.primaryKey) {
                gj.tree.methods.genAutoId(data, data.records);
            }
            gj.tree.methods.loadData($tree);
        }
        return $tree;
    },

    filter: function ($tree) {
        return $tree.data().dataSource;
    },

    genAutoId: function (data, records) {
        var i;
        for (i = 0; i < records.length; i++) {
            records[i][data.autoGenFieldName] = data.autoGenId++;
            if (records[i][data.childrenField] && records[i][data.childrenField].length) {
                gj.tree.methods.genAutoId(data, records[i][data.childrenField]);
            }
        }
    },

    loadData: function ($tree) {
        var i,
            records = $tree.data('records'),
            $root = $tree.children('ul');

        gj.tree.events.dataBinding($tree);
        $root.off().empty();
        for (i = 0; i < records.length; i++) {
            gj.tree.methods.appendNode($tree, $root, records[i], 1);
        }
        gj.tree.events.dataBound($tree);
    },

    appendNode: function ($tree, $parent, nodeData, level, position) {
        var i, $node, $newParent, $span, $img,
            data = $tree.data(),
            id = data.primaryKey ? nodeData[data.primaryKey] : nodeData[data.autoGenFieldName];
        $node = $('<li data-id="' + id + '" data-role="node" />').addClass(data.style.item),
            $wrapper = $('<div data-role="wrapper" />'),
            $expander = $('<span data-role="expander" data-mode="close"></span>').addClass(data.style.expander),
            $display = $('<span data-role="display">' + nodeData[data.textField] + '</span>'),
            hasChildren = typeof (nodeData[data.hasChildrenField]) !== 'undefined' && nodeData[data.hasChildrenField].toString().toLowerCase() === 'true',
            disabled = typeof (nodeData[data.disabledField]) !== 'undefined' && nodeData[data.disabledField].toString().toLowerCase() === 'true';

        if (data.indentation) {
            $wrapper.append('<span data-role="spacer" style="width: ' + (data.indentation * (level - 1)) + 'px;"></span>');
        }

        if (disabled) {
            gj.tree.methods.disableNode($tree, $node);
        } else {
            $expander.on('click', gj.tree.methods.expanderClickHandler($tree));
            $display.on('click', gj.tree.methods.displayClickHandler($tree));
        }
        $wrapper.append($expander);
        $wrapper.append($display);
        $node.append($wrapper);

        if (position) {
            $parent.find('li:eq(' + (position - 1) + ')').before($node);
        } else {
            $parent.append($node);
        }

        if (data.imageCssClassField && nodeData[data.imageCssClassField]) {
            $span = $('<span data-role="image"><span class="' + nodeData[data.imageCssClassField] + '"></span></span>');
            $span.insertBefore($display);
        } else if (data.imageUrlField && nodeData[data.imageUrlField]) {
            $span = $('<span data-role="image"></span>');
            $span.insertBefore($display);
            $img = $('<img src="' + nodeData[data.imageUrlField] + '"></img>');
            $img.attr('width', $span.width()).attr('height', $span.height());
            $span.append($img);
        } else if (data.imageHtmlField && nodeData[data.imageHtmlField]) {
            $span = $('<span data-role="image">' + nodeData[data.imageHtmlField] + '</span>');
            $span.insertBefore($display);
        }

        if ((nodeData[data.childrenField] && nodeData[data.childrenField].length) || hasChildren) {
            $expander.empty().append(data.icons.expand);
            $newParent = $('<ul />').addClass(data.style.list).addClass('gj-hidden');
            $node.append($newParent);

            if (nodeData[data.childrenField] && nodeData[data.childrenField].length) {
                for (i = 0; i < nodeData[data.childrenField].length; i++) {
                    gj.tree.methods.appendNode($tree, $newParent, nodeData[data.childrenField][i], level + 1);
                }
            }
        } else {
            data.style.leafIcon ? $expander.addClass(data.style.leafIcon) : $expander.html('&nbsp;');
        }

        gj.tree.events.nodeDataBound($tree, $node, nodeData.id, nodeData);
    },

    expanderClickHandler: function ($tree) {
        return function (e) {
            var $expander = $(this),
                $node = $expander.closest('li');
            if ($expander.attr('data-mode') === 'close') {
                $tree.expand($node);
            } else {
                $tree.collapse($node);
            }
        }
    },

    expand: function ($tree, $node, cascade) {
        var $children, i,
            $expander = $node.find('>[data-role="wrapper"]>[data-role="expander"]'),
            data = $tree.data(),
            id = $node.attr('data-id'),
            $list = $node.children('ul');
        if (gj.tree.events.expand($tree, $node, id) !== false && $list && $list.length) {
            $list.show();
            $expander.attr('data-mode', 'open');
            $expander.empty().append(data.icons.collapse);
            if (cascade) {
                $children = $node.find('ul>li');
                for (i = 0; i < $children.length; i++) {
                    gj.tree.methods.expand($tree, $($children[i]), cascade);
                }
            }
        }
        return $tree;
    },

    collapse: function ($tree, $node, cascade) {
        var $children, i,
            $expander = $node.find('>[data-role="wrapper"]>[data-role="expander"]'),
            data = $tree.data(),
            id = $node.attr('data-id'),
            $list = $node.children('ul');
        if (gj.tree.events.collapse($tree, $node, id) !== false && $list && $list.length) {
            $list.hide();
            $expander.attr('data-mode', 'close');
            $expander.empty().append(data.icons.expand);
            if (cascade) {
                $children = $node.find('ul>li');
                for (i = 0; i < $children.length; i++) {
                    gj.tree.methods.collapse($tree, $($children[i]), cascade);
                }
            }
        }
        return $tree;
    },

    expandAll: function ($tree) {
        var i, $nodes = $tree.find('ul>li');
        for (i = 0; i < $nodes.length; i++) {
            gj.tree.methods.expand($tree, $($nodes[i]), true);
        }
        return $tree;
    },

    collapseAll: function ($tree) {
        var i, $nodes = $tree.find('ul>li');
        for (i = 0; i < $nodes.length; i++) {
            gj.tree.methods.collapse($tree, $($nodes[i]), true);
        }
        return $tree;
    },

    displayClickHandler: function ($tree) {
        return function (e) {
            var $display = $(this),
                $node = $display.closest('li'),
                cascade = $tree.data().cascadeSelection;
            if ($node.attr('data-selected') === 'true') {
                gj.tree.methods.unselect($tree, $node, cascade);
            } else {
                if ($tree.data('selectionType') === 'single') {
                    gj.tree.methods.unselectAll($tree);
                }
                gj.tree.methods.select($tree, $node, cascade);
            }
        }
    },

    selectAll: function ($tree) {
        var i, $nodes = $tree.find('ul>li');
        for (i = 0; i < $nodes.length; i++) {
            gj.tree.methods.select($tree, $($nodes[i]), true);
        }
        return $tree;
    },

    select: function ($tree, $node, cascade) {
        var i, $children, data = $tree.data();
        var recordId = $node.attr('data-id');
        var record = this.getDataById($tree, recordId, data.records);
        if ($node.attr('data-selected') !== 'true' && gj.tree.events.select($tree, $node, recordId, record) !== false) {
            $node.addClass(data.style.active).attr('data-selected', 'true');
            if (cascade) {
                $children = $node.find('ul>li');
                for (i = 0; i < $children.length; i++) {
                    gj.tree.methods.select($tree, $($children[i]), cascade);
                }
            }
        }
    },

    unselectAll: function ($tree) {
        var i, $nodes = $tree.find('ul>li');
        for (i = 0; i < $nodes.length; i++) {
            gj.tree.methods.unselect($tree, $($nodes[i]), true);
        }
        return $tree;
    },

    unselect: function ($tree, $node, cascade) {
        var i, $children, data = $tree.data();
        if ($node.attr('data-selected') === 'true' && gj.tree.events.unselect($tree, $node, $node.attr('data-id')) !== false) {
            $node.removeClass($tree.data().style.active).removeAttr('data-selected');
            if (cascade) {
                $children = $node.find('ul>li');
                for (i = 0; i < $children.length; i++) {
                    gj.tree.methods.unselect($tree, $($children[i]), cascade);
                }
            }
        }
    },

    getSelections: function ($list) {
        var i, $node, children,
            result = [],
            $nodes = $list.children('li');
        if ($nodes && $nodes.length) {
            for (i = 0; i < $nodes.length; i++) {
                $node = $($nodes[i]);
                if ($node.attr('data-selected') === 'true') {
                    result.push($node.attr('data-id'));
                } else if ($node.has('ul')) {
                    children = gj.tree.methods.getSelections($node.children('ul'));
                    if (children.length) {
                        result = result.concat(children);
                    }
                }
            }
        }

        return result;
    },

    getDataById: function ($tree, id, records) {
        var i, data = $tree.data(), result = undefined;
        for (i = 0; i < records.length; i++) {
            if (data.primaryKey && records[i][data.primaryKey] == id) {
                result = records[i];
                break;
            } else if (records[i][data.autoGenFieldName] == id) {
                result = records[i];
                break;
            } else if (records[i][data.childrenField] && records[i][data.childrenField].length) {
                result = gj.tree.methods.getDataById($tree, id, records[i][data.childrenField]);
                if (result) {
                    break;
                }
            }
        }
        return result;
    },

    getDataByText: function ($tree, text, records) {
        var i, id,
            result = undefined,
            data = $tree.data();
        for (i = 0; i < records.length; i++) {
            if (text === records[i][data.textField]) {
                result = records[i];
                break;
            } else if (records[i][data.childrenField] && records[i][data.childrenField].length) {
                result = gj.tree.methods.getDataByText($tree, text, records[i][data.childrenField]);
                if (result) {
                    break;
                }
            }
        }
        return result;
    },

    getNodeById: function ($list, id) {
        var i, $node,
            $result = undefined,
            $nodes = $list.children('li');
        if ($nodes && $nodes.length) {
            for (i = 0; i < $nodes.length; i++) {
                $node = $($nodes[i]);
                if (id == $node.attr('data-id')) {
                    $result = $node;
                    break;
                } else if ($node.has('ul')) {
                    $result = gj.tree.methods.getNodeById($node.children('ul'), id);
                    if ($result) {
                        break;
                    }
                }
            }
        }
        return $result;
    },

    getNodeByText: function ($list, text) {
        var i, $node,
            $result = undefined,
            $nodes = $list.children('li');
        if ($nodes && $nodes.length) {
            for (i = 0; i < $nodes.length; i++) {
                $node = $($nodes[i]);
                if (text === $node.find('>[data-role="wrapper"]>[data-role="display"]').text()) {
                    $result = $node;
                    break;
                } else if ($node.has('ul')) {
                    $result = gj.tree.methods.getNodeByText($node.children('ul'), text);
                    if ($result) {
                        break;
                    }
                }
            }
        }
        return $result;
    },

    addNode: function ($tree, nodeData, $parent, position) {
        var level, record, data = $tree.data();

        if (!$parent || !$parent.length) {
            $parent = $tree.children('ul');
            $tree.data('records').push(nodeData);
        } else {
            if ($parent[0].tagName.toLowerCase() === 'li') {
                if ($parent.children('ul').length === 0) {
                    $parent.find('[data-role="expander"]').empty().append(data.icons.collapse);
                    $parent.append($('<ul />').addClass(data.style.list));
                }
                $parent = $parent.children('ul');
            }
            record = $tree.getDataById($parent.parent().data('id'));
            if (!record[data.childrenField]) {
                record[data.childrenField] = [];
            }
            record[data.childrenField].push(nodeData);
        }
        level = $parent.parentsUntil('[data-type="tree"]', 'ul').length + 1;
        if (!data.primaryKey) {
            gj.tree.methods.genAutoId(data, [nodeData]);
        }

        gj.tree.methods.appendNode($tree, $parent, nodeData, level, position);

        return $tree;
    },

    remove: function ($tree, $node) {
        gj.tree.methods.removeDataById($tree, $node.attr('data-id'), $tree.data('records'));
        $node.remove();
        return $tree;
    },

    removeDataById: function ($tree, id, records) {
        var i, data = $tree.data();
        for (i = 0; i < records.length; i++) {
            if (data.primaryKey && records[i][data.primaryKey] == id) {
                records.splice(i, 1);
                break;
            } else if (records[i][data.autoGenFieldName] == id) {
                records.splice(i, 1);
                break;
            } else if (records[i][data.childrenField] && records[i][data.childrenField].length) {
                gj.tree.methods.removeDataById($tree, id, records[i][data.childrenField]);
            }
        }
    },

    update: function ($tree, id, newRecord) {
        var data = $tree.data(),
            $node = $tree.getNodeById(id),
            oldRecord = $tree.getDataById(id);
        oldRecord = newRecord;
        $node.find('>[data-role="wrapper"]>[data-role="display"]').html(newRecord[data.textField]);
        gj.tree.events.nodeDataBound($tree, $node, id, newRecord);
        return $tree;
    },

    getChildren: function ($tree, $node, cascade) {
        var result = [], i, $children,
            cascade = typeof (cascade) === 'undefined' ? true : cascade;

        if (cascade) {
            $children = $node.find('ul li');
        } else {
            $children = $node.find('>ul>li');
        }

        for (i = 0; i < $children.length; i++) {
            result.push($($children[i]).data('id'));
        }

        return result;
    },

    enableAll: function ($tree) {
        var i, $children = $tree.find('ul>li');
        for (i = 0; i < $children.length; i++) {
            gj.tree.methods.enableNode($tree, $($children[i]), true);
        }
        return $tree;
    },

    enableNode: function ($tree, $node, cascade) {
        var i, $children,
            $expander = $node.find('>[data-role="wrapper"]>[data-role="expander"]'),
            $display = $node.find('>[data-role="wrapper"]>[data-role="display"]'),
            cascade = typeof (cascade) === 'undefined' ? true : cascade;

        $node.removeClass('disabled');
        $expander.on('click', gj.tree.methods.expanderClickHandler($tree));
        $display.on('click', gj.tree.methods.displayClickHandler($tree));
        gj.tree.events.enable($tree, $node);
        if (cascade) {
            $children = $node.find('ul>li');
            for (i = 0; i < $children.length; i++) {
                gj.tree.methods.enableNode($tree, $($children[i]), cascade);
            }
        }
    },

    disableAll: function ($tree) {
        var i, $children = $tree.find('ul>li');
        for (i = 0; i < $children.length; i++) {
            gj.tree.methods.disableNode($tree, $($children[i]), true);
        }
        return $tree;
    },

    disableNode: function ($tree, $node, cascade) {
        var i, $children,
            $expander = $node.find('>[data-role="wrapper"]>[data-role="expander"]'),
            $display = $node.find('>[data-role="wrapper"]>[data-role="display"]'),
            cascade = typeof (cascade) === 'undefined' ? true : cascade;

        $node.addClass('disabled');
        $expander.off('click');
        $display.off('click');
        gj.tree.events.disable($tree, $node);
        if (cascade) {
            $children = $node.find('ul>li');
            for (i = 0; i < $children.length; i++) {
                gj.tree.methods.disableNode($tree, $($children[i]), cascade);
            }
        }
    },

    destroy: function ($tree) {
        var data = $tree.data();
        if (data) {
            gj.tree.events.destroying($tree);
            $tree.xhr && $tree.xhr.abort();
            $tree.off();
            $tree.removeData();
            $tree.removeAttr('data-type');
            $tree.removeClass().empty();
        }
        return $tree;
    },

    pathFinder: function (data, list, id, parents) {
        var i, result = false;

        for (i = 0; i < list.length; i++) {
            if (list[i].id == id) {
                result = true;
                break;
            } else if (gj.tree.methods.pathFinder(data, list[i][data.childrenField], id, parents)) {
                parents.push(list[i].data[data.textField]);
                result = true;
                break;
            }
        }

        return result;
    }
}
gj.tree.widget = function ($element, jsConfig) {
    var self = this,
        methods = gj.tree.methods;
    self.reload = function (params) {
        return gj.widget.prototype.reload.call(this, params);
    };
    self.render = function (response) {
        return methods.render(this, response);
    };
    self.addNode = function (data, $parentNode, position) {
        return methods.addNode(this, data, $parentNode, position);
    };
    self.removeNode = function ($node) {
        return methods.remove(this, $node);
    };
    self.updateNode = function (id, record) {
        return methods.update(this, id, record);
    };
    self.destroy = function () {
        return methods.destroy(this);
    };
    self.expand = function ($node, cascade) {
        return methods.expand(this, $node, cascade);
    };
    self.collapse = function ($node, cascade) {
        return methods.collapse(this, $node, cascade);
    };
    self.expandAll = function () {
        return methods.expandAll(this);
    };
    self.collapseAll = function () {
        return methods.collapseAll(this);
    };
    self.getDataById = function (id) {
        return methods.getDataById(this, id, this.data('records'));
    };
    self.getDataByText = function (text) {
        return methods.getDataByText(this, text, this.data('records'));
    };
    self.getNodeById = function (id) {
        return methods.getNodeById(this.children('ul'), id);
    };
    self.getNodeByText = function (text) {
        return methods.getNodeByText(this.children('ul'), text);
    };
    self.getAll = function () {
        return this.data('records');
    };
    self.select = function ($node) {
        return methods.select(this, $node);
    };
    self.unselect = function ($node) {
        return methods.unselect(this, $node);
    };
    self.selectAll = function () {
        return methods.selectAll(this);
    };
    self.unselectAll = function () {
        return methods.unselectAll(this);
    };
    self.getSelections = function () {
        return methods.getSelections(this.children('ul'));
    };
    self.getChildren = function ($node, cascade) {
        return methods.getChildren(this, $node, cascade);
    };
    self.parents = function (id) {
        var parents = [], data = this.data();
        methods.pathFinder(data, data.records, id, parents);
        return parents.reverse();
    };
    self.enable = function ($node, cascade) {
        return methods.enableNode(this, $node, cascade);
    };
    self.enableAll = function () {
        return methods.enableAll(this);
    };
    self.disable = function ($node, cascade) {
        return methods.disableNode(this, $node, cascade);
    };
    self.disableAll = function () {
        return methods.disableAll(this);
    };

    $.extend($element, self);
    if ('tree' !== $element.attr('data-type')) {
        methods.init.call($element, jsConfig);
    }

    return $element;
};

gj.tree.widget.prototype = new gj.widget();
gj.tree.widget.constructor = gj.tree.widget;

(function ($) {
    $.fn.tree = function (method) {
        var $widget;
        if (this && this.length) {
            if (typeof method === 'object' || !method) {
                return new gj.tree.widget(this, method);
            } else {
                $widget = new gj.tree.widget(this, null);
                if ($widget[method]) {
                    return $widget[method].apply(this, Array.prototype.slice.call(arguments, 1));
                } else {
                    throw 'Method ' + method + ' does not exist.';
                }
            }
        }
    };
})(jQuery);
gj.tree.plugins.checkboxes = {
    config: {
        base: {
            checkboxes: undefined,
            checkedField: 'checked',
            cascadeCheck: true,
        }
    },

    private: {
        dataBound: function ($tree) {
            var $nodes;
            if ($tree.data('cascadeCheck')) {
                $nodes = $tree.find('li[data-role="node"]');
                $.each($nodes, function () {
                    var $node = $(this),
                        state = $node.find('[data-role="checkbox"] input[type="checkbox"]').checkbox('state');
                    if (state === 'checked') {
                        gj.tree.plugins.checkboxes.private.updateChildrenState($node, state);
                        gj.tree.plugins.checkboxes.private.updateParentState($node, state);
                    }
                });
            }
        },

        nodeDataBound: function ($tree, $node, id, record) {
            var data, $expander, $checkbox, $wrapper, disabled;

            if ($node.find('> [data-role="wrapper"] > [data-role="checkbox"]').length === 0) {
                data = $tree.data();
                $expander = $node.find('> [data-role="wrapper"] > [data-role="expander"]');
                $checkbox = $('<input type="checkbox"/>');
                $wrapper = $('<span data-role="checkbox"></span>').append($checkbox);
                disabled = typeof (record[data.disabledField]) !== 'undefined' && record[data.disabledField].toString().toLowerCase() === 'true';

                $checkbox = $checkbox.checkbox({
                    uiLibrary: data.uiLibrary,
                    iconsLibrary: data.iconsLibrary,
                    change: function (e, state) {
                        gj.tree.plugins.checkboxes.events.checkboxChange($tree, $node, record, $checkbox.state());
                    }
                });
                disabled && $checkbox.prop('disabled', true);
                record[data.checkedField] && $checkbox.state('checked');
                $checkbox.on('click', function (e) {
                    var $node = $checkbox.closest('li'),
                        state = $checkbox.state();
                    if (data.cascadeCheck) {
                        gj.tree.plugins.checkboxes.private.updateChildrenState($node, state);
                        gj.tree.plugins.checkboxes.private.updateParentState($node, state);
                    }
                });
                $expander.after($wrapper);
            }
        },

        updateParentState: function ($node, state) {
            var $parentNode, $parentCheckbox, $siblingCheckboxes, allChecked, allUnchecked, parentState;

            $parentNode = $node.parent('ul').parent('li');
            if ($parentNode.length === 1) {
                $parentCheckbox = $node.parent('ul').parent('li').find('> [data-role="wrapper"] > [data-role="checkbox"] input[type="checkbox"]');
                $siblingCheckboxes = $node.siblings().find('> [data-role="wrapper"] > span[data-role="checkbox"] input[type="checkbox"]');
                allChecked = (state === 'checked');
                allUnchecked = (state === 'unchecked');
                parentState = 'indeterminate';
                $.each($siblingCheckboxes, function () {
                    var state = $(this).checkbox('state');
                    if (allChecked && state !== 'checked') {
                        allChecked = false;
                    }
                    if (allUnchecked && state !== 'unchecked') {
                        allUnchecked = false;
                    }
                });
                if (allChecked && !allUnchecked) {
                    parentState = 'checked';
                }
                if (!allChecked && allUnchecked) {
                    parentState = 'unchecked';
                }
                $parentCheckbox.checkbox('state', parentState);
                gj.tree.plugins.checkboxes.private.updateParentState($parentNode, $parentCheckbox.checkbox('state'));
            }
        },

        updateChildrenState: function ($node, state) {
            var $childrenCheckboxes = $node.find('ul li [data-role="wrapper"] [data-role="checkbox"] input[type="checkbox"]');
            if ($childrenCheckboxes.length > 0) {
                $.each($childrenCheckboxes, function () {
                    $(this).checkbox('state', state);
                });
            }
        },

        update: function ($tree, $node, state) {
            var checkbox = $node.find('[data-role="checkbox"] input[type="checkbox"]').first();
            $(checkbox).checkbox('state', state);
            if ($tree.data().cascadeCheck) {
                gj.tree.plugins.checkboxes.private.updateChildrenState($node, state);
                gj.tree.plugins.checkboxes.private.updateParentState($node, state);
            }
        }
    },

    public: {
        getCheckedNodes: function () {
            var result = [],
                checkboxes = this.find('li [data-role="checkbox"] input[type="checkbox"]');
            $.each(checkboxes, function () {
                var checkbox = $(this);
                if (checkbox.checkbox('state') === 'checked') {
                    result.push(checkbox.closest('li').data('id'));
                }
            });
            return result;
        },
        checkAll: function () {
            var $checkboxes = this.find('li [data-role="checkbox"] input[type="checkbox"]');
            $.each($checkboxes, function () {
                $(this).checkbox('state', 'checked');
            });
            return this;
        },
        uncheckAll: function () {
            var $checkboxes = this.find('li [data-role="checkbox"] input[type="checkbox"]');
            $.each($checkboxes, function () {
                $(this).checkbox('state', 'unchecked');
            });
            return this;
        },
        check: function ($node) {
            gj.tree.plugins.checkboxes.private.update(this, $node, 'checked');
            return this;
        },
        uncheck: function ($node) {
            gj.tree.plugins.checkboxes.private.update(this, $node, 'unchecked');
            return this;
        }
    },

    events: {
        checkboxChange: function ($tree, $node, record, state) {
            return $tree.triggerHandler('checkboxChange', [$node, record, state]);
        }
    },

    configure: function ($tree) {
        if ($tree.data('checkboxes') && gj.checkbox) {
            $.extend(true, $tree, gj.tree.plugins.checkboxes.public);
            $tree.on('nodeDataBound', function (e, $node, id, record) {
                gj.tree.plugins.checkboxes.private.nodeDataBound($tree, $node, id, record);
            });
            $tree.on('dataBound', function () {
                gj.tree.plugins.checkboxes.private.dataBound($tree);
            });
            $tree.on('enable', function (e, $node) {
                $node.find('>[data-role="wrapper"]>[data-role="checkbox"] input[type="checkbox"]').prop('disabled', false);
            });
            $tree.on('disable', function (e, $node) {
                $node.find('>[data-role="wrapper"]>[data-role="checkbox"] input[type="checkbox"]').prop('disabled', true);
            });
        }
    }
};
gj.tree.plugins.dragAndDrop = {
    config: {
        base: {
            dragAndDrop: undefined,

            style: {
                dragEl: 'gj-tree-drag-el gj-tree-md-drag-el',
                dropAsChildIcon: 'gj-cursor-pointer gj-icon plus',
                dropAbove: 'gj-tree-drop-above',
                dropBelow: 'gj-tree-drop-below'
            }
        },

        bootstrap: {
            style: {
                dragEl: 'gj-tree-drag-el gj-tree-bootstrap-drag-el',
                dropAsChildIcon: 'glyphicon glyphicon-plus',
                dropAbove: 'drop-above',
                dropBelow: 'drop-below'
            }
        },

        bootstrap4: {
            style: {
                dragEl: 'gj-tree-drag-el gj-tree-bootstrap-drag-el',
                dropAsChildIcon: 'gj-cursor-pointer gj-icon plus',
                dropAbove: 'drop-above',
                dropBelow: 'drop-below'
            }
        }
    },

    private: {
        nodeDataBound: function ($tree, $node) {
            var $wrapper = $node.children('[data-role="wrapper"]'),
                $display = $node.find('>[data-role="wrapper"]>[data-role="display"]');
            if ($wrapper.length && $display.length) {
                $display.on('mousedown', gj.tree.plugins.dragAndDrop.private.createNodeMouseDownHandler($tree));
                $display.on('mousemove', gj.tree.plugins.dragAndDrop.private.createNodeMouseMoveHandler($tree, $node, $display));
                $display.on('mouseup', gj.tree.plugins.dragAndDrop.private.createNodeMouseUpHandler($tree));
            }
        },

        createNodeMouseDownHandler: function ($tree) {
            return function (e) {
                $tree.data('dragReady', true);
            }
        },

        createNodeMouseUpHandler: function ($tree) {
            return function (e) {
                $tree.data('dragReady', false);
            }
        },

        createNodeMouseMoveHandler: function ($tree, $node, $display) {
            return function (e) {
                if ($tree.data('dragReady')) {
                    var data = $tree.data(), $dragEl, $wrapper, offsetTop, offsetLeft;

                    $tree.data('dragReady', false);
                    $dragEl = $display.clone().wrap('<div data-role="wrapper"/>').closest('div')
                        .wrap('<li class="' + data.style.item + '" />').closest('li')
                        .wrap('<ul class="' + data.style.list + '" />').closest('ul');
                    $('body').append($dragEl);
                    $dragEl.attr('data-role', 'draggable-clone').addClass('gj-unselectable').addClass(data.style.dragEl);
                    $dragEl.find('[data-role="wrapper"]').prepend('<span data-role="indicator" />');
                    $dragEl.draggable({
                        drag: gj.tree.plugins.dragAndDrop.private.createDragHandler($tree, $node, $display),
                        stop: gj.tree.plugins.dragAndDrop.private.createDragStopHandler($tree, $node, $display)
                    });
                    $wrapper = $display.parent();
                    offsetTop = $display.offset().top;
                    offsetTop -= parseInt($wrapper.css("border-top-width")) + parseInt($wrapper.css("margin-top")) + parseInt($wrapper.css("padding-top"));
                    offsetLeft = $display.offset().left;
                    offsetLeft -= parseInt($wrapper.css("border-left-width")) + parseInt($wrapper.css("margin-left")) + parseInt($wrapper.css("padding-left"));
                    offsetLeft -= $dragEl.find('[data-role="indicator"]').outerWidth(true);
                    $dragEl.css({
                        position: 'absolute', top: offsetTop, left: offsetLeft, width: $display.outerWidth(true)
                    });
                    if ($display.attr('data-droppable') === 'true') {
                        $display.droppable('destroy');
                    }
                    gj.tree.plugins.dragAndDrop.private.getTargetDisplays($tree, $node, $display).each(function () {
                        var $dropEl = $(this);
                        if ($dropEl.attr('data-droppable') === 'true') {
                            $dropEl.droppable('destroy');
                        }
                        $dropEl.droppable();
                    });
                    gj.tree.plugins.dragAndDrop.private.getTargetDisplays($tree, $node).each(function () {
                        var $dropEl = $(this);
                        if ($dropEl.attr('data-droppable') === 'true') {
                            $dropEl.droppable('destroy');
                        }
                        $dropEl.droppable();
                    });
                    $dragEl.trigger('mousedown');
                }
            };
        },

        getTargetDisplays: function ($tree, $node, $display) {
            return $tree.find('[data-role="display"]').not($display).not($node.find('[data-role="display"]'));
        },

        getTargetWrappers: function ($tree, $node) {
            return $tree.find('[data-role="wrapper"]').not($node.find('[data-role="wrapper"]'));
        },

        createDragHandler: function ($tree, $node, $display) {
            var $displays = gj.tree.plugins.dragAndDrop.private.getTargetDisplays($tree, $node, $display),
                $wrappers = gj.tree.plugins.dragAndDrop.private.getTargetWrappers($tree, $node),
                data = $tree.data();
            return function (e, offset, mousePosition) {
                var $dragEl = $(this), success = false;
                $displays.each(function () {
                    var $targetDisplay = $(this),
                        $indicator;
                    if ($targetDisplay.droppable('isOver', mousePosition)) {
                        $indicator = $dragEl.find('[data-role="indicator"]');
                        data.style.dropAsChildIcon ? $indicator.addClass(data.style.dropAsChildIcon) : $indicator.text('+');
                        success = true;
                        return false;
                    } else {
                        $dragEl.find('[data-role="indicator"]').removeClass(data.style.dropAsChildIcon).empty();
                    }
                });
                $wrappers.each(function () {
                    var $wrapper = $(this),
                        $indicator, middle;
                    if (!success && $wrapper.droppable('isOver', mousePosition)) {
                        middle = $wrapper.position().top + ($wrapper.outerHeight() / 2);
                        if (mousePosition.y < middle) {
                            $wrapper.addClass(data.style.dropAbove).removeClass(data.style.dropBelow);
                        } else {
                            $wrapper.addClass(data.style.dropBelow).removeClass(data.style.dropAbove);
                        }
                    } else {
                        $wrapper.removeClass(data.style.dropAbove).removeClass(data.style.dropBelow);
                    }
                });
            };
        },

        createDragStopHandler: function ($tree, $sourceNode, $sourceDisplay) {
            var $displays = gj.tree.plugins.dragAndDrop.private.getTargetDisplays($tree, $sourceNode, $sourceDisplay),
                $wrappers = gj.tree.plugins.dragAndDrop.private.getTargetWrappers($tree, $sourceNode),
                data = $tree.data();
            return function (e, mousePosition) {
                var success = false, record, $targetNode, $sourceParentNode, parent;
                $(this).draggable('destroy').remove();
                $displays.each(function () {
                    var $targetDisplay = $(this), $ul;
                    if ($targetDisplay.droppable('isOver', mousePosition)) {
                        $targetNode = $targetDisplay.closest('li');
                        $sourceParentNode = $sourceNode.parent('ul').parent('li');
                        $ul = $targetNode.children('ul');
                        if ($ul.length === 0) {
                            $ul = $('<ul />').addClass(data.style.list);
                            $targetNode.append($ul);
                        }
                        if (gj.tree.plugins.dragAndDrop.events.nodeDrop($tree, $sourceNode.data('id'), $targetNode.data('id'), $ul.children('li').length + 1) !== false) {
                            $ul.append($sourceNode);
                            record = $tree.getDataById($sourceNode.data('id'));
                            gj.tree.methods.removeDataById($tree, $sourceNode.data('id'), data.records);
                            parent = $tree.getDataById($ul.parent().data('id'));
                            if (parent[data.childrenField] === undefined) {
                                parent[data.childrenField] = [];
                            }
                            parent[data.childrenField].push(record);

                            gj.tree.plugins.dragAndDrop.private.refresh($tree, $sourceNode, $targetNode, $sourceParentNode);
                        }
                        success = true;
                        return false;
                    }
                    $targetDisplay.droppable('destroy');
                });
                if (!success) {
                    $wrappers.each(function () {
                        var $targetWrapper = $(this), prepend, orderNumber, sourceNodeId;
                        if ($targetWrapper.droppable('isOver', mousePosition)) {
                            $targetNode = $targetWrapper.closest('li');
                            $sourceParentNode = $sourceNode.parent('ul').parent('li');
                            prepend = mousePosition.y < ($targetWrapper.position().top + ($targetWrapper.outerHeight() / 2));
                            sourceNodeId = $sourceNode.data('id');
                            orderNumber = $targetNode.prevAll('li:not([data-id="' + sourceNodeId + '"])').length + (prepend ? 1 : 2);
                            if (gj.tree.plugins.dragAndDrop.events.nodeDrop($tree, sourceNodeId, $targetNode.parent('ul').parent('li').data('id'), orderNumber) !== false) {
                                record = $tree.getDataById($sourceNode.data('id'));
                                gj.tree.methods.removeDataById($tree, $sourceNode.data('id'), data.records);
                                $tree.getDataById($targetNode.parent().data('id'))[data.childrenField].splice($targetNode.index() + (prepend ? 0 : 1), 0, record);

                                if (prepend) {
                                    $sourceNode.insertBefore($targetNode);
                                } else {
                                    $sourceNode.insertAfter($targetNode);
                                }

                                gj.tree.plugins.dragAndDrop.private.refresh($tree, $sourceNode, $targetNode, $sourceParentNode);
                            }
                            return false;
                        }
                        $targetWrapper.droppable('destroy');
                    });
                }
            }
        },

        refresh: function ($tree, $sourceNode, $targetNode, $sourceParentNode) {
            var data = $tree.data();
            gj.tree.plugins.dragAndDrop.private.refreshNode($tree, $targetNode);
            gj.tree.plugins.dragAndDrop.private.refreshNode($tree, $sourceParentNode);
            gj.tree.plugins.dragAndDrop.private.refreshNode($tree, $sourceNode);
            $sourceNode.find('li[data-role="node"]').each(function () {
                gj.tree.plugins.dragAndDrop.private.refreshNode($tree, $(this));
            });
            $targetNode.children('[data-role="wrapper"]').removeClass(data.style.dropAbove).removeClass(data.style.dropBelow);
        },

        refreshNode: function ($tree, $node) {
            var $wrapper = $node.children('[data-role="wrapper"]'),
                $expander = $wrapper.children('[data-role="expander"]'),
                $spacer = $wrapper.children('[data-role="spacer"]'),
                $list = $node.children('ul'),
                data = $tree.data(),
                level = $node.parentsUntil('[data-type="tree"]', 'ul').length;

            if ($list.length && $list.children().length) {
                if ($list.is(':visible')) {
                    $expander.empty().append(data.icons.collapse);
                } else {
                    $expander.empty().append(data.icons.expand);
                }
            } else {
                $expander.empty();
            }
            $wrapper.removeClass(data.style.dropAbove).removeClass(data.style.dropBelow);

            $spacer.css('width', (data.indentation * (level - 1)));
        }
    },

    public: {
    },

    events: {
        nodeDrop: function ($tree, id, parentId, orderNumber) {
            return $tree.triggerHandler('nodeDrop', [id, parentId, orderNumber]);
        }
    },

    configure: function ($tree) {
        $.extend(true, $tree, gj.tree.plugins.dragAndDrop.public);
        if ($tree.data('dragAndDrop') && gj.draggable && gj.droppable) {
            $tree.on('nodeDataBound', function (e, $node) {
                gj.tree.plugins.dragAndDrop.private.nodeDataBound($tree, $node);
            });
        }
    }
};
gj.tree.plugins.lazyLoading = {
    config: {
        base: {

            paramNames: {
                parentId: 'parentId'
            },
            lazyLoading: false
        }
    },

    private: {
        nodeDataBound: function ($tree, $node, id, record) {
            var data = $tree.data(),
                $expander = $node.find('> [data-role="wrapper"] > [data-role="expander"]');

            if (record.hasChildren) {
                $expander.empty().append(data.icons.expand);
            }
        },

        createDoneHandler: function ($tree, $node) {
            return function (response) {
                var i, $expander, $list, data = $tree.data();
                if (typeof (response) === 'string' && JSON) {
                    response = JSON.parse(response);
                }
                if (response && response.length) {
                    $list = $node.children('ul');
                    if ($list.length === 0) {
                        $list = $('<ul />').addClass(data.style.list);
                        $node.append($list);
                    }
                    for (i = 0; i < response.length; i++) {
                        $tree.addNode(response[i], $list);
                    }
                    $expander = $node.find('>[data-role="wrapper"]>[data-role="expander"]'),
                        $expander.attr('data-mode', 'open');
                    $expander.empty().append(data.icons.collapse);
                    gj.tree.events.dataBound($tree);
                }
            };
        },

        expand: function ($tree, $node, id) {
            var ajaxOptions, data = $tree.data(), params = {},
                $children = $node.find('>ul>li');

            if (!$children || !$children.length) {
                if (typeof (data.dataSource) === 'string') {
                    params[data.paramNames.parentId] = id;
                    ajaxOptions = { url: data.dataSource, data: params };
                    if ($tree.xhr) {
                        $tree.xhr.abort();
                    }
                    $tree.xhr = $.ajax(ajaxOptions).done(gj.tree.plugins.lazyLoading.private.createDoneHandler($tree, $node)).fail($tree.createErrorHandler());
                }
            }
        }
    },

    public: {},

    events: {},

    configure: function ($tree, fullConfig, clientConfig) {
        if (clientConfig.lazyLoading) {
            $tree.on('nodeDataBound', function (e, $node, id, record) {
                gj.tree.plugins.lazyLoading.private.nodeDataBound($tree, $node, id, record);
            });
            $tree.on('expand', function (e, $node, id) {
                gj.tree.plugins.lazyLoading.private.expand($tree, $node, id);
            });
        }
    }
};
/** 
 * @widget Checkbox 
 * @plugin Base
 */
gj.checkbox = {
    plugins: {}
};

gj.checkbox.config = {
    base: {
        uiLibrary: 'materialdesign',
        iconsLibrary: 'materialicons',

        style: {
            wrapperCssClass: 'gj-checkbox-md',
            spanCssClass: undefined
        }

    },

    bootstrap: {
        style: {
            wrapperCssClass: 'gj-checkbox-bootstrap'
        },
        iconsLibrary: 'glyphicons'
    },

    bootstrap4: {
        style: {
            wrapperCssClass: 'gj-checkbox-bootstrap gj-checkbox-bootstrap-4'
        },
        iconsLibrary: 'materialicons'
    },

    materialicons: {
        style: {
            iconsCssClass: 'gj-checkbox-material-icons',
            spanCssClass: 'gj-icon'
        }
    },

    glyphicons: {
        style: {
            iconsCssClass: 'gj-checkbox-glyphicons',
            spanCssClass: ''
        }
    },

    fontawesome: {
        style: {
            iconsCssClass: 'gj-checkbox-fontawesome',
            spanCssClass: 'fa'
        }
    }
};

gj.checkbox.methods = {
    init: function (jsConfig) {
        var $chkb = this;

        gj.widget.prototype.init.call(this, jsConfig, 'checkbox');
        $chkb.attr('data-checkbox', 'true');

        gj.checkbox.methods.initialize($chkb);

        return $chkb;
    },

    initialize: function ($chkb) {
        var data = $chkb.data(), $wrapper, $span;

        if (data.style.wrapperCssClass) {
            $wrapper = $('<label class="' + data.style.wrapperCssClass + ' ' + data.style.iconsCssClass + '"></label>');
            if ($chkb.attr('id')) {
                $wrapper.attr('for', $chkb.attr('id'));
            }
            $chkb.wrap($wrapper);
            $span = $('<span />');
            if (data.style.spanCssClass) {
                $span.addClass(data.style.spanCssClass);
            }
            $chkb.parent().append($span);
        }
    },

    state: function ($chkb, value) {
        if (value) {
            if ('checked' === value) {
                $chkb.prop('indeterminate', false);
                $chkb.prop('checked', true);
            } else if ('unchecked' === value) {
                $chkb.prop('indeterminate', false);
                $chkb.prop('checked', false);
            } else if ('indeterminate' === value) {
                $chkb.prop('checked', true);
                $chkb.prop('indeterminate', true);
            }
            gj.checkbox.events.change($chkb, value);
            return $chkb;
        } else {
            if ($chkb.prop('indeterminate')) {
                value = 'indeterminate';
            } else if ($chkb.prop('checked')) {
                value = 'checked';
            } else {
                value = 'unchecked';
            }
            return value;
        }
    },

    toggle: function ($chkb) {
        if ($chkb.state() == 'checked') {
            $chkb.state('unchecked');
        } else {
            $chkb.state('checked');
        }
        return $chkb;
    },

    destroy: function ($chkb) {
        if ($chkb.attr('data-checkbox') === 'true') {
            $chkb.removeData();
            $chkb.removeAttr('data-guid');
            $chkb.removeAttr('data-checkbox');
            $chkb.off();
            $chkb.next('span').remove();
            $chkb.unwrap();
        }
        return $chkb;
    }
};

gj.checkbox.events = {
    change: function ($chkb, state) {
        return $chkb.triggerHandler('change', [state]);
    }
};


gj.checkbox.widget = function ($element, jsConfig) {
    var self = this,
        methods = gj.checkbox.methods;
    self.toggle = function () {
        return methods.toggle(this);
    };
    self.state = function (value) {
        return methods.state(this, value);
    };
    self.destroy = function () {
        return methods.destroy(this);
    };

    $.extend($element, self);
    if ('true' !== $element.attr('data-checkbox')) {
        methods.init.call($element, jsConfig);
    }

    return $element;
};

gj.checkbox.widget.prototype = new gj.widget();
gj.checkbox.widget.constructor = gj.checkbox.widget;

(function ($) {
    $.fn.checkbox = function (method) {
        var $widget;
        if (this && this.length) {
            if (typeof method === 'object' || !method) {
                return new gj.checkbox.widget(this, method);
            } else {
                $widget = new gj.checkbox.widget(this, null);
                if ($widget[method]) {
                    return $widget[method].apply(this, Array.prototype.slice.call(arguments, 1));
                } else {
                    throw 'Method ' + method + ' does not exist.';
                }
            }
        }
    };
})(jQuery);
/** 
 * @widget Editor
 * @plugin Base
 */
gj.editor = {
    plugins: {},
    messages: {}
};

gj.editor.config = {
    base: {
        height: 300,
        width: undefined,
        uiLibrary: 'materialdesign',
        iconsLibrary: 'materialicons',
        locale: 'en-us',

        buttons: undefined,

        style: {
            wrapper: 'gj-editor gj-editor-md',
            buttonsGroup: 'gj-button-md-group',
            button: 'gj-button-md',
            buttonActive: 'active'
        }
    },

    bootstrap: {
        style: {
            wrapper: 'gj-editor gj-editor-bootstrap',
            buttonsGroup: 'btn-group',
            button: 'btn btn-default gj-cursor-pointer',
            buttonActive: 'active'
        }
    },

    bootstrap4: {
        style: {
            wrapper: 'gj-editor gj-editor-bootstrap',
            buttonsGroup: 'btn-group',
            button: 'btn btn-outline-secondary gj-cursor-pointer',
            buttonActive: 'active'
        }
    },

    materialicons: {
        icons: {
            bold: '<i class="gj-icon bold" />',
            italic: '<i class="gj-icon italic" />',
            strikethrough: '<i class="gj-icon strikethrough" />',
            underline: '<i class="gj-icon underlined" />',

            listBulleted: '<i class="gj-icon list-bulleted" />',
            listNumbered: '<i class="gj-icon list-numbered" />',
            indentDecrease: '<i class="gj-icon indent-decrease" />',
            indentIncrease: '<i class="gj-icon indent-increase" />',

            alignLeft: '<i class="gj-icon align-left" />',
            alignCenter: '<i class="gj-icon align-center" />',
            alignRight: '<i class="gj-icon align-right" />',
            alignJustify: '<i class="gj-icon align-justify" />',

            undo: '<i class="gj-icon undo" />',
            redo: '<i class="gj-icon redo" />'
        }
    },

    fontawesome: {
        icons: {
            bold: '<i class="fa fa-bold" aria-hidden="true"></i>',
            italic: '<i class="fa fa-italic" aria-hidden="true"></i>',
            strikethrough: '<i class="fa fa-strikethrough" aria-hidden="true"></i>',
            underline: '<i class="fa fa-underline" aria-hidden="true"></i>',

            listBulleted: '<i class="fa fa-list-ul" aria-hidden="true"></i>',
            listNumbered: '<i class="fa fa-list-ol" aria-hidden="true"></i>',
            indentDecrease: '<i class="fa fa-indent" aria-hidden="true"></i>',
            indentIncrease: '<i class="fa fa-outdent" aria-hidden="true"></i>',

            alignLeft: '<i class="fa fa-align-left" aria-hidden="true"></i>',
            alignCenter: '<i class="fa fa-align-center" aria-hidden="true"></i>',
            alignRight: '<i class="fa fa-align-right" aria-hidden="true"></i>',
            alignJustify: '<i class="fa fa-align-justify" aria-hidden="true"></i>',

            undo: '<i class="fa fa-undo" aria-hidden="true"></i>',
            redo: '<i class="fa fa-repeat" aria-hidden="true"></i>'
        }
    }
};

gj.editor.methods = {
    init: function (jsConfig) {
        gj.widget.prototype.init.call(this, jsConfig, 'editor');
        this.attr('data-editor', 'true');
        gj.editor.methods.initialize(this);
        return this;
    },

    initialize: function ($editor) {
        var self = this, data = $editor.data(),
            $group, $btn, wrapper, $body, $toolbar;

        $editor.hide();

        if ($editor[0].parentElement.attributes.role !== 'wrapper') {
            wrapper = document.createElement('div');
            wrapper.setAttribute('role', 'wrapper');
            $editor[0].parentNode.insertBefore(wrapper, $editor[0]);
            wrapper.appendChild($editor[0]);
        }

        gj.editor.methods.localization(data);
        $(wrapper).addClass(data.style.wrapper);
        if (data.width) {
            $(wrapper).width(data.width);
        }

        $body = $(wrapper).children('div[role="body"]');
        if ($body.length === 0) {
            $body = $('<div role="body"></div>');
            $(wrapper).append($body);
            if ($editor[0].innerText) {
                $body[0].innerHTML = $editor[0].innerText;
            }
        }
        $body.attr('contenteditable', true);
        $body.on('keydown', function (e) {
            var key = event.keyCode || event.charCode;
            if (gj.editor.events.changing($editor) === false && key !== 8 && key !== 46) {
                e.preventDefault();
            }
        });
        $body.on('mouseup keyup mouseout cut paste', function (e) {
            self.updateToolbar($editor, $toolbar);
            gj.editor.events.changed($editor);
            $editor.html($body.html());
        });

        $toolbar = $(wrapper).children('div[role="toolbar"]');
        if ($toolbar.length === 0) {
            $toolbar = $('<div role="toolbar"></div>');
            $body.before($toolbar);

            for (var group in data.buttons) {
                $group = $('<div />').addClass(data.style.buttonsGroup);
                for (var btn in data.buttons[group]) {
                    $btn = $(data.buttons[group][btn]);
                    $btn.on('click', function () {
                        gj.editor.methods.executeCmd($editor, $body, $toolbar, $(this));
                    });
                    $group.append($btn);
                }
                $toolbar.append($group);
            }
        }

        $body.height(data.height - gj.core.height($toolbar[0], true));
    },

    localization: function (data) {
        var msg = gj.editor.messages[data.locale];
        if (typeof (data.buttons) === 'undefined') {
            data.buttons = [
                [
                    '<button type="button" class="' + data.style.button + '" title="' + msg.bold + '" role="bold">' + data.icons.bold + '</button>',
                    '<button type="button" class="' + data.style.button + '" title="' + msg.italic + '" role="italic">' + data.icons.italic + '</button>',
                    '<button type="button" class="' + data.style.button + '" title="' + msg.strikethrough + '" role="strikethrough">' + data.icons.strikethrough + '</button>',
                    '<button type="button" class="' + data.style.button + '" title="' + msg.underline + '" role="underline">' + data.icons.underline + '</button>'
                ],
                [
                    '<button type="button" class="' + data.style.button + '" title="' + msg.listBulleted + '" role="insertunorderedlist">' + data.icons.listBulleted + '</button>',
                    '<button type="button" class="' + data.style.button + '" title="' + msg.listNumbered + '" role="insertorderedlist">' + data.icons.listNumbered + '</button>',
                    '<button type="button" class="' + data.style.button + '" title="' + msg.indentDecrease + '" role="outdent">' + data.icons.indentDecrease + '</button>',
                    '<button type="button" class="' + data.style.button + '" title="' + msg.indentIncrease + '" role="indent">' + data.icons.indentIncrease + '</button>'
                ],
                [
                    '<button type="button" class="' + data.style.button + '" title="' + msg.alignLeft + '" role="justifyleft">' + data.icons.alignLeft + '</button>',
                    '<button type="button" class="' + data.style.button + '" title="' + msg.alignCenter + '" role="justifycenter">' + data.icons.alignCenter + '</button>',
                    '<button type="button" class="' + data.style.button + '" title="' + msg.alignRight + '" role="justifyright">' + data.icons.alignRight + '</button>',
                    '<button type="button" class="' + data.style.button + '" title="' + msg.alignJustify + '" role="justifyfull">' + data.icons.alignJustify + '</button>'
                ],
                [
                    '<button type="button" class="' + data.style.button + '" title="' + msg.undo + '" role="undo">' + data.icons.undo + '</button>',
                    '<button type="button" class="' + data.style.button + '" title="' + msg.redo + '" role="redo">' + data.icons.redo + '</button>'
                ]
            ];
        }
    },

    updateToolbar: function ($editor, $toolbar) {
        var data = $editor.data();
        $buttons = $toolbar.find('[role]').each(function () {
            var $btn = $(this),
                cmd = $btn.attr('role');

            if (cmd && document.queryCommandEnabled(cmd) && document.queryCommandValue(cmd) === "true") {
                $btn.addClass(data.style.buttonActive);
            } else {
                $btn.removeClass(data.style.buttonActive);
            }
        });
    },

    executeCmd: function ($editor, $body, $toolbar, $btn) {
        $body.focus();
        document.execCommand($btn.attr('role'), false);
        gj.editor.methods.updateToolbar($editor, $toolbar);
    },

    content: function ($editor, html) {
        var $body = $editor.parent().children('div[role="body"]');
        if (typeof (html) === "undefined") {
            return $body.html();
        } else {
            return $body.html(html);
        }
    },

    destroy: function ($editor) {
        var $wrapper;
        if ($editor.attr('data-editor') === 'true') {
            $wrapper = $editor.parent();
            $wrapper.children('div[role="body"]').remove();
            $wrapper.children('div[role="toolbar"]').remove();
            $editor.unwrap();
            $editor.removeData();
            $editor.removeAttr('data-guid');
            $editor.removeAttr('data-editor');
            $editor.off();
            $editor.show();
        }
        return $editor;
    }
};

gj.editor.events = {
    changing: function ($editor) {
        return $editor.triggerHandler('changing');
    },
    changed: function ($editor) {
        return $editor.triggerHandler('changed');
    }
};

gj.editor.widget = function ($element, jsConfig) {
    var self = this,
        methods = gj.editor.methods;
    self.content = function (html) {
        return methods.content(this, html);
    };
    self.destroy = function () {
        return methods.destroy(this);
    };

    $.extend($element, self);
    if ('true' !== $element.attr('data-editor')) {
        methods.init.call($element, jsConfig);
    }

    return $element;
};

gj.editor.widget.prototype = new gj.widget();
gj.editor.widget.constructor = gj.editor.widget;

(function ($) {
    $.fn.editor = function (method) {
        var $widget;
        if (this && this.length) {
            if (typeof method === 'object' || !method) {
                return new gj.editor.widget(this, method);
            } else {
                $widget = new gj.editor.widget(this, null);
                if ($widget[method]) {
                    return $widget[method].apply(this, Array.prototype.slice.call(arguments, 1));
                } else {
                    throw 'Method ' + method + ' does not exist.';
                }
            }
        }
    };
})(jQuery);
gj.editor.messages['en-us'] = {
    bold: 'Bold',
    italic: 'Italic',
    strikethrough: 'Strikethrough',
    underline: 'Underline',
    listBulleted: 'List Bulleted',
    listNumbered: 'List Numbered',
    indentDecrease: 'Indent Decrease',
    indentIncrease: 'Indent Increase',
    alignLeft: 'Align Left',
    alignCenter: 'Align Center',
    alignRight: 'Align Right',
    alignJustify: 'Align Justify',
    undo: 'Undo',
    redo: 'Redo'
};
/**
  * @widget DropDown
  * @plugin Base
  */
gj.dropdown = {
    plugins: {}
};

gj.dropdown.config = {
    base: {
        dataSource: undefined,
        textField: 'text',
        valueField: 'value',
        selectedField: 'selected',
        width: undefined,
        maxHeight: 'auto',
        placeholder: undefined,

        fontSize: undefined,
        uiLibrary: 'materialdesign',
        iconsLibrary: 'materialicons',

        icons: {
            dropdown: '<i class="gj-icon arrow-dropdown" />',

            dropup: '<i class="gj-icon arrow-dropup" />'
        },

        style: {
            wrapper: 'gj-dropdown gj-dropdown-md gj-unselectable',
            list: 'gj-list gj-list-md gj-dropdown-list-md',
            active: 'gj-list-md-active'
        }
    },

    bootstrap: {
        style: {
            wrapper: 'gj-dropdown gj-dropdown-bootstrap gj-dropdown-bootstrap-3 gj-unselectable',
            presenter: 'btn btn-default',
            list: 'gj-list gj-list-bootstrap gj-dropdown-list-bootstrap list-group',
            item: 'list-group-item',
            active: 'active'
        },
        iconsLibrary: 'glyphicons'
    },

    bootstrap4: {
        style: {
            wrapper: 'gj-dropdown gj-dropdown-bootstrap gj-dropdown-bootstrap-4 gj-unselectable',
            presenter: 'btn btn-outline-secondary',
            list: 'gj-list gj-list-bootstrap gj-dropdown-list-bootstrap list-group',
            item: 'list-group-item',
            active: 'active'
        }
    },

    materialicons: {
        style: {
            expander: 'gj-dropdown-expander-mi'
        }
    },

    fontawesome: {
        icons: {
            dropdown: '<i class="fa fa-caret-down" aria-hidden="true"></i>',
            dropup: '<i class="fa fa-caret-up" aria-hidden="true"></i>'
        },
        style: {
            expander: 'gj-dropdown-expander-fa'
        }
    },

    glyphicons: {
        icons: {
            dropdown: '<span class="caret"></span>',
            dropup: '<span class="dropup"><span class="caret" ></span></span>'
        },
        style: {
            expander: 'gj-dropdown-expander-glyphicons'
        }
    }
};

gj.dropdown.methods = {
    init: function (jsConfig) {
        gj.widget.prototype.init.call(this, jsConfig, 'dropdown');
        this.attr('data-dropdown', 'true');
        gj.dropdown.methods.initialize(this);
        return this;
    },

    getHTMLConfig: function () {
        var result = gj.widget.prototype.getHTMLConfig.call(this),
            attrs = this[0].attributes;
        if (attrs['placeholder']) {
            result.placeholder = attrs['placeholder'].value;
        }
        return result;
    },

    initialize: function ($dropdown) {
        var $item,
            data = $dropdown.data(),
            $wrapper = $dropdown.parent('div[role="wrapper"]'),
            $display = $('<span role="display"></span>'),
            $expander = $('<span role="expander">' + data.icons.dropdown + '</span>').addClass(data.style.expander),
            $presenter = $('<button role="presenter" type="button"></button>').addClass(data.style.presenter),
            $list = $('<ul role="list" class="' + data.style.list + '"></ul>').attr('guid', $dropdown.attr('data-guid'));

        if ($wrapper.length === 0) {
            $wrapper = $('<div role="wrapper" />').addClass(data.style.wrapper); // The css class needs to be added before the wrapping, otherwise doesn't work.
            $dropdown.wrap($wrapper);
        } else {
            $wrapper.addClass(data.style.wrapper);
        }

        if (data.fontSize) {
            $presenter.css('font-size', data.fontSize);
        }

        $presenter.on('click', function (e) {
            if ($list.is(':visible')) {
                gj.dropdown.methods.close($dropdown, $list);
            } else {
                gj.dropdown.methods.open($dropdown, $list);
            }
        });
        $presenter.on('blur', function (e) {
            setTimeout(function () {
                gj.dropdown.methods.close($dropdown, $list);
            }, 500);
        });
        $presenter.append($display).append($expander);

        $dropdown.hide();
        $dropdown.after($presenter);
        $('body').append($list);
        $list.hide();

        $dropdown.reload();
    },

    setListPosition: function (presenter, list, data) {
        var top, listHeight, presenterHeight, newHeight, listElRect,
            mainElRect = presenter.getBoundingClientRect(),
            scrollY = window.scrollY || window.pageYOffset || 0,
            scrollX = window.scrollX || window.pageXOffset || 0;
        list.style.overflow = '';
        list.style.overflowX = '';
        list.style.height = '';

        gj.core.setChildPosition(presenter, list);

        listHeight = gj.core.height(list, true);
        listElRect = list.getBoundingClientRect();
        presenterHeight = gj.core.height(presenter, true);
        if (data.maxHeight === 'auto') {
            if (mainElRect.top < listElRect.top) { // The list is located below the main element
                if (mainElRect.top + listHeight + presenterHeight > window.innerHeight) {
                    newHeight = window.innerHeight - mainElRect.top - presenterHeight - 3;
                }
            } else { // The list is located above the main element                
                if (mainElRect.top - listHeight - 3 > 0) {
                    list.style.top = Math.round(mainElRect.top + scrollY - listHeight - 3) + 'px';
                } else {
                    list.style.top = scrollY + 'px';
                    newHeight = mainElRect.top - 3;
                }
            }
        } else if (!isNaN(data.maxHeight) && data.maxHeight < listHeight) {
            newHeight = data.maxHeight;
        }

        if (newHeight) {
            list.style.overflow = 'scroll';
            list.style.overflowX = 'hidden';
            list.style.height = newHeight + 'px';
        }
    },

    useHtmlDataSource: function ($dropdown, data) {
        var dataSource = [], i, record,
            $options = $dropdown.find('option');
        for (i = 0; i < $options.length; i++) {
            record = {};
            record[data.valueField] = $options[i].value;
            record[data.textField] = $options[i].innerHTML;
            record[data.selectedField] = $dropdown[0].value === $options[i].value;
            dataSource.push(record);
        }
        data.dataSource = dataSource;
    },

    filter: function ($dropdown) {
        var i, record, data = $dropdown.data();
        if (!data.dataSource) {
            data.dataSource = [];
        } else if (typeof data.dataSource[0] === 'string') {
            for (i = 0; i < data.dataSource.length; i++) {
                record = {};
                record[data.valueField] = data.dataSource[i];
                record[data.textField] = data.dataSource[i];
                data.dataSource[i] = record;
            }
        }
        return data.dataSource;
    },

    render: function ($dropdown, response) {
        var selections = [],
            data = $dropdown.data(),
            $parent = $dropdown.parent(),
            $list = $('body').children('[role="list"][guid="' + $dropdown.attr('data-guid') + '"]'),
            $presenter = $parent.children('[role="presenter"]'),
            $expander = $presenter.children('[role="expander"]'),
            $display = $presenter.children('[role="display"]');

        $dropdown.data('records', response);
        $dropdown.empty();
        $list.empty();

        if (response && response.length) {
            $.each(response, function () {
                var value = this[data.valueField],
                    text = this[data.textField],
                    selected = this[data.selectedField] && this[data.selectedField].toString().toLowerCase() === 'true',
                    i, $item;

                $item = $('<li value="' + value + '"><div data-role="wrapper"><span data-role="display">' + text + '</span></div></li>');
                $item.addClass(data.style.item);
                $item.on('click', function (e) {
                    gj.dropdown.methods.select($dropdown, value);
                });
                $list.append($item);

                $dropdown.append('<option value="' + value + '">' + text + '</option>');

                if (selected) {
                    selections.push(value);
                }
            });
            if (selections.length === 0) {
                $dropdown.prepend('<option value=""></option>');
                $dropdown[0].selectedIndex = 0;
                if (data.placeholder) {
                    $display[0].innerHTML = '<span class="placeholder">' + data.placeholder + '</span>';
                }
            } else {
                for (i = 0; i < selections.length; i++) {
                    gj.dropdown.methods.select($dropdown, selections[i]);
                }
            }
        }

        if (data.width) {
            $parent.css('width', data.width);
            $presenter.css('width', data.width);
        }

        if (data.fontSize) {
            $list.children('li').css('font-size', data.fontSize);
        }

        gj.dropdown.events.dataBound($dropdown);

        return $dropdown;
    },

    open: function ($dropdown, $list) {
        var data = $dropdown.data(),
            $expander = $dropdown.parent().find('[role="expander"]'),
            $presenter = $dropdown.parent().find('[role="presenter"]'),
            scrollParentEl = gj.core.getScrollParent($dropdown[0]);
        $list.css('width', gj.core.width($presenter[0]));
        $list.show();
        gj.dropdown.methods.setListPosition($presenter[0], $list[0], data);
        $expander.html(data.icons.dropup);
        if (scrollParentEl) {
            data.parentScrollHandler = function () {
                gj.dropdown.methods.setListPosition($presenter[0], $list[0], data);
            };
            gj.dropdown.methods.addParentsScrollListener(scrollParentEl, data.parentScrollHandler);
        }
    },

    close: function ($dropdown, $list) {
        var data = $dropdown.data(),
            $expander = $dropdown.parent().find('[role="expander"]'),
            scrollParentEl = gj.core.getScrollParent($dropdown[0]);
        $expander.html(data.icons.dropdown);
        if (scrollParentEl && data.parentScrollHandler) {
            gj.dropdown.methods.removeParentsScrollListener(scrollParentEl, data.parentScrollHandler);
        }
        $list.hide();
    },

    addParentsScrollListener: function (el, handler) {
        var scrollParentEl = gj.core.getScrollParent(el.parentNode);
        el.addEventListener('scroll', handler);
        if (scrollParentEl) {
            gj.dropdown.methods.addParentsScrollListener(scrollParentEl, handler);
        }
    },

    removeParentsScrollListener: function (el, handler) {
        var scrollParentEl = gj.core.getScrollParent(el.parentNode);
        el.removeEventListener('scroll', handler);
        if (scrollParentEl) {
            gj.dropdown.methods.removeParentsScrollListener(scrollParentEl, handler);
        }
    },

    select: function ($dropdown, value) {
        var data = $dropdown.data(),
            $list = $('body').children('[role="list"][guid="' + $dropdown.attr('data-guid') + '"]'),
            $item = $list.children('li[value="' + value + '"]'),
            $display = $dropdown.next('[role="presenter"]').find('[role="display"]'),
            record = gj.dropdown.methods.getRecordByValue($dropdown, value);

        $list.children('li').removeClass(data.style.active);
        if (record) {
            $item.addClass(data.style.active);
            $dropdown[0].value = value;
            $display[0].innerHTML = record[data.textField];
        } else {
            if (data.placeholder) {
                $display[0].innerHTML = '<span class="placeholder">' + data.placeholder + '</span>';
            }
            $dropdown[0].value = '';
        }
        gj.dropdown.events.change($dropdown);
        gj.dropdown.methods.close($dropdown, $list);
        return $dropdown;
    },

    getRecordByValue: function ($dropdown, value) {
        var data = $dropdown.data(),
            i, result = undefined;

        for (i = 0; i < data.records.length; i++) {
            if (data.records[i][data.valueField] === value) {
                result = data.records[i];
                break;
            }
        }

        return result;
    },

    value: function ($dropdown, value) {
        if (typeof (value) === "undefined") {
            return $dropdown.val();
        } else {
            gj.dropdown.methods.select($dropdown, value);
            return $dropdown;
        }
    },

    destroy: function ($dropdown) {
        var data = $dropdown.data(),
            $parent = $dropdown.parent('div[role="wrapper"]');
        if (data) {
            $dropdown.xhr && $dropdown.xhr.abort();
            $dropdown.off();
            $dropdown.removeData();
            $dropdown.removeAttr('data-type').removeAttr('data-guid').removeAttr('data-dropdown');
            $dropdown.removeClass();
            if ($parent.length > 0) {
                $parent.children('[role="presenter"]').remove();
                $parent.children('[role="list"]').remove();
                $dropdown.unwrap();
            }
            $dropdown.show();
        }
        return $dropdown;
    }
};

gj.dropdown.events = {
    change: function ($dropdown) {
        return $dropdown.triggerHandler('change');
    },
    dataBound: function ($dropdown) {
        return $dropdown.triggerHandler('dataBound');
    }
};

gj.dropdown.widget = function ($element, jsConfig) {
    var self = this,
        methods = gj.dropdown.methods;
    self.value = function (value) {
        return methods.value(this, value);
    };

    self.enable = function () {
        return methods.enable(this);
    };

    self.disable = function () {
        return methods.disable(this);
    };
    self.destroy = function () {
        return methods.destroy(this);
    };

    $.extend($element, self);
    if ('true' !== $element.attr('data-dropdown')) {
        methods.init.call($element, jsConfig);
    }

    return $element;
};

gj.dropdown.widget.prototype = new gj.widget();
gj.dropdown.widget.constructor = gj.dropdown.widget;

gj.dropdown.widget.prototype.getHTMLConfig = gj.dropdown.methods.getHTMLConfig;

(function ($) {
    $.fn.dropdown = function (method) {
        var $widget;
        if (this && this.length) {
            if (typeof method === 'object' || !method) {
                return new gj.dropdown.widget(this, method);
            } else {
                $widget = new gj.dropdown.widget(this, null);
                if ($widget[method]) {
                    return $widget[method].apply(this, Array.prototype.slice.call(arguments, 1));
                } else {
                    throw 'Method ' + method + ' does not exist.';
                }
            }
        }
    };
})(jQuery);
/**
  * @widget DatePicker
  * @plugin Base
  */
gj.datepicker = {
    plugins: {}
};

gj.datepicker.config = {
    base: {
        showOtherMonths: false,
        selectOtherMonths: true,
        width: undefined,
        minDate: undefined,
        maxDate: undefined,
        format: 'mm/dd/yyyy',
        uiLibrary: 'materialdesign',
        iconsLibrary: 'materialicons',
        value: undefined,
        weekStartDay: 0,
        disableDates: undefined,
        disableDaysOfWeek: undefined,
        calendarWeeks: false,
        keyboardNavigation: true,
        locale: 'en-us',

        icons: {
            rightIcon: '<i class="gj-icon">event</i>',

            previousMonth: '<i class="gj-icon chevron-left"></i>',
            nextMonth: '<i class="gj-icon chevron-right"></i>'
        },

        fontSize: undefined,
        size: 'default',
        modal: false,
        header: false,
        footer: false,
        showOnFocus: true,
        showRightIcon: true,

        style: {
            modal: 'gj-modal',
            wrapper: 'gj-datepicker gj-datepicker-md gj-unselectable',
            input: 'gj-textbox-md',
            calendar: 'gj-picker gj-picker-md datepicker gj-unselectable',
            footer: '',
            button: 'gj-button-md'
        }
    },

    bootstrap: {
        style: {
            wrapper: 'gj-datepicker gj-datepicker-bootstrap gj-unselectable input-group',
            input: 'form-control',
            calendar: 'gj-picker gj-picker-bootstrap datepicker gj-unselectable',
            footer: 'modal-footer',
            button: 'btn btn-default'
        },
        iconsLibrary: 'glyphicons',
        showOtherMonths: true
    },

    bootstrap4: {
        style: {
            wrapper: 'gj-datepicker gj-datepicker-bootstrap gj-unselectable input-group',
            input: 'form-control',
            calendar: 'gj-picker gj-picker-bootstrap datepicker gj-unselectable',
            footer: 'modal-footer',
            button: 'btn btn-default'
        },
        showOtherMonths: true
    },

    fontawesome: {
        icons: {
            rightIcon: '<i class="fa fa-calendar" aria-hidden="true"></i>',
            previousMonth: '<i class="fa fa-chevron-left" aria-hidden="true"></i>',
            nextMonth: '<i class="fa fa-chevron-right" aria-hidden="true"></i>'
        }
    },

    glyphicons: {
        icons: {
            rightIcon: '<span class="glyphicon glyphicon-calendar"></span>',
            previousMonth: '<span class="glyphicon glyphicon-chevron-left"></span>',
            nextMonth: '<span class="glyphicon glyphicon-chevron-right"></span>'
        }
    }
};

gj.datepicker.methods = {
    init: function (jsConfig) {
        gj.widget.prototype.init.call(this, jsConfig, 'datepicker');
        this.attr('data-datepicker', 'true');
        gj.datepicker.methods.initialize(this, this.data());
        return this;
    },

    initialize: function ($datepicker, data) {
        var $calendar, $rightIcon,
            $wrapper = $datepicker.parent('div[role="wrapper"]');

        if ($wrapper.length === 0) {
            $wrapper = $('<div role="wrapper" />').addClass(data.style.wrapper); // The css class needs to be added before the wrapping, otherwise doesn't work.
            $datepicker.wrap($wrapper);
        } else {
            $wrapper.addClass(data.style.wrapper);
        }
        $wrapper = $datepicker.parent('div[role="wrapper"]');

        data.width && $wrapper.css('width', data.width);

        $datepicker.val(data.value).addClass(data.style.input).attr('role', 'input');

        data.fontSize && $datepicker.css('font-size', data.fontSize);

        if (data.uiLibrary === 'bootstrap' || data.uiLibrary === 'bootstrap4') {
            if (data.size === 'small') {
                $wrapper.addClass('input-group-sm');
                $datepicker.addClass('form-control-sm');
            } else if (data.size === 'large') {
                $wrapper.addClass('input-group-lg');
                $datepicker.addClass('form-control-lg');
            }
        } else {
            if (data.size === 'small') {
                $wrapper.addClass('small');
            } else if (data.size === 'large') {
                $wrapper.addClass('large');
            }
        }

        if (data.showRightIcon) {
            if (data.uiLibrary === 'bootstrap') {
                $rightIcon = $('<span class="input-group-addon">' + data.icons.rightIcon + '</span>');
            } else if (data.uiLibrary === 'bootstrap4') {
                $rightIcon = $('<span class="input-group-append"><button class="btn btn-outline-secondary border-left-0" type="button">' + data.icons.rightIcon + '</button></span>');
            } else {
                $rightIcon = $(data.icons.rightIcon);
            }
            $rightIcon.attr('role', 'right-icon');
            $rightIcon.on('click', function (e) {
                var $calendar = $('body').find('[role="calendar"][guid="' + $datepicker.attr('data-guid') + '"]');
                if ($calendar.is(':visible')) {
                    gj.datepicker.methods.close($datepicker);
                } else {
                    gj.datepicker.methods.open($datepicker, data);
                }
            });
            $wrapper.append($rightIcon);
        }

        if (data.showOnFocus) {
            $datepicker.on('focus', function () {
                gj.datepicker.methods.open($datepicker, data);
            });
        }

        $calendar = gj.datepicker.methods.createCalendar($datepicker, data);

        if (data.footer !== true) {
            $datepicker.on('blur', function () {
                $datepicker.timeout = setTimeout(function () {
                    gj.datepicker.methods.close($datepicker);
                }, 500);
            });
            $calendar.mousedown(function () {
                clearTimeout($datepicker.timeout);
                document.activeElement !== $datepicker[0] && $datepicker.focus();
                return false;
            });
            $calendar.on('click', function () {
                clearTimeout($datepicker.timeout);
                document.activeElement !== $datepicker[0] && $datepicker.focus();
            });
        }

        if (data.keyboardNavigation) {
            $(document).on('keydown', gj.datepicker.methods.createKeyDownHandler($datepicker, $calendar, data));
        }
    },

    createCalendar: function ($datepicker, data) {
        var date, $body, $footer, $btnCancel, $btnOk,
            $calendar = $('<div role="calendar" type="month"/>').addClass(data.style.calendar).attr('guid', $datepicker.attr('data-guid'));

        data.fontSize && $calendar.css('font-size', data.fontSize);

        date = gj.core.parseDate(data.value, data.format, data.locale);
        if (!date || isNaN(date.getTime())) {
            date = new Date();
        } else {
            $datepicker.attr('day', date.getFullYear() + '-' + date.getMonth() + '-' + date.getDate());
        }

        $calendar.attr('month', date.getMonth());
        $calendar.attr('year', date.getFullYear());

        gj.datepicker.methods.renderHeader($datepicker, $calendar, data, date);

        $body = $('<div role="body" />');
        $calendar.append($body);

        if (data.footer) {
            $footer = $('<div role="footer" class="' + data.style.footer + '" />');

            $btnCancel = $('<button class="' + data.style.button + '">' + gj.core.messages[data.locale].cancel + '</button>');
            $btnCancel.on('click', function () { $datepicker.close(); });
            $footer.append($btnCancel);

            $btnOk = $('<button class="' + data.style.button + '">' + gj.core.messages[data.locale].ok + '</button>');
            $btnOk.on('click', function () {
                var date, dayArr, dayStr = $calendar.attr('selectedDay');
                if (dayStr) {
                    dayArr = dayStr.split('-');
                    date = new Date(dayArr[0], dayArr[1], dayArr[2], $calendar.attr('hour') || 0, $calendar.attr('minute') || 0);
                    gj.datepicker.methods.change($datepicker, $calendar, data, date);
                } else {
                    $datepicker.close();
                }
            });
            $footer.append($btnOk);

            $calendar.append($footer);
        }

        $calendar.hide();
        $('body').append($calendar);

        if (data.modal) {
            $calendar.wrapAll('<div role="modal" class="' + data.style.modal + '"/>');
            gj.core.center($calendar);
        }

        return $calendar;
    },

    renderHeader: function ($datepicker, $calendar, data, date) {
        var $header, $date, $year;

        if (data.header) {
            $header = $('<div role="header" />');
            $year = $('<div role="year" />').on('click', function () {
                gj.datepicker.methods.renderDecade($datepicker, $calendar, data);
                $year.addClass('selected');
                $date.removeClass('selected');
            });
            $year.html(gj.core.formatDate(date, 'yyyy', data.locale));
            $header.append($year);
            $date = $('<div role="date" class="selected" />').on('click', function () {
                gj.datepicker.methods.renderMonth($datepicker, $calendar, data);
                $date.addClass('selected');
                $year.removeClass('selected');
            });
            $date.html(gj.core.formatDate(date, 'ddd, mmm dd', data.locale));
            $header.append($date);
            $calendar.append($header);
        }
    },

    updateHeader: function ($calendar, data, date) {
        $calendar.find('[role="header"] [role="year"]').removeClass('selected').html(gj.core.formatDate(date, 'yyyy', data.locale));
        $calendar.find('[role="header"] [role="date"]').addClass('selected').html(gj.core.formatDate(date, 'ddd, mmm dd', data.locale));
        $calendar.find('[role="header"] [role="hour"]').removeClass('selected').html(gj.core.formatDate(date, 'HH', data.locale));
        $calendar.find('[role="header"] [role="minute"]').removeClass('selected').html(gj.core.formatDate(date, 'MM', data.locale));
    },

    createNavigation: function ($datepicker, $body, $table, data) {
        var $row, $navigator, $thead = $('<thead/>');

        $navigator = $('<div role="navigator" />');
        $navigator.append($('<div>' + data.icons.previousMonth + '</div>').on('click', gj.datepicker.methods.prev($datepicker, data)));
        $navigator.append($('<div role="period"></div>').on('click', gj.datepicker.methods.changePeriod($datepicker, data)));
        $navigator.append($('<div>' + data.icons.nextMonth + '</div>').on('click', gj.datepicker.methods.next($datepicker, data)));
        $body.append($navigator);

        $row = $('<tr role="week-days" />');
        if (data.calendarWeeks) {
            $row.append('<th><div>&nbsp;</div></th>');
        }
        for (i = data.weekStartDay; i < gj.core.messages[data.locale].weekDaysMin.length; i++) {
            $row.append('<th><div>' + gj.core.messages[data.locale].weekDaysMin[i] + '</div></th>');
        }
        for (i = 0; i < data.weekStartDay; i++) {
            $row.append('<th><div>' + gj.core.messages[data.locale].weekDaysMin[i] + '</div></th>');
        }
        $thead.append($row);

        $table.append($thead);
    },

    renderMonth: function ($datepicker, $calendar, data) {
        var weekDay, selectedDay, day, month, year, daysInMonth, total, firstDayPosition, i, now, prevMonth, nextMonth, $cell, $day, date,
            $body = $calendar.children('[role="body"]'),
            $table = $('<table/>'),
            $tbody = $('<tbody/>'),
            period = gj.core.messages[data.locale].titleFormat;

        $body.off().empty();
        gj.datepicker.methods.createNavigation($datepicker, $body, $table, data);

        month = parseInt($calendar.attr('month'), 10);
        year = parseInt($calendar.attr('year'), 10);

        $calendar.attr('type', 'month');
        period = period.replace('mmmm', gj.core.messages[data.locale].monthNames[month]).replace('yyyy', year);
        $calendar.find('div[role="period"]').text(period);
        daysInMonth = new Array(31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31);
        if (year % 4 == 0 && year != 1900) {
            daysInMonth[1] = 29;
        }
        total = daysInMonth[month];

        firstDayPosition = (new Date(year, month, 1).getDay() + 7 - data.weekStartDay) % 7;

        weekDay = 0;
        $row = $('<tr />');
        prevMonth = gj.datepicker.methods.getPrevMonth(month, year);
        for (i = 1; i <= firstDayPosition; i++) {
            day = (daysInMonth[prevMonth.month] - firstDayPosition + i);
            date = new Date(prevMonth.year, prevMonth.month, day);
            if (data.calendarWeeks && i === 1) {
                $row.append('<td class="calendar-week"><div>' + gj.datepicker.methods.getWeekNumber(date) + '</div></td>');
            }
            $cell = $('<td class="other-month" />');
            if (data.showOtherMonths) {
                $day = $('<div>' + day + '</div>');
                $cell.append($day);
                if (data.selectOtherMonths && gj.datepicker.methods.isSelectable(data, date)) {
                    $cell.addClass('gj-cursor-pointer').attr('day', day).attr('month', prevMonth.month).attr('year', prevMonth.year);
                    $day.on('click', gj.datepicker.methods.dayClickHandler($datepicker, $calendar, data, date));
                    $day.on('mousedown', function (e) { e.stopPropagation() });
                } else {
                    $cell.addClass('disabled');
                }
            }
            $row.append($cell);
            weekDay++;
        }
        if (i > 1) {
            $tbody.append($row);
        }

        now = new Date();
        for (i = 1; i <= total; i++) {
            date = new Date(year, month, i);
            if (weekDay == 0) {
                $row = $('<tr>');
                if (data.calendarWeeks) {
                    $row.append('<td class="calendar-week"><div>' + gj.datepicker.methods.getWeekNumber(date) + '</div></td>');
                }
            }
            $cell = $('<td day="' + i + '" month="' + month + '" year="' + year + '" />');
            if (year === now.getFullYear() && month === now.getMonth() && i === now.getDate()) {
                $cell.addClass('today');
            } else {
                $cell.addClass('current-month');
            }
            $day = $('<div>' + i + '</div>');
            if (gj.datepicker.methods.isSelectable(data, date)) {
                $cell.addClass('gj-cursor-pointer');
                $day.on('click', gj.datepicker.methods.dayClickHandler($datepicker, $calendar, data, date));
                $day.on('mousedown', function (e) { e.stopPropagation() });
            } else {
                $cell.addClass('disabled');
            }
            $cell.append($day);
            $row.append($cell);
            weekDay++;
            if (weekDay == 7) {
                $tbody.append($row);
                weekDay = 0;
            }
        }

        nextMonth = gj.datepicker.methods.getNextMonth(month, year);
        for (i = 1; weekDay != 0; i++) {
            date = new Date(nextMonth.year, nextMonth.month, i);
            $cell = $('<td class="other-month" />');
            if (data.showOtherMonths) {
                $day = $('<div>' + i + '</div>');
                if (data.selectOtherMonths && gj.datepicker.methods.isSelectable(data, date)) {
                    $cell.addClass('gj-cursor-pointer').attr('day', i).attr('month', nextMonth.month).attr('year', nextMonth.year);
                    $day.on('click', gj.datepicker.methods.dayClickHandler($datepicker, $calendar, data, date));
                    $day.on('mousedown', function (e) { e.stopPropagation() });
                } else {
                    $cell.addClass('disabled');
                }
                $cell.append($day);
            }
            $row.append($cell);
            weekDay++;
            if (weekDay == 7) {
                $tbody.append($row);
                weekDay = 0;
            }
        }

        $table.append($tbody);
        $body.append($table);

        if ($calendar.attr('selectedDay')) {
            selectedDay = $calendar.attr('selectedDay').split('-');
            date = new Date(selectedDay[0], selectedDay[1], selectedDay[2], $calendar.attr('hour') || 0, $calendar.attr('minute') || 0);
            $calendar.find('tbody td[day="' + selectedDay[2] + '"][month="' + selectedDay[1] + '"]').addClass('selected');
            gj.datepicker.methods.updateHeader($calendar, data, date);
        }
    },

    renderYear: function ($datepicker, $calendar, data) {
        var year, i, m, $month,
            $table = $calendar.find('>[role="body"]>table'),
            $tbody = $table.children('tbody');

        $table.children('thead').hide();

        year = parseInt($calendar.attr('year'), 10);

        $calendar.attr('type', 'year');
        $calendar.find('div[role="period"]').text(year);

        $tbody.empty();

        for (i = 0; i < 3; i++) {
            $row = $('<tr />');
            for (m = (i * 4); m <= (i * 4) + 3; m++) {
                $month = $('<div>' + gj.core.messages[data.locale].monthShortNames[m] + '</div>');
                $month.on('click', gj.datepicker.methods.selectMonth($datepicker, $calendar, data, m));
                $cell = $('<td></td>').append($month);
                $row.append($cell);
            }
            $tbody.append($row);
        }
    },

    renderDecade: function ($datepicker, $calendar, data) {
        var year, decade, i, y, $year,
            $table = $calendar.find('>[role="body"]>table'),
            $tbody = $table.children('tbody');

        $table.children('thead').hide();

        year = parseInt($calendar.attr('year'), 10);
        decade = year - (year % 10);

        $calendar.attr('type', 'decade');
        $calendar.find('div[role="period"]').text(decade + ' - ' + (decade + 9));

        $tbody.empty();

        for (i = decade - 1; i <= decade + 10; i += 4) {
            $row = $('<tr />');
            for (y = i; y <= i + 3; y++) {
                $year = $('<div>' + y + '</div>');
                $year.on('click', gj.datepicker.methods.selectYear($datepicker, $calendar, data, y));
                $cell = $('<td></td>').append($year);
                $row.append($cell);
            }
            $tbody.append($row);
        }
    },

    renderCentury: function ($datepicker, $calendar, data) {
        var year, century, i, d, $decade,
            $table = $calendar.find('>[role="body"]>table'),
            $tbody = $table.children('tbody');

        $table.children('thead').hide();

        year = parseInt($calendar.attr('year'), 10);
        century = year - (year % 100);

        $calendar.attr('type', 'century');
        $calendar.find('div[role="period"]').text(century + ' - ' + (century + 99));

        $tbody.empty();

        for (i = (century - 10); i < century + 100; i += 40) {
            $row = $('<tr />');
            for (d = i; d <= i + 30; d += 10) {
                $decade = $('<div>' + d + '</div>');
                $decade.on('click', gj.datepicker.methods.selectDecade($datepicker, $calendar, data, d));
                $cell = $('<td></td>').append($decade);
                $row.append($cell);
            }
            $tbody.append($row);
        }
    },

    getWeekNumber: function (date) {
        var d = new Date(date.valueOf());
        d.setDate(d.getDate() + 6);
        d = new Date(Date.UTC(d.getFullYear(), d.getMonth(), d.getDate()));
        d.setUTCDate(d.getUTCDate() + 4 - (d.getUTCDay() || 7));
        var yearStart = new Date(Date.UTC(d.getUTCFullYear(), 0, 1));
        var weekNo = Math.ceil((((d - yearStart) / 86400000) + 1) / 7);
        return weekNo;
    },

    getMinDate: function (data) {
        var minDate;
        if (data.minDate) {
            if (typeof (data.minDate) === 'string') {
                minDate = gj.core.parseDate(data.minDate, data.format, data.locale);
            } else if (typeof (data.minDate) === 'function') {
                minDate = data.minDate();
                if (typeof minDate === 'string') {
                    minDate = gj.core.parseDate(minDate, data.format, data.locale);
                }
            } else if (typeof data.minDate.getMonth === 'function') {
                minDate = data.minDate;
            }
        }
        return minDate;
    },

    getMaxDate: function (data) {
        var maxDate;
        if (data.maxDate) {
            if (typeof data.maxDate === 'string') {
                maxDate = gj.core.parseDate(data.maxDate, data.format, data.locale);
            } else if (typeof data.maxDate === 'function') {
                maxDate = data.maxDate();
                if (typeof maxDate === 'string') {
                    maxDate = gj.core.parseDate(maxDate, data.format, data.locale);
                }
            } else if (typeof data.maxDate.getMonth === 'function') {
                maxDate = data.maxDate;
            }
        }
        return maxDate;
    },

    isSelectable: function (data, date) {
        var result = true,
            minDate = gj.datepicker.methods.getMinDate(data),
            maxDate = gj.datepicker.methods.getMaxDate(data),
            i;

        if (minDate && date < minDate) {
            result = false;
        } else if (maxDate && date > maxDate) {
            result = false;
        }

        if (result) {
            if (data.disableDates) {
                if ($.isArray(data.disableDates)) {
                    for (i = 0; i < data.disableDates.length; i++) {
                        if (data.disableDates[i] instanceof Date && data.disableDates[i].getTime() === date.getTime()) {
                            result = false;
                        } else if (typeof data.disableDates[i] === 'string' && gj.core.parseDate(data.disableDates[i], data.format, data.locale).getTime() === date.getTime()) {
                            result = false;
                        }
                    }
                } else if (data.disableDates instanceof Function) {
                    result = data.disableDates(date);
                }
            }
            if ($.isArray(data.disableDaysOfWeek) && data.disableDaysOfWeek.indexOf(date.getDay()) > -1) {
                result = false;
            }
        }
        return result;
    },

    getPrevMonth: function (month, year) {
        date = new Date(year, month, 1);
        date.setMonth(date.getMonth() - 1);
        return { month: date.getMonth(), year: date.getFullYear() };
    },

    getNextMonth: function (month, year) {
        date = new Date(year, month, 1);
        date.setMonth(date.getMonth() + 1);
        return { month: date.getMonth(), year: date.getFullYear() };
    },

    prev: function ($datepicker, data) {
        return function () {
            var date, month, year, decade, century,
                $calendar = $('body').find('[role="calendar"][guid="' + $datepicker.attr('data-guid') + '"]');

            year = parseInt($calendar.attr('year'), 10);
            switch ($calendar.attr('type')) {
                case 'month':
                    month = parseInt($calendar.attr('month'), 10);
                    date = gj.datepicker.methods.getPrevMonth(month, year);
                    $calendar.attr('month', date.month);
                    $calendar.attr('year', date.year);
                    gj.datepicker.methods.renderMonth($datepicker, $calendar, data);
                    break;
                case 'year':
                    $calendar.attr('year', year - 1);
                    gj.datepicker.methods.renderYear($datepicker, $calendar, data);
                    break;
                case 'decade':
                    decade = year - (year % 10);
                    $calendar.attr('year', decade - 10);
                    gj.datepicker.methods.renderDecade($datepicker, $calendar, data);
                    break;
                case 'century':
                    century = year - (year % 100);
                    $calendar.attr('year', century - 100);
                    gj.datepicker.methods.renderCentury($datepicker, $calendar, data);
                    break;
            }
        }
    },

    next: function ($datepicker, data) {
        return function () {
            var date, month, year, decade, century,
                $calendar = $('body').find('[role="calendar"][guid="' + $datepicker.attr('data-guid') + '"]');

            year = parseInt($calendar.attr('year'), 10);
            switch ($calendar.attr('type')) {
                case 'month':
                    month = parseInt($calendar.attr('month'), 10);
                    date = gj.datepicker.methods.getNextMonth(month, year);
                    $calendar.attr('month', date.month);
                    $calendar.attr('year', date.year);
                    gj.datepicker.methods.renderMonth($datepicker, $calendar, data);
                    break;
                case 'year':
                    $calendar.attr('year', year + 1);
                    gj.datepicker.methods.renderYear($datepicker, $calendar, data);
                    break;
                case 'decade':
                    decade = year - (year % 10);
                    $calendar.attr('year', decade + 10);
                    gj.datepicker.methods.renderDecade($datepicker, $calendar, data);
                    break;
                case 'century':
                    century = year - (year % 100);
                    $calendar.attr('year', century + 100);
                    gj.datepicker.methods.renderCentury($datepicker, $calendar, data);
                    break;
            }
        }
    },

    changePeriod: function ($datepicker, data) {
        return function (e) {
            var $calendar = $('body').find('[role="calendar"][guid="' + $datepicker.attr('data-guid') + '"]');

            switch ($calendar.attr('type')) {
                case 'month':
                    gj.datepicker.methods.renderYear($datepicker, $calendar, data);
                    break;
                case 'year':
                    gj.datepicker.methods.renderDecade($datepicker, $calendar, data);
                    break;
                case 'decade':
                    gj.datepicker.methods.renderCentury($datepicker, $calendar, data);
                    break;
            }
        }
    },

    dayClickHandler: function ($datepicker, $calendar, data, date) {
        return function (e) {
            e && e.stopPropagation();
            gj.datepicker.methods.selectDay($datepicker, $calendar, data, date);
            if (data.footer !== true && data.autoClose !== false) {
                gj.datepicker.methods.change($datepicker, $calendar, data, date);
            }
            return $datepicker;
        };
    },

    change: function ($datepicker, $calendar, data, date) {
        var day = date.getDate(),
            month = date.getMonth(),
            year = date.getFullYear(),
            value = gj.core.formatDate(date, data.format, data.locale);
        $calendar.attr('month', month);
        $calendar.attr('year', year);
        $datepicker.val(value);
        gj.datepicker.events.change($datepicker);
        if (window.getComputedStyle($calendar[0]).display !== 'none') {
            gj.datepicker.methods.close($datepicker);
        }
    },

    selectDay: function ($datepicker, $calendar, data, date) {
        var day = date.getDate(),
            month = date.getMonth(),
            year = date.getFullYear();
        $calendar.attr('selectedDay', year + '-' + month + '-' + day);
        $calendar.find('tbody td').removeClass('selected');
        $calendar.find('tbody td[day="' + day + '"][month="' + month + '"]').addClass('selected');
        gj.datepicker.methods.updateHeader($calendar, data, date);
        gj.datepicker.events.select($datepicker, 'day');
    },

    selectMonth: function ($datepicker, $calendar, data, month) {
        return function (e) {
            $calendar.attr('month', month);
            gj.datepicker.methods.renderMonth($datepicker, $calendar, data);
            gj.datepicker.events.select($datepicker, 'month');
        };
    },

    selectYear: function ($datepicker, $calendar, data, year) {
        return function (e) {
            $calendar.attr('year', year);
            gj.datepicker.methods.renderYear($datepicker, $calendar, data);
            gj.datepicker.events.select($datepicker, 'year');
        };
    },

    selectDecade: function ($datepicker, $calendar, data, year) {
        return function (e) {
            $calendar.attr('year', year);
            gj.datepicker.methods.renderDecade($datepicker, $calendar, data);
            gj.datepicker.events.select($datepicker, 'decade');
        };
    },

    open: function ($datepicker, data) {
        var date, $calendar = $('body').find('[role="calendar"][guid="' + $datepicker.attr('data-guid') + '"]');

        if ($datepicker.val()) {
            $datepicker.value($datepicker.val());
        } else {
            date = new Date();
            $calendar.attr("month", date.getMonth());
            $calendar.attr("year", date.getFullYear());
        }

        switch ($calendar.attr('type')) {
            case 'month':
                gj.datepicker.methods.renderMonth($datepicker, $calendar, data);
                break;
            case 'year':
                gj.datepicker.methods.renderYear($datepicker, $calendar, data);
                break;
            case 'decade':
                gj.datepicker.methods.renderDecade($datepicker, $calendar, data);
                break;
            case 'century':
                gj.datepicker.methods.renderCentury($datepicker, $calendar, data);
                break;
        }

        $calendar.show();
        $calendar.closest('div[role="modal"]').show();
        if (data.modal) {
            gj.core.center($calendar);
        } else {
            gj.core.setChildPosition($datepicker[0], $calendar[0]);
            document.activeElement !== $datepicker[0] && $datepicker.focus();
        }
        clearTimeout($datepicker.timeout);
        gj.datepicker.events.open($datepicker);
    },

    close: function ($datepicker) {
        var $calendar = $('body').find('[role="calendar"][guid="' + $datepicker.attr('data-guid') + '"]');
        $calendar.hide();
        $calendar.closest('div[role="modal"]').hide();
        gj.datepicker.events.close($datepicker);
    },

    createKeyDownHandler: function ($datepicker, $calendar, data) {
        return function (e) {
            var month, year, day, index, $new, $active, e = e || window.event;

            if (window.getComputedStyle($calendar[0]).display !== 'none') {
                $active = gj.datepicker.methods.getActiveCell($calendar);

                if ($active.length === 0) { // ad:bugfix
                    return;
                }

                if (e.keyCode == '38') { // up
                    index = $active.index();
                    $new = $active.closest('tr').prev('tr').find('td:eq(' + index + ')');
                    if (!$new.is('[day]')) {
                        gj.datepicker.methods.prev($datepicker, data)();
                        $new = $calendar.find('tbody tr').last().find('td:eq(' + index + ')');
                        if ($new.is(':empty')) {
                            $new = $calendar.find('tbody tr').last().prev().find('td:eq(' + index + ')');
                        }
                    }
                    if ($new.is('[day]')) {
                        $new.addClass('focused');
                        $active.removeClass('focused');
                    }
                } else if (e.keyCode == '40') { // down
                    index = $active.index();
                    $new = $active.closest('tr').next('tr').find('td:eq(' + index + ')');
                    if (!$new.is('[day]')) {
                        gj.datepicker.methods.next($datepicker, data)();
                        $new = $calendar.find('tbody tr').first().find('td:eq(' + index + ')');
                        if (!$new.is('[day]')) {
                            $new = $calendar.find('tbody tr:eq(1)').find('td:eq(' + index + ')');
                        }
                    }
                    if ($new.is('[day]')) {
                        $new.addClass('focused');
                        $active.removeClass('focused');
                    }
                } else if (e.keyCode == '37') { // left
                    $new = $active.prev('td[day]:not(.disabled)');
                    if ($new.length === 0) {
                        $new = $active.closest('tr').prev('tr').find('td[day]').last();
                    }
                    if ($new.length === 0) {
                        gj.datepicker.methods.prev($datepicker, data)();
                        $new = $calendar.find('tbody tr').last().find('td[day]').last();
                    }
                    if ($new.length > 0) {
                        $new.addClass('focused');
                        $active.removeClass('focused');
                    }
                } else if (e.keyCode == '39') { // right
                    $new = $active.next('[day]:not(.disabled)');
                    if ($new.length === 0) {
                        $new = $active.closest('tr').next('tr').find('td[day]').first();
                    }
                    if ($new.length === 0) {
                        gj.datepicker.methods.next($datepicker, data)();
                        $new = $calendar.find('tbody tr').first().find('td[day]').first();
                    }
                    if ($new.length > 0) {
                        $new.addClass('focused');
                        $active.removeClass('focused');
                    }
                } else if (e.keyCode == '13') { // enter
                    day = parseInt($active.attr('day'), 10);
                    month = parseInt($active.attr('month'), 10);
                    year = parseInt($active.attr('year'), 10);
                    gj.datepicker.methods.dayClickHandler($datepicker, $calendar, data, new Date(year, month, day))();
                } else if (e.keyCode == '27') { // esc
                    $datepicker.close();
                }
            }
        }
    },

    getActiveCell: function ($calendar) {
        var $cell = $calendar.find('td[day].focused');
        if ($cell.length === 0) {
            $cell = $calendar.find('td[day].selected');
            if ($cell.length === 0) {
                $cell = $calendar.find('td[day].today');
                if ($cell.length === 0) {
                    $cell = $calendar.find('td[day]:not(.disabled)').first();
                }
            }
        }
        return $cell;
    },

    value: function ($datepicker, value) {
        var $calendar, date, data = $datepicker.data();
        if (typeof (value) === "undefined") {
            return $datepicker.val();
        } else {
            date = gj.core.parseDate(value, data.format, data.locale);
            if (date && date.getTime()) {
                $calendar = $('body').find('[role="calendar"][guid="' + $datepicker.attr('data-guid') + '"]');
                gj.datepicker.methods.dayClickHandler($datepicker, $calendar, data, date)();
            } else {
                $datepicker.val('');
            }
            return $datepicker;
        }
    },

    destroy: function ($datepicker) {
        var data = $datepicker.data(),
            $parent = $datepicker.parent(),
            $picker = $('body').find('[role="calendar"][guid="' + $datepicker.attr('data-guid') + '"]');
        if (data) {
            $datepicker.off();
            if ($picker.parent('[role="modal"]').length > 0) {
                $picker.unwrap();
            }
            $picker.remove();
            $datepicker.removeData();
            $datepicker.removeAttr('data-type').removeAttr('data-guid').removeAttr('data-datepicker');
            $datepicker.removeClass();
            $parent.children('[role="right-icon"]').remove();
            $datepicker.unwrap();
        }
        return $datepicker;
    }
};

gj.datepicker.events = {
    change: function ($datepicker) {
        return $datepicker.triggerHandler('change');
    },
    select: function ($datepicker, type) {
        return $datepicker.triggerHandler('select', [type]);
    },
    open: function ($datepicker) {
        return $datepicker.triggerHandler('open');
    },
    close: function ($datepicker) {
        return $datepicker.triggerHandler('close', [$datepicker, $datepicker.val()]);
    }
};

gj.datepicker.widget = function ($element, jsConfig) {
    var self = this,
        methods = gj.datepicker.methods;
    self.value = function (value) {
        return methods.value(this, value);
    };
    self.destroy = function () {
        return methods.destroy(this);
    };
    self.open = function () {
        return methods.open(this, this.data());
    };
    self.close = function () {
        return methods.close(this);
    };

    $.extend($element, self);
    if ('true' !== $element.attr('data-datepicker')) {
        methods.init.call($element, jsConfig);
    }

    return $element;
};

gj.datepicker.widget.prototype = new gj.widget();
gj.datepicker.widget.constructor = gj.datepicker.widget;

(function ($) {
    $.fn.datepicker = function (method) {
        var $widget;
        if (this && this.length) {
            if (typeof method === 'object' || !method) {
                return new gj.datepicker.widget(this, method);
            } else {
                $widget = new gj.datepicker.widget(this, null);
                if ($widget[method]) {
                    return $widget[method].apply(this, Array.prototype.slice.call(arguments, 1));
                } else {
                    throw 'Method ' + method + ' does not exist.';
                }
            }
        }
    };
})(jQuery);
/**
  * @widget TimePicker
  * @plugin Base
  */
gj.timepicker = {
    plugins: {}
};

gj.timepicker.config = {
    base: {
        width: undefined,
        modal: true,
        header: true,
        footer: true,
        format: 'HH:MM',
        uiLibrary: 'materialdesign',
        value: undefined,
        mode: 'ampm',
        locale: 'en-us',
        size: 'default',

        icons: {
            rightIcon: '<i class="gj-icon clock" />'
        },

        style: {
            modal: 'gj-modal',
            wrapper: 'gj-timepicker gj-timepicker-md gj-unselectable',
            input: 'gj-textbox-md',
            clock: 'gj-picker gj-picker-md timepicker',
            footer: '',
            button: 'gj-button-md'
        }
    },

    bootstrap: {
        style: {
            wrapper: 'gj-timepicker gj-timepicker-bootstrap gj-unselectable input-group',
            input: 'form-control',
            clock: 'gj-picker gj-picker-bootstrap timepicker',
            footer: 'modal-footer',
            button: 'btn btn-default'
        },
        iconsLibrary: 'glyphicons'
    },

    bootstrap4: {
        style: {
            wrapper: 'gj-timepicker gj-timepicker-bootstrap gj-unselectable input-group',
            input: 'form-control border',
            clock: 'gj-picker gj-picker-bootstrap timepicker',
            footer: 'modal-footer',
            button: 'btn btn-default'
        }
    }
};

gj.timepicker.methods = {
    init: function (jsConfig) {
        gj.picker.widget.prototype.init.call(this, jsConfig, 'timepicker');
        return this;
    },

    initialize: function () {

    },

    initMouse: function ($body, $input, $picker, data) {
        $body.off();
        $body.on('mousedown', gj.timepicker.methods.mouseDownHandler($input, $picker));
        $body.on('mousemove', gj.timepicker.methods.mouseMoveHandler($input, $picker, data));
        $body.on('mouseup', gj.timepicker.methods.mouseUpHandler($input, $picker, data));
    },

    createPicker: function ($timepicker) {
        var date, data = $timepicker.data(),
            $clock = $('<div role="picker" />').addClass(data.style.clock).attr('guid', $timepicker.attr('data-guid')),
            $hour = $('<div role="hour" />'),
            $minute = $('<div role="minute" />'),
            $header = $('<div role="header" />'),
            $mode = $('<div role="mode" />'),
            $body = $('<div role="body" />'),
            $btnOk = $('<button class="' + data.style.button + '">' + gj.core.messages[data.locale].ok + '</button>'),
            $btnCancel = $('<button class="' + data.style.button + '">' + gj.core.messages[data.locale].cancel + '</button>'),
            $footer = $('<div role="footer" class="' + data.style.footer + '" />');

        date = gj.core.parseDate(data.value, data.format, data.locale);
        if (!date || isNaN(date.getTime())) {
            date = new Date();
        } else {
            $timepicker.attr('hours', date.getHours());
        }

        gj.timepicker.methods.initMouse($body, $timepicker, $clock, data);

        if (data.header) {
            $hour.on('click', function () {
                gj.timepicker.methods.renderHours($timepicker, $clock, data);
            });
            $minute.on('click', function () {
                gj.timepicker.methods.renderMinutes($timepicker, $clock, data);
            });
            $header.append($hour).append(':').append($minute);
            if (data.mode === 'ampm') {
                $mode.append($('<span role="am">' + gj.core.messages[data.locale].am + '</span>').on('click', function () {
                    var hour = gj.timepicker.methods.getHour($clock);
                    $clock.attr('mode', 'am');
                    $(this).addClass('selected');
                    $(this).parent().children('[role="pm"]').removeClass('selected');
                    if (hour >= 12) {
                        $clock.attr('hour', hour - 12);
                    }
                    if (!data.modal) {
                        clearTimeout($timepicker.timeout);
                        $timepicker.focus();
                    }
                }));
                $mode.append('<br />');
                $mode.append($('<span role="pm">' + gj.core.messages[data.locale].pm + '</span>').on('click', function () {
                    var hour = gj.timepicker.methods.getHour($clock);
                    $clock.attr('mode', 'pm');
                    $(this).addClass('selected');
                    $(this).parent().children('[role="am"]').removeClass('selected');
                    if (hour < 12) {
                        $clock.attr('hour', hour + 12);
                    }
                    if (!data.modal) {
                        clearTimeout($timepicker.timeout);
                        $timepicker.focus();
                    }
                }));
                $header.append($mode);
            }
            $clock.append($header);
        }

        $clock.append($body);

        if (data.footer) {
            $btnCancel.on('click', function () { $timepicker.close(); });
            $footer.append($btnCancel);
            $btnOk.on('click', gj.timepicker.methods.setTime($timepicker, $clock));
            $footer.append($btnOk);
            $clock.append($footer);
        }

        $clock.hide();

        $('body').append($clock);

        if (data.modal) {
            $clock.wrapAll('<div role="modal" class="' + data.style.modal + '"/>');
            gj.core.center($clock);
        }
        return $clock;
    },

    getHour: function ($clock) {
        return parseInt($clock.attr('hour'), 10) || 0;
    },

    getMinute: function ($clock) {
        return parseInt($clock.attr('minute'), 10) || 0;
    },

    setTime: function ($timepicker, $clock) {
        return function () {
            var hour = gj.timepicker.methods.getHour($clock),
                minute = gj.timepicker.methods.getMinute($clock),
                mode = $clock.attr('mode'),
                date = new Date(0, 0, 0, (hour === 12 && mode === 'am' ? 0 : hour), minute),
                data = $timepicker.data(),
                value = gj.core.formatDate(date, data.format, data.locale);
            $timepicker.value(value);
            $timepicker.close();
        }
    },

    getPointerValue: function (x, y, mode) {
        var value, radius, size = 256,
            angle = Math.atan2(size / 2 - x, size / 2 - y) / Math.PI * 180;

        if (angle < 0) {
            angle = 360 + angle;
        }

        switch (mode) {
            case 'ampm': {
                value = 12 - Math.round(angle * 12 / 360);
                return value === 0 ? 12 : value;
            }
            case '24hr': {
                radius = Math.sqrt(Math.pow(size / 2 - x, 2) + Math.pow(size / 2 - y, 2));
                value = 12 - Math.round(angle * 12 / 360);
                if (value === 0) {
                    value = 12;
                }
                if (radius < size / 2 - 32) {
                    value = value === 12 ? 0 : value + 12;
                }
                return value;
            }
            case 'minutes': {
                value = Math.round(60 - 60 * angle / 360);
                return value === 60 ? 0 : value;
            }
        }
    },

    updateArrow: function (e, $timepicker, $clock, data) {
        var rect, value,
            mouseX = $timepicker.mouseX(e),
            mouseY = $timepicker.mouseY(e),
            scrollY = window.scrollY || window.pageYOffset || 0,
            scrollX = window.scrollX || window.pageXOffset || 0;

        rect = e.target.getBoundingClientRect();
        if (data.dialMode == 'hours') {
            value = gj.timepicker.methods.getPointerValue(mouseX - scrollX - rect.left, mouseY - scrollY - rect.top, data.mode);
            $clock.attr('hour', data.mode === 'ampm' && $clock.attr('mode') === 'pm' && value < 12 ? value + 12 : value);
        } else if (data.dialMode == 'minutes') {
            value = gj.timepicker.methods.getPointerValue(mouseX - scrollX - rect.left, mouseY - scrollY - rect.top, 'minutes');
            $clock.attr('minute', value);
        }

        gj.timepicker.methods.update($timepicker, $clock, data);
    },

    update: function ($timepicker, $clock, data) {
        var hour, minute, $arrow, visualHour, $header, $numbers;
        hour = gj.timepicker.methods.getHour($clock);
        minute = gj.timepicker.methods.getMinute($clock);
        $arrow = $clock.find('[role="arrow"]');
        if (data.dialMode == 'hours' && (hour == 0 || hour > 12) && data.mode === '24hr') {
            $arrow.css('width', 'calc(50% - 52px)');
        } else {
            $arrow.css('width', 'calc(50% - 20px)');
        }

        if (data.dialMode == 'hours') {
            $arrow.css('transform', 'rotate(' + ((hour * 30) - 90).toString() + 'deg)');
        } else {
            $arrow.css('transform', 'rotate(' + ((minute * 6) - 90).toString() + 'deg)');
        }
        $arrow.show();
        visualHour = (data.mode === 'ampm' && hour > 12 ? hour - 12 : (hour == 0 ? 12 : hour));
        $numbers = $clock.find('[role="body"] span');
        $numbers.removeClass('selected');
        $numbers.filter(function (e) {
            if (data.dialMode == 'hours') {
                return parseInt($(this).text(), 10) == visualHour;
            } else {
                return parseInt($(this).text(), 10) == minute;
            }
        }).addClass('selected');
        if (data.header) {
            $header = $clock.find('[role="header"]');
            $header.find('[role="hour"]').text(visualHour);
            $header.find('[role="minute"]').text(gj.core.pad(minute));
            if (data.mode === 'ampm') {
                if (hour >= 12) {
                    $header.find('[role="pm"]').addClass('selected');
                    $header.find('[role="am"]').removeClass('selected');
                } else {
                    $header.find('[role="am"]').addClass('selected');
                    $header.find('[role="pm"]').removeClass('selected');
                }
            }
        }
    },

    mouseDownHandler: function ($timepicker, $clock) {
        return function (e) {
            $timepicker.mouseMove = true;
        }
    },

    mouseMoveHandler: function ($timepicker, $clock, data) {
        return function (e) {
            if ($timepicker.mouseMove) {
                gj.timepicker.methods.updateArrow(e, $timepicker, $clock, data);
            }
        }
    },

    mouseUpHandler: function ($timepicker, $clock, data) {
        return function (e) {
            gj.timepicker.methods.updateArrow(e, $timepicker, $clock, data);
            $timepicker.mouseMove = false;
            if (!data.modal) {
                clearTimeout($timepicker.timeout);
                $timepicker.focus();
            }
            if (data.dialMode == 'hours') {
                setTimeout(function () {
                    gj.timepicker.events.select($timepicker, 'hour');
                    gj.timepicker.methods.renderMinutes($timepicker, $clock, data);
                }, 1000);
            } else if (data.dialMode == 'minutes') {
                if (data.footer !== true && data.autoClose !== false) {
                    gj.timepicker.methods.setTime($timepicker, $clock)();
                }
                gj.timepicker.events.select($timepicker, 'minute');
            }
        }
    },

    renderHours: function ($timepicker, $clock, data) {
        var $dial, $body = $clock.find('[role="body"]');

        clearTimeout($timepicker.timeout);
        $body.empty();
        $dial = $('<div role="dial"></div>');

        $dial.append('<div role="arrow" style="transform: rotate(-90deg); display: none;"><div class="arrow-begin"></div><div class="arrow-end"></div></div>');

        $dial.append('<span role="hour" style="transform: translate(54px, -93.5307px);">1</span>');
        $dial.append('<span role="hour" style="transform: translate(93.5307px, -54px);">2</span>');
        $dial.append('<span role="hour" style="transform: translate(108px, 0px);">3</span>');
        $dial.append('<span role="hour" style="transform: translate(93.5307px, 54px);">4</span>');
        $dial.append('<span role="hour" style="transform: translate(54px, 93.5307px);">5</span>');
        $dial.append('<span role="hour" style="transform: translate(6.61309e-15px, 108px);">6</span>');
        $dial.append('<span role="hour" style="transform: translate(-54px, 93.5307px);">7</span>');
        $dial.append('<span role="hour" style="transform: translate(-93.5307px, 54px);">8</span>');
        $dial.append('<span role="hour" style="transform: translate(-108px, 1.32262e-14px);">9</span>');
        $dial.append('<span role="hour" style="transform: translate(-93.5307px, -54px);">10</span>');
        $dial.append('<span role="hour" style="transform: translate(-54px, -93.5307px);">11</span>');
        $dial.append('<span role="hour" style="transform: translate(-1.98393e-14px, -108px);">12</span>');
        if (data.mode === '24hr') {
            $dial.append('<span role="hour" style="transform: translate(38px, -65.8179px);">13</span>');
            $dial.append('<span role="hour" style="transform: translate(65.8179px, -38px);">14</span>');
            $dial.append('<span role="hour" style="transform: translate(76px, 0px);">15</span>');
            $dial.append('<span role="hour" style="transform: translate(65.8179px, 38px);">16</span>');
            $dial.append('<span role="hour" style="transform: translate(38px, 65.8179px);">17</span>');
            $dial.append('<span role="hour" style="transform: translate(4.65366e-15px, 76px);">18</span>');
            $dial.append('<span role="hour" style="transform: translate(-38px, 65.8179px);">19</span>');
            $dial.append('<span role="hour" style="transform: translate(-65.8179px, 38px);">20</span>');
            $dial.append('<span role="hour" style="transform: translate(-76px, 9.30732e-15px);">21</span>');
            $dial.append('<span role="hour" style="transform: translate(-65.8179px, -38px);">22</span>');
            $dial.append('<span role="hour" style="transform: translate(-38px, -65.8179px);">23</span>');
            $dial.append('<span role="hour" style="transform: translate(-1.3961e-14px, -76px);">00</span>');
        }
        $body.append($dial);

        $clock.find('[role="header"] [role="hour"]').addClass('selected');
        $clock.find('[role="header"] [role="minute"]').removeClass('selected');

        data.dialMode = 'hours';

        gj.timepicker.methods.update($timepicker, $clock, data);
    },

    renderMinutes: function ($timepicker, $clock, data) {
        var $body = $clock.find('[role="body"]');

        clearTimeout($timepicker.timeout);
        $body.empty();
        $dial = $('<div role="dial"></div>');

        $dial.append('<div role="arrow" style="transform: rotate(-90deg); display: none;"><div class="arrow-begin"></div><div class="arrow-end"></div></div>');

        $dial.append('<span role="hour" style="transform: translate(54px, -93.5307px);">5</span>');
        $dial.append('<span role="hour" style="transform: translate(93.5307px, -54px);">10</span>');
        $dial.append('<span role="hour" style="transform: translate(108px, 0px);">15</span>');
        $dial.append('<span role="hour" style="transform: translate(93.5307px, 54px);">20</span>');
        $dial.append('<span role="hour" style="transform: translate(54px, 93.5307px);">25</span>');
        $dial.append('<span role="hour" style="transform: translate(6.61309e-15px, 108px);">30</span>');
        $dial.append('<span role="hour" style="transform: translate(-54px, 93.5307px);">35</span>');
        $dial.append('<span role="hour" style="transform: translate(-93.5307px, 54px);">40</span>');
        $dial.append('<span role="hour" style="transform: translate(-108px, 1.32262e-14px);">45</span>');
        $dial.append('<span role="hour" style="transform: translate(-93.5307px, -54px);">50</span>');
        $dial.append('<span role="hour" style="transform: translate(-54px, -93.5307px);">55</span>');
        $dial.append('<span role="hour" style="transform: translate(-1.98393e-14px, -108px);">00</span>');
        $body.append($dial);

        $clock.find('[role="header"] [role="hour"]').removeClass('selected');
        $clock.find('[role="header"] [role="minute"]').addClass('selected');

        data.dialMode = 'minutes';

        gj.timepicker.methods.update($timepicker, $clock, data);
    },

    open: function ($timepicker) {
        var time, hour, data = $timepicker.data(),
            $clock = $('body').find('[role="picker"][guid="' + $timepicker.attr('data-guid') + '"]');

        if ($timepicker.value()) {
            time = gj.core.parseDate($timepicker.value(), data.format, data.locale);
        } else {
            time = new Date();
        }
        hour = time.getHours();
        if (data.mode === 'ampm') {
            $clock.attr('mode', hour > 12 ? 'pm' : 'am');
        }
        $clock.attr('hour', hour);
        $clock.attr('minute', time.getMinutes());

        gj.timepicker.methods.renderHours($timepicker, $clock, data);

        gj.picker.widget.prototype.open.call($timepicker, 'timepicker');
        return $timepicker;
    },

    value: function ($timepicker, value) {
        var $clock, time, data = $timepicker.data();
        if (typeof (value) === "undefined") {
            return $timepicker.val();
        } else {
            $timepicker.val(value);
            gj.timepicker.events.change($timepicker);
            return $timepicker;
        }
    }
};

gj.timepicker.events = {
    change: function ($timepicker) {
        return $timepicker.triggerHandler('change');
    },
    select: function ($timepicker, type) {
        return $timepicker.triggerHandler('select', [type]);
    },
    open: function ($timepicker) {
        return $timepicker.triggerHandler('open');
    },
    close: function ($timepicker) {
        return $timepicker.triggerHandler('close');
    }
};

gj.timepicker.widget = function ($element, jsConfig) {
    var self = this,
        methods = gj.timepicker.methods;

    self.mouseMove = false;
    self.value = function (value) {
        return methods.value(this, value);
    };
    self.destroy = function () {
        return gj.picker.widget.prototype.destroy.call(this, 'timepicker');
    };
    self.open = function () {
        return gj.timepicker.methods.open(this);
    };
    self.close = function () {
        return gj.picker.widget.prototype.close.call(this, 'timepicker');
    };

    $.extend($element, self);
    if ('true' !== $element.attr('data-timepicker')) {
        methods.init.call($element, jsConfig);
    }

    return $element;
};

gj.timepicker.widget.prototype = new gj.picker.widget();
gj.timepicker.widget.constructor = gj.timepicker.widget;

(function ($) {
    $.fn.timepicker = function (method) {
        var $widget;
        if (this && this.length) {
            if (typeof method === 'object' || !method) {
                return new gj.timepicker.widget(this, method);
            } else {
                $widget = new gj.timepicker.widget(this, null);
                if ($widget[method]) {
                    return $widget[method].apply(this, Array.prototype.slice.call(arguments, 1));
                } else {
                    throw 'Method ' + method + ' does not exist.';
                }
            }
        }
    };
})(jQuery);
/**
  * @widget DateTimePicker
  * @plugin Base
  */
gj.datetimepicker = {
    plugins: {},
    messages: {
        'en-us': {
        }
    }
};

gj.datetimepicker.config = {
    base: {
        datepicker: gj.datepicker.config.base,

        timepicker: gj.timepicker.config.base,
        uiLibrary: 'materialdesign',
        value: undefined,
        format: 'HH:MM mm/dd/yyyy',
        width: undefined,
        modal: false,
        footer: false,
        size: 'default',
        locale: 'en-us',

        icons: {},

        style: {
            calendar: 'gj-picker gj-picker-md datetimepicker gj-unselectable'
        }
    },

    bootstrap: {
        style: {
            calendar: 'gj-picker gj-picker-bootstrap datetimepicker gj-unselectable'
        },
        iconsLibrary: 'glyphicons'
    },

    bootstrap4: {
        style: {
            calendar: 'gj-picker gj-picker-bootstrap datetimepicker gj-unselectable'
        }
    }
};

gj.datetimepicker.methods = {
    init: function (jsConfig) {
        gj.widget.prototype.init.call(this, jsConfig, 'datetimepicker');
        this.attr('data-datetimepicker', 'true');
        gj.datetimepicker.methods.initialize(this);
        return this;
    },

    getConfig: function (clientConfig, type) {
        var config = gj.widget.prototype.getConfig.call(this, clientConfig, type);

        uiLibrary = clientConfig.hasOwnProperty('uiLibrary') ? clientConfig.uiLibrary : config.uiLibrary;
        if (gj.datepicker.config[uiLibrary]) {
            $.extend(true, config.datepicker, gj.datepicker.config[uiLibrary]);
        }
        if (gj.timepicker.config[uiLibrary]) {
            $.extend(true, config.timepicker, gj.timepicker.config[uiLibrary]);
        }

        iconsLibrary = clientConfig.hasOwnProperty('iconsLibrary') ? clientConfig.iconsLibrary : config.iconsLibrary;
        if (gj.datepicker.config[iconsLibrary]) {
            $.extend(true, config.datepicker, gj.datepicker.config[iconsLibrary]);
        }
        if (gj.timepicker.config[iconsLibrary]) {
            $.extend(true, config.timepicker, gj.timepicker.config[iconsLibrary]);
        }

        return config;
    },

    initialize: function ($datetimepicker) {
        var $picker, $header, $date, $time, date,
            $switch, $calendarMode, $clockMode,
            data = $datetimepicker.data();
        data.datepicker.uiLibrary = data.uiLibrary;
        data.datepicker.iconsLibrary = data.iconsLibrary;
        data.datepicker.width = data.width;
        data.datepicker.format = data.format;
        data.datepicker.locale = data.locale;
        data.datepicker.modal = data.modal;
        data.datepicker.footer = data.footer;
        data.datepicker.style.calendar = data.style.calendar;
        data.datepicker.value = data.value;
        data.datepicker.size = data.size;
        data.datepicker.autoClose = false;
        gj.datepicker.methods.initialize($datetimepicker, data.datepicker);
        $datetimepicker.on('select', function (e, type) {
            var date, value;
            if (type === 'day') {
                gj.datetimepicker.methods.createShowHourHandler($datetimepicker, $picker, data)();
            } else if (type === 'minute') {
                if ($picker.attr('selectedDay') && data.footer !== true) {
                    selectedDay = $picker.attr('selectedDay').split('-');
                    date = new Date(selectedDay[0], selectedDay[1], selectedDay[2], $picker.attr('hour') || 0, $picker.attr('minute') || 0);
                    value = gj.core.formatDate(date, data.format, data.locale);
                    $datetimepicker.val(value);
                    gj.datetimepicker.events.change($datetimepicker);
                    gj.datetimepicker.methods.close($datetimepicker);
                }
            }
        });
        $datetimepicker.on('open', function () {
            var $header = $picker.children('[role="header"]');
            $header.find('[role="calendarMode"]').addClass("selected");
            $header.find('[role="clockMode"]').removeClass("selected");
        });

        $picker = $('body').find('[role="calendar"][guid="' + $datetimepicker.attr('data-guid') + '"]');
        date = data.value ? gj.core.parseDate(data.value, data.format, data.locale) : new Date();
        $picker.attr('hour', date.getHours());
        $picker.attr('minute', date.getMinutes());
        data.timepicker.uiLibrary = data.uiLibrary;
        data.timepicker.iconsLibrary = data.iconsLibrary;
        data.timepicker.format = data.format;
        data.timepicker.locale = data.locale;
        data.timepicker.header = true;
        data.timepicker.footer = data.footer;
        data.timepicker.size = data.size;
        data.timepicker.mode = '24hr';
        data.timepicker.autoClose = false;
        $header = $('<div role="header" />');
        $date = $('<div role="date" class="selected" />');
        $date.on('click', gj.datetimepicker.methods.createShowDateHandler($datetimepicker, $picker, data));
        $date.html(gj.core.formatDate(new Date(), 'ddd, mmm dd', data.locale));
        $header.append($date);

        $switch = $('<div role="switch"></div>');

        $calendarMode = $('<i class="gj-icon selected" role="calendarMode">event</i>');
        $calendarMode.on('click', gj.datetimepicker.methods.createShowDateHandler($datetimepicker, $picker, data));
        $switch.append($calendarMode);

        $time = $('<div role="time" />');
        $time.append($('<div role="hour" />').on('click', gj.datetimepicker.methods.createShowHourHandler($datetimepicker, $picker, data)).html(gj.core.formatDate(new Date(), 'HH', data.locale)));
        $time.append(':');
        $time.append($('<div role="minute" />').on('click', gj.datetimepicker.methods.createShowMinuteHandler($datetimepicker, $picker, data)).html(gj.core.formatDate(new Date(), 'MM', data.locale)));
        $switch.append($time);

        $clockMode = $('<i class="gj-icon" role="clockMode">clock</i>');
        $clockMode.on('click', gj.datetimepicker.methods.createShowHourHandler($datetimepicker, $picker, data));
        $switch.append($clockMode);
        $header.append($switch);

        $picker.prepend($header);
    },

    createShowDateHandler: function ($datetimepicker, $picker, data) {
        return function (e) {
            var $header = $picker.children('[role="header"]');
            $header.find('[role="calendarMode"]').addClass("selected");
            $header.find('[role="date"]').addClass("selected");
            $header.find('[role="clockMode"]').removeClass("selected");
            $header.find('[role="hour"]').removeClass("selected");
            $header.find('[role="minute"]').removeClass("selected");
            gj.datepicker.methods.renderMonth($datetimepicker, $picker, data.datepicker);
        };
    },

    createShowHourHandler: function ($datetimepicker, $picker, data) {
        return function () {
            var $header = $picker.children('[role="header"]');
            $header.find('[role="calendarMode"]').removeClass("selected");
            $header.find('[role="date"]').removeClass("selected");
            $header.find('[role="clockMode"]').addClass("selected");
            $header.find('[role="hour"]').addClass("selected");
            $header.find('[role="minute"]').removeClass("selected");

            gj.timepicker.methods.initMouse($picker.children('[role="body"]'), $datetimepicker, $picker, data.timepicker);
            gj.timepicker.methods.renderHours($datetimepicker, $picker, data.timepicker);
        };
    },

    createShowMinuteHandler: function ($datetimepicker, $picker, data) {
        return function () {
            var $header = $picker.children('[role="header"]');
            $header.find('[role="calendarMode"]').removeClass("selected");
            $header.find('[role="date"]').removeClass("selected");
            $header.find('[role="clockMode"]').addClass("selected");
            $header.find('[role="hour"]').removeClass("selected");
            $header.find('[role="minute"]').addClass("selected");
            gj.timepicker.methods.initMouse($picker.children('[role="body"]'), $datetimepicker, $picker, data.timepicker);
            gj.timepicker.methods.renderMinutes($datetimepicker, $picker, data.timepicker);
        };
    },

    close: function ($datetimepicker) {
        var $calendar = $('body').find('[role="calendar"][guid="' + $datetimepicker.attr('data-guid') + '"]');
        $calendar.hide();
        $calendar.closest('div[role="modal"]').hide();
    },

    value: function ($datetimepicker, value) {
        var $calendar, date, hour, data = $datetimepicker.data();
        if (typeof (value) === "undefined") {
            return $datetimepicker.val();
        } else {
            date = gj.core.parseDate(value, data.format, data.locale);
            if (date) {
                $calendar = $('body').find('[role="calendar"][guid="' + $datetimepicker.attr('data-guid') + '"]');
                gj.datepicker.methods.dayClickHandler($datetimepicker, $calendar, data, date)();
                hour = date.getHours();
                if (data.mode === 'ampm') {
                    $calendar.attr('mode', hour > 12 ? 'pm' : 'am');
                }
                $calendar.attr('hour', hour);
                $calendar.attr('minute', date.getMinutes());
                $datetimepicker.val(value);
            } else {
                $datetimepicker.val('');
            }
            return $datetimepicker;
        }
    },

    destroy: function ($datetimepicker) {
        var data = $datetimepicker.data(),
            $parent = $datetimepicker.parent(),
            $picker = $('body').find('[role="calendar"][guid="' + $datetimepicker.attr('data-guid') + '"]');
        if (data) {
            $datetimepicker.off();
            if ($picker.parent('[role="modal"]').length > 0) {
                $picker.unwrap();
            }
            $picker.remove();
            $datetimepicker.removeData();
            $datetimepicker.removeAttr('data-type').removeAttr('data-guid').removeAttr('data-datetimepicker');
            $datetimepicker.removeClass();
            $parent.children('[role="right-icon"]').remove();
            $datetimepicker.unwrap();
        }
        return $datetimepicker;
    }
};

gj.datetimepicker.events = {
    change: function ($datetimepicker) {
        return $datetimepicker.triggerHandler('change');
    }
};

gj.datetimepicker.widget = function ($element, jsConfig) {
    var self = this,
        methods = gj.datetimepicker.methods;

    self.mouseMove = false;
    self.value = function (value) {
        return methods.value(this, value);
    };
    self.open = function () {
        gj.datepicker.methods.open(this, this.data().datepicker);
    };
    self.close = function () {
        gj.datepicker.methods.close(this);
    };
    self.destroy = function () {
        return methods.destroy(this);
    };

    $.extend($element, self);
    if ('true' !== $element.attr('data-datetimepicker')) {
        methods.init.call($element, jsConfig);
    }

    return $element;
};

gj.datetimepicker.widget.prototype = new gj.widget();
gj.datetimepicker.widget.constructor = gj.datetimepicker.widget;

gj.datetimepicker.widget.prototype.getConfig = gj.datetimepicker.methods.getConfig;

(function ($) {
    $.fn.datetimepicker = function (method) {
        var $widget;
        if (this && this.length) {
            if (typeof method === 'object' || !method) {
                return new gj.datetimepicker.widget(this, method);
            } else {
                $widget = new gj.datetimepicker.widget(this, null);
                if ($widget[method]) {
                    return $widget[method].apply(this, Array.prototype.slice.call(arguments, 1));
                } else {
                    throw 'Method ' + method + ' does not exist.';
                }
            }
        }
    };
})(jQuery);
/**
  * @widget Slider
  * @plugin Base
  */
gj.slider = {
    plugins: {},
    messages: {
        'en-us': {
        }
    }
};

gj.slider.config = {
    base: {
        min: 0,
        max: 100,
        width: undefined,
        uiLibrary: 'materialdesign',
        value: undefined,

        icons: {},

        style: {
            wrapper: 'gj-slider gj-slider-md',
            progress: undefined,
            track: undefined
        }
    },

    bootstrap: {
        style: {
            wrapper: 'gj-slider gj-slider-bootstrap gj-slider-bootstrap-3',
            progress: 'progress-bar',
            track: 'progress'
        }
    },

    bootstrap4: {
        style: {
            wrapper: 'gj-slider gj-slider-bootstrap gj-slider-bootstrap-4',
            progress: 'progress-bar',
            track: 'progress'
        }
    }
};

gj.slider.methods = {
    init: function (jsConfig) {
        gj.widget.prototype.init.call(this, jsConfig, 'slider');
        this.attr('data-slider', 'true');
        gj.slider.methods.initialize(this, this.data());
        return this;
    },

    initialize: function ($slider, data) {
        var wrapper, track, handle, progress;

        $slider[0].style.display = 'none';

        if ($slider[0].parentElement.attributes.role !== 'wrapper') {
            wrapper = document.createElement('div');
            wrapper.setAttribute('role', 'wrapper');
            $slider[0].parentNode.insertBefore(wrapper, $slider[0]);
            wrapper.appendChild($slider[0]);
        } else {
            wrapper = $slider[0].parentElement;
        }

        if (data.width) {
            wrapper.style.width = data.width + 'px';
        }

        gj.core.addClasses(wrapper, data.style.wrapper);

        track = $slider[0].querySelector('[role="track"]');
        if (track == null) {
            track = document.createElement('div');
            track.setAttribute('role', 'track');
            wrapper.appendChild(track);
        }
        gj.core.addClasses(track, data.style.track);

        handle = $slider[0].querySelector('[role="handle"]');
        if (handle == null) {
            handle = document.createElement('div');
            handle.setAttribute('role', 'handle');
            wrapper.appendChild(handle);
        }

        progress = $slider[0].querySelector('[role="progress"]');
        if (progress == null) {
            progress = document.createElement('div');
            progress.setAttribute('role', 'progress');
            wrapper.appendChild(progress);
        }
        gj.core.addClasses(progress, data.style.progress);

        if (!data.value) {
            data.value = data.min;
        }
        gj.slider.methods.value($slider, data, data.value);

        gj.documentManager.subscribeForEvent('mouseup', $slider.data('guid'), gj.slider.methods.createMouseUpHandler($slider, handle, data));
        handle.addEventListener('mousedown', gj.slider.methods.createMouseDownHandler(handle, data));
        gj.documentManager.subscribeForEvent('mousemove', $slider.data('guid'), gj.slider.methods.createMouseMoveHandler($slider, track, handle, progress, data));

        handle.addEventListener('click', function (e) { e.stopPropagation(); });
        wrapper.addEventListener('click', gj.slider.methods.createClickHandler($slider, track, handle, data));
    },

    createClickHandler: function ($slider, track, handle, data) {
        return function (e) {
            var sliderPos, x, offset, stepSize, newValue;
            if (handle.getAttribute('drag') !== 'true') {
                sliderPos = gj.core.position($slider[0].parentElement);
                x = new gj.widget().mouseX(e) - sliderPos.left;
                offset = gj.core.width(handle) / 2;
                stepSize = gj.core.width(track) / (data.max - data.min);
                newValue = Math.round((x - offset) / stepSize) + data.min;
                gj.slider.methods.value($slider, data, newValue);
            }
        };
    },

    createMouseUpHandler: function ($slider, handle, data) {
        return function (e) {
            if (handle.getAttribute('drag') === 'true') {
                handle.setAttribute('drag', 'false');
                gj.slider.events.change($slider);
            }
        }
    },

    createMouseDownHandler: function (handle, data) {
        return function (e) {
            handle.setAttribute('drag', 'true');
        }
    },

    createMouseMoveHandler: function ($slider, track, handle, progress, data) {
        return function (e) {
            var sliderPos, x, trackWidth, offset, stepSize, valuePos, newValue;
            if (handle.getAttribute('drag') === 'true') {
                sliderPos = gj.core.position($slider[0].parentElement);
                x = new gj.widget().mouseX(e) - sliderPos.left;

                trackWidth = gj.core.width(track);
                offset = gj.core.width(handle) / 2;
                stepSize = trackWidth / (data.max - data.min);
                valuePos = (data.value - data.min) * stepSize;

                if (x >= offset && x <= (trackWidth + offset)) {
                    if (x > valuePos + (stepSize / 2) || x < valuePos - (stepSize / 2)) {
                        newValue = Math.round((x - offset) / stepSize) + data.min;
                        gj.slider.methods.value($slider, data, newValue);
                    }
                }
            }
        }
    },

    value: function ($slider, data, value) {
        var stepSize, track, handle, progress;
        if (typeof (value) === "undefined") {
            return $slider[0].value;
        } else {
            $slider[0].setAttribute('value', value);
            data.value = value;
            track = $slider.parent().children('[role="track"]')[0]
            stepSize = gj.core.width(track) / (data.max - data.min);
            handle = $slider.parent().children('[role="handle"]')[0];
            handle.style.left = ((value - data.min) * stepSize) + 'px';
            progress = $slider.parent().children('[role="progress"]')[0];
            progress.style.width = ((value - data.min) * stepSize) + 'px';
            gj.slider.events.slide($slider, value);
            return $slider;
        }
    },

    destroy: function ($slider) {
        var data = $slider.data(),
            $wrapper = $slider.parent();
        if (data) {
            $wrapper.children('[role="track"]').remove();
            $wrapper.children('[role="handle"]').remove();
            $wrapper.children('[role="progress"]').remove();
            $slider.unwrap();
            $slider.off();
            $slider.removeData();
            $slider.removeAttr('data-type').removeAttr('data-guid').removeAttr('data-slider');
            $slider.removeClass();
            $slider.show();
        }
        return $slider;
    }
};

gj.slider.events = {
    change: function ($slider) {
        return $slider.triggerHandler('change');
    },
    slide: function ($slider, value) {
        return $slider.triggerHandler('slide', [value]);
    }
};

gj.slider.widget = function ($element, jsConfig) {
    var self = this,
        methods = gj.slider.methods;
    self.value = function (value) {
        return methods.value(this, this.data(), value);
    };
    self.destroy = function () {
        return methods.destroy(this);
    };

    $.extend($element, self);
    if ('true' !== $element.attr('data-slider')) {
        methods.init.call($element, jsConfig);
    }

    return $element;
};

gj.slider.widget.prototype = new gj.widget();
gj.slider.widget.constructor = gj.slider.widget;

(function ($) {
    $.fn.slider = function (method) {
        var $widget;
        if (this && this.length) {
            if (typeof method === 'object' || !method) {
                return new gj.slider.widget(this, method);
            } else {
                $widget = new gj.slider.widget(this, null);
                if ($widget[method]) {
                    return $widget[method].apply(this, Array.prototype.slice.call(arguments, 1));
                } else {
                    throw 'Method ' + method + ' does not exist.';
                }
            }
        }
    };
})(jQuery);
/**
  * @widget ColorPicker
  * @plugin Base
  */
gj.colorpicker = {
    plugins: {},
    messages: {
        'en-us': {
        }
    }
};

gj.colorpicker.config = {
    base: {
        uiLibrary: 'materialdesign',
        value: undefined,

        icons: {
            rightIcon: '<i class="gj-icon">event</i>'
        },

        style: {
            modal: 'gj-modal',
            wrapper: 'gj-colorpicker gj-colorpicker-md gj-unselectable',
            input: 'gj-textbox-md',
            picker: 'gj-picker gj-picker-md colorpicker gj-unselectable',
            footer: '',
            button: 'gj-button-md'
        }
    },

    bootstrap: {
        style: {}
    },

    bootstrap4: {
        style: {}
    }
};

gj.colorpicker.methods = {
    init: function (jsConfig) {
        gj.picker.widget.prototype.init.call(this, jsConfig, 'colorpicker');
        gj.colorpicker.methods.initialize(this);
        return this;
    },

    initialize: function ($colorpicker) {
    },

    createPicker: function ($input, data) {
        var $picker = $('<div role="picker" />').addClass(data.style.picker).attr('guid', $input.attr('data-guid'));

        $picker.html('test');

        $picker.hide();
        $('body').append($picker);

        return $picker;
    },

    open: function ($input) {
        if ($input.val()) {
            $input.value($input.val());
        }
        return gj.picker.widget.prototype.open.call($input, 'colorpicker');
    }
};

gj.colorpicker.events = {
    change: function ($colorpicker) {
        return $colorpicker.triggerHandler('change');
    },
    select: function ($colorpicker) {
        return $colorpicker.triggerHandler('select');
    },
    open: function ($colorpicker) {
        return $colorpicker.triggerHandler('open');
    },
    close: function ($colorpicker) {
        return $colorpicker.triggerHandler('close');
    }
};

gj.colorpicker.widget = function ($element, jsConfig) {
    var self = this,
        methods = gj.colorpicker.methods;
    self.value = function (value) {
        return methods.value(this, value);
    };
    self.destroy = function () {
        return gj.picker.widget.prototype.destroy.call(this, 'colorpicker');
    };
    self.open = function () {
        return methods.open(this);
    };
    self.close = function () {
        return gj.picker.widget.prototype.close.call(this, 'colorpicker');
    };

    $.extend($element, self);
    if ('true' !== $element.attr('data-colorpicker')) {
        methods.init.call($element, jsConfig);
    }

    return $element;
};

gj.colorpicker.widget.prototype = new gj.picker.widget();
gj.colorpicker.widget.constructor = gj.colorpicker.widget;

(function ($) {
    $.fn.colorpicker = function (method) {
        var $widget;
        if (this && this.length) {
            if (typeof method === 'object' || !method) {
                return new gj.colorpicker.widget(this, method);
            } else {
                $widget = new gj.colorpicker.widget(this, null);
                if ($widget[method]) {
                    return $widget[method].apply(this, Array.prototype.slice.call(arguments, 1));
                } else {
                    throw 'Method ' + method + ' does not exist.';
                }
            }
        }
    };
})(jQuery);