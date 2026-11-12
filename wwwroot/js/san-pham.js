(() => {
  const grid = document.querySelector('[data-catalog]');
  if (!grid) return;
  const $ = (s, r = document) => r.querySelector(s), $$ = (s, r = document) => [...r.querySelectorAll(s)];
  const PAGE = 12; let limit = PAGE;
  const cards = () => $$('[data-product-card]', grid);
  const more = $('[data-load-more]'), reset = $('[data-reset-filters]'), countEl = $('[data-result-count]');
  const emptyBox = $('[data-empty-catalog]'), emptyMsg = emptyBox?.querySelector('p');
  const qIn = $('[data-filter="search"]'), sizeSel = $('[data-filter="size"]');
  const hform = $('.dragneel-header .search'), hin = hform?.querySelector('input[name=q]');
  const box = $('[data-date-filter]'), from = $('[data-date-from]'), to = $('[data-date-to]'), msg = $('[data-date-msg]'), clr = $('[data-date-clear]');
  const dates = { active: false };

  const renderSizes = c => {
    const info = c.querySelector('.dragneel-product-info'); if (!info) return;
    info.querySelector('.dragneel-sizes')?.remove();
    const sizes = (c.dataset.size || '').split(' ').filter(Boolean); if (!sizes.length) return;
    const b = document.createElement('div'); b.className = 'dragneel-sizes';
    sizes.forEach(s => { const i = document.createElement('i'); i.textContent = s; b.append(i); });
    info.append(b);
  };
  cards().forEach(c => { c.dataset.sizeAll = c.dataset.size || ''; renderSizes(c); });

  const visible = () => cards().filter(c => !c.hidden && c.dataset.free !== '0');
  const dangLoc = () => (($('[data-filter="category"]:checked')?.value || '') !== '') || !!sizeSel?.value ||
    !!$('[data-filter="price"]')?.value || !!qIn?.value.trim() || dates.active || !!(from?.value || to?.value);
  const fold = () => {
    const vis = visible();
    cards().forEach(c => c.classList.remove('is-folded'));
    vis.forEach((c, i) => { c.classList.toggle('is-folded', i >= limit); c.style.setProperty('--i', i % PAGE); });
    if (countEl && countEl.textContent !== String(vis.length)) {
      countEl.textContent = vis.length; countEl.classList.remove('bump'); void countEl.offsetWidth; countEl.classList.add('bump');
    }
    const rest = Math.max(0, vis.length - limit);
    if (more) { more.hidden = rest === 0; const n = $('[data-more-n]', more); if (n) n.textContent = Math.min(rest, PAGE); }
    if (reset) reset.hidden = !dangLoc();
    if (emptyBox) {
      emptyBox.hidden = vis.length > 0;
      if (emptyMsg) emptyMsg.textContent = dates.active ? 'Không có mẫu nào còn trống trong khoảng ngày này.' : 'Không có sản phẩm nào phù hợp bộ lọc.';
    }
  };
  const sau = () => { limit = PAGE; setTimeout(fold, 0); };
  const relayout = () => { ['input', 'change'].forEach(ev => (qIn || sizeSel)?.dispatchEvent(new Event(ev, { bubbles: true }))); };
  $$('[data-filter]').forEach(x => { x.addEventListener('input', sau); x.addEventListener('change', sau); });
  $('[data-sort]')?.addEventListener('change', () => setTimeout(fold, 0));
  more?.addEventListener('click', () => { limit += PAGE; fold(); });

  const setParam = (k, v) => { const u = new URL(location.href); v ? u.searchParams.set(k, v) : u.searchParams.delete(k); history.replaceState(null, '', u); };

  hform?.addEventListener('submit', e => {
    e.preventDefault();
    const v = hin.value.trim();
    if (qIn) { qIn.value = v; qIn.dispatchEvent(new Event('input', { bubbles: true })); }
    setParam('q', v);
    $('.dragneel-chips')?.scrollIntoView({ behavior: 'smooth', block: 'start' });
  });

  const addDays = (v, n) => { const d = new Date(v + 'T00:00:00'); d.setDate(d.getDate() + n); return d.toLocaleDateString('sv-SE'); };
  const diff = (a, b) => Math.round((new Date(b + 'T00:00:00') - new Date(a + 'T00:00:00')) / 864e5);
  const vn = v => v.split('-').reverse().join('/');
  let seq = 0;
  const setMsg = (t, bad) => { if (!msg) return; msg.hidden = !t; msg.textContent = t || ''; msg.classList.toggle('bad', !!bad); };
  const clearAvail = () => { dates.active = false; box?.classList.remove('is-on'); cards().forEach(c => { c.dataset.free = '1'; c.dataset.size = c.dataset.sizeAll; renderSizes(c); }); };
  const applyAvail = list => {
    const map = new Map(list.map(x => [x.ten.toLowerCase(), new Set(x.size)]));
    dates.active = true; box.classList.add('is-on');
    cards().forEach(c => {
      const set = map.get(c.dataset.name);
      if (!set) c.dataset.free = '0';
      else { c.dataset.free = '1'; c.dataset.size = c.dataset.sizeAll.split(' ').filter(s => set.has(s)).join(' '); }
      renderSizes(c);
    });
  };
  const validate = () => {
    const today = box.dataset.today, max = +box.dataset.max;
    from.min = today; to.min = addDays(from.value && from.value >= today ? from.value : today, 1); to.max = from.value ? addDays(from.value, max) : '';
    if (!from.value && !to.value) return { empty: true };
    if (from.value && from.value < today) return { err: 'Ngày nhận không được ở quá khứ.' };
    if (to.value && to.value <= today) return { err: 'Ngày trả phải sau hôm nay.' };
    if (from.value && !to.value) return { hint: 'Chọn thêm ngày trả để xem mẫu còn trống.' };
    if (!from.value && to.value) return { hint: 'Chọn thêm ngày nhận để xem mẫu còn trống.' };
    if (to.value <= from.value) return { err: 'Ngày trả phải sau ngày nhận.' };
    if (diff(from.value, to.value) > max) return { err: `Thuê tối đa ${max} ngày.` };
    return { ok: true };
  };
  const apply = async () => {
    const v = validate(), my = ++seq;
    clr.hidden = !(from.value || to.value);
    setParam('tu', v.ok ? from.value : ''); setParam('den', v.ok ? to.value : '');
    if (!v.ok) { clearAvail(); setMsg(v.empty ? '' : (v.err || v.hint), !!v.err); relayout(); return; }
    grid.classList.add('is-loading'); setMsg('');
    try {
      const res = await fetch(`${box.dataset.url}?tu=${from.value}&den=${to.value}`, { credentials: 'same-origin' });
      const data = await res.json().catch(() => null);
      if (my !== seq) return;
      if (!res.ok || !Array.isArray(data)) { clearAvail(); setMsg(data?.loi || 'Chưa kiểm tra được ngày thuê, vui lòng thử lại.', true); }
      else { applyAvail(data); setMsg(`Đang hiện mẫu còn trống từ ${vn(from.value)} đến ${vn(to.value)} (${diff(from.value, to.value)} ngày).`); }
    } catch { if (my === seq) { clearAvail(); setMsg('Không kết nối được máy chủ, vui lòng thử lại.', true); } }
    finally { if (my === seq) { grid.classList.remove('is-loading'); relayout(); } }
  };
  if (box && from && to) {
    [from, to].forEach(x => x.addEventListener('change', apply));
    clr?.addEventListener('click', () => { from.value = to.value = ''; apply(); });
    const p = new URLSearchParams(location.search);
    if (p.get('tu') && p.get('den')) { from.value = p.get('tu'); to.value = p.get('den'); apply(); }
  }

  const clearAll = () => {
    if (hin) hin.value = ''; if (qIn) qIn.value = '';
    if (from) from.value = ''; if (to) to.value = '';
    ['q', 'tu', 'den', 'category'].forEach(k => setParam(k, ''));
    clearAvail(); setMsg(''); clr && (clr.hidden = true);
    $$('.dragneel-select select').forEach(s => s.dispatchEvent(new Event('dg-sync')));
    limit = PAGE; setTimeout(() => { relayout(); fold(); }, 0);
  };
  reset?.addEventListener('click', clearAll);
  $('[data-empty-reset]')?.addEventListener('click', () => { reset?.click(); clearAll(); });

  const CARET = '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.4" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="m6 9 6 6 6-6"/></svg>';
  const menus = [];
  const enhance = sel => {
    const wrap = sel.closest('.dragneel-select'); if (!wrap) return;
    wrap.classList.add('is-enhanced');
    const btn = document.createElement('button'); btn.type = 'button'; btn.className = 'dd-btn';
    btn.setAttribute('aria-haspopup', 'listbox'); btn.setAttribute('aria-expanded', 'false');
    const val = document.createElement('span'); btn.append(val); btn.insertAdjacentHTML('beforeend', CARET);
    const ul = document.createElement('ul'); ul.className = 'dd-list'; ul.setAttribute('role', 'listbox'); ul.tabIndex = -1;
    const items = [...sel.options].map((o, k) => {
      const li = document.createElement('li'); li.setAttribute('role', 'option'); li.textContent = o.textContent; li.dataset.v = o.value; li.style.setProperty('--k', k); ul.append(li); return li;
    });
    wrap.append(btn, ul);
    let act = 0;
    const sync = () => { const cur = sel.selectedIndex < 0 ? 0 : sel.selectedIndex; val.textContent = sel.options[cur]?.textContent || ''; items.forEach((li, k) => li.setAttribute('aria-selected', String(k === cur))); act = cur; };
    const mark = () => items.forEach((li, k) => { li.classList.toggle('is-act', k === act); if (k === act) li.scrollIntoView({ block: 'nearest' }); });
    const close = () => { wrap.classList.remove('is-open'); btn.setAttribute('aria-expanded', 'false'); };
    const open = () => {
      menus.forEach(m => m.close()); sync(); mark();
      ul.classList.toggle('align-right', wrap.getBoundingClientRect().left + Math.max(ul.offsetWidth, 190) > innerWidth - 12);
      wrap.classList.add('is-open'); btn.setAttribute('aria-expanded', 'true');
    };
    const pick = k => { sel.selectedIndex = k; sel.dispatchEvent(new Event('input', { bubbles: true })); sel.dispatchEvent(new Event('change', { bubbles: true })); sync(); close(); btn.focus(); };
    btn.addEventListener('click', () => wrap.classList.contains('is-open') ? close() : open());
    ul.addEventListener('click', e => { const li = e.target.closest('li'); if (li) pick(items.indexOf(li)); });
    ul.addEventListener('mousemove', e => { const li = e.target.closest('li'); if (li) { act = items.indexOf(li); mark(); } });
    btn.addEventListener('keydown', e => {
      const o = wrap.classList.contains('is-open');
      if (['ArrowDown', 'ArrowUp', 'Enter', ' '].includes(e.key)) {
        e.preventDefault();
        if (!o) { open(); return; }
        if (e.key === 'ArrowDown') act = Math.min(items.length - 1, act + 1);
        else if (e.key === 'ArrowUp') act = Math.max(0, act - 1);
        else { pick(act); return; }
        mark();
      } else if (e.key === 'Escape' && o) { e.preventDefault(); close(); }
      else if (e.key === 'Home' && o) { act = 0; mark(); } else if (e.key === 'End' && o) { act = items.length - 1; mark(); }
    });
    sel.addEventListener('change', sync); sel.addEventListener('dg-sync', sync);
    menus.push({ close }); sync();
  };
  $$('.dragneel-select select').forEach(enhance);
  document.addEventListener('click', e => { if (!e.target.closest('.dragneel-select')) menus.forEach(m => m.close()); });
  addEventListener('keydown', e => { if (e.key === 'Escape') menus.forEach(m => m.close()); });

  setTimeout(fold, 0);
})();
