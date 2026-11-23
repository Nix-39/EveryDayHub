# EveryDayHub
A daily family hub with schedules, notes and meals - C# group project
## Funktioner

### Dagens översikt – Startsidan

Startsidan ger familjen en snabb överblick över dagen och visar:

- **Dagens schema:** Dagens aktiviteter och arbetspass, sorterade efter starttid.
- **Lappar:** Sparade meddelanden med text och författare.
- **Middagsförslag:** Ett recept som kan lagas med tillgängliga ingredienser.

Funktionen finns i `UI/StartScreen.cs` och använder C#, LINQ, `DataStore<T>` och Spectre.Console.

### Kylskåpet – Bäst före-koll

Funktionen hjälper användaren att upptäcka matvaror som har passerat eller närmar sig sitt utgångsdatum.

- Röd varning för passerat datum.
- Gul varning för matvaror inom varningsgränsen.
- Sortering efter utgångsdatum.
- Grönt meddelande när inga varningar finns.

Varningsgränserna är 3 dagar för mejeriprodukter, 2 dagar för grönsaker och 1 dag för kött.

Implementationen använder `FridgeItem`, `ExpirationDate`, `WarningDays` samt LINQ-metoderna `OfType()`, `Where()` och `OrderBy()`.

### Kylskåpet – Vad kan vi laga?

Funktionen föreslår recept utifrån de ingredienser som finns tillgängliga hemma.

- Läser matvaror och recept från `DataStore<T>`.
- Kontrollerar tillgängliga ingredienser.
- Matchar recept där alla ingredienser finns hemma.
- Visar möjliga recept och tillagningstid.
- Visar ett meddelande när inga recept kan lagas.

LINQ-metoderna `Where()`, `All()` och `Any()` används för matchningen.

Receptnamn hanteras med `Markup.Escape()` för att specialtecken som `[ ]` ska kunna visas korrekt.

Matchningen kontrollerar ingrediensnamn och tillgänglighet, men inte exakta mängder.

### Lappar – Välj författare (Issue #53)

När en ny lapp skapas kan användaren välja en registrerad familjemedlem som författare.

- Familjemedlemmar hämtas från `DataStore<FamilyMember>`.
- Valet görs med `SelectionPrompt<FamilyMember>`.
- Författarens namn sparas i `Note.Author`.
- Om inga familjemedlemmar finns visas ett informationsmeddelande.

Detta minskar risken för felstavade namn och gör hanteringen mer konsekvent.

### Modeller – Lappar och inköp

Modellerna beskriver den data som lapparna och inköpslistan arbetar med.

- `Note` ärver från den abstrakta klassen `BoardItem` och har text, författare, typ och datum. Klassen skriver sin egen `Describe()`.
- `ShoppingItem` ärver inte från något och har Id, namn, antal (`int`) och en flagga `IsBought`.
- `Note` är registrerad med `[JsonDerivedType]` i `BoardItem`, så att den fungerar med JSON-lagring.

### Lappar – Visa, lägg till, ändra och ta bort

Lappmenyn (`UI/NotesMenu.cs`) innehåller de fyra grundfunktionerna (CRUD).

- Lapparna visas i en Spectre.Console-tabell med typ, text, författare och datum.
- En ny lapp skapas med typ, text och författare. Datumet sätts automatiskt.
- Ändra och ta bort delar hjälpmetoden `PickNote`, där användaren väljer lapp med piltangenterna.
- Texten skyddas med `Markup.Escape()` så att specialtecken som `[ ]` visas korrekt.
- Om det inte finns några lappar visas ett meddelande i stället för en tom tabell.

### Lappar – Sök, filtrera och sortera

Undermenyn "Sök och filtrera" använder LINQ.

- **Sök på text:** `Where()` med `Contains()`. Sökningen skiljer inte på stora och små bokstäver.
- **Filtrera på person och typ:** `Select()`, `Distinct()` och `OrderBy()` tar fram valbara värden, och `Where()` behåller lappar som matchar. Samma metod används för båda, genom att skicka in vilken egenskap som ska användas.
- **Sortera på datum:** `OrderByDescending()` (nyast först) eller `OrderBy()` (äldst först).
- Tabellen ritas av en gemensam hjälpmetod, `DisplayTable`, så att koden inte upprepas.

### Laddning och sparning

Data sparas som JSON och läses in när programmet startar (`Program.cs`).

- Vid start laddas lappar och inköpslistan med en progress bar (`AnsiConsole.Progress()`).
- Vid avslut sparas allt med en spinner (`AnsiConsole.Status()`).
- Filerna `notes.json` och `shopping.json` ligger i mappen `Data` bredvid programmet.
- Första körningen finns ingen fil, och då startar listorna tomma utan fel.
- Om laddning eller sparning misslyckas visas felet i rött, och programmet kraschar inte.
- Mappen `Data` skapas automatiskt om den saknas.

### Inköpslistan

Inköpslistan (`UI/ShoppingMenu.cs`) använder samma mönster som lappmenyn.

- Varor kan visas, läggas till, bockas av (och ångras) och tas bort.
- Listan visas i en tabell med bock, namn och antal.
- Antal läses in som heltal, och användaren får fråga igen om något annat skrivs.
- Varje vara får ett eget Id (högsta befintliga Id + 1).
- Meddelanden skrivs med klassen `Messages`, så att utseendet är detsamma i hela programmet.
- Listan sparas och laddas tillsammans med lapparna.

### Huvudmenyn

Huvudmenyn (`UI/MainMenu.cs`) är en loop med en `SelectionPrompt` och en `switch` som skickar användaren vidare till respektive meny. Loopen avslutas när användaren väljer Avsluta.

### Designval – Lappar och inköp

- **Generiska `DataStore<T>`:** samma klass lagrar lappar, varor och recept utan att koden skrivs om för varje typ.
- **Gemensamma datalådor:** lådorna skapas en gång och skickas in i menyerna, så att menyerna och sparningen arbetar mot samma data. 
- **Hjälpmetoder (`PickNote`, `DisplayTable`):** koden som flera funktioner behöver skrivs en gång. 
- **LINQ för sökning:**

### Teknik

Funktionerna är utvecklade med C#, .NET, OOP, LINQ, Spectre.Console, generiska `DataStore<T>` och JSON-baserad datalagring.

### Testning

Funktionerna har testats manuellt under utvecklingen. Genomförda kontroller omfattar:

- Varningar för utgångsdatum och korrekt sortering.
- Receptförslag utifrån tillgängliga ingredienser.
- Hantering när inga recept kan lagas.
- Visning av dagens översikt och sparade lappar.
- Val av familjemedlem som författare.
- Hantering av specialtecken i receptnamn.
- Lyckade byggen med `dotnet build`.
- Lägga till, visa, ändra och ta bort lappar och varor.
- Sökning på text, filtrering på person och typ, och sortering på datum.
- Att lappar och varor ligger kvar efter att programmet stängts och startats igen.
- Hantering av tom lista och ogiltig inmatning av antal.

**Kända begränsningar:** Receptmatchningen tar inte hänsyn till ingrediensmängder. Startsidan visar endast det första matchande middagsförslaget. Visning av ett faktiskt arbetspass eller en aktivitet på startsidan har ännu inte verifierats manuellt.