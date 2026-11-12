(() => {
  const form = document.querySelector('[data-cart-form]');
  if (!form) return;
  const lines = [...form.querySelectorAll('[data-line]')];
  const bar = form.querySelector('[data-save-bar]');
  const money = n => Math.round(n).toLocaleString('vi-VN') + '₫';
  const fmtDate = v => v ? v.split('-').reverse().join('/') : '';
  const dayNum = v => v ? Date.UTC(...v.split('-').map((x, i) => i === 1 ? x - 1 : +x)) / 864e5 : null;
  const today = form.dataset.today;

  const calc = line => {
    const q = Math.min(10, Math.max(1, parseInt(line.querySelector('input[name$=".SoLuong"]').value, 10) || 1));
    const nhan = line.querySelector('[data-nhan]'), tra = line.querySelector('[data-tra]');
    const gia = +line.dataset.gia, goi = +line.dataset.goi, them = +line.dataset.them, coc = +line.dataset.coc;
    const max = +line.dataset.max || goi + 7;
    const addDays = (v, n) => new Date((dayNum(v) + n) * 864e5).toISOString().slice(0, 10);
    nhan.min = today;
    tra.min = addDays(nhan.value && nhan.value >= today ? nhan.value : today, 1);
    tra.max = nhan.value ? addDays(nhan.value, max) : '';
    tra.dataset.keepCustom = '1'; nhan.dataset.keepCustom = '1';
    nhan.setCustomValidity(nhan.value && nhan.value < today ? 'Ngày nhận không được ở quá khứ.'
      : !nhan.value !== !tra.value ? 'Vui lòng chọn đủ ngày nhận và ngày trả.' : '');
    tra.setCustomValidity(tra.value && tra.value <= today ? 'Ngày trả không được ở quá khứ.'
      : nhan.value && tra.value && dayNum(tra.value) <= dayNum(nhan.value) ? 'Ngày trả phải sau ngày nhận.'
      : nhan.value && tra.value && dayNum(tra.value) - dayNum(nhan.value) > max ? `Thuê tối đa ${max} ngày (gói ${goi} ngày + ${max - goi} ngày thêm).` : '');
    const days = nhan.value && tra.value ? dayNum(tra.value) - dayNum(nhan.value) : null;
    const thue = q * (gia + (days != null && days > 0 ? Math.max(0, days - goi) * them : 0));
    line.querySelector('[data-v-price]').textContent = money(thue);
    line.querySelector('[data-v-coc]').textContent = 'Cọc ' + money(coc * q);
    line.querySelector('[data-v-dates]').textContent = days != null && days > 0
      ? `${fmtDate(nhan.value)} → ${fmtDate(tra.value)} · ${days} ngày` + (days > goi ? ` (gói ${goi} + ${days - goi} ngày thêm: +${money(q * (days - goi) * them)})` : '')
      : `Chưa chọn ngày · tạm tính theo gói ${goi} ngày`;
    const sel = line.querySelector('[data-v-sizeselect]');
    line.querySelector('[data-v-size]').textContent = sel.options[sel.selectedIndex]?.text || '';
    return { q, thue, coc: coc * q };
  };

  const dirty = () => [...form.querySelectorAll('[data-line] input:not([type=hidden]), [data-line] select')].some(el =>
    el.tagName === 'SELECT' ? [...el.options].some(o => o.selected !== o.defaultSelected) : el.value !== el.defaultValue);

  const refresh = () => {
    let q = 0, thue = 0, coc = 0;
    lines.forEach(l => { const r = calc(l); q += r.q; thue += r.thue; coc += r.coc; });
    const set = (sel, v) => document.querySelectorAll(sel).forEach(e => e.textContent = v);
    set('[data-sum-count]', q); set('[data-sum-thue]', money(thue)); set('[data-sum-coc]', money(coc)); set('[data-sum-total]', money(thue + coc));
    bar.hidden = !dirty();
    document.querySelectorAll('[data-checkout]').forEach(a => a.classList.toggle('is-disabled', !bar.hidden));
  };

  form.addEventListener('input', refresh);
  form.addEventListener('change', refresh);
  form.querySelectorAll('[data-edit-toggle]').forEach(b => b.addEventListener('click', () => {
    const line = b.closest('.cart-line'), on = line.classList.toggle('editing');
    b.textContent = on ? 'Đóng' : 'Chỉnh sửa'; b.setAttribute('aria-expanded', String(on));
  }));
  form.addEventListener('invalid', e => {
    const line = e.target.closest('.cart-line');
    if (line && !line.classList.contains('editing')) { line.classList.add('editing'); const b = line.querySelector('[data-edit-toggle]'); if (b) { b.textContent = 'Đóng'; b.setAttribute('aria-expanded', 'true'); } }
  }, true);
  const picks = [...form.querySelectorAll('[data-pick]')], pickAll = form.querySelector('[data-pick-all]');
  const delSel = form.querySelector('[data-delete-selected]');
  const syncPick = () => {
    const on = picks.filter(p => p.checked);
    form.querySelector('[data-pick-count]').textContent = on.length;
    delSel.disabled = on.length === 0;
    delSel.dataset.confirm = on.length === 1
      ? `Bạn có chắc muốn xóa “${on[0].closest('.cart-line').querySelector('h3').textContent.trim()}” khỏi giỏ thuê không?`
      : `${on.length} sản phẩm đã chọn sẽ bị xóa khỏi giỏ thuê. Bạn có chắc không?`;
    pickAll.checked = on.length === picks.length; pickAll.indeterminate = on.length > 0 && on.length < picks.length;
    picks.forEach(p => p.closest('.cart-line').classList.toggle('picked', p.checked));
  };
  picks.forEach(p => p.addEventListener('change', syncPick));
  pickAll.addEventListener('change', () => { picks.forEach(p => p.checked = pickAll.checked); syncPick(); });
  syncPick();
  form.querySelector('[data-reset]')?.addEventListener('click', () => { form.reset(); setTimeout(() => { refresh(); syncPick(); }, 0); });
  document.querySelectorAll('[data-checkout]').forEach(a => a.addEventListener('click', e => {
    if (!bar.hidden) { e.preventDefault(); window.DragneelUI?.flash('Bạn cần lưu thay đổi giỏ thuê trước khi đặt thuê.', 'error'); }
  }));
  refresh();
})();
