/* ==========================================================
   GURMEDER — Kimlik doğrulama
   Gerçek, backend-doğrulamalı giriş/kayıt. Oturum bir HttpOnly
   çerezle yönetilir (bkz. ../gurmeder-api/GurmederApi/Program.cs) —
   bu dosya token'ı KENDİSİ saklamaz/yönetmez, tarayıcı çerezi
   otomatik gönderir; her isteğe yalnızca credentials:'include'
   eklemek yeterlidir.
   ========================================================== */

let currentUser = null; // {id, fullName, email, favoriteRecipeIds} | null (misafir)

function isLoggedIn(){ return currentUser !== null; }

/** Sayfa her açıldığında çağrılır: sunucuda geçerli bir oturum var mı diye bakar. */
async function checkSession(){
  try{
    const res = await fetch('/api/auth/me', { credentials:'include' });
    if(res.ok){
      currentUser = await res.json();
      await syncGuestFavoritesToServer();
    }
  }catch(err){
    console.warn('[GURMEDER/auth] Oturum kontrolü başarısız (API çalışmıyor olabilir):', err);
  }
  updateAuthUI();
}

function showAuthError(elId, message){
  const el = document.getElementById(elId);
  el.textContent = message;
  el.style.display = 'block';
}
function hideAuthError(elId){
  document.getElementById(elId).style.display = 'none';
}

async function handleLogin(event){
  event.preventDefault();
  hideAuthError('loginError');

  const email = document.getElementById('loginEmail').value.trim();
  const password = document.getElementById('loginPassword').value;
  const btn = document.getElementById('loginSubmitBtn');
  btn.disabled = true; btn.textContent = 'Giriş yapılıyor…';

  try{
    const res = await fetch('/api/auth/login', {
      method:'POST',
      headers:{'Content-Type':'application/json'},
      credentials:'include',
      body: JSON.stringify({ email, password }),
    });

    if(res.ok){
      currentUser = await res.json();
      await syncGuestFavoritesToServer();
      updateAuthUI();
      closeAuth();
      document.getElementById('authLogin').reset();
    }else if(res.status === 401){
      showAuthError('loginError', 'E-posta veya şifre hatalı.');
    }else{
      showAuthError('loginError', 'Giriş yapılamadı, lütfen tekrar deneyin.');
    }
  }catch(err){
    showAuthError('loginError', 'Sunucuya bağlanılamadı. API çalışıyor mu?');
  }finally{
    btn.disabled = false; btn.textContent = 'Giriş Yap';
  }
}

async function handleRegister(event){
  event.preventDefault();
  hideAuthError('registerError');

  const fullName = document.getElementById('registerName').value.trim();
  const email = document.getElementById('registerEmail').value.trim();
  const password = document.getElementById('registerPassword').value;
  const btn = document.getElementById('registerSubmitBtn');

  if(password.length < 6){
    showAuthError('registerError', 'Şifre en az 6 karakter olmalı.');
    return;
  }

  btn.disabled = true; btn.textContent = 'Hesap oluşturuluyor…';

  try{
    const res = await fetch('/api/auth/register', {
      method:'POST',
      headers:{'Content-Type':'application/json'},
      credentials:'include',
      body: JSON.stringify({ fullName, email, password }),
    });

    if(res.ok){
      currentUser = await res.json();
      await syncGuestFavoritesToServer();
      updateAuthUI();
      closeAuth();
      document.getElementById('authRegister').reset();
    }else if(res.status === 409){
      showAuthError('registerError', 'Bu e-posta adresiyle zaten bir hesap var.');
    }else{
      showAuthError('registerError', 'Kayıt oluşturulamadı, bilgileri kontrol edin.');
    }
  }catch(err){
    showAuthError('registerError', 'Sunucuya bağlanılamadı. API çalışıyor mu?');
  }finally{
    btn.disabled = false; btn.textContent = 'Kayıt Ol';
  }
}

function handleForgotPassword(){
  // Gerçek e-posta gönderimi bir e-posta servisi (SMTP/SendGrid vb.)
  // yapılandırması gerektirir; bu proje kapsamında kurulmadı. Burada
  // sahte bir "gönderildi" mesajı göstermek yerine dürüstçe bunu
  // belirtiyoruz.
  showAuthError('forgotNotice', 'Bu özellik için e-posta gönderimi henüz yapılandırılmadı. Şimdilik yeni bir hesap oluşturabilir ya da mevcut şifrenizi hatırlamaya çalışabilirsiniz.');
}

async function handleLogout(){
  try{
    await fetch('/api/auth/logout', { method:'POST', credentials:'include' });
  }catch(err){ /* çevrimdışı olsa bile yerel oturumu kapatmaya devam et */ }
  currentUser = null;
  closeAccountMenu();
  favorites = loadFavoritesFromStorage(); // misafir (localStorage) moduna dön
  document.getElementById('favBadge').textContent = favorites.size;
  renderPopular();
  if(document.getElementById('view-tarifler')?.classList.contains('active')) applyFilters();
  updateAuthUI();
}

/**
 * Misafirken (localStorage) eklenen favorileri, giriş/kayıt sonrası
 * sunucudaki hesaba aktarır — kullanıcı giriş yapmadan önce
 * beğendiği tarifleri kaybetmesin diye. Sunucudaki liste zaten
 * içeriyorsa tekrar eklenmez.
 */
async function syncGuestFavoritesToServer(){
  if(!isLoggedIn()) return;
  const guestFavs = loadFavoritesFromStorage();
  const serverFavs = new Set(currentUser.favoriteRecipeIds || []);
  const toSync = [...guestFavs].filter(id => !serverFavs.has(id));

  for(const id of toSync){
    try{
      const res = await fetch(`/api/favorites/${id}`, { method:'POST', credentials:'include' });
      if(res.ok) currentUser.favoriteRecipeIds = await res.json();
    }catch(err){ /* tek bir tarif senkronize olamazsa akışı durdurmaz */ }
  }

  favorites = new Set(currentUser.favoriteRecipeIds || []);
  saveFavoritesToStorage(); // localStorage'ı da sunucuyla hizala — sonraki bir
                            // oturumda (ör. çerez süresi dolmuşsa) yerel kopya bayat kalmasın.
  document.getElementById('favBadge').textContent = favorites.size;
  renderPopular();
}

/* ============ hesap ikonu / dropdown ============ */
function updateAuthUI(){
  const btn = document.getElementById('userIconBtn');
  if(isLoggedIn()){
    const initial = (currentUser.fullName || currentUser.email || '?').trim().charAt(0).toUpperCase();
    btn.innerHTML = `<span class="user-avatar">${initial}</span>`;
    btn.title = currentUser.fullName;
    btn.onclick = toggleAccountMenu;
  }else{
    btn.innerHTML = `<svg width="17" height="17" viewBox="0 0 24 24" fill="none" stroke="#3A3532" stroke-width="2"><circle cx="12" cy="8" r="3.6"/><path d="M4.5 20c1.4-3.6 4.4-5.5 7.5-5.5s6.1 1.9 7.5 5.5"/></svg>`;
    btn.title = 'Giriş yap';
    btn.onclick = openAuth;
  }
}

function toggleAccountMenu(){
  let menu = document.getElementById('accountMenu');
  if(!menu){
    menu = document.createElement('div');
    menu.className = 'account-menu';
    menu.id = 'accountMenu';
    document.querySelector('.header-actions').appendChild(menu);
  }
  menu.innerHTML = `
    <div class="who"><b>${currentUser.fullName}</b><span>${currentUser.email}</span></div>
    <button onclick="goToTopView('favoriler'); closeAccountMenu();">❤️ Favorilerim</button>
    <button onclick="handleLogout()">🚪 Çıkış Yap</button>
  `;
  menu.classList.toggle('show');
}
function closeAccountMenu(){
  document.getElementById('accountMenu')?.classList.remove('show');
}
document.addEventListener('click', e => {
  const menu = document.getElementById('accountMenu');
  const btn = document.getElementById('userIconBtn');
  if(menu && menu.classList.contains('show') && !menu.contains(e.target) && e.target !== btn && !btn.contains(e.target)){
    closeAccountMenu();
  }
});
