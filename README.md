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

**Kända begränsningar:** Receptmatchningen tar inte hänsyn till ingrediensmängder. Startsidan visar endast det första matchande middagsförslaget. Visning av ett faktiskt arbetspass eller en aktivitet på startsidan har ännu inte verifierats manuellt.