# MeerkeuzevragenApp — README

## Beschrijving
Een WPF-applicatie voor het opstellen, importeren, beheren en uitvoeren van meerkeuzevragen, gebouwd met een 3-lagenarchitectuur in C# en MySQL.

---

## Vereisten

| Software | Versie |
|---|---|
| Visual Studio | 2022 of nieuwer |
| .NET | 8.0 |
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
Open `App.xaml.cs` in het UI-project en pas de connectiestring aan:
```csharp
string connectionString = "Server=localhost;Port=3306;Database=meerkeuzeDB;User ID=root;Password=jouwwachtwoord;";
```

### 3. NuGet packages herstellen
Open de solution in Visual Studio en klik:
```
Build → Restore NuGet Packages
```

Gebruikte packages:
- `MySql.Data` — MySQL connectie
- `Dapper` — ORM voor SQL queries
- `Moq` — Mocking voor unit tests
- `xunit` — Unit testing framework

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

Ondersteunde bestandsformaten:
- Standaard formaat (antwoorden onderaan na `Antwoorden`)
- C#/LINQ formaat (`Correct: X` per vraag, 5 antwoordopties)

### Test opstellen
1. Klik op **Test Opstellen**
2. Vul een testnaam in
3. Kies een onderwerp en aantal vragen
4. Klik op **Genereer Test**
5. Optioneel: exporteer naar `.txt` via **Exporteer naar .txt**

### Vragen beheren
1. Klik op **Vragen Beheren**
2. Filter op onderwerp en beschikbaarheid
3. Voeg nieuwe vragen toe via het formulier bovenaan
4. Selecteer een vraag en klik **Stel Niet Beschikbaar** om te deactiveren

### Test uitvoeren
**Interactief:**
1. Klik op **Test Uitvoeren**
2. Kies een test en vul een gebruikersnaam in
3. Klik **Start Test** en beantwoord de vragen
4. Na het indienen zie je score en feedback

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

## Projectstructuur

```
MeerkeuzevragenApp.sln
├── MeerkeuzevragenApp.DOMAIN    → Domeinmodellen
├── MeerkeuzevragenApp.DATA      → Repositories + DB connectie
├── MeerkeuzevragenApp.BUSINESS  → Services + businesslogica
├── MeerkeuzeVragenApp.UI        → WPF schermen
└── MeerkeuzevragenApp.TESTS     → XUnit unit tests
```

---

## Unit tests uitvoeren
```
Test → Run All Tests
```
21 tests verwacht, verdeeld over:
- `VraagServiceTests` — 10 tests
- `TestServiceTests` — 8 tests
- `ImportServiceTests` — 3 tests
