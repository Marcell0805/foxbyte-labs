window.foxbyteNav = (function () {
  let dotNet = null;
  let onScroll = null;
  let last = null;

  function update() {
    const scrolled = window.scrollY > 12;
    if (scrolled === last) return;
    last = scrolled;
    if (dotNet) dotNet.invokeMethodAsync('OnScrollChanged', scrolled);
  }

  return {
    init: function (ref) {
      dotNet = ref;
      onScroll = function () { update(); };
      window.addEventListener('scroll', onScroll, { passive: true });
      update();
    },
    dispose: function () {
      if (onScroll) window.removeEventListener('scroll', onScroll);
      dotNet = null;
      onScroll = null;
      last = null;
    }
  };
})();
