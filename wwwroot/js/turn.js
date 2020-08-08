function ConfirmAccess(uniqueId, isAccessedClicked) {
    var accessedSpan = 'accessedSpan_' + uniqueId;
    var confirmaccessedSpan = 'confirmaccessedSpan_' + uniqueId;

    if (isAccessedClicked) {
        $('#' + accessedSpan).hide();
        $('#' + confirmaccessedSpan).show();
    } else {
        $('#' + accessedSpan).show();
        $('#' + confirmaccessedSpan).hide();
    }
}