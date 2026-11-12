(() => {
  const form = document.querySelector('[data-add-cart]');
  if (!form) return;
  const group = form.closest('.detail-copy')?.querySelector('[data-size-group]');
  const sizes = group ? [...group.querySelectorAll('button')] : [];
  if (sizes.length === 1) sizes[0].click();
  const nhan = form.querySelector('[name=NgayNhan]'), tra = form.querySelector('[name=NgayTra]');
  const today = form.dataset.today, goi = +form.dataset.goi, max = +form.dataset.max, gia = +form.dataset.gia, them = +form.dataset.them;
  const calc = form.querySelector('[data-rent-calc]');
  const addDays = (v, n) => { const d = new Date(v + 'T00:00:00'); d.setDate(d.getDate() + n); return d.toLocaleDateString('sv-SE'); };
  const diff = (a, b) => Math.round((new Date(b + 'T00:00:00') - new Date(a + 'T00:00:00')) / 864e5);
  const money = n => Math.round(n).toLocaleString('vi-VN') + '₫';
  const checkDates = () => {
    nhan.dataset.keepCustom = tra.dataset.keepCustom = '1';
    nhan.min = today;
    tra.min = addDays(nhan.value && nhan.value >= today ? nhan.value : today, 1);
    tra.max = nhan.value ? addDays(nhan.value, max) : '';
    let mn = '', mt = '';
    if (nhan.value && nhan.value < today) mn = 'Ngày nhận không được ở quá khứ.';
    if (tra.value && tra.value <= today) mt = 'Ngày trả không được ở quá khứ.';
    else if (nhan.value && tra.value && tra.value <= nhan.value) mt = 'Ngày trả phải sau ngày nhận.';
    else if (nhan.value && tra.value && diff(nhan.value, tra.value) > max) mt = `Thuê tối đa ${max} ngày (gói ${goi} ngày + ${max - goi} ngày thêm). Vui lòng chọn ngày trả sớm hơn.`;
    nhan.setCustomValidity(mn); tra.setCustomValidity(mt);
    const ok = nhan.value && tra.value && !mn && !mt;
    calc.hidden = !(ok || mn || mt);
    if (mn || mt) { calc.textContent = mn || mt; calc.classList.add('bad'); }
    else if (ok) {
      const days = diff(nhan.value, tra.value), extra = Math.max(0, days - goi), q = Math.max(1, parseInt(form.querySelector('[name=SoLuong]').value, 10) || 1);
      calc.classList.remove('bad');
      calc.textContent = `Thuê ${days} ngày: gói ${goi} ngày` + (extra ? ` + ${extra} ngày thêm (+${money(extra * them)}/bộ)` : ' (không phát sinh thêm)') + ` → ${money(q * (gia + extra * them))}` + (q > 1 ? ` cho ${q} bộ.` : '.');
    }
  };
  ['input', 'change'].forEach(ev => form.addEventListener(ev, e => { if (e.target.matches('[name=NgayNhan],[name=NgayTra],[name=SoLuong]')) checkDates(); }));
  checkDates();

  const maInput = form.querySelector('[name=MaSanPham]'), qtyInput = form.querySelector('[name=SoLuong]');
  const vnDate = v => v.split('-').reverse().join('/');
  let lastShown = '', seq = 0, timer;
  const thongBao = (title, message) => window.DragneelConfirm ? DragneelConfirm({ title, message, ok: 'OK', single: true }) : Promise.resolve(alert(message));
  const kiemTra = async (show, force) => {
    const size = group?.querySelector('.is-active')?.textContent.trim() || '';
    const q = Math.max(1, parseInt(qtyInput.value, 10) || 1);
    if (!form.dataset.checkUrl || !nhan.value || !tra.value || nhan.validationMessage || tra.validationMessage || (group && !size)) return true;
    const my = ++seq, key = [size, nhan.value, tra.value, q].join('|');
    let data = null;
    try {
      const res = await fetch(`${form.dataset.checkUrl}?maSanPham=${encodeURIComponent(maInput.value)}&kichThuoc=${encodeURIComponent(size)}&tu=${nhan.value}&den=${tra.value}`, { credentials: 'same-origin' });
      data = res.ok ? await res.json() : null;
    } catch { data = null; }
    if (my !== seq || !data) return true;
    const ok = data.conLai >= q;
    if (!ok && show && (force || key !== lastShown)) {
      lastShown = key;
      const ten = `${data.ten}${size ? ' (size ' + size + ')' : ''}`;
      await thongBao(data.conLai < 0 ? 'Vượt thời gian thuê tối đa' : 'Đã có người thuê',
        data.conLai < 0 ? 'Thời gian thuê vượt mức tối đa của mẫu này. Vui lòng chọn ngày trả sớm hơn.'
        : data.conLai === 0 ? `${ten} đã có người thuê trong khoảng ${vnDate(nhan.value)} – ${vnDate(tra.value)}. Vui lòng chọn ngày khác hoặc size khác.`
        : `${ten} chỉ còn ${data.conLai} bộ trong khoảng ${vnDate(nhan.value)} – ${vnDate(tra.value)}. Vui lòng giảm số lượng hoặc chọn ngày khác.`);
    }
    return ok;
  };
  const hen = () => { clearTimeout(timer); timer = setTimeout(() => kiemTra(true, false), 350); };
  [nhan, tra, qtyInput].forEach(x => x.addEventListener('change', hen));
  sizes.forEach(b => b.addEventListener('click', () => setTimeout(hen, 0)));

  form.addEventListener('submit', async e => {
    e.preventDefault();
    if (form.dataset.auth !== '1') { location.href = form.dataset.loginUrl; return; }
    const size = group?.querySelector('.is-active')?.textContent.trim();
    if (group && !size) {
      DragneelUI.flash('Vui lòng chọn kích thước trước khi thêm vào giỏ thuê.', 'error');
      group.classList.remove('shake'); void group.offsetWidth; group.classList.add('shake');
      return;
    }
    if (!(await kiemTra(true, true))) return;
    const btn = form.querySelector('[data-add-btn]'); btn.disabled = true;
    const fd = new FormData(form); if (size) fd.set('KichThuoc', size);
    try {
      const res = await fetch(form.action, { method: 'POST', body: fd, credentials: 'same-origin' });
      if (res.redirected) { location.href = res.url; return; }
      const ct = res.headers.get('content-type') || '';
      const data = ct.includes('json') ? await res.json() : null;
      if (!res.ok || !data) {
        if (res.status === 401 || res.status === 403) { location.href = form.dataset.loginUrl; return; }
        DragneelUI.flash(data?.loi || 'Chưa thêm được vào giỏ thuê, vui lòng thử lại.', 'error'); return;
      }
      const img = document.querySelector('.gallery-main');
      DragneelCart.fly(img, () => DragneelCart.setCount(data.total));
      DragneelUI.flash(`Đã thêm “${data.ten}” (size ${size || 'Freesize'}) vào giỏ thuê`);
    } catch {
      DragneelUI.flash('Không kết nối được máy chủ, vui lòng thử lại.', 'error');
    } finally { btn.disabled = false; }
  });
})();
