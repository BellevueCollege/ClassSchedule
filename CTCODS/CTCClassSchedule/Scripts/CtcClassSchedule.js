/// <reference path="jquery-1.5.1-vsdoc.js" />
/// <reference path="jquery-ui-1.8.11.js" />

// The above lines provide intellisense for jQuery

$(document).ready(function () {

		$('.course-updated a').click(function () {

		var courseIdPlusYRQ = this.id;
		var availability = '#availability-' + courseIdPlusYRQ + ' .seatsAvailable';
		var courseUpdated = '#availability-' + courseIdPlusYRQ + ' .course-updated .update-time';
		var originalSeatsAvailable = $(availability).html();

		//load the throbber
		$(availability).html('<img src="' + g_ajaxLoaderImgPath + '" title="loading_seats_available" alt="loading_seats_available" />');

		//post the ajax call to get the available seats and update the UI
		$.ajax({
			url: g_getSeatsUrl,
			type: 'POST',
			data: { courseIdPlusYRQ: courseIdPlusYRQ },
			timeout: 4000,
			success: function (result) {
				var indexOfPipe = result.indexOf("|");
				var seatsAvailable = result.substring(0, indexOfPipe);
				var friendlyTime = result.substring(indexOfPipe + 1, result.length);

				$(availability).html(seatsAvailable);
				$(courseUpdated).html(friendlyTime);
			},
			error: function (x, t, m) {
				//alert("got timeout");
				$(availability).html(originalSeatsAvailable);
				$(courseUpdated).html("server busy, try again");
			}

		});
	});
});
