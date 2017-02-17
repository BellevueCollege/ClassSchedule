/*
This file is part of CtcClassSchedule.

CtcClassSchedule is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

CtcClassSchedule is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with CtcClassSchedule.  If not, see <http://www.gnu.org/licenses/>.
*/

/*
contents:
- custom functions
- hoverintent r6 plugin
- tooltipsy
*/

function AppendUrlPath(base, part) {
  var lastChar = base.length - 1;

  if (base[lastChar] == "/") {
    base = base.substring(0, lastChar);
  }
  if (part[0] == "/") {
    part = part.substring(1, part.length);
  }
  var url = base + "/" + part;
  return url;
}

/* program editor pop-up dialog */
$(function () {

    var bootstrapButton = $.fn.button.noConflict() // return $.fn.button to previously assigned value
    $.fn.bootstrapBtn = bootstrapButton            // give $().bootstrapBtn the Bootstrap functionality

    $("#edit-program").dialog({
        autoOpen: false,
        width: 'auto', // overcomes width:'auto' and maxWidth bug
        maxWidth: 600,
        height: 'auto',
        modal: true,
        fluid: true, //new option
        resizable: false,
        position: {
            my: "center center",
            at: "center center",
            of: window
        }
    });

    $("#edit-section").dialog({
        autoOpen: false,
        width: 'auto', // overcomes width:'auto' and maxWidth bug
        maxWidth: 600,
        height: 'auto',
        modal: true,
        fluid: true, //new option
        resizable: false,
        position: {
            my: "center center",
            at: "center center",
            of: window
        }
    });

   $("#edit-class").dialog({
        autoOpen: false,
        width: 'auto', // overcomes width:'auto' and maxWidth bug
        maxWidth: 600,
        height: 'auto',
        modal: true,
        fluid: true, //new option
        resizable: false,
        position: {
            my: "center center",
            at: "center center",
            of: window
        }
    });

    $("#nav-choose-subjects").dialog({
        autoOpen: false,
        width: 'auto', // overcomes width:'auto' and maxWidth bug
        maxWidth: 600,
        height: 'auto',
        modal: true,
        fluid: true, //new option
        resizable: false,
        position: {
            my: "center center",
            at: "center center",
            of: window
        }
    });

    // on window resize run function
    $(window).resize(function () {
        fluidDialog();
    });

    // catch dialog if opened within a viewport smaller than the dialog width
    $(document).on("dialogopen", ".ui-dialog", function (event, ui) {
        fluidDialog();
    });

});

function fluidDialog() {
    var $visible = $(".ui-dialog:visible");
    // each open dialog
    $visible.each(function () {
        var $this = $(this);
        var dialog = $this.find(".ui-dialog-content").data("ui-dialog");
        // if fluid option == true
        if (dialog.options.fluid) {
            var wWidth = $(window).width();
            // check window width against dialog width
            if (wWidth < (parseInt(dialog.options.maxWidth) + 50)) {
                // keep dialog from filling entire screen
                $this.css("max-width", "90%");
            } else {
                // fix maxWidth bug
                $this.css("max-width", dialog.options.maxWidth + "px");
            }
            //reposition dialog
            dialog.option("position", dialog.options.position);
        }
    });
}

$(function () {
    /*has javascript*/
    //$("body").addClass("js");

    /*make classes tab selected*/
   /* $("#mainnav-classes").find("a").addClass("selected");

    $("#mainnav-list").find(".mainnav-item").hoverIntent({
        interval: 50, // milliseconds delay before onMouseOver
        over: dropdown_show,
        timeout: 100, // milliseconds delay before onMouseOut
        out: dropdown_hide
    })*/

    //Generate dropdown menus
    //$("#mainnav-about").addClass('hasarrow').append('<div class="downarrow"></div><div class="dropdown"></div>').find(".dropdown").load(AppendUrlPath(g_globalsPath, "dropdown_about.asp"));
    //$("#mainnav-programs").addClass('hasarrow').append('<div class="downarrow"></div><div class="dropdown"></div>').find(".dropdown").load(AppendUrlPath(g_globalsPath, "dropdown_programs.asp"));
    //$("#mainnav-enrollment").addClass('hasarrow').append('<div class="downarrow"></div><div class="dropdown"></div>').find(".dropdown").load(AppendUrlPath(g_globalsPath, "dropdown_enrollment.asp"));
    //$("#mainnav-resources").addClass('hasarrow').append('<div class="downarrow"></div><div class="dropdown"></div>').find(".dropdown").load(AppendUrlPath(g_globalsPath, "dropdown_resources.asp"));


    /*$('#mainnav-list .dropdown').focusin(function () {
        $(this).parent(".mainnav-item").addClass('hover');
    });
    $('#mainnav-list .dropdown').focusout(function () {
        $(this).parent(".mainnav-item").removeClass('hover');
    });*/

    /*tooltip*/
    //$('abbr').tooltipsy();

    /*drowdown menu*/
   /* $('#browse').hoverIntent({
        interval: 50, // milliseconds delay before onMouseOver
        over: dropdown_show,
        timeout: 400, // milliseconds delay before onMouseOut
        out: dropdown_hide
    }).find(".nav-dropdown").prepend('<div class="downarrow"></div>');*/

    /*search bar*/
    $("#search-keyword").focus(function () {
        $(this).parents('#searchfield-wrap').addClass('focus');
    }).blur(function () {
        $(this).parents('#searchfield-wrap').removeClass('focus');
    });


});