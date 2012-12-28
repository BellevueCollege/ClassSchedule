/// <reference path="jquery-1.5.1-vsdoc.js" />
/// <reference path="jquery-ui-1.8.11.js" />

// The above lines provide intellisense for jQuery

$(document).ready(function () {

	$('.course-updated a').click(function () {

		var classID = this.id;
		var availability = '#availability-' + classID + ' .seatsAvailable';
		var courseUpdated = '#availability-' + classID + ' .course-updated .update-time';
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
					$(courseUpdated).html(friendlyTime);
				} else {
					$(availability).html("class full");
					$('.course-updated').html("");
				}
			},
			error: function (x, t, m) {
				//alert("got timeout");
				$(availability).html(originalSeatsAvailable);
				$(courseUpdated).html("[service unavailable]");
			}

		});
	});
});