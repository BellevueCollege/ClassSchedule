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
- edit dialog popups definition
- fluiddialog for auto-resizing, centering dialogs
*/

/* program editor pop-up dialog */
$(function () {

    var bootstrapButton = $.fn.button.noConflict(); // return $.fn.button to previously assigned value
    $.fn.bootstrapBtn = bootstrapButton;            // give $().bootstrapBtn the Bootstrap functionality

    $("#edit-program").dialog({
        autoOpen: false,
        width: 'auto', // overcomes width:'auto' and maxWidth bug
        maxWidth: 600,
        height: 'auto',
        modal: true,
        fluid: true, //new option
        resizable: false,
        closeOnEscape: true,
        position: {
            my: "center center",
            at: "center center",
            of: window
        },
        close: function (event, ui) {
            $(this).empty();
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
        closeOnEscape: true,
        position: {
            my: "center center",
            at: "center center",
            of: window
        },
        close: function (event, ui) {
            $(this).empty();
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
        closeOnEscape: true,
        position: {
            my: "center center",
            at: "center center",
            of: window
        },
        close: function (event, ui) {
            $(this).empty();
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
        closeOnEscape: true,
        position: {
            my: "center center",
            at: "center center",
            of: window,
            collision: "fit"
        },
        close: function (event, ui) {
            $(this).empty();
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

    $(document).ready(function () {
        jQuery("time.timeago").timeago();
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
            //dialog.option("position", dialog.options.position);
            dialog.option("position", { my: "center center", at: "center center", of: window, collision: "fit" });
        }
    });
}