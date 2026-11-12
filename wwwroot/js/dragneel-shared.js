(() => {
  const $ = s => document.querySelector(s), $$ = s => [...document.querySelectorAll(s)];
  const reduce = matchMedia('(prefers-reduced-motion: reduce)').matches;
  const head = $('.dragneel-header'), bar = $('#bar'), up = $('#toTop');

  const io = new IntersectionObserver(es => es.forEach(e => {
    if (e.isIntersecting) { const t = e.target; t.classList.add('in'); io.unobserve(t); setTimeout(() => t.classList.add('done'), 1800 + (parseInt(t.dataset.delay, 10) || 0)); }
  }), { threshold: .15 });
  $$('[data-reveal]').forEach(el => io.observe(el));
  $$('[data-delay]').forEach(el => el.style.setProperty('--d', el.dataset.delay + 'ms'));

  addEventListener('pageshow', e => { if (e.persisted) { document.activeElement?.blur?.(); $$('.dragneel-product-photo').forEach(p => p.style.transform = ''); } });

  if (!reduce && matchMedia('(hover:hover)').matches) {
    $$('.dragneel-product-photo').forEach(ph => {
      ph.addEventListener('mousemove', e => {
        const r = ph.getBoundingClientRect(), x = (e.clientX - r.left) / r.width - .5, y = (e.clientY - r.top) / r.height - .5;
        ph.style.transform = `perspective(800px) rotateY(${(x * 6).toFixed(2)}deg) rotateX(${(-y * 6).toFixed(2)}deg)`;
      });
      ph.addEventListener('mouseleave', () => { ph.style.transform = ''; });
    });
  }


  let tick = false;
  const onScroll = () => {
    const y = scrollY, max = document.documentElement.scrollHeight - innerHeight || 1;
    head?.classList.toggle('scrolled', y > 20);
    if (bar) bar.style.transform = `scaleX(${Math.min(y / max, 1)})`;
    if (up) { up.classList.toggle('show', y > 500); up.style.setProperty('--p', Math.round(Math.min(y / max, 1) * 100)); }
    tick = false;
  };
  addEventListener('scroll', () => { if (!tick) { tick = true; requestAnimationFrame(onScroll); } }, { passive: true });
  onScroll();
  up?.addEventListener('click', () => scrollTo({ top: 0, behavior: reduce ? 'auto' : 'smooth' }));

  const burger = $('.burger');
  burger?.addEventListener('click', () => { const o = head.classList.toggle('open'); burger.setAttribute('aria-expanded', o); });
  $$('.nav a').forEach(a => a.addEventListener('click', () => { head?.classList.remove('open'); burger?.setAttribute('aria-expanded', 'false'); }));

  $$('.sub-toggle').forEach(btn => btn.addEventListener('click', () => {
    const li = btn.closest('.has-sub'), open = li.classList.toggle('sub-open');
    btn.setAttribute('aria-expanded', String(open));
    btn.setAttribute('aria-label', open ? 'Thu gọn danh mục sản phẩm' : 'Mở danh mục sản phẩm');
  }));
  const closeMenu = () => {
    head?.classList.remove('open'); burger?.setAttribute('aria-expanded', 'false');
    $$('.has-sub.sub-open').forEach(h => { h.classList.remove('sub-open'); h.querySelector('.sub-toggle')?.setAttribute('aria-expanded', 'false'); });
  };
  matchMedia('(min-width:901px)').addEventListener('change', e => { if (e.matches) closeMenu(); });
  addEventListener('keydown', e => { if (e.key === 'Escape') closeMenu(); });

  const fab = $('#fab'), fabBtn = $('.fab-main');
  fabBtn?.addEventListener('click', e => { e.stopPropagation(); const o = fab.classList.toggle('open'); fabBtn.setAttribute('aria-expanded', o); });
  document.addEventListener('click', e => { if (fab && !fab.contains(e.target)) fab.classList.remove('open'); });
  addEventListener('keydown', e => { if (e.key === 'Escape') fab?.classList.remove('open'); });

  const fmt = (n, suf) => n.toLocaleString('vi-VN') + (suf || '');
  const cio = new IntersectionObserver(es => es.forEach(e => {
    if (!e.isIntersecting) return;
    const el = e.target, end = Number(el.dataset.count) || 0, suf = el.dataset.suffix, t0 = performance.now(), dur = 1600;
    cio.unobserve(el);
    const step = t => { const k = Math.min((t - t0) / dur, 1), v = 1 - Math.pow(1 - k, 3); el.textContent = fmt(Math.round(end * v), suf); if (k < 1) requestAnimationFrame(step); };
    requestAnimationFrame(step);
  }), { threshold: .4 });
  $$('[data-count]').forEach(el => { if (reduce) el.textContent = fmt(Number(el.dataset.count) || 0, el.dataset.suffix); else cio.observe(el); });

  if (!reduce) addEventListener('pointerdown', e => {
    if (e.button > 0 || document.querySelectorAll('.petal-pop').length > 40) return;
    const n = 9;
    for (let k = 0; k < n; k++) {
      const p = document.createElement('i'); p.className = 'petal-pop'; document.body.append(p);
      const ang = (Math.PI * 2 * k) / n + Math.random() * .6, dist = 36 + Math.random() * 70, dx = Math.cos(ang) * dist, dy = Math.sin(ang) * dist;
      const s = (.7 + Math.random() * .8).toFixed(2), r = (Math.random() * 360) | 0, x = e.clientX - 6, y = e.clientY - 8;
      p.animate([
        { transform: `translate(${x}px,${y}px) rotate(${r}deg) scale(0)`, opacity: 1 },
        { transform: `translate(${x + dx}px,${y + dy}px) rotate(${r + 120}deg) scale(${s})`, opacity: 1, offset: .35 },
        { transform: `translate(${x + dx * 1.2}px,${y + dy + 90}px) rotate(${r + 320}deg) scale(${s})`, opacity: 0 }
      ], { duration: 950 + Math.random() * 500, easing: 'cubic-bezier(.2,.7,.3,1)' }).onfinish = () => p.remove();
    }
  }, { passive: true });
})();

(() => {
  document.querySelectorAll('.flash').forEach(f => {
    setTimeout(() => { f.classList.add('out'); setTimeout(() => f.remove(), 450); }, 4800);
    f.addEventListener('click', () => f.remove());
  });

  document.querySelectorAll('[data-toggle-password]').forEach(b => b.addEventListener('click', () => {
    const input = b.parentElement.querySelector('input'); if (!input) return;
    const show = input.type === 'password';
    input.type = show ? 'text' : 'password';
    b.classList.toggle('on', show);
    b.setAttribute('aria-label', show ? 'Ẩn mật khẩu' : 'Hiện mật khẩu');
  }));

  document.querySelectorAll('input[data-match]').forEach(input => {
    const other = document.querySelector(input.dataset.match); if (!other) return;
    const box = input.closest('.field')?.querySelector('.js-error');
    const check = () => {
      let m = '';
      if (input.value) {
        if (!other.value) m = input.dataset.matchEmpty || 'Vui lòng nhập mật khẩu mới ở ô phía trên trước.';
        else if (input.value !== other.value) m = input.dataset.matchMsg || 'Mật khẩu xác nhận chưa khớp, vui lòng nhập lại cho đúng.';
      }
      input.setCustomValidity(m);
      if (box) box.textContent = m;
      input.classList.toggle('input-validation-error', !!m);
    };
    input.addEventListener('input', check); other.addEventListener('input', check);
  });

  const fieldLabelMsg = el => {
    if (el.type === 'checkbox' || el.tagName === 'SELECT') return '';
    const t = el.closest('.field')?.querySelector('label')?.textContent.trim().toLowerCase();
    return t ? `Vui lòng nhập ${t}.` : '';
  };
  const viMsg = el => {
    const v = el.validity;
    if (v.valueMissing) return el.dataset.msgRequired || fieldLabelMsg(el) || (el.type === 'checkbox' ? 'Vui lòng đánh dấu ô này để tiếp tục.' : el.tagName === 'SELECT' ? 'Vui lòng chọn một mục.' : 'Vui lòng điền thông tin này.');
    if (v.typeMismatch) return el.type === 'email' ? 'Email chưa đúng định dạng.' : 'Giá trị chưa hợp lệ.';
    if (v.patternMismatch) return el.dataset.msgPattern || 'Định dạng chưa hợp lệ.';
    if (v.tooShort) return `Cần tối thiểu ${el.minLength} ký tự.`;
    if (v.tooLong) return `Tối đa ${el.maxLength} ký tự.`;
    if (v.rangeUnderflow) return el.type === 'date' ? 'Vui lòng chọn ngày từ hôm nay trở đi.' : `Giá trị tối thiểu là ${el.min}.`;
    if (v.rangeOverflow) return `Giá trị tối đa là ${el.max}.`;
    if (v.badInput) return 'Giá trị chưa hợp lệ.';
    return '';
  };
  document.addEventListener('invalid', e => {
    const el = e.target;
    if ((el.dataset.match || el.dataset.keepCustom) && el.validity.customError) return;
    el.setCustomValidity('');
    const m = viMsg(el); if (m) el.setCustomValidity(m);
  }, true);
  document.addEventListener('input', e => {
    const el = e.target;
    if (el.willValidate && !el.dataset.match && el.validity.customError && !el.dataset.keepCustom) el.setCustomValidity('');
  }, true);

  const slot = el => {
    if (el.type === 'checkbox' && !el.closest('.field')) {
      const n = el.closest('label')?.nextElementSibling;
      if (n?.classList.contains('field-error')) return n;
    }
    const f = el.closest('.field') || el.parentElement;
    let s = f.querySelector('.js-error') || f.querySelector('.field-error');
    if (!s) { s = document.createElement('span'); s.className = 'field-error'; f.append(s); }
    return s;
  };
  const showErr = (el, m) => {
    const s = slot(el);
    el.closest('.field')?.querySelectorAll('.field-error').forEach(x => { if (x !== s) x.textContent = ''; });
    s.textContent = m || '';
    el.classList.toggle('input-validation-error', !!m);
    el.setAttribute('aria-invalid', m ? 'true' : 'false');
  };
  const fieldMsg = el => el.validity.valid ? '' : (el.validity.customError ? el.validationMessage : viMsg(el));
  const lockable = '.auth-wrap input:not([type=checkbox]):not([type=hidden]), .account-content input[type=password]';
  const unlock = el => { if (el.dataset.locked) { el.readOnly = false; delete el.dataset.locked; } };
  const lockAll = () => document.querySelectorAll(lockable).forEach(el => {
    if (el === document.activeElement || el.readOnly) return;
    el.readOnly = true; el.dataset.locked = '1';
  });
  ['focusin', 'pointerdown', 'touchstart', 'keydown'].forEach(ev => document.addEventListener(ev, e => { if (e.target.matches?.(lockable)) unlock(e.target); }, true));
  document.querySelectorAll(lockable).forEach(el => el.addEventListener('focus', () => unlock(el)));
  lockAll(); addEventListener('pageshow', lockAll);
  const unlockAll = () => document.querySelectorAll(lockable).forEach(unlock);
  const forms = [...document.querySelectorAll('.auth-wrap form, .account-content form')];
  forms.forEach(f => {
    f.noValidate = true;
    const fields = () => [...f.elements].filter(el => el.willValidate && el.type !== 'hidden');
    f.addEventListener('focusout', e => { const el = e.target; if (el.willValidate && !el.dataset.match && el.value !== '') showErr(el, fieldMsg(el)); });
    f.addEventListener('input', e => { const el = e.target; if (el.willValidate && !el.dataset.match && el.classList.contains('input-validation-error')) showErr(el, fieldMsg(el)); });
    f.addEventListener('change', e => { const el = e.target; if (el.type === 'checkbox' && el.classList.contains('input-validation-error')) showErr(el, fieldMsg(el)); });
    f.addEventListener('submit', e => { unlockAll(); if (!window.DragneelForm.validate(f)) e.preventDefault(); });
  });
  window.DragneelForm = {
    validate(form) {
      if (!form.noValidate) return form.checkValidity();
      unlockAll();
      let first = null;
      [...form.elements].filter(el => el.willValidate && el.type !== 'hidden').forEach(el => {
        const m = fieldMsg(el); showErr(el, m); if (m && !first) first = el;
      });
      if (first) first.focus();
      return !first;
    }
  };

  document.addEventListener('click', e => {
    const b = e.target.closest('[data-qty]'); if (!b) return;
    const input = b.closest('.qty')?.querySelector('input'); if (!input) return;
    const min = Number(input.min) || 1, max = Number(input.max) || 99;
    const v = Math.min(max, Math.max(min, (parseInt(input.value, 10) || min) + Number(b.dataset.qty)));
    if (String(v) !== input.value) { input.value = v; input.dispatchEvent(new Event('input', { bubbles: true })); input.dispatchEvent(new Event('change', { bubbles: true })); }
  });
})();

window.DragneelUI = {
  flash(msg, type) {
    document.querySelectorAll('.flash.js').forEach(x => x.remove());
    const f = document.createElement('div');
    f.className = 'flash js' + (type === 'error' ? ' error' : ''); f.setAttribute('role', 'status'); f.textContent = msg;
    document.body.append(f);
    setTimeout(() => { f.classList.add('out'); setTimeout(() => f.remove(), 450); }, 3600);
    f.addEventListener('click', () => f.remove());
  }
};
window.DragneelCart = (() => {
  const reduce = matchMedia('(prefers-reduced-motion: reduce)').matches;
  const link = () => document.querySelector('.dragneel-cart');
  const bump = () => { const a = link(); if (!a) return; a.classList.remove('bump'); void a.offsetWidth; a.classList.add('bump'); };
  const setCount = n => {
    document.querySelectorAll('[data-cart-count]').forEach(e => e.textContent = String(n));
    document.querySelectorAll('[data-cart-badge]').forEach(b => { b.textContent = String(n); b.hidden = !n; });
    const a = link(); if (a) a.setAttribute('aria-label', `Giỏ thuê, ${n} sản phẩm`);
  };
  const fly = (srcEl, done) => {
    const target = link()?.querySelector('.ico');
    if (!srcEl || !target || reduce || !document.body.animate) { done?.(); bump(); return; }
    const a = srcEl.getBoundingClientRect(), b = target.getBoundingClientRect();
    const w = Math.min(120, a.width), h = Math.min(160, a.height);
    const el = document.createElement('div'); el.className = 'cart-fly';
    const src = srcEl.currentSrc || srcEl.src; if (src) el.style.backgroundImage = `url("${src}")`;
    el.style.width = w + 'px'; el.style.height = h + 'px'; document.body.append(el);
    const x0 = a.left + a.width / 2 - w / 2, y0 = a.top + a.height / 2 - h / 2;
    const x1 = b.left + b.width / 2 - w / 2, y1 = b.top + b.height / 2 - h / 2;
    const anim = el.animate([
      { transform: `translate(${x0}px,${y0}px) scale(1) rotate(0)`, opacity: 1, borderRadius: '18px' },
      { transform: `translate(${x0 + (x1 - x0) * .45}px,${Math.min(y0, y1) - 90}px) scale(.55) rotate(-8deg)`, opacity: 1, offset: .5, borderRadius: '24px' },
      { transform: `translate(${x1}px,${y1}px) scale(.08) rotate(0)`, opacity: .55, borderRadius: '50%' }
    ], { duration: 900, easing: 'cubic-bezier(.5,0,.25,1)' });
    anim.onfinish = () => { el.remove(); done?.(); bump(); };
  };
  return { setCount, bump, fly };
})();

(() => {
  const KEY = 'dragneel:goi-y', MAX = 5;
  const read = () => { try { const v = JSON.parse(localStorage.getItem(KEY) || '{}'); return v && typeof v === 'object' ? v : {}; } catch { return {}; } };
  const write = v => { try { localStorage.setItem(KEY, JSON.stringify(v)); } catch {} };
  const valid = (k, s) => k === 'sdt' ? /^(\+84|0)\d{8,10}$/.test(s.replace(/[\s.\-]/g, '')) : s.length >= 2;
  const save = (k, s) => {
    s = s.trim();
    if (!valid(k, s)) return;
    const all = read();
    all[k] = [s, ...(all[k] || []).filter(x => x !== s)].slice(0, MAX);
    write(all);
  };
  document.querySelectorAll('input[data-suggest]').forEach(inp => {
    const k = inp.dataset.suggest, wrap = inp.closest('.field') || inp.parentElement;
    const box = document.createElement('div');
    box.className = 'sg-list'; box.hidden = true; box.setAttribute('role', 'listbox');
    wrap.append(box);
    const items = () => {
      const seed = (inp.dataset.suggestSeed || '').trim();
      const list = [...(read()[k] || [])];
      if (seed && k === 'ten' && !list.includes(seed)) list.push(seed);
      return list.filter(x => x.toLowerCase() !== inp.value.trim().toLowerCase());
    };
    const render = () => {
      const list = items();
      box.replaceChildren(...list.map(v => {
        const b = document.createElement('button');
        b.type = 'button'; b.className = 'sg-item'; b.setAttribute('role', 'option'); b.textContent = v;
        b.addEventListener('mousedown', e => e.preventDefault());
        b.addEventListener('click', () => { inp.value = v; inp.dispatchEvent(new Event('input', { bubbles: true })); inp.dispatchEvent(new Event('change', { bubbles: true })); box.hidden = true; });
        return b;
      }));
      if (list.length) { const h = document.createElement('small'); h.className = 'sg-hint'; h.textContent = 'Đã lưu trên thiết bị này, bấm để chọn'; box.prepend(h); }
      box.hidden = list.length === 0;
    };
    inp.addEventListener('focus', render);
    inp.addEventListener('input', render);
    inp.addEventListener('blur', () => setTimeout(() => { box.hidden = true; }, 120));
    inp.addEventListener('change', () => save(k, inp.value));
    inp.form?.addEventListener('submit', () => save(k, inp.value));
    inp.addEventListener('keydown', e => { if (e.key === 'Escape') box.hidden = true; });
  });
})();
