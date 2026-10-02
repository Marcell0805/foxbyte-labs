window.foxbyteNav = (function () {
  let dotNet = null;
  let onScroll = null;
  let last = null;

  function onKey(event) {
    if ((event.ctrlKey || event.metaKey) && event.key.toLowerCase() === 'k') {
      event.preventDefault();
      const input = document.getElementById('hero-search');
      if (input) input.focus();
    }
  }

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
      window.addEventListener('keydown', onKey);
      update();
    },
    focusSearch: function () {
      const input = document.getElementById('hero-search');
      if (!input) return;
      input.scrollIntoView({ behavior: 'smooth', block: 'center' });
      input.focus();
    },
    dispose: function () {
      if (onScroll) window.removeEventListener('scroll', onScroll);
      window.removeEventListener('keydown', onKey);
      dotNet = null;
      onScroll = null;
      last = null;
    }
  };
})();
