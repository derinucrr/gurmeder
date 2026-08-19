/* ==========================================================
   GURMEDER — API katmanı
   Tek sorumluluğu var: tarif verisini getirmek. Bugün bu bir
   yerel JSON endpoint'i (data/recipes.json), ama arayüz
   kasıtlı olarak gerçek bir REST API çağrısıyla birebir aynı
   şekilde davranır (async, fetch tabanlı, hataları fırlatır).
   İleride gerçek bir Recipe API'ye geçmek istenirse, tek
   yapılması gereken bu dosyadaki RECIPES_ENDPOINT sabitini ve
   fetchRecipesFromAPI() içindeki fetch çağrısını güncellemektir
   — recipes.js ve üzerindeki hiçbir katman değişmez.

   Not: Bu proje GitHub Pages gibi statik bir ortamda çalışacağı
   için gizli tutulması gereken bir API anahtarı YOKTUR ve
   burada da hiçbir anahtar tutulmaz. Gerçek, anahtar gerektiren
   bir API'ye bağlanılacaksa, anahtar frontend'e yazılmamalı;
   bunun yerine bu dosyadaki fetch çağrısı kendi backend/proxy
   uç noktanıza yönlendirilmelidir.
   ========================================================== */

const RECIPES_ENDPOINT = 'https://gurmeder.onrender.com/api/Recipes'; // TODO: gerçek API uç noktası

/**
 * Tarif verisini API'den (bugün: yerel JSON) çeker.
 * Başarısız olursa anlamlı bir Error fırlatır — çağıran katman
 * (recipes.js) bunu yakalayıp uygulamaya "yüklenemedi" durumunu
 * bildirir; sayfa asla çökmez veya boş kalmaz.
 */
async function fetchRecipesFromAPI(){
  let res;
  try{
    res = await fetch(RECIPES_ENDPOINT, {cache:'no-cache'});
  }catch(networkErr){
    console.error('[GURMEDER/api] Ağ isteği başarısız (fetch reddedildi):', networkErr);
    throw new Error('NETWORK_ERROR');
  }

  if(!res.ok){
    console.error(`[GURMEDER/api] API ${res.status} ${res.statusText} döndürdü.`);
    throw new Error('HTTP_' + res.status);
  }

  let data;
  try{
    data = await res.json();
  }catch(parseErr){
    console.error('[GURMEDER/api] Yanıt geçerli JSON değil:', parseErr);
    throw new Error('INVALID_JSON');
  }

  if(!Array.isArray(data)){
    console.error('[GURMEDER/api] Beklenmeyen veri biçimi — dizi bekleniyordu.');
    throw new Error('INVALID_SHAPE');
  }

  // Backend DTO'ları nesne olarak döndürür; mevcut frontend ise
  // ingredients/steps alanlarını [amount, unit, name] ve [title, text]
  // dizileri olarak kullanıyor. Burada tek noktada frontend formatına
  // dönüştürüyoruz; geri kalan tasarım ve uygulama kodu değişmez.
  return data.map(r => ({
    ...r,
    ingredients: Array.isArray(r.ingredients)
      ? r.ingredients.map(i => Array.isArray(i) ? i : [i.amount ?? 0, i.unit ?? '', i.name ?? ''])
      : [],
    steps: Array.isArray(r.steps)
      ? r.steps.map(s => Array.isArray(s) ? s : [s.title ?? '', s.text ?? ''])
      : []
  }));
}
