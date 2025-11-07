# banken - Hur jag skapade koden. (Delarna jag jobbade på)

Pin Koden är: 1234

Jag kodade för det mesta på svenska med några delar på engelska, men inför framtiden kommer jag att koda primärt nu när jag vet att det är vad jag ska göra. En stor del av min kod skapades genom att följa Arbers vägledning under lektionerna, men annat än det behövde jag lösa ut själv.

Mitt första poblem uppstod i min CreateAccount sida där jag behövde bestämma hur sidans utseende skulle se ut. Som jag hade det var UI'n rörig i hur "Ta Bort" knapparna var för stora och låg för nära varandra, input fälten hade olika längd, och såg konstig ut. För att lösa detta valde jag att byta till Bootstrap format. Genom att använda rader och kolumner med row och col kunde jag skapa ett mer strukturerat och ren UI. Det gjorde att alla fält fick samma storlek och sidan blev både renare och mer professionell.

Nästa steg var att skapa Transaction History sidan, som skulle visa historik för alla insättningar, uttag och överföringar. För att komma igång analyserade jag Blazors Weather sida, eftersom den hade en tydlig struktur. Jag använde samma struktur och en del samma kod men anpassade logiken och innehållet efter transaktionshistorik. Utseendet var enkelt att skapa tack vare den färdiga strukturen, men logiken krävde mer eget arbete.

För att visa transaktioner använde jag en foreach loop som skapade tabelrader. Till en början använde jag if satser för att avgöra vilken typ av transaktion som visades, men efter att ha lärt mig om switch satser bytter jag till det. Switch gjorde koden renare, kortare och lättare att läsa, särskilt när flera olika transaktionstyper behövde hanteras.

I min @code använde jag samma struktur som på Weather-sidan. Jag använde @Inject för att hämta data via IAccountService och OnInitializedAsync för att ladda konton och transaktioner vid sidans start. Denna struktur gjorde att koden blev tydligt uppdelad.

Därefter skapade jag Konton sidan, där användaren kan se sina konton och sätta in eller ta ut pengar. Jag använde samma tabellstruktur som tidigare och visade varje konto i en tabellrad med tillhörande knappar för insättning och uttag. Under beloppsfältet la jag även till felmeddelanden med klassen text-danger för att ge direkt feedback vid ogiltiga värden, till exempel negativa tal.

Sammanfattningsvis skapade jag mina sidor med fokus på struktur och användarvänlighet. Med Bootstrap fick jag ett rent gränssnitt, och genom att utgå från Weather sidan kunde jag bygga logiska och tydliga Blazor sidor. Kombinationen av async metoder och tydlig komponentstruktur gjorde också att UI'n förblev responsivt och uppdaterat.