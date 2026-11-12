(() => {
  const AUTO = [
    [/^\s*xóa/i, { title: 'Xác nhận xóa', message: 'Bạn có chắc muốn xóa không? Thao tác này không thể hoàn tác.', ok: 'Xóa', tone: 'danger' }],
    [/^\s*(lưu|cập nhật)/i, { title: 'Xác nhận lưu thay đổi', message: 'Bạn có muốn lưu các thay đổi này không?', ok: 'Lưu thay đổi', tone: 'primary' }],
    [/^\s*đăng xuất/i, { title: 'Đăng xuất', message: 'Bạn muốn đăng xuất khỏi tài khoản?', ok: 'Đăng xuất', tone: 'primary' }]
  ];
  const ICON = {
    danger: '<svg viewBox="0 0 24 24" width="26" height="26" fill="none" stroke="currentColor" stroke-width="1.7" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M4 7h16M10 11v6M14 11v6M6 7l1 13h10l1-13M9 7V4h6v3"/></svg>',
    primary: '<svg viewBox="0 0 24 24" width="26" height="26" fill="none" stroke="currentColor" stroke-width="1.7" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><circle cx="12" cy="12" r="9"/><path d="M12 7.5v5M12 16v.01"/></svg>'
  };
  let dlg, busy = false;
  const build = () => {
    dlg = document.createElement('dialog'); dlg.className = 'dg-confirm';
    dlg.innerHTML = '<div class="dg-box"><div class="dg-ico"></div><h2 class="dg-title"></h2><p class="dg-msg"></p><div class="dg-actions"><button type="button" class="dg-btn" data-no></button><button type="button" class="dg-btn dg-yes" data-yes></button></div></div>';
    document.body.append(dlg);
  };
  const ask = (o = {}) => new Promise(resolve => {
    if (!dlg) build();
    const tone = o.tone === 'danger' ? 'danger' : 'primary';
    dlg.dataset.tone = tone;
    dlg.querySelector('.dg-ico').innerHTML = ICON[tone];
    dlg.querySelector('.dg-title').textContent = o.title || 'Xác nhận';
    dlg.querySelector('.dg-msg').textContent = o.message || 'Bạn có chắc chắn không?';
    const yes = dlg.querySelector('[data-yes]'), no = dlg.querySelector('[data-no]');
    yes.textContent = o.ok || 'Đồng ý'; no.textContent = o.cancel || 'Hủy'; no.hidden = !!o.single;
    let done = false;
    const fin = v => { if (done) return; done = true; dlg.close(); resolve(v); };
    yes.onclick = () => fin(true); no.onclick = () => fin(false);
    dlg.oncancel = e => { e.preventDefault(); fin(false); };
    dlg.onclick = e => { if (e.target === dlg) fin(false); };
    dlg.showModal();
    (tone === 'danger' && !o.single ? no : yes).focus();
  });
  window.DragneelConfirm = ask;

  const cfgOf = el => {
    if (el.dataset.confirm !== undefined) {
      const auto = AUTO.find(([re]) => re.test(el.textContent || el.value || ''))?.[1] || {};
      return { title: el.dataset.confirmTitle || auto.title, message: el.dataset.confirm || auto.message, ok: el.dataset.confirmOk || auto.ok, tone: el.dataset.confirmTone || auto.tone };
    }
    if (el.dataset.noConfirm !== undefined || el.disabled) return null;
    const isSubmit = el.matches('input[type=submit],button[type=submit],button:not([type])') && el.form;
    if (!isSubmit) return null;
    return AUTO.find(([re]) => re.test(el.textContent || el.value || ''))?.[1] || null;
  };

  document.addEventListener('click', async e => {
    const el = e.target.closest('button,a,input[type=submit]');
    if (!el || el.dataset.confirmed === '1' || busy) return;
    const cfg = cfgOf(el); if (!cfg) return;
    if (el.form && !el.hasAttribute('formnovalidate') && el.matches('[type=submit],button:not([type])') && !(window.DragneelForm ? window.DragneelForm.validate(el.form) : el.form.checkValidity())) return;
    e.preventDefault(); e.stopImmediatePropagation();
    busy = true;
    const ok = await ask(cfg);
    busy = false;
    if (!ok) return;
    el.dataset.confirmed = '1'; el.click(); setTimeout(() => delete el.dataset.confirmed, 0);
  }, true);
})();
