/* ==========================================================
   GURMEDER — tarif veri katmanı
   api.js'in üstünde ince bir katman: veriyi bir kez çeker,
   bellekte tutar (gereksiz tekrar istek atmaz) ve app.js'in
   ihtiyaç duyduğu sorguları (id ile bulma, kategoriye göre
   filtreleme, arama, malzemeye göre eşleştirme) tek bir yerden,
   tutarlı biçimde sağlar. app.js hiçbir zaman ham veriyi
   doğrudan fetch etmez — her zaman bu katmandan geçer.
   ========================================================== */

const CATEGORY_META = {
  "Ana Yemekler":     {emo:"🍛", grad:["#F2D9B8","#E3B98A"]},
  "Çorbalar":         {emo:"🍲", grad:["#F6E3C6","#E7B978"]},
  "Tatlılar":         {emo:"🍰", grad:["#F7DCE3","#E9A9BB"]},
  "Salatalar":        {emo:"🥗", grad:["#E6EED4","#B9CE95"]},
  "Hamur İşleri":     {emo:"🥐", grad:["#F3E4C8","#E0BE8C"]},
  "Kahvaltı":         {emo:"🍳", grad:["#FCEFD2","#F0CE8B"]},
  "İçecekler":        {emo:"🥤", grad:["#DCEAE9","#AFD3D0"]},
  "Atıştırmalıklar":  {emo:"🥪", grad:["#F0E2CE","#DCC49E"]},
};

let RECIPES = [];          // API yüklenene kadar boş — UI bu süre boyunca loading gösterir
let _recipesLoadPromise = null;

/**
 * Tarifleri API'den yükler. Aynı anda birden fazla çağrı yapılırsa
 * (örn. iki bileşen aynı anda init olursa) tek bir fetch'e indirger.
 * Zaten yüklenmişse (force=true verilmediği sürece) tekrar ağa gitmez.
 */
async function loadRecipes(force){
  if(RECIPES.length && !force) return RECIPES;
  if(_recipesLoadPromise && !force) return _recipesLoadPromise;

  _recipesLoadPromise = fetchRecipesFromAPI().then(data => {
    RECIPES = data;
    return RECIPES;
  }).finally(() => { _recipesLoadPromise = null; });

  return _recipesLoadPromise;
}

function getAllRecipes(){
  return RECIPES;
}

function getRecipeById(id){
  const numId = Number(id);
  return RECIPES.find(r => r.id === numId) || null;
}

function getRecipesByCategory(category){
  return RECIPES.filter(r => r.category === category);
}

function searchRecipes(query){
  const q = (query || '').trim().toLowerCase();
  if(!q) return RECIPES;
  return RECIPES.filter(r => {
    const haystack = (
      r.name + ' ' + r.category + ' ' + (r.tags || []).join(' ') + ' ' +
      r.ingredients.map(i => i.name).join(' ')
    ).toLowerCase();
    return haystack.includes(q);
  });
}

/**
 * Ana sayfa için öne çıkan tarifler. Sahte bir "popülerlik" puanı
 * uydurmak yerine (elimizde gerçek kullanıcı değerlendirmesi yok),
 * editöryel olarak seçilmiş, kategori çeşitliliği gözetilmiş sabit
 * bir liste kullanılır: önce gerçek fotoğrafı olan tarifler, sonra
 * fotoğrafsız kalan kategorilerden birer klasik.
 */
const FEATURED_NAMES = [
  "Etli Taco (Limonlu)", "Elmalı Somon", "Çilekli Yulaf", "Zerdeçallı Altın Süt",
  "Bruschetta (Domates ve Krem Peynirli)", "Az Pişirilmiş Ton Balığı", "Taze İstiridye (Limon ve Mignonette Soslu)",
  "Karnıyarık", "Mercimek Çorbası", "Kazandibi", "Çoban Salatası", "Lahmacun",
];
function getFeaturedRecipes(){
  const byName = new Map(RECIPES.map(r => [r.name, r]));
  return FEATURED_NAMES.map(n => byName.get(n)).filter(Boolean);
}

/**
 * Kullanıcının girdiği malzeme listesine göre tarifleri eşleştirir.
 * Her tarif için, kullanıcının malzemelerinden kaçının o tarifte
 * geçtiğini sayar ve bunu bir uyumluluk yüzdesine çevirir.
 */
function matchRecipesByIngredients(userIngredients, categoryFilter){
  const chipsLc = userIngredients.map(c => c.toLowerCase());
  if(chipsLc.length === 0) return [];

  let pool = RECIPES;
  if(categoryFilter) pool = pool.filter(r => r.category === categoryFilter);

  return pool.map(r => {
    const names = r.ingredients.map(i => i.name.toLowerCase());
    let score = 0;
    chipsLc.forEach(c => { if(names.some(n => n.includes(c) || c.includes(n))) score++; });
    const percent = Math.round((score / chipsLc.length) * 100);
    return {recipe:r, score, percent};
  })
  .filter(x => x.score > 0)
  .sort((a,b) => b.score - a.score || a.recipe.name.localeCompare(b.recipe.name, 'tr'));
}
