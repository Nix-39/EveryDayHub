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

### Modeller – Familj och schema

Modellerna beskriver familjemedlemmarna och det som visas i schemat.

- `FamilyMember` har Id, namn och roll. Övriga klasser kopplas till en person via personens Id.
- `BoardItem` är en abstrakt basklass med det som alla inlägg har gemensamt: `Id`, `Title`, `Date` och `CreatedById`. Den har den abstrakta metoden `Describe()`.
- `WorkShift` ärver från `BoardItem` och har person (`FamilyMemberId`), starttid och sluttid. Egenskapen `Hours` räknas ut från `End - Start`.
- `Activity` ärver från `BoardItem` och har plats, starttid, sluttid och en lista med deltagare (`ParticipantIds`).
- `WorkShift` och `Activity` skriver var sin `Describe()`, till exempel `Kvällspass: 16:00-22:00 (6 h)` och `Fotboll på Heden, 18:00-19:30`.
- `BoardItem` har `[JsonPolymorphic]` och `[JsonDerivedType]`, så att varje objekt får en `"type"`-rad i JSON-filen och återskapas som rätt subklass vid inläsning.

### Färgade meddelanden

Klassen `Messages` (`UI/Messages.cs`) används av hela programmet för att visa meddelanden på samma sätt.

- `Messages.Error` skriver `Fel:` i rött, till exempel i `catch`-block.
- `Messages.Success` skriver `Klart:` i grönt.
- `Messages.Info` skriver `Info:` i turkost.
- Texten skyddas med `Markup.Escape()` så att specialtecken som `[ ]` inte tolkas som färgkoder.

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

### Schemat – Visa, lägg till, ändra och ta bort

Schemamenyn (`UI/ScheduleMenu.cs`) hanterar familjens arbetspass och aktiviteter.

- **Visa veckan:** en tabell med en rad per person och en kolumn per dag. Arbetspass visas i gult och aktiviteter i turkost.
- **Lägg till arbetspass:** välj person, rubrik, datum, starttid och sluttid.
- **Lägg till aktivitet:** rubrik, plats, datum, tider och en eller flera deltagare (`MultiSelectionPrompt`).
- **Ändra:** nuvarande värde visas, och Enter behåller det.
- **Ta bort:** kräver bekräftelse. Ja/nej-frågor är en meny där Nej står först, så att Enter alltid är det säkra valet.
- Datum och klockslag kontrolleras med `Validate`, och sluttiden måste vara efter starttiden.
- Ändra och ta bort delar hjälpmetoden `ChooseItem`, där användaren väljer pass eller aktivitet med piltangenterna.
- Om det inte finns några familjemedlemmar visas ett felmeddelande i stället.

### Schemat – Filtrera och vem är ledig

Schemat kan filtreras med LINQ.

- **Filtrera schemat:** välj en person (eller Alla) och om man vill en viss dag. Varje filter är en `Where()`, och resultatet sorteras med `OrderBy()` och visas i en tabell.
- **Vem är ledig?:** välj en dag. Varje person visas som Ledig (grönt) eller Upptagen (rött) med det som är bokat. En person är ledig om inget pass eller ingen aktivitet den dagen gäller personen.
- `OfType<WorkShift>()` och `OfType<Activity>()` plockar ut rätt sorts inlägg ur den gemensamma listan.

### Statistik

Statistikmenyn (`UI/StatisticsMenu.cs`) visar sammanfattningar med `BarChart`.

- **Timmar per person och vecka:** summerar arbetspass (`Hours`) och aktiviteter per person med `Sum()`, och visar en tabell och ett stapeldiagram.
- **Rätter som går att laga i dag:** antal recept som kan lagas jämfört med antal som saknar ingredienser, och en lista över dem som kan lagas.
- **Varor som snart går ut:** antal varor som har passerat, går ut snart eller är OK, och en lista över dem som behöver kontrolleras.
- Statistiken läser `fooditems.json` och `recipes.json` och använder samma regler som kylskåpet (`IsAvailable` och `WarningDays`), så kylskåpsmenyn behövde inte ändras.

### Specialtecken (Issue #54)

Bocken i inköpslistan visades som `?` på vissa datorer.

- `Console.OutputEncoding = System.Text.Encoding.UTF8;` ligger först i `Main`, innan något skrivs ut.
- Specialtecken som ✓ och å, ä, ö visas nu rätt i hela programmet.

### Huvudmenyn

Huvudmenyn (`UI/MainMenu.cs`) är en loop med en `SelectionPrompt` och en `switch` som skickar användaren vidare till respektive meny. Loopen avslutas när användaren väljer Avsluta.

### Designval – Lappar och inköp

- **Generiska `DataStore<T>`:** samma klass lagrar lappar, varor och recept utan att koden skrivs om för varje typ.
- **Gemensamma datalådor:** lådorna skapas en gång och skickas in i menyerna, så att menyerna och sparningen arbetar mot samma data.
- **Hjälpmetoder (`PickNote`, `DisplayTable`):** koden som flera funktioner behöver skrivs en gång.
- **LINQ för sökning:**

### Designval – Schema, statistik och meddelanden

- **Abstrakt `BoardItem`:** ett tomt inlägg kan aldrig skapas, och gemensamma fält skrivs bara en gång.
- **Polymorfism med `Describe()`:** schemat kan lägga pass och aktiviteter i samma lista och anropa `Describe()` utan att veta vilken sort varje objekt är.
- **`Hours` räknas ut:** timmarna sparas inte separat, så de kan aldrig bli fel när tiderna ändras.
- **`[JsonPolymorphic]` och `[JsonDerivedType]`:** pass och aktiviteter sparas i samma fil och återskapas ändå som rätt klass.
- **Statisk `Messages`-klass:** klassen behöver inte komma ihåg något, så alla kan anropa den direkt, och meddelandena ser likadana ut överallt.
- **Gemensamma datalådor:** `boardStore` och `familyStore` skapas i `MainMenu` och skickas till schemat och statistiken, så att de ser samma familjemedlemmar och samma pass.
- **Inga halvändrade objekt:** vid ändring skrivs svaren in i objektet först när alla frågor är besvarade.
- **Svenska texter, engelska namn:** allt användaren ser och alla kommentarer är på svenska, medan namn i koden är på engelska. Format som datorn läser (`yyyy-MM-dd`, `HH:mm`) är oförändrade.

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
- Lägga till, visa, ändra och ta bort arbetspass och aktiviteter, och att veckotabellen visar dem i rätt ruta.
- Felaktigt datum eller klockslag, och sluttid före starttid.
- Filtrering på person och dag, och vem som är ledig en viss dag.
- Statistik för timmar, rätter som går att laga och varor som snart går ut.
- Att bocken i inköpslistan visas rätt efter UTF-8-ändringen.

**Kända begränsningar:** Receptmatchningen tar inte hänsyn till ingrediensmängder. Startsidan visar endast det första matchande middagsförslaget. Visning av ett faktiskt arbetspass eller en aktivitet på startsidan har ännu inte verifierats manuellt. Arbetspass och aktiviteter kan inte gå över midnatt. Programmet har ingen inloggning, så `CreatedById` sätts till den som jobbar passet eller den första deltagaren i aktiviteten.
