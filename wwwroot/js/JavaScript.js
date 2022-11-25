/*    $.fn.isInViewport = function() {
    var elementTop = $(this).offset().top;
    var elementBottom = elementTop + $(this).outerHeight();

    var viewportTop = $(window).scrollTop();
    var viewportBottom = viewportTop + $(window).height();

    return elementBottom > viewportTop && elementTop < viewportBottom;
    };

    $("#scrollWindow").on('scroll', function() {
        DotNet.invokeMethodAsync('BlazorSample', 'AddPerson')
            .then(data => {
                console.log(data);
            });
    });

*/