var backdrop = document.getElementById('modalBackdrop');
var quickview = document.getElementById('quickview-popup');
var menu = document.getElementById('menu-wrapper')


//DETAIL MODAL PAGE START
$('.quickview-btn').click(function (e) {
    e.preventDefault();
    quickview.style.display="block";
    document.body.classList.add('modal-open');
    backdrop.style.display = "block";
})
$('.quickview-close').click(function(e){
    e.preventDefault();
    quickview.style.display="none"
    document.body.classList.remove('modal-open');
    backdrop.style.display = "none";
})
backdrop.addEventListener('click', function(e) {
    if (e.target === backdrop) {
        quickview.style.display="none"
        document.body.classList.remove('modal-open');
        backdrop.style.display = "none";
    }
});
//DETAIL MODAL PAGE END

//MOBILE MENU MODAL PAGE START
$('.mobile-menu-button').click(function (e) {
    e.preventDefault();
    menu.style.display="block";
    menu.classList.add('active-menu');
})
$('.menu-close-icon').click(function(e){
    e.preventDefault();
    menu.style.display="none"
    menu.classList.remove('active-menu');
})
//MOBILE MENU MODAL PAGE END