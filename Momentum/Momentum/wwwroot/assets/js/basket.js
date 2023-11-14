$(document).ready(function () {
    var backdrop = document.getElementById('modalBackdrop');


    $(".hcart").on('click', function(){
        document.getElementById('cart-modal').classList.add('cartOpen');
        document.getElementById('cart-header').classList.add('cart-header');
        document.getElementById('cart-header').classList.remove('d-none');
        $('body').addClass('mobile-open no-scroll');
        backdrop.style.display = "block";
	})
    $(".closecart").on('click', function(){
        document.getElementById('cart-modal').classList.remove('cartOpen');
        document.getElementById('cart-header').classList.remove('cart-header');
        document.getElementById('cart-header').classList.add('d-none');
        $('body').removeClass('mobile-open no-scroll');
        backdrop.style.display = "none";
	})
    backdrop.addEventListener('click', function(e) {
        if (e.target === backdrop) {
            document.getElementById('cart-modal').classList.remove('cartOpen');
            document.getElementById('cart-header').classList.remove('cart-header');
            document.getElementById('cart-header').classList.add('d-none');
            document.body.classList.remove('modal-open');
            $('body').removeClass('mobile-open no-scroll');
            backdrop.style.display = "none";
        }
    });
})




