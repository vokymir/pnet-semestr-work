# Ptáčková aplikace - dokumentace

Aplikace využívá dotnet SDK 8 (a 9), jazyk C# a BlazorWASM pro vizualizaci.

## Spuštění

Je popsáno v README.md.

## Struktura

Hlavní solution *BirdWatching* obsahuje projekty:

- Api
- Client
- Shared

Navíc jsou tu ještě

- SetupViaCLI
- ConsoleClient
- downloading_birds

Což jsou pomocné projekty.

### Shared

Obsahuje společné části, tedy hlavně model, interfacey a něco tu zbylo.
Všechny funkce komunikující přímo s databází jsou zde také.
Navíc je zde i automaticky generovaný klient pro api.

### Api

Kostra aplikace, obsahuje kontrollery a vystavuje API pro kohokoliv.
Také je tu databáze (SQLite) a její migrace.

### Client

BlazorWASM aplikace, uživatelské rozhraní, které komunikuje se serverem
pomocí API, respektive pomocí klienta v Shared projektu.

## Databáze

Používá se SQL databáze, konkrétně SQLite. V té se nachází tabulky pro

- User - uživatel
- Watcher - ptáčkař
- Bird - ptáček
- Record - záznam
- Event - soutěž

Navíc některé propojovací tabulky. Aby se umožnilo uživatelům spravovat více *účtů*
najednou, vznikli *ptáčkaři*. Tedy každý uživatel (user) si pro pozorování musí
vytvořit svého ptáčkaře - navíc ale může spravovat ještě další, např. své děti.
Každý ptáčkař si zapisuje všechny ptáčky, které vidí. K tomu slouží záznamy (record)
které se váží přímo k ptáčkaři, ne k soutěžím. Do soutěží se může ptáčkař
přihlašovat, a jeho skóre se spočítá automaticky podle toho, kolik ptáčků odpovídá
parametrům soutěže. Soutěže jsou vytvářeny také uživateli.

Všechny entity jsou popsány pomocí C# tříd v projektu *Shared*. Navíc ke každé je
vytvořený její DTO - i s metodami, které je převádí. Aby se zabránilo zacyklení,
tak metoda ToDto (kterou mají třídy) nepřevádí žádné vazbou propojené entity.
To se ale často hodilo, proto je metoda ToFullDto, která převede jednu úroveň.
Funce na opačnou stranu jsou sice napsány, ale prakticky nepoužity, kvůli omezení
EF, která je pak nerozpoznala.

## Api projekt

Vycházelo se ze základního ukázkového projektu webapi. Pro přehlednost má každá oblast
vlastní kontroler, všechny jsou oddědeny od BaseControlleru, aby měly nějaké společné
funkce a lehčí práce pro mě.

Používají se Nswag anotace, protože pomocí Nswagu se i automaticky generuje klient.
Generace klienta probíhá v projektu *Shared* v době buildu. Celá je definovaná
v .csproj souboru, kde akorát pomocí příkazů pro CLI automaticky modifikuje soubor
BirdApiClient.cs v podsložce Api.

## BlazorWASM

V uživatelském rozhraní jsou vytvořené stránky pro všechny důležité akce, jako
jsou například nový záznam, přehled soutěží, ...
Pro uchování informací o přihlášeném uživateli je použito LocalStorage pomocí
[Blazored](https://github.com/Blazored/LocalStorage), které umožňuje přístup
bez nutnosti použití JS.

Když došlo na vytváření Services, nabyl jsem dojmu, že už je to zbytečné,
dost se mi příčilo opakovaně psát skoro stejný kód nejdřív pro databázi,
pak pro api kontroler a pak pro servisu. Pozdě jsem seznal svůj omyl, proto
nejsou servisy téměř využité.

Frontend práci jsem si zjednodušil díky Bootstrapu. Mnohdy jsem ale po
dokončení strany nechal AI ji projet a vytvořit modernější vzhled. Kvůli
tomu není web vizuálně jednotný, ale to by nebyl ani kdybych jej dělal
sám, a vypadá lépe.

## Autentizace

Nejprve jsem chtěl tuto část obejít a prostě prohlásit, že data uložená v tomto
projektu nejsou natolik důležitá, a mít prostě heslo (byť hashnuté) uložené
v databázi a vesele si ho posílat po síti. Pak jsem si řekl, že k tomu účelu
vyrobím tokeny, aby se alespoň heslo neprohánělo volně po netu. Když byly téměř hotové,
zjistil jsem, že implementace kontroly by byla hodně otravná, a když už existuje
řešení v podobě JWT tokenů, rozhodl jsem se ho využít, i když mu i po ukončení
projektu stále plně nerozumím. Ale teď se tokeny generují automaticky,
také se samy validují a je to prostě super.

## Sdílení ptáčkařů a soutěží

K tomu slouží generované kódy o pěti symbolech. Při přibližně 30 možných
je jak pro soutěže, tak pro ptáčkaře 5^30 volných pozic. Víc, než kolik se reálně
využije (leda že by se web dostal pod útok, ale to se snad nestane...).

## Konzolový klient

Původně byl v plánu, ale nakonec nebylo třeba jej odevzdávat a tudíž zůstal
v plenkách. I když bych ho dělal mnohem raději, než Blazor...

## Plnění databáze

Bylo potřeba sehnat běžné české ptáčky a nahrát je do databáze. K tomu účelu
jsem stáhnul webové stránky eBird a následně nechal AI vygenerovat mi python
scripty, které uložily ptáčky do CSV a následně k nim dohledaly další info
na wikipedii.

Celé moje snažení je *zvěčněno* v adresáři *downloading_birds*.

Pro automatické naplnění databáze jsem si nechal od AI napsat krátký projekt
*SetupViaCLI*, který projde CSV a nahraje všechny ptáčky do databáze - musí
se poskytnout přihlašovací údaje, ale k tomu se dají použít super-adminské.

## Nasazení

Než se projekt nasadí a začne používat, musí se projít a změnit pár http
adres a navíc super-adminské přístupové údaje (zatím jméno: string, heslo:
string), JWT hash klíč.

Původně bylo v plánu navíc udělat aplikaci PWA, ale rozhodl jsem se alespoň
v rámci přemětu již toto rozšíření nezahrnout, protože přináší velkou
spousu nových problémů. Notifikace uživatelům také nebyly implementovány.

## Závěr

Projekt je ve fázi, kdy jsem relativně spokojený a myslím si, že nebude
problém jej začít používat. Neimplementoval jsem všechny předsevzaté funkce,
ale s přihlédnutím k tomu, že vlastně celou Blazor část jsem se učil sám,
tak bylo na cestě už tolik nesnází, že jsem to raději ukončil, dokud
to šlo.

Jazyk C# a prostředí dotnet byl dobrým úvodem do API, které jsem nikdy dřív
(před předmětem KIV/PNET) nedělal. Blazor je taky fajn, ale stále jsem
plně nevychytal podporu v NeoVimu, takže jsem spokojený, že jej nějakou
dobu nebudu muset používat.
