
(() => {
  'use strict';
  document.querySelectorAll('form[data-ui-only]').forEach(form => form.addEventListener('submit', e => e.preventDefault()));
  const page = document.body.dataset.page;
  const activePage = ({order_detail:'orders',appointment_detail:'appointments',fitting:'appointments',admin_product_form:'admin_products',admin_order_detail:'admin_orders'})[page] || page;
  document.querySelectorAll('[data-page-link]').forEach(a => {
    if(a.dataset.pageLink === activePage) { a.classList.add('is-active'); a.setAttribute('aria-current','page'); }
  });
  document.querySelectorAll('details.dropdown').forEach(d => {
    d.addEventListener('toggle', () => { if(d.open) document.querySelectorAll('details.dropdown').forEach(other => {if(other!==d)other.open=false;}); });
  });
  document.addEventListener('click', e => document.querySelectorAll('details.dropdown[open]').forEach(d => { if(!d.contains(e.target))d.open=false; }));
  document.addEventListener('keydown', e => { if(e.key==='Escape'){document.querySelectorAll('details.dropdown').forEach(d=>d.open=false);document.querySelector('.admin-sidebar')?.classList.remove('is-open');} });
  document.querySelector('[data-toggle-admin]')?.addEventListener('click',e=>{const s=document.querySelector('.admin-sidebar');s.classList.toggle('is-open');e.currentTarget.setAttribute('aria-expanded',String(s.classList.contains('is-open')));});
  document.querySelectorAll('[data-size-group]').forEach(g => g.querySelectorAll('button').forEach(b => b.addEventListener('click',()=>{g.querySelectorAll('button').forEach(x=>{x.classList.remove('is-active');x.setAttribute('aria-pressed','false');});b.classList.add('is-active');b.setAttribute('aria-pressed','true');})));
  const list = document.querySelector('[data-catalog]');
  if(list){
    const controls=[...document.querySelectorAll('[data-filter]')];
    const filter=()=>{
      const cat=document.querySelector('[data-filter="category"]:checked')?.value || '';
      const size=document.querySelector('[data-filter="size"]')?.value || '';
      const price=document.querySelector('[data-filter="price"]')?.value || '';
      const q=(document.querySelector('[data-filter="search"]')?.value || '').toLocaleLowerCase('vi');
      let count=0;
      list.querySelectorAll('[data-product-card]').forEach(c=>{
        const cost=Number(c.dataset.price);
        const priceOk=!price || price==='low'&&cost<2000000 || price==='mid'&&cost>=2000000&&cost<=4000000 || price==='high'&&cost>4000000;
        c.hidden=Boolean((cat&&c.dataset.category!==cat)||(size&&!c.dataset.size.split(' ').includes(size))||!priceOk||!c.dataset.name.includes(q));
        if(!c.hidden)count++;
      });
      document.querySelectorAll('[data-result-count]').forEach(x=>x.textContent=String(count));
      document.querySelector('[data-empty-catalog]').hidden=count!==0;
    };
    controls.forEach(x=>x.addEventListener(x.type==='search'?'input':'change',filter));
    document.querySelector('[data-reset-filters]')?.addEventListener('click',()=>{controls.forEach(x=>{if(x.type==='radio')x.checked=x.value==='';else x.value='';});filter();});
    document.querySelector('[data-sort]')?.addEventListener('change',e=>{[...list.querySelectorAll('[data-product-card]')].sort((a,b)=>e.target.value==='asc'?Number(a.dataset.price)-Number(b.dataset.price):e.target.value==='desc'?Number(b.dataset.price)-Number(a.dataset.price):Number(a.dataset.original)-Number(b.dataset.original)).forEach(x=>list.append(x));});
    list.querySelectorAll('[data-product-card]').forEach((c,i)=>c.dataset.original=String(i));
    const params=new URLSearchParams(location.search);
    const queryInput=document.querySelector('[data-filter="search"]');
    if(queryInput && params.has('q'))queryInput.value=params.get('q');
    if(params.has('category'))document.querySelectorAll('[data-filter="category"]').forEach(r=>r.checked=r.value===params.get('category'));
    filter();
  }
  document.querySelectorAll('[data-status-group]').forEach(group=>{
    group.querySelectorAll('[data-status-filter]').forEach(b=>b.addEventListener('click',()=>{
      group.querySelectorAll('button').forEach(x=>{x.classList.remove('is-active');x.setAttribute('aria-pressed','false');});
      b.classList.add('is-active');b.setAttribute('aria-pressed','true');
      const target=document.querySelector(group.dataset.statusGroup);
      target?.querySelectorAll('[data-status]').forEach(row=>row.hidden=Boolean(b.dataset.statusFilter&&row.dataset.status!==b.dataset.statusFilter));
      const empty=document.querySelector('[data-empty-status]');
      if(empty)empty.hidden=!!target?.querySelector('[data-status]:not([hidden])');
    }));
  });
  document.querySelectorAll('[data-table-search]').forEach(input=>input.addEventListener('input',()=>{
    const q=input.value.toLocaleLowerCase('vi');document.querySelector(input.dataset.tableSearch)?.querySelectorAll('tbody tr').forEach(r=>r.hidden=!r.textContent.toLocaleLowerCase('vi').includes(q));
  }));
  document.querySelectorAll('[data-open-dialog]').forEach(b=>b.addEventListener('click',()=>document.getElementById(b.dataset.openDialog)?.showModal()));
  document.querySelectorAll('[data-close-dialog]').forEach(b=>b.addEventListener('click',()=>b.closest('dialog')?.close()));
})();

// Đánh dấu giao diện trên thiết bị này, không thêm bảng yêu thích vào SQL.
(() => {
  const key = 'mira:marked-products';
  let marked = [];
  try { marked = JSON.parse(localStorage.getItem(key) || '[]'); if(!Array.isArray(marked)) marked=[]; } catch {}
  document.querySelectorAll('[data-mark-product]').forEach(button => {
    const id = button.dataset.markProduct;
    button.setAttribute('aria-pressed', String(marked.includes(id)));
    button.addEventListener('click', () => {
      marked = marked.includes(id) ? marked.filter(x => x !== id) : [...marked, id];
      document.querySelectorAll('[data-mark-product]').forEach(b => b.setAttribute('aria-pressed', String(marked.includes(b.dataset.markProduct))));
      try { localStorage.setItem(key, JSON.stringify(marked)); } catch {}
    });
  });
})();
