function font(e) {

	var el = document.getElementById('accessibility');
	var el2 = document.getElementById('accessibility2');
	var style = window.getComputedStyle(el, null).getPropertyValue('font-size');
	var fontSize = parseFloat(style);

	if (e == 'a') {
		el.style.fontSize = (fontSize + 1) + 'px';
		//el2.style.fontSize = (fontSize + 1) + 'px';
	} else if (e== 'd') {
		el.style.fontSize = (fontSize - 1) + 'px';
		//el2.style.fontSize = (fontSize + 1) - 'px';
	}
}