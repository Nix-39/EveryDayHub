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
### Grund – DataStore<T> och Pause

`DataStore<T>` är den generiska lagringsklassen som alla rum använder för att hålla en lista i minnet och spara den som JSON.

- `AddItem()`, `RemoveItem()`, `FindItem()` och `GetAll()` hanterar listan. `GetAll()` returnerar en kopia av listan, så att andra klasser inte kan lägga till eller ta bort objekt direkt i den.
- `SaveToJson()` och `LoadFromJson()` sparar och läser filen med `System.Text.Json`.
- Fel som saknad fil, saknad behörighet, I/O-fel och ogiltig JSON fångas med `try/catch`. Metoden returnerar då `false` och förklaringen finns i `LastError`.
- `Pause.Wait()` väntar på en knapptryckning efter varje menyval, så att användaren hinner läsa resultatet.

**Designmotivering:**

- **Generisk klass (`where T : class`):** samma kod används för familjemedlemmar, recept, matvaror, lappar och schema, i stället för en lagringsklass per typ.
- **Lagring och visning är separerade (Single Responsibility):** `DataStore<T>` sköter data och filer, medan UI-klasserna sköter text och färger. Därför skriver `DataStore<T>` aldrig något på skärmen. Den returnerar `true` eller `false`, och menyn visar felet från `LastError` i rött.

### Familjen – Familjemedlemmar

Rummet hanterar familjens medlemmar, som sedan används i schemat och som författare till lappar.

- **Visa:** alla familjemedlemmar i en tabell.
- **Lägg till:** namnet får inte vara tomt. Nytt Id blir högsta befintliga Id + 1.
- **Ändra:** nytt namn, eller Enter för att behålla det gamla.
- **Ta bort:** kräver bekräftelse, och Nej är förvalt så att ingen tas bort av misstag.

Funktionen finns i `UI/FamilyMenu.cs` och `Models/FamilyMember.cs` och sparas i `Data/familymembers.json`. Valet av person görs med `SelectionPrompt<FamilyMember>` och `UseConverter()`, och namnen visas med `Markup.Escape()` så att tecken som `[ ]` inte får programmet att krascha.

### Recept – Receptrummet

Receptrummet låter familjen visa, lägga till, ändra och ta bort recept.

- **Visa alla:** tabell med nummer, namn, tillagningstid och ingredienser, sorterad på Id.
- **Visa ett recept:** en ram (`Panel`) med tillagningstid, ingredienserna på egna rader och beskrivningen.
- **Lägg till:** namn, tid i minuter, ingredienser separerade med komma och beskrivning.
- **Ändra:** Enter behåller det nuvarande värdet i varje fält.
- **Ta bort:** kräver bekräftelse, och Nej är förvalt.

All inmatning kontrolleras innan något ändras: tomt namn, namn som redan finns (oavsett versaler och gemener), tid som inte är ett heltal större än 0, minst en ingrediens och tom beskrivning. Ett fel lämnar därför aldrig ett recept halvändrat.

Ingredienserna delas upp av hjälpmetoden `ParseIngredients()`, som använder `Split()` med `TrimEntries` och `RemoveEmptyEntries`, gör om texten till små bokstäver och tar bort dubbletter med `Distinct()`. Då matchar de kylskåpets matvaror i "Vad kan vi laga?".

Funktionen finns i `UI/RecipeMenu.cs` och `Models/Recipe.cs`. Recepten sparas i `Data/recipes.json`, samma fil som Kylskåpet och startsidan läser, så ändringar syns direkt där.

### Recept – Sök och sortera

- Sök på en del av ett receptnamn eller en ingrediens, oavsett versaler och gemener.
- En tom sökning visar alla recept, så att hela listan kan sorteras.
- Sortera på tillagningstid med kortast eller längst tid först. Recept med samma tid sorteras i bokstavsordning.
- Antalet träffar visas. Om inget recept matchar visas ett gult meddelande.

Implementationen använder LINQ-metoderna `Where()`, `Any()`, `OrderBy()`, `OrderByDescending()` och `ThenBy()`, samt `Contains()` med `StringComparison.OrdinalIgnoreCase`. Tabellen ritas av hjälpmetoden `DisplayTable()`, som delas med "Visa alla" så att tabellkoden bara finns på ett ställe.

### Recept – Ångra och gör om

- **Ångra** går tillbaka till listan som den såg ut före den senaste ändringen (lägga till, ändra eller ta bort). Det går att ångra flera steg i rad.
- **Gör om** återställer det som ångrats. En ny ändring efter en ångring tömmer gör om-historiken.
- Efter varje ångring och omgörning visas receptlistan, så att resultatet syns direkt.
- Finns det inget att ångra eller göra om visas ett gult meddelande.

**Designmotivering:**

- **I `DataStore<T>`:** funktionen ligger i lagringsklassen och inte i menyn, så att andra rum kan få den med två nya menyval.
- **Två `Stack<string>`:** en stack lämnar ut den senast tillagda versionen först, vilket är precis den ordning som behövs för att ångra.
- **JSON-text i stället för en kopia av listan:** Ändra ändrar receptobjekten direkt. En kopia av listan skulle peka på samma objekt och ändras den också, medan JSON-texten är en fast ögonblicksbild.
- **Bara verkliga ändringar sparas som en ny version**, så en sparning utan ändring ger inget tomt ångra-steg.
- **Minnet och filen hålls i takt:** `Restore()` läser versionen och skriver filen först, och ändrar listan i minnet och stackarna först när båda stegen har lyckats. Misslyckas sparningen visas ett rött fel och ingenting ändras.

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
Ingredienser jämförs som text, så "tomat" och "tomater" räknas som olika ingredienser. Ångra och gör om finns i receptrummet, och historiken nollställs när programmet startas om. Två familjemedlemmar kan ha samma namn.
