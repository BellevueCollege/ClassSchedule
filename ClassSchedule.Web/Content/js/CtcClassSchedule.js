$(document).ready(function() {

  $('.course-updated a').click(function () {
      //alert("here");
    var classID = this.id;
    var availability = '#availability-' + classID + ' .seatsAvailable';
    var courseUpdated = '#availability-' + classID + ' .course-updated';
    var updateTime = '#availability-' + classID + ' .course-updated .update-time';
    var originalSeatsAvailable = $(availability).html();

    //load the throbber
    $(availability).html('<img src="' + g_ajaxLoaderImgPath + '" title="loading_seats_available" alt="loading_seats_available" />');

    //post the ajax call to get the available seats and update the UI
    $.ajax({
      url: g_getSeatsUrl,
      type: 'POST',
      data: { classID: classID },
      timeout: 4000,
      success: function (result) {
        var indexOfPipe = result.indexOf("|");
        var seatsAvailable = result.substring(0, indexOfPipe);
        var friendlyTime = result.substring(indexOfPipe + 1, result.length);

        if (seatsAvailable > 0) {
          $(availability).html(seatsAvailable);
          $(updateTime).html("refreshed " + friendlyTime);
        } else {
          $(availability).html("Class full");
//          $(updateTime).html("refreshed " + friendlyTime);
          $(updateTime).html("recheck");
          //$(courseUpdated).empty();
        }
      },
      error: function (x, t, m) {
        $(availability).html(originalSeatsAvailable);
        $(updateTime).html("[service unavailable]");
      }

    });
  });
});


function LoadCrossListedCourses(jsonUrl, div, quarter) {
  //console.log("Loading cross-listed courses...");
  div.html("Loading cross-listed courses...");

  // TODO: move this code into a function that we can put in an external file
  jQuery.ajax({
    url: jsonUrl,
    // JSONP does not seem to be working for some reason. 3/19/2013 - shawn.south@bellevuecollege.edu
    dataType: 'json',
    success: function (data) {
      //console.log("Received:");
      //console.log(data);

      if (data != null) {
        var crossListedCourses = "";

        // loop through the return data and build a heading for each cross-listed course.
        $.each(data, function (k, v) {
          //console.log("Processing: '" + k + "' = '" + v + "'");

          // TODO: Move construction of course heading into a shared function
          var cid = v.CourseID.Subject + (v.IsCommonCourse ? "&amp;" : "") + " " + v.CourseID.Number;

          // convert the search query to the expected format
          var searchID = v.SectionID.ItemNumber;
          var searchQuarter = quarter.replace(/\s+/g, '');

          // generate the lookup link (via search)
          var searchHref = g_searchRootUrl + "?quarter=" + searchQuarter + "&searchterm=" + searchID;

          var courseID = "<span class=\"courseID\">" + cid + "</span>";
          //console.log("courseID = '" + courseID + "'");

          var courseTitle = "<span class=\"courseTitle\">" + v.Title + "</span>";
         // console.log("courseTitle = '" + courseTitle + "'");

          var courseCredits = "<span class=\"courseCredits\">&#8226; ";

          if (v.IsVariableCredits) {
            courseCredits += "V1-" + v.Credits + " <abbr title='variable credits'>Cr.</abbr>";
          } else {
            courseCredits += v.Credits + " <abbr title='credit(s)'>Cr.</abbr>";
          }
          courseCredits += "</span>";
         // console.log("courseCredits = '" + courseCredits + "'");

          crossListedCourses += "<li class='section-cross-listed-course-title'><a href='" + searchHref + "'>" + courseID + " " + courseTitle + "</a> " + courseCredits + "</li>";
        });
        //console.log(crossListedCourses);

        // account for possible failure in generating the list of courses
        if (crossListedCourses == "") {
          crossListedCourses = "<li>FAILED TO RETRIEVE CROSS-LISTED COURSES</li>";
        }

        div.html(
          "<ul>" + crossListedCourses + "</ul>"
        );
      } else {
        div.html("(Not found)");
      }
    },
    error: function (ex, xhr) {
      //console.log("Error: " + ex.message);
      //console.log(xhr);
    }
  });
}
