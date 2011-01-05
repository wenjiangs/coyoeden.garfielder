/**
* JS logic for adding media files
*/
Garfielder.AddModule("AddMedia", {
    init: function (opts) {
        this.opts = opts;
        this.$items = $("#media-items");
    }, //init
    onLoad: function () {
        if (this.opts.noFlash) {
            this.$items.css("opacity", 0.3).animate({ opacity: 1 }, 1500);
        }; //if
    } //onload
});