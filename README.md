# IsakChat

Klijent-server chat aplikacija za odbranu ispita (91/24) — .NET MAUI klijent + ASP.NET Core Web API server.

Ideja: omanji "Discord-lite" — opšta soba u koju svi ulogovani korisnici pišu, privatni četovi (DM)
sa drugim korisnicima, slanje slika, odgovaranje (reply) na konkretnu poruku i brisanje sopstvene
poruke (tap na poruku otvara meni sa tim opcijama).

## Struktura projekta

```
IsakChat.sln
src/
  IsakChat.Shared/   - zajednički modeli (DTO-i) koje koriste i server i klijent
  IsakChat.Server/   - ASP.NET Core Web API + SQLite baza (na serveru)
  IsakChat.App/      - .NET MAUI klijent (Android / Windows)
```

Zašto ovako: da server i klijent ne duplirају definicije poruke/korisnika, oba projekta referenciraju
`IsakChat.Shared` i razmenjuju iste C# klase (serijalizovane u JSON).

## Kako se koje gradivo sa predmeta mapira na kod

| Tema sa predmeta | Gde je u projektu |
|---|---|
| Organizacija projekta u MAUI app-u | `src/IsakChat.App` - Views / ViewModels / Services / Controls / Converters su odvojeni folderi |
| AppShell, kretanje po stranicama | `AppShell.xaml(.cs)` - TabBar + `Routing.RegisterRoute` za login/register/dmchat/newdm, navigacija preko `Shell.Current.GoToAsync(...)` |
| XAML | sve `.xaml` stranice u `Views/` i `Controls/` |
| Resursi | `Resources/Styles/Colors.xaml`, `Styles.xaml`, `Converters.xaml`, `Resources/AppIcon`, `Resources/Splash` |
| Data Binding | svuda - `{Binding ...}` u XAML-u ka ViewModel property-jima |
| MVVM arhitektura | `ViewModels/` (CommunityToolkit.Mvvm - `[ObservableProperty]`, `[RelayCommand]`), `Views/` ne sadrže logiku, samo XAML + minimalni code-behind |
| Lokalna baza (SQLite) | `Services/LocalDatabase.cs` (sqlite-net-pcl) - kešira poruke i čuva sesiju na uređaju |
| Rad sa fajlovima lokalno | slike se biraju preko `MediaPicker` (galerija/kamera) i šalju serveru; server ih čuva na disku (`wwwroot/uploads`) |
| Kontrole | standardne MAUI kontrole (`CollectionView`, `Entry`, `Border`...) kroz cele stranice |
| Trigeri | `Resources/Styles/Styles.xaml` (Entry fokus, Button pressed/disabled) i `Controls/MessageBubbleView.xaml` (`DataTrigger` na `IsMine` menja boju balončića, `DataTrigger` na `IsDeleted` prigušuje obrisanu poruku) |
| Stilovi | `Resources/Styles/Styles.xaml` - implicitni i named stilovi za sve kontrole |
| CollectionView | `Controls/ChatBodyView.xaml` (lista poruka), `Views/DmListPage.xaml`, `Views/NewDmPage.xaml` |
| Korisničke (custom) kontrole | `Controls/MessageBubbleView` (balončić poruke, sopstveni `BindableProperty`-ji) i `Controls/ChatBodyView` (deljeno telo ceta - lista + traka za pisanje, koriste ga i opšta soba i DM) |
| Async/await, Task, kolekcija Task-ova | `Services/ApiClient.cs`, `ViewModels/ChatViewModelBase.cs` (`PollLoopAsync` sa `CancellationToken`) |
| Klijent-server asinhrona komunikacija | `Services/ApiClient.cs` (HttpClient) ↔ `IsakChat.Server/Endpoints/*.cs` |
| Komunikacija sa web servisom (API) | REST API (`/api/auth`, `/api/messages`, `/api/users`, `/api/conversations`) - JSON + multipart (za slike) |
| Logovanje i sesija korisnika | `IsakChat.Server/Services/SessionAuth.cs` - token sesije u bazi, klijent ga šalje kao `Authorization: Bearer <token>`, `SessionService` na klijentu ga pamti lokalno (auto-login) |
| Prikaz podataka sa web servisa | `PublicChatViewModel` / `DmChatViewModel` - učitavaju keš pa se osvežavaju sa servera (polling na 3s) |
| TabbedPage koncept | `AppShell.xaml` - `<TabBar>` sa 3 taba: Četovnica / Poruke / Profil |

## Arhitektura komunikacije

Nema real-time (SignalR i sl. namerno nije korišćen - nije bilo na predavanjima). Klijent na svakoj
otvorenoj cet-stranici na svake 3 sekunde pita server "ima li novih poruka posle Id-ja X"
(`GET /api/messages/public?afterId=...`) - klasičan primer asinhronog rada sa `Task`/`await` i
periodičnog osvežavanja, bez potrebe za websocket-ima.

Autentifikacija je sopstvena (ne JWT) - login vraća nasumičan token koji se čuva u tabeli `Sessions`
na serveru; klijent ga šalje u `Authorization: Bearer <token>` headeru. Jednostavnije za objasniti na
odbrani od JWT potpisivanja.

## Pokretanje servera

```bash
cd src/IsakChat.Server
dotnet run
```

Server sluša na `http://localhost:5203` (i na `0.0.0.0:5203`, pa je dostupan i sa drugih uređaja na
istoj mreži). Baza (`isakchat.db`, SQLite fajl) i `wwwroot/uploads` folder se prave sami pri prvom
pokretanju - ne treba ništa ručno podešavati.

## Pokretanje klijenta

### Na Linuxu (razvoj) - Android

Ovaj računar nema Windows/iOS SDK, pa se ovde može buildovati/testirati samo Android cilj.

```bash
export DOTNET_ROOT="$HOME/.dotnet"
export PATH="$HOME/.dotnet:$PATH"
export ANDROID_HOME="$HOME/Android/Sdk"
export ANDROID_SDK_ROOT="$HOME/Android/Sdk"

cd src/IsakChat.App
dotnet build -f net8.0-android -t:Run
```

**Fizički telefon preko USB-a** (Isak već ima uključen Developer mode + USB debugging):
1. Prikačiti telefon kablom, dozvoliti "USB debugging" na telefonu ako pita.
2. `adb devices` - telefon treba da se pojavi na listi.
3. `adb reverse tcp:5203 tcp:5203` - ovo "provuče" server sa računara na telefon, tako da telefon
   može da koristi `http://localhost:5203` kao adresu servera (isto kao da je server na njemu).
4. Server adresa u app-u (na login ekranu) ostaje `http://localhost:5203`.

### Na Windows-u (odbrana)

1. Instalirati **Visual Studio 2022** sa workload-om **.NET Multi-platform App UI development**.
2. Otvoriti `IsakChat.sln` - Windows cilj (`net8.0-windows10.0.19041.0`) se automatski pojavljuje u
   `IsakChat.App.csproj` (uslov u fajlu proverava da li je OS Windows) - ne treba ništa menjati ručno.
3. Server i klijent mogu da rade na istom računaru - pokrenuti `IsakChat.Server` (F5 ili `dotnet run`),
   pa `IsakChat.App` (izabrati Windows Machine ili Android telefon kao target u VS-u).
4. Ako se ipak testira na fizičkom telefonu preko USB-a na Windows-u, isti `adb reverse tcp:5203 tcp:5203`
   trik radi identično (adb dolazi sa Android workload-om iz Visual Studio-a).

## Funkcionalnosti

- Registracija / login (lozinke hešrane PBKDF2-om, `Server/Services/PasswordHasher.cs`)
- Opšta ("zajednička") soba - svi ulogovani korisnici vide iste poruke
- Privatni četovi (DM) sa bilo kojim drugim korisnikom
- Slanje slika (iz galerije ili direktno kamerom)
- Tap na poruku otvara meni: **Odgovori** (prikaže se traka "Odgovaraš: ..." iznad polja za kucanje),
  ili **Obriši poruku** (samo za sopstvene poruke - server proverava da si ti pošiljalac, 403 inače).
  Obrisana poruka ostaje u istoriji kao "🚫 Poruka je obrisana" (kurziv, zatamnjeno) umesto da
  potpuno nestane - tako reply lanci ostaju ispravni i vidi se da je nešto bilo obrisano.
- Lokalni keš poruka (SQLite na uređaju) - stranica se odmah popuni iz keša, pa se osveži sa servera
- Auto-login - sesija se pamti lokalno, ne treba ponovo login posle gašenja app-a
- Odjava (briše lokalnu sesiju i keš)

## Poznato ograničenje

Brisanje poruke se odmah vidi kod onoga ko je obrisao (i u njegovom lokalnom kešu), ali kod DRUGIH
korisnika će se osvežiti tek ako ta poruka bude ponovo učitana (npr. novi login) - polling sistem
osvežava samo poruke sa VEĆIM Id-jem od poslednje viđene, ne i izmene postojećih poruka. Za pravu
sinhronizaciju u realnom vremenu bi trebao SignalR ili slično - namerno izostavljeno (nije bilo u
gradivu, videti napomenu o arhitekturi komunikacije gore).

## Šta bi bilo lepo dodati (ako ostane vremena/volje)

- Ikonice u tabovima (trenutno samo tekst)
- Push-style indikator "nova poruka" na tabu Poruke
- Editovanje sopstvene poruke (brisanje je već dodato)
