# ---- derleme aşaması ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Önce yalnızca .csproj kopyalanıp restore edilir — bağımlılıklar
# değişmediği sürece Docker bu katmanı önbellekten kullanır, kaynak
# kodu her değiştiğinde paketleri yeniden indirmez.
COPY GurmederApi/GurmederApi.csproj GurmederApi/
RUN dotnet restore GurmederApi/GurmederApi.csproj

COPY GurmederApi/ GurmederApi/
RUN dotnet publish GurmederApi/GurmederApi.csproj -c Release -o /app/publish --no-restore

# ---- çalıştırma aşaması ----
# SDK yerine yalnızca ASP.NET Core çalışma zamanını içeren, çok daha
# küçük bir imaj — üretimde derleme araçlarına gerek yok.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Render, dinleyeceği portu PORT ortam değişkeniyle bildirir; Program.cs
# bunu zaten okuyup 0.0.0.0'a bağlanıyor (bkz. Program.cs). Buradaki 10000,
# yalnızca Render'ın belgelenmiş varsayılan beklentisiyle tutarlı bir
# belgeleme/varsayılan değeri — gerçek port her zaman PORT'tan gelir.
ENV PORT=10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "GurmederApi.dll"]
