window.bodyScrollLock = {
    lock: function () {
        document.body.classList.add("overflow-hidden");
    },
    unlock: function () {
        document.body.classList.remove("overflow-hidden");
    }
};
