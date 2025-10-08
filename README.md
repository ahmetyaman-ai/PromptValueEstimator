Prompt Value Estimator

ChatGPT’e benzer promptların ne kadar sık sorulabileceğini, keyword search volume verileriyle tahmin eden bir servis.
Promtu keywords/phrases’e dönüştürür, Serpstat API’inden related keywords ve monthly search volume çeker, similarity + intent skorlarıyla birleştirip:

Estimated monthly prompt volume

Related high-volume prompts

Confidence score (High/Medium/Low)

çıktılarını üretir.

0) Önkoşullar (Prerequisites)

.NET SDK: 9.0.x

dotnet --list-sdks


(Opsiyonel) Serpstat API Key (trial olur). Yoksa “dummy fallback” ile de çalışır.

Windows/Linux/macOS hepsi desteklenir. Komutlar PowerShell örnekleriyle verilmiştir.

1) Kurulum (Setup)
git clone <repo-url> prompt-value-estimator
cd prompt-value-estimator


Çözüm yapısı:

PromptValueEstimator.sln
├─ PromptValueEstimator.Api/           # Minimal API + Swagger
├─ PromptValueEstimator.Application/   # Business logic (PromptEstimator, scoring)
└─ PromptValueEstimator.Infrastructure/# External integrations (Serpstat)

2) Konfigürasyon (Configuration)

Dosya: PromptValueEstimator.Api/appsettings.Development.json

Gerçek token’ın varsa:

{
  "Logging": { "LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" } },
  "AllowedHosts": "*",
  "Serpstat": {
    "BaseUrl": "https://api.serpstat.com",
    "Token": "YOUR_API_TOKEN_HERE",
    "DefaultEngine": "google",
    "DefaultGeo": "US",
    "TimeoutSeconds": 15
  }
}


Token’ın yoksa, sadece test için "Token": "dummy" yaz → servis fallback ile çalışır (confidence düşer ama çökmez).

Çevresel değişkenle override etmek istersen:

# örnek (Linux/macOS)
export Serpstat__Token="YOUR_API_TOKEN_HERE"
# örnek (PowerShell)
$env:Serpstat__Token = "YOUR_API_TOKEN_HERE"

3) Çalıştırma (Run)
Terminalden
dotnet run --project .\PromptValueEstimator.Api\PromptValueEstimator.Api.csproj


Swagger: http://localhost:5092/swagger

Health: http://localhost:5092/healthz

Root: http://localhost:5092/

(İstersen HTTPS için: dotnet dev-certs https --trust + launchSettings.json ayarı.)

4) Endpoint’ler (API)
GET /healthz

Servisin canlılığını, başlama zamanını ve uptime’ı döner.

Örnek response

{
  "status": "ok",
  "startedAt": "2025-10-08T13:50:12.473Z",
  "uptimeSeconds": 392,
  "serverTimeUtc": "2025-10-08T13:56:44.889Z"
}

GET /

Servis bilgisi ve mevcut endpoint listesi.

Örnek response

{
  "Service": "Prompt Value Estimator API",
  "Version": "1.0.0.0",
  "Environment": "Development",
  "Endpoints": ["GET /healthz", "POST /estimate"],
  "TimeUtc": "2025-10-08T13:56:44.889Z"
}

POST /estimate

Body (Request)

{
  "promptText": "explain blockchain in simple terms",
  "languageCode": "en",
  "geoTarget": "US",
  "engine": "google",
  "maxRelatedKeywords": 20,
  "includeTrends": false,
  "similarityThreshold": 0.7,
  "intentFilter": true
}


Response (Örnek)

{
  "promptText": "explain blockchain in simple terms",
  "estimatedMonthlyPromptVolume": 861,
  "confidenceScore": 0.49,
  "confidenceLabel": "Low",
  "confidenceReasons": [
    "No external volume (fallback heuristic)",
    "HighSimShare=0.75"
  ],
  "relatedHighVolumePrompts": [
    {
      "text": "explain blockchain in simple",
      "estimatedVolume": 228,
      "similarity": 0.8,
      "intentScore": 0.95
    }
  ],
  "lastUpdated": "2025-10-08T13:13:18Z"
}


Swagger’da örnek request/response gömülü olarak görünür.

5) Mimari (Architecture) — kısa

API Layer: Minimal API + Swagger (/estimate, /healthz).

Application Layer: PromptEstimator, TextNormalizer, SimilarityScorer (Jaccard), IntentScorer, MediatR Query/Handler.

Infrastructure Layer: SerpstatKeywordExpansionClient (related), SerpstatKeywordVolumeProvider (volume), SerpstatOptions.

Flow

POST /estimate → MediatR → PromptEstimator
  → Normalize + N-grams
  → Related keywords (Serpstat)
  → Volumes (Serpstat)
  → Similarity + Intent + Confidence
  → Result JSON

6) Confidence (Nasıl hesaplanıyor?)
Confidence = 0.40 * HighSimShare
           + 0.25 * CandidateCountFactor
           + 0.35 * ExternalDataFactor


HighSimShare: ≥0.8 benzerlikteki öneri oranı

CandidateCountFactor: farklı aday sayısı (max 1.0’a normalize)

ExternalDataFactor: Serpstat verisi varsa 1.0, yoksa 0.4

7) Geliştirme (Development) — adım adım

Değişiklik yap (ör. PromptEstimator mantığı).

Build

dotnet build


Run

dotnet run --project .\PromptValueEstimator.Api\PromptValueEstimator.Api.csproj


Test → Swagger veya Postman.

Commit & Push

git checkout -b feature/your-change
git add .
git commit -m "feat: improve confidence calculation"
git push origin feature/your-change

8) Sık Karşılaşılan Sorunlar (Troubleshooting)

NU1107 (MediatR version conflict): Tüm projelerde MediatR v13 hizala.

File in use / locked DLL: Run eden dotnet süreçlerini kapat → dotnet clean → bin/obj sil.

Swagger 500: SwaggerDoc/SwaggerUI’yi explicit tanımla; paketleri güncelle.

Serpstat 401/403: Token yanlış/plan kapalı olabilir; trial aktif mi kontrol et.

HTTPS sertifikası: dotnet dev-certs https --trust.

9) Yol Haritası (Roadmap)

DataForSEO entegrasyonu (bonus)

Trend verisi (time-series confidence katkısı)

UI Dashboard (React) basit görselleştirme

Caching & rate-limit ayarları

10) Lisans / Notlar

Bu proje Cognizo Assignment – Problem 1 kapsamında hazırlanmıştır.

Kullanılan dış API’lerin (Serpstat vb.) kullanım koşulları ve ücretlendirmeleri geliştirici sorumluluğundadır.