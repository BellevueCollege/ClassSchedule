/*
contents:
- custom functions
- hoverintent r6 plugin
- tooltipsy
*/

$(function () {
    /*has javascript*/
    $("body").addClass("js");

    /*make classes tab selected*/
    $("#mainnav-classes").find("a").addClass("selected");

    $("#mainnav-list").find(".mainnav-item").hoverIntent({
        interval: 50, // milliseconds delay before onMouseOver
        over: dropdown_show,
        timeout: 100, // milliseconds delay before onMouseOut
        out: dropdown_hide
    })

    //Generate dropdown menus
    $("#mainnav-about").addClass('hasarrow').append('<div class="downarrow"></div><div class="dropdown"></div>').find(".dropdown").load("/globals/dropdown_about.asp");
    $("#mainnav-programs").addClass('hasarrow').append('<div class="downarrow"></div><div class="dropdown"></div>').find(".dropdown").load("/globals/dropdown_programs.asp");
    $("#mainnav-enrollment").addClass('hasarrow').append('<div class="downarrow"></div><div class="dropdown"></div>').find(".dropdown").load("/globals/dropdown_enrollment.asp");
    $("#mainnav-resources").addClass('hasarrow').append('<div class="downarrow"></div><div class="dropdown"></div>').find(".dropdown").load("/globals/dropdown_resources.asp");


    $('#mainnav-list .dropdown').focusin(function () {
        $(this).parent(".mainnav-item").addClass('hover');
    });
    $('#mainnav-list .dropdown').focusout(function () {
        $(this).parent(".mainnav-item").removeClass('hover');
    });

    /*tooltip*/
    $('abbr').tooltipsy();

    /*drowdown menu*/
    $('#browse').hoverIntent({
        interval: 50, // milliseconds delay before onMouseOver
        over: dropdown_show,
        timeout: 400, // milliseconds delay before onMouseOut
        out: dropdown_hide
    }).find(".nav-dropdown").prepend('<div class="downarrow"></div>');

    /*search bar*/
    $("#search-keyword").focus(function () {
        $(this).parents('#searchfield-wrap').addClass('focus');
    }).blur(function () {
        $(this).parents('#searchfield-wrap').removeClass('focus');
    });

    /*set min-height for #content based on sidebar  */
    $('#container.sidebar #content').css({ 'min-height': (($("#sidebar").height())) + 'px' });

    /*set arrow on sectionTitle */
    $("#sectionTitle").prepend('<div class="arrow"></div>');

});

function dropdown_show(){ $(this).addClass('hover'); }
function dropdown_hide(){ $(this).removeClass('hover'); }

/**
* hoverIntent r6 // 2011.02.26 // jQuery 1.5.1+
* <http://cherne.net/brian/resources/jquery.hoverIntent.html>
*
* @param  f  onMouseOver function || An object with configuration options
* @param  g  onMouseOut function  || Nothing (use configuration options object)
* @author    Brian Cherne brian(at)cherne(dot)net
*/
(function ($) { $.fn.hoverIntent = function (f, g) { var cfg = { sensitivity: 7, interval: 100, timeout: 0 }; cfg = $.extend(cfg, g ? { over: f, out: g} : f); var cX, cY, pX, pY; var track = function (ev) { cX = ev.pageX; cY = ev.pageY }; var compare = function (ev, ob) { ob.hoverIntent_t = clearTimeout(ob.hoverIntent_t); if ((Math.abs(pX - cX) + Math.abs(pY - cY)) < cfg.sensitivity) { $(ob).unbind("mousemove", track); ob.hoverIntent_s = 1; return cfg.over.apply(ob, [ev]) } else { pX = cX; pY = cY; ob.hoverIntent_t = setTimeout(function () { compare(ev, ob) }, cfg.interval) } }; var delay = function (ev, ob) { ob.hoverIntent_t = clearTimeout(ob.hoverIntent_t); ob.hoverIntent_s = 0; return cfg.out.apply(ob, [ev]) }; var handleHover = function (e) { var ev = jQuery.extend({}, e); var ob = this; if (ob.hoverIntent_t) { ob.hoverIntent_t = clearTimeout(ob.hoverIntent_t) } if (e.type == "mouseenter") { pX = ev.pageX; pY = ev.pageY; $(ob).bind("mousemove", track); if (ob.hoverIntent_s != 1) { ob.hoverIntent_t = setTimeout(function () { compare(ev, ob) }, cfg.interval) } } else { $(ob).unbind("mousemove", track); if (ob.hoverIntent_s == 1) { ob.hoverIntent_t = setTimeout(function () { delay(ev, ob) }, cfg.timeout) } } }; return this.bind('mouseenter', handleHover).bind('mouseleave', handleHover) } })(jQuery);


/* tooltipsy by Brian Cray
* Lincensed under GPL2 - http://www.gnu.org/licenses/gpl-2.0.html
* Option quick reference:
* - alignTo: "element" or "cursor" (Defaults to "element")
* - offset: Tooltipsy distance from element or mouse cursor, dependent on alignTo setting. Set as array [x, y] (Defaults to [0, -1])
* - content: HTML or text content of tooltip. Defaults to "" (empty string), which pulls content from target element's title attribute
* - show: function(event, tooltip) to show the tooltip. Defaults to a show(100) effect
* - hide: function(event, tooltip) to hide the tooltip. Defaults to a fadeOut(100) effect
* - delay: A delay in milliseconds before showing a tooltip. Set to 0 for no delay. Defaults to 200
* - css: object containing CSS properties and values. Defaults to {} to use stylesheet for styles
* - className: DOM class for styling tooltips with CSS. Defaults to "tooltipsy"
* - showEvent: Set a custom event to bind the show function. Defaults to mouseenter
* - hideEvent: Set a custom event to bind the show function. Defaults to mouseleave
* Method quick reference:
* - $('element').data('tooltipsy').show(): Force the tooltip to show
* - $('element').data('tooltipsy').hide(): Force the tooltip to hide
* - $('element').data('tooltipsy').destroy(): Remove tooltip from DOM
* More information visit http://tooltipsy.com/
*/
(function (a) { a.tooltipsy = function (c, b) { this.options = b; this.$el = a(c); this.title = this.$el.attr("title") || ""; this.$el.attr("title", ""); this.random = parseInt(Math.random() * 10000); this.ready = false; this.shown = false; this.width = 0; this.height = 0; this.delaytimer = null; this.$el.data("tooltipsy", this); this.init() }; a.tooltipsy.prototype.init = function () { var b = this; b.settings = a.extend({}, b.defaults, b.options); b.settings.delay = parseInt(b.settings.delay); if (typeof b.settings.content === "function") { b.readify() } if (b.settings.showEvent === b.settings.hideEvent && b.settings.showEvent === "click") { b.$el.toggle(function (c) { if (b.settings.showEvent === "click" && b.$el[0].tagName == "A") { c.preventDefault() } if (b.settings.delay > 0) { b.delaytimer = window.setTimeout(function () { b.show(c) }, b.settings.delay) } else { b.show(c) } }, function (c) { if (b.settings.showEvent === "click" && b.$el[0].tagName == "A") { c.preventDefault() } window.clearTimeout(b.delaytimer); b.delaytimer = null; b.hide(c) }) } else { b.$el.bind(b.settings.showEvent, function (c) { if (b.settings.showEvent === "click" && b.$el[0].tagName == "A") { c.preventDefault() } if (b.settings.delay > 0) { b.delaytimer = window.setTimeout(function () { b.show(c) }, b.settings.delay) } else { b.show(c) } }).bind(b.settings.hideEvent, function (c) { if (b.settings.showEvent === "click" && b.$el[0].tagName == "A") { c.preventDefault() } window.clearTimeout(b.delaytimer); b.delaytimer = null; b.hide(c) }) } }; a.tooltipsy.prototype.show = function (f) { var d = this; if (d.ready === false) { d.readify() } if (d.shown === false) { if ((function (h) { var g = 0, e; for (e in h) { if (h.hasOwnProperty(e)) { g++ } } return g })(d.settings.css) > 0) { d.$tip.css(d.settings.css) } d.width = d.$tipsy.outerWidth(); d.height = d.$tipsy.outerHeight() } if (d.settings.alignTo === "cursor" && f) { var c = [f.pageX + d.settings.offset[0], f.pageY + d.settings.offset[1]]; if (c[0] + d.width > a(window).width()) { var b = { top: c[1] + "px", right: c[0] + "px", left: "auto"} } else { var b = { top: c[1] + "px", left: c[0] + "px", right: "auto"} } } else { var c = [(function (e) { if (d.settings.offset[0] < 0) { return e.left - Math.abs(d.settings.offset[0]) - d.width } else { if (d.settings.offset[0] === 0) { return e.left - ((d.width - d.$el.outerWidth()) / 2) } else { return e.left + d.$el.outerWidth() + d.settings.offset[0] } } })(d.offset(d.$el[0])), (function (e) { if (d.settings.offset[1] < 0) { return e.top - Math.abs(d.settings.offset[1]) - d.height } else { if (d.settings.offset[1] === 0) { return e.top - ((d.height - d.$el.outerHeight()) / 2) } else { return e.top + d.$el.outerHeight() + d.settings.offset[1] } } })(d.offset(d.$el[0]))] } d.$tipsy.css({ top: c[1] + "px", left: c[0] + "px" }); d.settings.show(f, d.$tipsy.stop(true, true)) }; a.tooltipsy.prototype.hide = function (c) { var b = this; if (b.ready === false) { return } if (c && c.relatedTarget === b.$tip[0]) { b.$tip.bind("mouseleave", function (d) { if (d.relatedTarget === b.$el[0]) { return } b.settings.hide(d, b.$tipsy.stop(true, true)) }); return } b.settings.hide(c, b.$tipsy.stop(true, true)) }; a.tooltipsy.prototype.readify = function () { this.ready = true; this.$tipsy = a('<div id="tooltipsy' + this.random + '" style="position:absolute;z-index:2147483647;display:none">').appendTo("body"); this.$tip = a('<div class="' + this.settings.className + '">').appendTo(this.$tipsy); this.$tip.data("rootel", this.$el); var c = this.$el; var b = this.$tip; this.$tip.html(this.settings.content != "" ? (typeof this.settings.content == "string" ? this.settings.content : this.settings.content(c, b)) : this.title) }; a.tooltipsy.prototype.offset = function (c) { var b = ot = 0; if (c.offsetParent) { do { if (c.tagName != "BODY") { b += c.offsetLeft - c.scrollLeft; ot += c.offsetTop - c.scrollTop } } while (c = c.offsetParent) } return { left: b, top: ot} }; a.tooltipsy.prototype.destroy = function () { this.$tipsy.remove(); a.removeData(this.$el, "tooltipsy") }; a.tooltipsy.prototype.defaults = { alignTo: "element", offset: [0, -1], content: "", show: function (c, b) { b.fadeIn(100) }, hide: function (c, b) { b.fadeOut(100) }, css: {}, className: "tooltipsy", delay: 200, showEvent: "mouseenter", hideEvent: "mouseleave" }; a.fn.tooltipsy = function (b) { return this.each(function () { new a.tooltipsy(this, b) }) } })(jQuery);
