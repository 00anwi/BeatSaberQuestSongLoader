var ContentLoads = 0;

function CheckScroll() {

    if (($(window).scrollTop() == $(document).height() - $(window).height()) && ($(document).height() != $(window).height())) {
        if (ContentLoads <= 2) {
            ContentLoads += 1;
            return true;
        }
        else {
            return false
        }
    }
    else {
        ContentLoads = 0;
        return false;
    }
}

function ResetScroll() {
    LastContentLoadDocHeight = 0;

    if (LastContentLoadDocHeight == 0) {
        return true;
    }
    else {
        return false;
    }
}