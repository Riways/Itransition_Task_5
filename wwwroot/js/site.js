// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


function isBottomOfScroll() {
    let elementTop = $("#scrollBottom").offset().top;
    let elementBottom = $("#scrollBottom").offset().bottom + 200;
    let parent = document.querySelector("#scrollWindow")
    let parentBottom = parent.getBoundingClientRect().bottom;
    return elementTop <= parentBottom  || elementBottom <= parentBottom ;
}

