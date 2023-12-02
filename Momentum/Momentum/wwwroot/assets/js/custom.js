$(document).ready(function () {

    $('.quickview-btn').click(function(e){
        $('.slider-for').slick({
            slidesToShow: 1,
            slidesToScroll: 1,
            arrows: false,
            dots:false,
            fade: true,
            asNavFor: '.slider-nav',
            responsive: [
                {
                breakpoint: 1024,
                settings: {
                    slidesToShow: 1,
                    slidesToScroll: 1,
                    infinite: true,
                    dots: false
                }
                },
                {
                breakpoint: 600,
                settings: {
                    slidesToShow: 1,
                    slidesToScroll: 1,
                    dots:false,
                    arrows:false
                }
                },
                {
                breakpoint: 480,
                settings: {
                    slidesToShow: 1,
                    slidesToScroll: 1,
                    dots:false,
                    arrows:false
                }
                }
            ]
        });
        $('.slider-nav').slick({
            slidesToShow: 3,
            slidesToScroll: 1,
            asNavFor: '.slider-for',
            arrows: true,
            dots: false,
            centerMode: true,
            focusOnSelect: true,
            draggable:true,
            vertical:true,
        });
    })
    $('.categories-slider').slick({
        dots: false,
        infinite: false,
        speed: 300,
        slidesToShow: 4,
        slidesToScroll: 1,
        arrows:true,
        responsive: [
            {
            breakpoint: 991,
            settings: {
                slidesToShow: 3,
                slidesToScroll: 1,
                infinite: true,
                dots: false
            }
            },
            {
            breakpoint: 768,
            settings: {
                slidesToShow: 2,
                slidesToScroll: 1,
                dots:false,
                arrows:true
            }
            },
            {
            breakpoint: 575,
            settings: {
                slidesToShow: 1,
                slidesToScroll: 1,
                dots:false,
                arrows:true
            }
            }
        ]
    });
    $('.responsive').slick({
        dots: false,
        infinite: false,
        speed: 300,
        slidesToShow: 4,
        arrows:true,
        slidesToScroll: 4,
        draggable: false,
        responsive: [
        {
            breakpoint: 1400,
            settings: {
            slidesToShow: 4,
            slidesToScroll: 3,
            infinite: false,
            arrows:true,
            dots: false
            }
        },
        {
            breakpoint: 991,
            settings: {
            slidesToShow: 3,
            slidesToScroll: 2,
            arrows:true,
            draggable:true,
            infinite: false,
            }
        },
        {
            breakpoint: 768,
            settings: {
            slidesToShow: 2,
            slidesToScroll: 2,
            arrows:true,
            draggable:true,
            infinite: false,
            }
        },
        {
            breakpoint: 575,
            settings: {
            slidesToShow: 1,
            slidesToScroll: 1,
            arrows:true,
            draggable:true,
            infinite: false,
            }
        }
    ]
    });
    $('.home-product-slider').slick({
        dots:true,
        arrows:false,
        swipeToSlide: true,
        infinite:true,
        slidesToShow: 5,
        centerMode: true,
        speed:400,
        responsive: [
            {
            breakpoint: 1400,
            settings: {
                slidesToShow: 3,
                speed:500,
            }
            },
            {
            breakpoint: 991,
            settings: {
                slidesToShow: 3,
                speed:500,
                
            }
            },
            {
            breakpoint: 768,
            settings: {
                slidesToShow: 1,
                speed:500,
                centerMode: false,
            }
            },
            {
            breakpoint: 575,
            settings: {
                slidesToShow: 1,
                speed:500,
                centerMode:false,
            }
            }
        ]
    });



    // scroll to top
    $(window).on('scroll', function () {
    if ($(this).scrollTop() > 600) {
        $('.scroll-top').removeClass('not-visible');
    } else {
        $('.scroll-top').addClass('not-visible');
    }
    });
    $('.scroll-top').click(function(){
    $('html, body').animate({
        scrollTop: 0
    }, 100);
    });

    var backdrop = document.getElementById('modalBackdrop');

    $(".mobile-facets__open").click( function(e){
        e.preventDefault();
        document.getElementById('mobile-filter-content').classList.remove('d-none');
        document.getElementById('mobile-filter-content').classList.add('d-block');
        document.getElementById('close-filter').classList.remove('d-none');
        document.getElementById('close-filter').classList.add('d-block');
        document.getElementById('mobile-filter-content').classList.add('filterOpen');
        $('body').addClass('mobile-open no-scroll');
        backdrop.style.display = 'block';
	})
    $(".close-filter").click(function(e){
        e.preventDefault();
        document.getElementById('mobile-filter-content').classList.remove('d-block');
        document.getElementById('mobile-filter-content').classList.add('d-none');
        document.getElementById('close-filter').classList.remove('d-block');
        document.getElementById('close-filter').classList.add('d-none');
        document.getElementById('mobile-filter-content').classList.remove('filterOpen');
        $('body').removeClass('mobile-open no-scroll');
        backdrop.style.display = 'none';
	})
    backdrop.addEventListener('click', function(e) {
        if (e.target === backdrop) {
            document.getElementById('mobile-filter-content').classList.remove('d-block');
            document.getElementById('mobile-filter-content').classList.add('d-none');
            document.getElementById('close-filter').classList.remove('d-block');
            document.getElementById('close-filter').classList.add('d-none');
            document.getElementById('mobile-filter-content').classList.remove('filterOpen');
            $('body').removeClass('mobile-open no-scroll');
            backdrop.style.display = 'none';
        }
    });
    
  	// offcanvas search form active start
	$(".offcanvas-btn").on('click', function(){
		$("body").addClass('fix');
		$(".offcanvas-search-inner").addClass('show')
	})

	$(".minicart-btn").on('click', function(){
		$("body").addClass('fix');
		$(".minicart-inner").addClass('show')
	})

	$(".offcanvas-close, .minicart-close, .closecart ,.offcanvas-overlay").on('click', function(){
		$("body").removeClass('fix');
		$(".offcanvas-search-inner, .minicart-inner").removeClass('show')
	})

	// Off Canvas Open close start
	$(".off-canvas-btn").on('click', function () {
		$("body").addClass('fix');
		$(".off-canvas-wrapper").addClass('open');
	});

	$(".btn-close-off-canvas,.off-canvas-overlay").on('click', function () {
		$("body").removeClass('fix');
		$(".off-canvas-wrapper").removeClass('open');
	});

    /***** Mobile Menu *****/
    $("#menu").on("click", function () {
        $(".mobile-menu-wrapper").addClass("active-menu");
    });
    $(".menu-close-icon").on("click", function () {
        $(".mobile-menu-wrapper").toggleClass("active-menu");
    });

    /***** Mobile Filter Popup *****/
    $('.mobile-facets__open').on('click', function (e) {
    $('body').addClass('mobile-open no-scroll');
    $('.overlay').addClass('active');
  });
  $('.overlay,.close-filter,.apply-btn').on('click', function (e) {
    $('body').removeClass('mobile-open no-scroll');
    $('.overlay').removeClass('active');
  });
    //   /*********  size Popup  ********/ 
    // $(".size-btn").click(function() { 
    //     $(".size-popup").toggleClass("active"); 
    //     $("body").toggleClass("no-scroll");
    // });
    // $(".close-search").click(function() { 
    //     $(".size-popup").removeClass("active"); 
    //     $("body").removeClass("no-scroll");
    // });
    // $(".close-btn").click(function() { 
    //     $(".size-popup").removeClass("active"); 
    //     $("body").removeClass("no-scroll");
    // });
});

function responsiveMenu(){
  if(jQuery(window).width() < 768) {
    $('.product-filter-column').prependTo('.product-heading-row .container .row');
  }
  else { 
    $('.product-filter-column').prependTo('.product-list-row');
  }
}
jQuery(document).ready(function() {
  responsiveMenu();
});

jQuery(window).resize(function() {
  responsiveMenu();
});

//quantity section start
document.addEventListener('DOMContentLoaded', function () {
    var quantityInputs = document.querySelectorAll('.quantity__input');
    var plusButtons = document.querySelectorAll('.quantity-increment');
    var minusButtons = document.querySelectorAll('.quantity-decrement');

    quantityInputs.forEach(function (quantityInput, index) {
        plusButtons[index].addEventListener('click', function () {
            quantityInput.value = parseInt(quantityInput.value, 10) + 1;
        });

        minusButtons[index].addEventListener('click', function () {
            if (parseInt(quantityInput.value, 10) > 1) {
                quantityInput.value = parseInt(quantityInput.value, 10) - 1;
            }
        });
    });
});
//quantity section end

//cart notification start
//document.addEventListener('DOMContentLoaded', function() {
//    var cartBtns = document.querySelectorAll('.add-cart-btn');
//    var cartNotifClose = document.querySelector('.cart-notification__close');
//    var cartNotif = document.getElementById('cart-notification');
//    cartBtns.forEach(function(cartBtn){
//        cartBtn.addEventListener('click', function(e) {
//            e.preventDefault();
//            cartNotif.classList.add('active');
//        });
//        cartNotifClose.addEventListener('click', function() {
//            cartNotif.classList.remove('active');
//        });
//    })
//});
//cart notification end


//FILTERS OPENING CLOSING AREA START
//document.addEventListener('DOMContentLoaded', function () {
//    const labels = $('.acnav-label');
//    const lists = $('.acnav-list');

//    labels.each(function (index) {
//        $(this).click(function () {
//            if (lists[index].style.display !== 'block') {
//                lists[index].style.display = 'block';
//                $(this).addClass('clicked');
//            } else {
//                lists[index].style.display = 'none';
//                $(this).removeClass('clicked');
//            }
//        });
//    });
//});
//FILTERS OPENING CLOSING AREA END

//FILTERS MOBILE OPENING CLOSING AREA START
const details = document.querySelectorAll('.mobile-facets__details');

details.forEach(detail => {
    const summary = detail.querySelector('.mobile-facets__summary');
    const submenu = detail.querySelector('.mobile-facets__submenu');
    const closesubmenu = detail.querySelector('.mobile-facets__close-button');
    summary.addEventListener('click', function () {
        submenu.classList.add('show');
    });
    closesubmenu.addEventListener('click', function (e) {
        e.preventDefault();
        submenu.classList.remove('show');
    });
});
//FILTERS MOBILE OPENING CLOSING AREA END
$('.quickview-btn').click(function (e) {

    let url = $(this).attr('href');

    fetch(url)
        .then(res => res.text())
        .then(data => {
            $('.quickview_popup_data').html(data);

            $('.slider-for').slick({
                slidesToShow: 1,
                slidesToScroll: 1,
                arrows: false,
                dots: false,
                fade: true,
                asNavFor: '.slider-nav',
                responsive: [
                    {
                        breakpoint: 1024,
                        settings: {
                            slidesToShow: 1,
                            slidesToScroll: 1,
                            infinite: true,
                            dots: false
                        }
                    },
                    {
                        breakpoint: 600,
                        settings: {
                            slidesToShow: 1,
                            slidesToScroll: 1,
                            dots: false,
                            arrows: false
                        }
                    },
                    {
                        breakpoint: 480,
                        settings: {
                            slidesToShow: 1,
                            slidesToScroll: 1,
                            dots: false,
                            arrows: false
                        }
                    }
                ]
            });
            $('.slider-nav').slick({
                slidesToShow: 3,
                slidesToScroll: 1,
                asNavFor: '.slider-for',
                arrows: true,
                dots: false,
                centerMode: true,
                focusOnSelect: true,
                draggable: true,
                vertical: true,
            });
        });
});
