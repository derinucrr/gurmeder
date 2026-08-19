/* ==========================================================
   GURMEDER — sayfa / UI mantığı
   Ham veriye asla doğrudan erişmez — her zaman recipes.js'in
   sağladığı fonksiyonlar (getAllRecipes, getRecipeById,
   getRecipesByCategory, searchRecipes, matchRecipesByIngredients)
   ve loadRecipes() üzerinden çalışır. Bu dosya yüklendiğinde
   RECIPES henüz boştur — gerçek veri yalnızca init() içindeki
   loadRecipes() tamamlandıktan sonra kullanılabilir olur.
   ========================================================== */

const DIFF_EMO = {'Kolay':'😊','Orta':'🙂','Zor':'😅'};
const DIFF_COLOR = {'Kolay':'#8C8177','Orta':'#C48A93','Zor':'#9C5B68'}; // gri → toz pembe → koyu toz pembe

let favorites = new Set();
let currentRecipeId = null;
let currentServings = 4;
let ingredientChips = ['tavuk göğsü','patates','soğan','sarımsak','yoğurt'];
let activeCatFilters = new Set();
let activeDiffFilters = new Set();
let activeTimeFilter = 0;
let activeCalFilter = 0;
let currentListSource = 'all'; // 'all' | 'fav'
let displayMode = 'grid';
let currentPage = 1;
let pageSize = 12;

function grad(category){
  const m = CATEGORY_META[category];
  return m ? `linear-gradient(140deg, ${m.grad[0]}, ${m.grad[1]})` : 'linear-gradient(140deg,#EAD9C4,#D8B98E)';
}

/* ============================================================
   RENDER: recipe card
   ============================================================ */
function cardHTML(r, opts){
  opts = opts || {};
  const fav = favorites.has(r.id) ? 'active' : '';
  const diffClass = r.difficulty==='Kolay'?'kolay':r.difficulty==='Orta'?'orta':'zor';
  const matchBadge = (typeof opts.matchPercent === 'number')
    ? `<span class="match-badge">% ${opts.matchPercent} uyumlu</span>` : '';
  const mediaStyle = r.image ? `background-image:url('${r.image}')` : `background:${grad(r.category)}`;
  return `
  <div class="card" onclick="openRecipe(${r.id})">
    <div class="card-media${r.image?' photo-loaded':''}" style="${mediaStyle}">
      <div class="card-time">⏱ Toplam ${r.time} dk</div>
      <button class="fav-btn ${fav}" onclick="event.stopPropagation(); toggleFav(${r.id})">
        <svg viewBox="0 0 24 24"><path d="M12 21s-7.5-4.6-10-9.3C.5 8 2.3 4.5 6 4c2.2-.3 4.2.9 6 3 1.8-2.1 3.8-3.3 6-3 3.7.5 5.5 4 4 7.7C19.5 16.4 12 21 12 21Z"/></svg>
      </button>
      <span class="emo">${r.emo}</span>
    </div>
    <div class="card-body">
      <span class="card-cat">${r.category}${matchBadge}</span>
      <div class="card-title">${r.name}</div>
      <div class="card-meta">
        <div class="card-stats">
          <span>⏱ Hazırlama: ${r.prep} dk</span>
          <span>⏲ Toplam: ${r.time} dk</span>
          <span><span class="diff-dot diff-${diffClass}"></span>${r.difficulty}</span>
          <span>🔥 ${r.calories} kcal</span>
        </div>
      </div>
    </div>
  </div>`;
}

/* ============================================================
   VIEW SWITCHING + TARAYICI GEÇMİŞİ (Geri butonu)
   ============================================================
   Her üst-seviye görünüm değişikliği ve her tarif detayı açılışı
   history.pushState ile kaydedilir. Bu sayede hem sayfadaki
   "← Geri" butonu hem de tarayıcının kendi geri tuşu birebir
   aynı şekilde, kullanıcıyı geldiği kategoriye/sayfaya döndürerek
   çalışır — native "back" davranışı hiç bozulmaz. */
function pushViewState(view, extra){
  const state = Object.assign({view}, extra || {});
  const hash = '#' + view + (extra && extra.id ? '-' + extra.id : '');
  history.pushState(state, '', hash);
}

function switchView(name, opts){
  opts = opts || {};
  document.querySelectorAll('.view').forEach(v=>v.classList.remove('active'));
  document.getElementById('view-'+name).classList.add('active');
  document.querySelectorAll('.nav-link').forEach(b=>b.classList.remove('active'));
  const map = {home:0, tarifler:1, malzeme:2, favoriler:3};
  const idx = map[name];
  if(idx !== undefined) document.querySelectorAll('.nav-link')[idx].classList.add('active');
  if(opts.pushState !== false) pushViewState(name);
  window.scrollTo({top:0, behavior:'smooth'});
}

function goToTopView(name, opts){
  if(name==='favoriler'){ currentListSource='fav'; currentPage=1; document.getElementById('tariflerTitle').textContent='Favorilerim'; applyFilters(); }
  if(name==='tarifler'){ currentListSource='all'; currentPage=1; document.getElementById('tariflerTitle').textContent='Tarifler'; applyFilters(); }
  switchView(name, opts);
}
document.querySelectorAll('.nav-link').forEach(btn=>{
  btn.addEventListener('click', ()=>{ goToTopView(btn.dataset.view); });
});

window.addEventListener('popstate', (e)=>{
  const state = e.state || {view:'home'};
  if(state.view === 'detay' && state.id){
    openRecipe(state.id, {pushState:false});
  } else {
    goToTopView(state.view || 'home', {pushState:false});
  }
});

/* ============================================================
   HOME: category strip + popular grid
   ============================================================ */
function catCounts(){
  const counts = {};
  getAllRecipes().forEach(r=> counts[r.category] = (counts[r.category]||0)+1);
  return counts;
}
function renderCatStrip(){
  const counts = catCounts();
  const el = document.getElementById('catStrip');
  el.innerHTML = Object.keys(CATEGORY_META).map(name=>{
    const m = CATEGORY_META[name];
    return `
    <button class="cat-item" onclick="filterByCategory('${name}')">
      <span class="ico" style="background:${m.grad[0]}">${m.emo}</span>
      <span>${name}</span>
      <span class="n">${counts[name]||0} tarif</span>
    </button>`;
  }).join('');
}
function renderPopular(){
  const top = getFeaturedRecipes();
  const grid = document.getElementById('popularGrid');
  grid.innerHTML = top.map(r=>cardHTML(r)).join('');
  return top;
}
function filterByCategory(category){
  clearFiltersSilent();
  activeCatFilters = new Set([category]);
  currentListSource = 'all';
  currentPage = 1;
  switchView('tarifler');
  applyFilters();
}
function renderFooterCats(){
  const list = Object.keys(CATEGORY_META).slice(0,4);
  document.getElementById('footCatList').innerHTML = list.map(c=>`<li><a href="#" onclick="filterByCategory('${c}'); return false;">${c}</a></li>`).join('');
}
function renderFinderCatSelect(){
  const sel = document.getElementById('finderCatSelect');
  Object.keys(CATEGORY_META).forEach(c=>{
    const opt = document.createElement('option');
    opt.value = c; opt.textContent = c;
    sel.appendChild(opt);
  });
}

/* ============================================================
   TARİFLER VIEW: filters + search + sort + pagination
   ============================================================ */
function renderCatFilters(){
  const counts = catCounts();
  const el = document.getElementById('catFilters');
  el.innerHTML = Object.keys(counts).map(c=>`
    <div class="filter-row">
      <label><input type="checkbox" class="catFilter" value="${c}" ${activeCatFilters.has(c)?'checked':''} onchange="applyFilters()"> ${c}</label>
      <span class="n">${counts[c]}</span>
    </div>`).join('');
}
function clearFiltersSilent(){
  activeCatFilters.clear(); activeDiffFilters.clear(); activeTimeFilter=0; activeCalFilter=0;
}
function clearFilters(){
  clearFiltersSilent();
  document.querySelectorAll('.timeFilter').forEach(r=>r.checked = r.value==='0');
  document.querySelectorAll('.calFilter').forEach(r=>r.checked = r.value==='0');
  document.querySelectorAll('.diffFilter').forEach(cb=>cb.checked=false);
  document.getElementById('listSearch').value = '';
  currentPage = 1;
  currentListSource = 'all';
  document.getElementById('tariflerTitle').textContent = 'Tarifler';
  applyFilters();
}
function getFilteredSorted(){
  document.querySelectorAll('.catFilter').forEach(cb=>{ if(cb.checked) activeCatFilters.add(cb.value); else activeCatFilters.delete(cb.value); });
  document.querySelectorAll('.diffFilter').forEach(cb=>{ if(cb.checked) activeDiffFilters.add(cb.value); else activeDiffFilters.delete(cb.value); });
  const tf = document.querySelector('.timeFilter:checked');
  activeTimeFilter = tf ? parseInt(tf.value) : 0;
  const cf = document.querySelector('.calFilter:checked');
  activeCalFilter = cf ? parseInt(cf.value) : 0;
  const q = (document.getElementById('listSearch').value || '').trim().toLowerCase();

  let base = currentListSource==='fav' ? getAllRecipes().filter(r=>favorites.has(r.id)) : getAllRecipes();

  let items = base.filter(r=>{
    if(activeCatFilters.size && !activeCatFilters.has(r.category)) return false;
    if(activeDiffFilters.size && !activeDiffFilters.has(r.difficulty)) return false;
    if(activeTimeFilter && r.prep > activeTimeFilter) return false; // "Hazırlama Süresi" filtresi → r.prep
    if(activeCalFilter && r.calories > activeCalFilter) return false;
    if(q){
      const hay = (r.name + ' ' + r.category + ' ' + r.tags.join(' ') + ' ' + r.ingredients.map(i=>i[2]).join(' ')).toLowerCase();
      if(!hay.includes(q)) return false;
    }
    return true;
  });

  const sort = document.getElementById('sortSelect').value;
  if(sort==='name') items = [...items].sort((a,b)=>a.name.localeCompare(b.name, 'tr'));
  else if(sort==='time') items = [...items].sort((a,b)=>a.time-b.time);
  else if(sort==='cal') items = [...items].sort((a,b)=>a.calories-b.calories);
  // 'default': doğal (veri setindeki) sıra korunur

  return items;
}
function applyFilters(){
  const items = getFilteredSorted();
  currentPage = Math.min(currentPage, Math.max(1, Math.ceil(items.length/pageSize)));
  const start = (currentPage-1)*pageSize;
  const pageItems = items.slice(start, start+pageSize);

  const grid = document.getElementById('listGrid');
  grid.className = 'grid list-grid' + (displayMode==='list' ? ' list-mode' : '');
  grid.innerHTML = pageItems.map(r=>cardHTML(r)).join('');
  document.getElementById('resultCount').textContent = items.length;

  const emptyStateEl = document.getElementById('emptyState');
  emptyStateEl.style.display = items.length ? 'none':'block';
  if(!items.length){
    const title = activeCatFilters.size ? 'Bu kategoriye uygun tarif bulunamadı.' : 'Uygun tarif bulunamadı.';
    emptyStateEl.querySelector('h3').textContent = title;
  }

  document.getElementById('paginationBox').style.display = items.length > pageSize ? 'flex' : 'none';
  renderCatFilters();
  renderPagination(items.length);

  const chipsEl = document.getElementById('activeChips');
  let chips = [];
  activeCatFilters.forEach(c=>chips.push({label:c, fn:`removeCatFilter('${c}')`}));
  activeDiffFilters.forEach(c=>chips.push({label:c, fn:`removeDiffFilter('${c}')`}));
  if(activeTimeFilter) chips.push({label:activeTimeFilter+' dk altı', fn:'removeTimeFilter()'});
  if(activeCalFilter) chips.push({label:activeCalFilter+' kcal altı', fn:'removeCalFilter()'});
  chipsEl.innerHTML = chips.map(c=>`<span class="chip">${c.label}<button onclick="${c.fn}">✕</button></span>`).join('') +
    (chips.length? `<span class="chip" style="background:none; border:none; color:var(--paprika); cursor:pointer;" onclick="clearFilters()">🗑 Tümünü Temizle</span>`:'');
}
function removeCatFilter(c){ activeCatFilters.delete(c); currentPage=1; applyFilters(); }
function removeDiffFilter(c){ activeDiffFilters.delete(c); currentPage=1; applyFilters(); }
function removeTimeFilter(){ activeTimeFilter=0; document.querySelectorAll('.timeFilter').forEach(r=>r.checked = r.value==='0'); currentPage=1; applyFilters(); }
function removeCalFilter(){ activeCalFilter=0; document.querySelectorAll('.calFilter').forEach(r=>r.checked = r.value==='0'); currentPage=1; applyFilters(); }

function setDisplayMode(mode){
  displayMode = mode;
  document.getElementById('gridViewBtn').classList.toggle('active', mode==='grid');
  document.getElementById('listViewBtn').classList.toggle('active', mode==='list');
  applyFilters();
}
function changePageSize(){
  pageSize = parseInt(document.getElementById('pageSizeSelect').value);
  currentPage = 1;
  applyFilters();
}
function renderPagination(total){
  const pages = Math.max(1, Math.ceil(total/pageSize));
  const el = document.getElementById('pageNums');
  if(pages<=1){ el.innerHTML=''; return; }

  let html = `<button class="page-nav" ${currentPage===1?'disabled':''} onclick="goToPage(${currentPage-1})">‹ Önceki</button>`;
  const windowSize = 2;
  let shown = [];
  for(let p=1;p<=pages;p++){
    if(p===1 || p===pages || (p>=currentPage-windowSize && p<=currentPage+windowSize)) shown.push(p);
  }
  let prev = 0;
  shown.forEach(p=>{
    if(prev && p-prev>1) html += `<span class="dots">…</span>`;
    html += `<button class="${p===currentPage?'active':''}" onclick="goToPage(${p})">${p}</button>`;
    prev = p;
  });
  html += `<button class="page-nav" ${currentPage===pages?'disabled':''} onclick="goToPage(${currentPage+1})">Sonraki ›</button>`;
  el.innerHTML = html;
}
function goToPage(p){
  currentPage = p;
  applyFilters();
  document.querySelector('.list-layout').scrollIntoView({behavior:'smooth', block:'start'});
}

/* ============================================================
   SEARCH
   ============================================================ */
function runTextSearch(q){
  clearFiltersSilent();
  currentListSource = 'all';
  currentPage = 1;
  switchView('tarifler');
  document.getElementById('listSearch').value = q;
  document.getElementById('tariflerTitle').textContent = q ? `"${q}" için sonuçlar` : 'Tarifler';
  applyFilters();
}
document.getElementById('headerSearch').addEventListener('keydown', e=>{
  if(e.key==='Enter'){ runTextSearch(e.target.value.trim()); }
});

/* ============================================================
   FAVORITES  (localStorage — sayfa yenilense bile kaybolmaz)
   ============================================================ */
const FAV_STORAGE_KEY = 'gurmeder_favorites';
function loadFavoritesFromStorage(){
  try{
    const raw = localStorage.getItem(FAV_STORAGE_KEY);
    if(!raw) return new Set();
    const arr = JSON.parse(raw);
    return new Set(Array.isArray(arr) ? arr : []);
  }catch(err){
    console.warn('[GURMEDER] Favoriler localStorage’dan okunamadı:', err);
    return new Set();
  }
}
function saveFavoritesToStorage(){
  try{
    localStorage.setItem(FAV_STORAGE_KEY, JSON.stringify([...favorites]));
  }catch(err){
    console.warn('[GURMEDER] Favoriler localStorage’a yazılamadı:', err);
  }
}
function toggleFav(id){
  if(favorites.has(id)) favorites.delete(id); else favorites.add(id);
  saveFavoritesToStorage();
  document.getElementById('favBadge').textContent = favorites.size;
  renderPopular();
  if(document.getElementById('view-tarifler').classList.contains('active')) applyFilters();
  if(currentRecipeId===id) updateDetailFav();
}
function updateDetailFav(){
  const btn = document.getElementById('detailFavBtn');
  btn.classList.toggle('active', favorites.has(currentRecipeId));
}
document.getElementById('detailFavBtn').addEventListener('click', ()=>{ toggleFav(currentRecipeId); });
document.getElementById('favIconBtn').addEventListener('click', ()=>{ goToTopView('favoriler'); });

/* ============================================================
   RECIPE DETAIL
   ============================================================ */
function ringGradient(pct, color){
  return `background: conic-gradient(${color} ${pct*3.6}deg, #E6E1DE 0deg); `;
}
function openRecipe(id, opts){
  opts = opts || {};
  const r = getRecipeById(id);
  if(!r) return; // geçersiz/bulunamayan id — sayfa bozulmadan sessizce yok sayılır
  currentRecipeId = r.id;
  currentServings = r.servings;

  document.getElementById('crumbCat').textContent = r.category;
  document.getElementById('crumbTitle').textContent = r.name;
  document.getElementById('detailCat').textContent = r.category.toUpperCase();
  document.getElementById('detailTitle').textContent = r.name;
  document.getElementById('detailDesc').textContent = r.description;
  document.getElementById('detailEmo').textContent = r.emo;
  const detailMediaEl = document.getElementById('detailMedia');
  if(r.image){
    detailMediaEl.style.backgroundImage = `url('${r.image}')`;
    detailMediaEl.classList.add('photo-loaded');
  } else {
    detailMediaEl.style.backgroundImage = '';
    detailMediaEl.style.background = grad(r.category);
    detailMediaEl.classList.remove('photo-loaded');
  }
  document.getElementById('detailPairs').textContent = r.pairs;
  updateDetailFav();

  const maxTime = 90;
  document.getElementById('gaugePrep').style = ringGradient(Math.min(r.prep/maxTime*100,100), 'var(--olive)');
  document.getElementById('gaugePrep').querySelector('span').textContent = r.prep+' dk';
  document.getElementById('gaugeCook').style = ringGradient(Math.min(r.cook/maxTime*100,100), 'var(--paprika)');
  document.getElementById('gaugeCook').querySelector('span').textContent = r.cook+' dk';
  document.getElementById('gaugeTotal').style = ringGradient(Math.min(r.time/maxTime*100,100), 'var(--berry)');
  document.getElementById('gaugeTotal').querySelector('span').textContent = r.time+' dk';
  document.getElementById('gaugeCal').style = ringGradient(Math.min(r.calories/700*100,100), 'var(--saffron)');
  document.getElementById('gaugeCal').querySelector('span').textContent = r.calories;
  document.getElementById('gaugeDiff').style = ringGradient(r.difficulty==='Kolay'?33:r.difficulty==='Orta'?66:100, DIFF_COLOR[r.difficulty]);
  document.getElementById('gaugeDiff').querySelector('span').textContent = DIFF_EMO[r.difficulty];
  document.getElementById('diffLabel').textContent = r.difficulty;

  document.getElementById('tagRow').innerHTML = r.tags.map(t=>`<span class="tag"># ${t}</span>`).join('');
  document.getElementById('chefNote').textContent = r.chef;

  const n = r.nutri;
  const total = Math.max(1, n.carb+n.protein+n.fat+n.fiber);
  const colors = {carb:'var(--saffron)', protein:'var(--olive)', fat:'var(--paprika)', fiber:'#6B645C'};
  let acc = 0;
  let stops = [];
  ['carb','protein','fat','fiber'].forEach(k=>{
    const pct = n[k]/total*100;
    stops.push(`${colors[k]} ${acc*3.6}deg ${(acc+pct)*3.6}deg`);
    acc += pct;
  });
  document.getElementById('nutriDonut').style.background = `conic-gradient(${stops.join(',')})`;
  document.getElementById('nutriKcal').textContent = r.calories;
  document.getElementById('nutriLegend').innerHTML = `
    <div class="row"><span class="l"><span class="dot" style="background:var(--saffron)"></span>Karbonhidrat</span><b>${n.carb} g</b></div>
    <div class="row"><span class="l"><span class="dot" style="background:var(--olive)"></span>Protein</span><b>${n.protein} g</b></div>
    <div class="row"><span class="l"><span class="dot" style="background:var(--paprika)"></span>Yağ</span><b>${n.fat} g</b></div>
    <div class="row"><span class="l"><span class="dot" style="background:#6B645C"></span>Lif</span><b>${n.fiber} g</b></div>`;

  const similar = getRecipesByCategory(r.category).filter(x=>x.id!==r.id).sort((a,b)=>a.name.localeCompare(b.name,'tr')).slice(0,3);
  const similarListEl = document.getElementById('similarList');
  similarListEl.innerHTML = similar.map(s=>{
    const miniStyle = s.image ? `background-image:url('${s.image}')` : `background:${grad(s.category)}`;
    return `
    <div class="mini-card" onclick="openRecipe(${s.id})">
      <div class="mini-media${s.image?' photo-loaded':''}" style="${miniStyle}"><span class="emo">${s.emo}</span></div>
      <div><b>${s.name}</b><div class="m"><span>⏱ ${s.time} dk</span><span>${s.difficulty}</span></div></div>
    </div>`;
  }).join('');

  renderIngredients(r);
  document.getElementById('stepsList').innerHTML = r.steps.map((s,i)=>`
    <div class="step-row">
      <div class="step-num">${i+1}</div>
      <div class="step-content"><b>${s[0]}</b><p>${s[1]}</p></div>
    </div>`).join('');

  switchView('detay', {pushState:false});
  if(opts.pushState !== false) pushViewState('detay', {id:r.id});
}
function goBack(){
  history.back(); // popstate işleyicisi kullanıcıyı geldiği kategoriye/sayfaya geri döndürür
}

function fmtAmount(n){
  if(n===0) return '';
  const r = Math.round(n*100)/100;
  return (r % 1 === 0) ? r.toString() : r.toFixed(2).replace(/0+$/,'').replace(/\.$/,'');
}
function renderIngredients(r){
  const factor = currentServings / r.servings;
  document.getElementById('serveCount').textContent = currentServings + ' porsiyon';
  document.getElementById('servingVal').textContent = currentServings;
  document.getElementById('ingList').innerHTML = r.ingredients.map((ing,i)=>{
    const [amt,unit,name] = ing;
    const scaled = amt * factor;
    const label = amt ? `${fmtAmount(scaled)} ${unit}` : '';
    return `
    <div class="ing-row" id="ingRow${i}">
      <input type="checkbox" onchange="document.getElementById('ingRow${i}').classList.toggle('done')">
      <span class="amt">${label}</span>
      <label>${name}</label>
    </div>`;
  }).join('');
}
function changeServings(delta){
  const r = getRecipeById(currentRecipeId);
  currentServings = Math.max(1, Math.min(20, currentServings+delta));
  renderIngredients(r);
}
function copyLink(){
  navigator.clipboard?.writeText(window.location.href).catch(()=>{});
  alert('Bağlantı kopyalandı!');
}

/* ============================================================
   MALZEMEDEN TARİF BUL
   ============================================================ */
function renderChips(){
  const box = document.getElementById('chipInputBox');
  const input = document.getElementById('ingInput');
  box.querySelectorAll('.ing-chip').forEach(c=>c.remove());
  ingredientChips.forEach(name=>{
    const chip = document.createElement('span');
    chip.className='ing-chip';
    chip.innerHTML = `${name} <button onclick="removeChip('${name}')">✕</button>`;
    box.insertBefore(chip, input);
  });
}
function addIngredientChip(val){
  val = val.trim();
  if(val && !ingredientChips.includes(val)){ ingredientChips.push(val); renderChips(); }
}
function removeChip(name){ ingredientChips = ingredientChips.filter(c=>c!==name); renderChips(); }
function clearIngredientChips(){ ingredientChips = []; renderChips(); document.getElementById('finderGrid').innerHTML=''; document.getElementById('finderResultTitle').textContent='Sizin için bulduğumuz tarifler'; }

function findByIngredients(){
  if(ingredientChips.length===0){
    document.getElementById('finderGrid').innerHTML = '';
    document.getElementById('finderResultTitle').textContent = 'Lütfen en az bir malzeme ekleyin';
    return;
  }
  const catFilter = document.getElementById('finderCatSelect').value;
  const scored = matchRecipesByIngredients(ingredientChips, catFilter || null).slice(0,12);

  document.getElementById('finderResultTitle').textContent = `Sizin için bulduğumuz tarifler (${scored.length} tarif bulundu)`;
  const finderGridEl = document.getElementById('finderGrid');
  finderGridEl.innerHTML = scored.length
    ? scored.map(x=>cardHTML(x.recipe, {matchPercent:x.percent})).join('')
    : `<div class="empty-state" style="grid-column:1/-1;"><div class="emo">🍽️</div><h3>Eşleşen tarif bulunamadı</h3><p style="color:var(--ink-soft); margin-top:8px; font-size:13.5px;">Farklı malzemeler eklemeyi deneyin.</p></div>`;
}

/* ============================================================
   AUTH MODAL
   ============================================================ */
function openAuth(){ document.getElementById('authOverlay').classList.add('show'); showAuth('login'); }
function closeAuth(){ document.getElementById('authOverlay').classList.remove('show'); }
function showAuth(which){
  document.getElementById('authLogin').style.display = which==='login' ? 'block':'none';
  document.getElementById('authRegister').style.display = which==='register' ? 'block':'none';
  document.getElementById('authForgot').style.display = which==='forgot' ? 'block':'none';
}
document.getElementById('userIconBtn').addEventListener('click', openAuth);
document.getElementById('authOverlay').addEventListener('click', e=>{ if(e.target.id==='authOverlay') closeAuth(); });
document.addEventListener('keydown', e=>{ if(e.key==='Escape') closeAuth(); });

/* ============================================================
   MOBILE BURGER
   ============================================================ */
document.getElementById('burgerBtn').addEventListener('click', ()=>{
  const nav = document.querySelector('nav.main');
  const isOpen = nav.classList.toggle('mobile-open');
  nav.style.cssText = isOpen
    ? 'display:flex; position:absolute; top:100%; left:0; right:0; background:var(--cream); flex-direction:column; padding:12px 18px; border-bottom:1px solid var(--line); gap:2px; z-index:99;'
    : 'display:none;';
});

/* ============================================================
   YÜKLEME / HATA DURUMLARI
   ============================================================ */
function showLoadingState(){
  document.getElementById('globalLoading').style.display = 'flex';
  document.getElementById('globalError').style.display = 'none';
  document.getElementById('appMain').style.display = 'none';
}
function showErrorState(){
  document.getElementById('globalLoading').style.display = 'none';
  document.getElementById('globalError').style.display = 'flex';
  document.getElementById('appMain').style.display = 'none';
}
function hideStateScreens(){
  document.getElementById('globalLoading').style.display = 'none';
  document.getElementById('globalError').style.display = 'none';
  document.getElementById('appMain').style.display = '';
}
async function retryLoad(){
  showLoadingState();
  try{
    await loadRecipes(true);
  }catch(err){
    showErrorState();
    return;
  }
  hideStateScreens();
  renderApp();
}

/* ============================================================
   INIT
   ============================================================ */
function renderApp(){
  const all = getAllRecipes();
  document.getElementById('statTotal').textContent = all.length;
  document.getElementById('statCats').textContent = Object.keys(CATEGORY_META).length;
  document.getElementById('statPhotos').textContent = all.filter(r=>r.image).length;
  document.getElementById('favBadge').textContent = favorites.size;

  renderCatStrip();
  renderPopular();
  renderFooterCats();
  renderFinderCatSelect();
  renderCatFilters();
  applyFilters();
  renderChips();
  findByIngredients();

  history.replaceState({view:'home'}, '', '#home');
}

async function init(){
  favorites = loadFavoritesFromStorage();
  showLoadingState();
  try{
    await loadRecipes();
  }catch(err){
    console.error('[GURMEDER/app] Tarifler yüklenemedi:', err);
    showErrorState();
    return;
  }
  hideStateScreens();
  renderApp();
}
init();
