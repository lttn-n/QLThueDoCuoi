(() => {
  const $ = s => document.querySelector(s), $$ = s => [...document.querySelectorAll(s)];
  const reduce = matchMedia('(prefers-reduced-motion: reduce)').matches;

  const par = $$('[data-parallax]');
  if (par.length && !reduce) {
    let tick = false;
    const run = () => { par.forEach(p => { const r = p.parentElement.getBoundingClientRect(); p.style.transform = `translateY(${(r.top * -.07).toFixed(1)}px) scale(1.12)`; }); tick = false; };
    addEventListener('scroll', () => { if (!tick) { tick = true; requestAnimationFrame(run); } }, { passive: true });
    run();
  }

  const img = $('#catImg');
  if (img) $$('.dragneel-category-item').forEach(a => a.addEventListener('mouseenter', () => {
    if (!a.dataset.img) return;
    img.src = a.dataset.img; img.classList.remove('pop'); void img.offsetWidth; img.classList.add('pop');
  }));

  const pet = $('.petals');
  if (pet && !reduce) for (let k = 0; k < (innerWidth < 700 ? 6 : 12); k++) {
    const p = document.createElement('i'); p.className = 'petal';
    p.style.cssText = `--x:${Math.random() * 100}%;--s:${(.6 + Math.random() * .9).toFixed(2)};--t:${(9 + Math.random() * 8).toFixed(1)}s;--dl:-${(Math.random() * 12).toFixed(1)}s;--dx:${(Math.random() * 160 - 80) | 0}px`;
    pet.append(p);
  }
})();
