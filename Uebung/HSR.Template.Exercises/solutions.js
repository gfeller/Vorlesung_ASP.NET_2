document.addEventListener('DOMContentLoaded', function() {
	var body = document.getElementsByTagName('body')[0];
	if(window.location.search.match( /show-solutions/gi )) {
		body.setAttribute('data-show-solutions','true');
	}

	var showSolutionsButton = document.getElementById('showSolutions');
	if(showSolutionsButton) {
		showSolutionsButton.addEventListener('click', function() {
			if(body.hasAttribute('data-show-solutions')) {
				body.removeAttribute('data-show-solutions');
			} else {
				body.setAttribute('data-show-solutions','true');
			}
		});
	}
});