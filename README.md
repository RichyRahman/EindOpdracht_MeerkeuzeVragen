# MeerkeuzevragenApp — README

## Beschrijving
Een WPF-applicatie voor het opstellen, importeren, beheren en uitvoeren van meerkeuzevragen, gebouwd met een 3-lagenarchitectuur in C# en MySQL. Businesslogica zit in de domeinklassen zelf en wordt gecoördineerd via managerklassen — alle databanktoegang verloopt via pure ADO.NET zonder externe frameworks.

---

## Vereisten

| Software | Versie |
|---|---|
| Visual Studio | 2022 of nieuwer |
| .NET | 9.0 |
| MySQL Server | 8.0 of nieuwer |
| MySQL Workbench | Optioneel |

---

## Installatie

### 1. Database aanmaken
Open MySQL Workbench en voer het meegeleverde script `database_script.sql` uit:
```sql
CREATE DATABASE meerkeuzeDB;
USE meerkeuzeDB;

CREATE TABLE Onderwerp (
	ID INT AUTO_INCREMENT,
    Naam VARCHAR(255) NOT NULL,
    PRIMARY KEY(ID)
);

CREATE TABLE Vraag (
	ID INT AUTO_INCREMENT,
    onderwerpID INT NOT NULL,
    Moeilijkheidsgraad  VARCHAR(255) NOT NULL,
    Tekst VARCHAR(255) NOT NULL,
    isBeschikbaar BOOL DEFAULT TRUE,
    PRIMARY KEY(ID),
    FOREIGN KEY(onderwerpID) REFERENCES Onderwerp(ID)
);

CREATE TABLE Antwoord(
	vraagID INT NOT NULL,
    Tekst VARCHAR(255) NOT NULL,
    isCorrect BOOL DEFAULT FALSE,
    Feedback VARCHAR(255),
    PRIMARY KEY(vraagID, Tekst),
    FOREIGN KEY(vraagID) REFERENCES Vraag(ID)
);

CREATE TABLE Test(
	ID INT AUTO_INCREMENT,
    Naam VARCHAR(255) NOT NULL,
    onderwerpID INT NOT NULL,
    PRIMARY KEY(ID),
    FOREIGN KEY(onderwerpID) REFERENCES Onderwerp(ID)
);

CREATE TABLE TestVragen(
	testID INT NOT NULL,
    vraagID INT NOT NULL,
    PRIMARY KEY(testID, vraagID),
    FOREIGN KEY(testID) REFERENCES Test(ID),
    FOREIGN KEY(vraagID) REFERENCES Vraag(ID)
);

CREATE TABLE Gebruiker(
	ID INT AUTO_INCREMENT,
    Naam VARCHAR(255) NOT NULL,
    PRIMARY KEY(ID)
);

CREATE TABLE GemaakteTest(
	ID INT AUTO_INCREMENT,
    gebruikerID INT NOT NULL,
    testID INT NOT NULL,
    PRIMARY KEY(ID),
    FOREIGN KEY(gebruikerID) REFERENCES Gebruiker(ID),
    FOREIGN KEY(testID) REFERENCES Test(ID)
);

CREATE TABLE GemaakteVraag(
	gemaakteTestID INT NOT NULL,
    vraagID INT NOT NULL,
    Tekst VARCHAR(255),
    PRIMARY KEY(gemaakteTestID, vraagID),
    FOREIGN KEY(gemaakteTestID) REFERENCES GemaakteTest(ID),
    FOREIGN KEY(vraagID) REFERENCES Vraag(ID),
    FOREIGN KEY(vraagID, Tekst) REFERENCES Antwoord(vraagID, Tekst)
);
```

### 2. Connectiestring instellen
De connectiestring wordt **niet** in de UI-laag bewaard, maar uitsluitend gelezen door de DATA-laag via `App.config`. Open `App.config` in het **UI-project** (`MeerkeuzeVragenApp.UI`) en pas de connectiestring aan:
```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <connectionStrings>
    <add name="MeerkeuzeDB"
         connectionString="Server=localhost;Port=3306;Database=meerkeuzeDB;User ID=root;Password=jouwwachtwoord;"
         providerName="MySql.Data.MySqlClient"/>
  </connectionStrings>
</configuration>
```
De klasse `DatabaseConnection` (in `MeerkeuzevragenApp.DATA`) leest deze string in via `ConfigurationManager` — de UI-laag kent enkel de `DatabaseConnection`-klasse, niet de connectiestring zelf.

### 3. NuGet packages herstellen
Open de solution in Visual Studio en klik:
```
Build → Restore NuGet Packages
```

Gebruikte packages:
- `MySql.Data` — MySQL connectie (ADO.NET)
- `System.Configuration.ConfigurationManager` — inlezen `App.config`
- `xunit` — Unit testing framework
- `xunit.runner.visualstudio` — Test Explorer integratie

> Er worden **geen** ORM- of mocking-frameworks gebruikt (geen Dapper, geen Entity Framework, geen Moq). Alle databanktoegang gebeurt via pure ADO.NET (`MySqlConnection`, `MySqlCommand`, `MySqlDataReader`).

### 4. Applicatie starten
Stel `MeerkeuzeVragenApp.UI` in als startup project en druk op `F5`.

---

## Gebruik

### Data importeren
1. Klik op **Data Importeren**
2. Kies een `.txt` vragenlijst via **Bladeren**
3. Vul een onderwerpnaam in (bv. `SQL`, `Muziek`)
4. Kies een moeilijkheidsgraad
5. Klik op **Importeer**

Ondersteunde bestandsformaten worden automatisch herkend via het `ITestParser`-mechanisme (zie [Schaalbaarheid: ITestParser](#schaalbaarheid-itestparser)):
- **Standaard formaat** — antwoorden onderaan na een regel `Antwoorden`
- **Correct-formaat** — `Correct: X` direct na elke vraag, 5 antwoordopties (A–E)

### Test opstellen
1. Klik op **Test Opstellen**
2. Vul een testnaam in
3. Kies een onderwerp en aantal vragen
4. Klik op **Genereer Test** — er wordt een willekeurige selectie van beschikbare vragen gemaakt
5. Optioneel: exporteer naar `.txt` via **Exporteer naar .txt** (antwoorden worden hierbij geschud per vraag)

### Vragen beheren
1. Klik op **Vragen Beheren**
2. Filter op onderwerp en beschikbaarheid
3. Voeg nieuwe vragen toe via het formulier bovenaan — validatie gebeurt in de domeinklasse `Vraag` zelf
4. Selecteer een vraag en klik **Stel Niet Beschikbaar** om te deactiveren (soft delete, vraag blijft in de databank)

### Test uitvoeren
**Interactief:**
1. Klik op **Test Uitvoeren**
2. Kies een test en vul een gebruikersnaam in
3. Klik **Start** en beantwoord de vragen — antwoorden worden per vraag geschud
4. Na het indienen zie je score en feedback per fout beantwoorde vraag

**Bulk verwerking:**
1. Ga naar tabblad **Bulk Verwerking**
2. Kies een test en laad een CSV-bestand in
3. CSV formaat:
```
IDGebruiker,Antwoorden
101,ABCDABCDAB
102,AAAAABBBBB
```
4. Klik **Verwerk Bulk** voor scores van alle gebruikers

---

## Architectuur

De applicatie volgt een **3-lagenarchitectuur** waarbij het domein centraal staat. Zowel de DATA-laag als de UI-laag kennen het domein, maar het domein kent geen van beide.

```
MeerkeuzevragenApp.DOMEIN  (centraal — geen dependencies)
        ↑                              ↑
MeerkeuzevragenApp.DATA      MeerkeuzeVragenApp.UI
   (implementeert interfaces)   (gebruikt managers)
```

### MeerkeuzevragenApp.DOMEIN
Het hart van de applicatie. Bevat geen verwijzingen naar andere projecten.

- **Model/** — domeinklassen met ingebouwde validatie en businesslogica:
  - `Vraag` — `Valideer()`, `IsCorrectAntwoord()`, `GetFeedback()`, `GetGeschuddeAntwoorden()`
  - `Antwoord`, `Onderwerp`, `Gebruiker`, `GemaakteTest`, `GemaakteVraag`
  - `Test` — `BerekenScore()`, `ExporteerNaarTxt()`
  - `VraagManager` — coördineert vraagbeheer via `IVraagRepository`
  - `TestManager` — coördineert testgeneratie, import en bulkverwerking via `ITestRepository`, `IVraagRepository` en `ITestParser`
- **Interfaces/** — `IVraagRepository`, `ITestRepository`, `ITestParser`
- **Exceptions/** — `DomeinException`

Validatie gebeurt in de **setters** van de domeinklassen zelf (bv. `Vraag.Tekst` gooit een `DomeinException` bij een lege waarde), en businessregels zoals scoreberekening en feedback zitten als methoden **op** de domeinobjecten — niet in een afzonderlijke servicelaag.

### MeerkeuzevragenApp.DATA
Implementeert de interfaces uit het DOMEIN met pure ADO.NET.

- `DatabaseConnection` — leest de connectiestring uit `App.config` via `ConfigurationManager`
- **Repositories/**
  - `VraagRepository : IVraagRepository`
  - `TestRepository : ITestRepository`
- **Parsers/**
  - `StandaardFormaatParser : ITestParser`
  - `CorrectFormaatParser : ITestParser`

Alle databankoperaties gebruiken `MySqlConnection`, `MySqlCommand` en `MySqlDataReader` met geparametriseerde queries. Het toevoegen van een vraag met antwoorden gebeurt binnen een transactie (`BeginTransaction` / `Commit` / `Rollback`).

### MeerkeuzeVragenApp.UI
WPF-schermen die uitsluitend communiceren met `VraagManager` en `TestManager` uit het DOMEIN. De UI heeft geen kennis van de DATA-laag, ADO.NET of de connectiestring — deze instanties worden eenmalig samengesteld in `App.xaml.cs`:

```csharp
var db = new DatabaseConnection();
var vraagRepo = new VraagRepository(db);
var testRepo = new TestRepository(db);
var parsers = new List<ITestParser>
{
    new CorrectFormaatParser(),
    new StandaardFormaatParser()
};

VraagManager = new VraagManager(vraagRepo);
TestManager = new TestManager(testRepo, vraagRepo, parsers);
```

- `MainWindow` — navigatie
- `Views/ImportView`, `Views/VraagBeheerView`, `Views/TestBeheerView`, `Views/TestUitvoerenView`

### MeerkeuzevragenApp.TESTS
xUnit-testproject dat **rechtstreeks de domeinklassen** test — zonder Moq, zonder database.

---

## Schaalbaarheid: ITestParser

Om nieuwe importformaten te ondersteunen zonder bestaande code aan te passen, definieert het DOMEIN de interface:

```csharp
public interface ITestParser
{
    bool KanVerwerken(string[] regels);
    List<Vraag> Parse(string[] regels, int onderwerpID, string moeilijkheid);
}
```

`TestManager.ImporteerBestand()` doorloopt de geregistreerde parsers en gebruikt de eerste die `KanVerwerken()` met `true` beantwoordt. Een nieuw bestandsformaat toevoegen vereist enkel:

1. Een nieuwe klasse in `MeerkeuzevragenApp.DATA/Parsers/` die `ITestParser` implementeert
2. Deze klasse toevoegen aan de parserlijst in `App.xaml.cs`

Bestaande parsers, managers en repositories blijven ongewijzigd.

---

## Projectstructuur

```
EindOpdracht_MeerkeuzeVragen.sln
├── MeerkeuzevragenApp.DOMEIN
│   ├── Model/
│   │   ├── Vraag.cs
│   │   ├── Antwoord.cs
│   │   ├── Onderwerp.cs
│   │   ├── Test.cs
│   │   ├── Gebruiker.cs
│   │   ├── GemaakteTest.cs
│   │   ├── GemaakteVraag.cs
│   │   ├── VraagManager.cs
│   │   └── TestManager.cs
│   ├── Interfaces/
│   │   ├── IVraagRepository.cs
│   │   ├── ITestRepository.cs
│   │   └── ITestParser.cs
│   └── Exceptions/
│       └── DomeinException.cs
├── MeerkeuzevragenApp.DATA
│   ├── DatabaseConnection.cs
│   ├── App.config
│   ├── Repositories/
│   │   ├── VraagRepository.cs
│   │   └── TestRepository.cs
│   └── Parsers/
│       ├── StandaardFormaatParser.cs
│       └── CorrectFormaatParser.cs
├── MeerkeuzeVragenApp.UI
│   ├── App.xaml(.cs)
│   ├── MainWindow.xaml(.cs)
│   └── Views/
│       ├── ImportView.xaml(.cs)
│       ├── VraagBeheerView.xaml(.cs)
│       ├── TestBeheerView.xaml(.cs)
│       └── TestUitvoerenView.xaml(.cs)
└── MeerkeuzevragenApp.TESTS
    ├── VraagTests.cs
    ├── AntwoordTests.cs
    ├── OnderwerpTests.cs
    └── TestTests.cs
```

**Project references:**
```
DOMEIN  → geen dependencies
DATA    → DOMEIN
UI      → DOMEIN, DATA
TESTS   → DOMEIN
```

---

## Unit tests uitvoeren
```
Test → Run All Tests
```
33 tests verwacht, verdeeld over de domeinklassen:
- `VraagTests` — 13 tests (validatie, `IsCorrectAntwoord`, `GetFeedback`, `GetGeschuddeAntwoorden`, `Valideer`)
- `TestTests` — 10 tests (validatie, `BerekenScore`, `ExporteerNaarTxt`)
- `AntwoordTests` — 4 tests (validatie, constructor)
- `OnderwerpTests` — 6 tests (validatie, constructors, `ToString`)

Alle tests werken **rechtstreeks op domeinobjecten** — er is geen database, geen Mock en geen servicelaag nodig om deze uit te voeren.
