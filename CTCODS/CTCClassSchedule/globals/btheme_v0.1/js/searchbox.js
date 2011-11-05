$(document).ready(function(){
	/*Search*/
	var searchBoxValue = 'Search';
	$('.searchfield').val(searchBoxValue);
	$('.searchfield').focus(function(){
		if ($(this).val() == searchBoxValue){
			$(this).val('');
			$(this).parents('.searchform').addClass('selected');
		}
	}).blur(function(){
		if ($(this).val() == ''){
			$(this).val(searchBoxValue);
			$(this).parents('.searchform').removeClass('selected');
		}
	});
});