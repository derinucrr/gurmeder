/* ==========================================================
   GURMEDER — API katmanı
   Tek sorumluluğu var: tarif verisini getirmek. Gerçek bir .NET/
   ASP.NET Core Web API'den çekiyor — bu API, verileri bir SQLite
   veritabanından okuyor. Bu dosya, mimari kasıtlı olarak "gerçek
   bir REST API çağrısı" şeklinde tasarlandığı için BAŞKA HİÇBİR
   DOSYA DEĞİŞMEDEN bu geçişi yapabildi — recipes.js ve üzeri hâlâ
   yalnızca "bir dizi tarif nesnesi" bekliyor.

   Bu dosya artık API'yle AYNI .NET uygulaması tarafından, AYNI
   origin'den sunuluyor (bkz. Program.cs — wwwroot statik dosya
   sunumu) — bu yüzden endpoint göreli bir yol (/api/recipes),
   ayrı bir sunucu adresi/port belirtmeye gerek yok. Uygulamayı
   çalıştırmak için tek komut yeterli: cd GurmederApi && dotnet run
   (bkz. ana README.md).
   ========================================================== */

const RECIPES_ENDPOINT = '/api/recipes';

/**
 * Tarif verisini .NET API'den çeker.
 * Başarısız olursa anlamlı bir Error fırlatır — çağıran katman
 * (recipes.js) bunu yakalayıp uygulamaya "yüklenemedi" durumunu
 * bildirir; sayfa asla çökmez veya boş kalmaz.
 */
async function fetchRecipesFromAPI(){
  let res;
  try{
    res = await fetch(RECIPES_ENDPOINT, {cache:'no-cache'});
  }catch(networkErr){
    console.error('[GURMEDER/api] Ağ isteği başarısız:', networkErr);
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

  return data;
}
