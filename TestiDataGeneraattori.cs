using System;
using System.Collections.Generic;
using System.Linq;
using Bogus;

namespace Toimistotilojen_varausjarjestelma
{
    public class TestiDataGeneraattori
    {
        
        public List<Asiakas> Asiakkaat { get; private set; } = new List<Asiakas>();
        public List<Toimipiste> Toimipisteet { get; private set; } = new List<Toimipiste>();
        public List<Tila> Tilat { get; private set; } = new List<Tila>();
        public List<Varaus> Varaukset { get; private set; } = new List<Varaus>();
        public List<Palvelu> Palvelut { get; private set; } = new List<Palvelu>();
        public List<Laite> Laitteet { get; private set; } = new List<Laite>();
        public List<Lasku> Laskut { get; private set; } = new List<Lasku>();

        public void GeneroiData(int asiakasMaara, int toimipisteMaara, int tilaMaara, int varausMaara, int palveluMaara, int laiteMaara)
        {
            // 1. Generate Asiakas data
            var asiakasFaker = new Faker<Asiakas>("fi")
                .RuleFor(a => a.AsiakasId, f => f.IndexFaker + 1) // Incremental IDs
                .RuleFor(a => a.Etunimi, f => f.Name.FirstName())
                .RuleFor(a => a.Sukunimi, f => f.Name.LastName())
                .RuleFor(a => a.Sähköposti, (f, a) => f.Internet.Email(a.Etunimi, a.Sukunimi))
                .RuleFor(a => a.Puhelin, f => f.Phone.PhoneNumber("040 ### ####"))
                .RuleFor(a => a.Osoite, f => f.Address.StreetAddress())
                .RuleFor(a => a.Tyyppi, f => f.PickRandom<Asiakastyyppi>())
                .RuleFor(a => a.Laskutustapa, f => f.PickRandom<LaskunTyyppi>())
                .RuleFor(a => a.YTunnus, (f, a) =>(a.Tyyppi == Asiakastyyppi.Yritys || a.Tyyppi == Asiakastyyppi.Organisaatio) ? $"{f.Random.Number(1000000, 9999999)}-{f.Random.Number(0, 9)}" : string.Empty);
            Asiakkaat = asiakasFaker.Generate(asiakasMaara);

            // 2. Generate Toimipiste data
            var toimipisteFaker = new Faker<Toimipiste>("fi")
                 .RuleFor(t => t.ToimipisteId, f => f.IndexFaker + 1)
                 .RuleFor(t => t.ToimipisteenNimi, f => f.Company.CompanyName() + " Toimisto")
                 .RuleFor(t => t.Kaupunki, f => f.Address.City())
                 .RuleFor(t => t.Osoite, f => f.Address.StreetAddress())
                 .RuleFor(t => t.Postitoimipaikka, (f, t) => t.Kaupunki) // Yleensä postitoimipaikka on sama kuin kaupunki
                 .RuleFor(t => t.Postinumero, f => f.Random.Int(10000, 99999)) // Postinumero on int Luokat.cs:ssä
                 .RuleFor(t => t.Puhelinnumero, f => f.Phone.PhoneNumber("09 ### ####"));

            Toimipisteet = toimipisteFaker.Generate(toimipisteMaara);

            // 3. Generate Tila data
            var tilaFaker = new Faker<Tila>("fi")
                .RuleFor(t => t.TilaId, f => f.IndexFaker + 1)
                .RuleFor(t => t.TilanNimi, f => f.Commerce.ProductName() + " Neuvotteluhuone")
                // Respects Luokat.cs: Kapasiteetti restricted to 1-8
                .RuleFor(t => t.Kapasiteetti, f => f.Random.Int(1, 8))
                // Respects Luokat.cs: Hinta must be strictly non-negative
                .RuleFor(t => t.Hinta, f => Math.Round(f.Random.Decimal(20m, 200m), 2))
                .RuleFor(t => t.Lisätiedot, f => f.Lorem.Sentence());

            Tilat = tilaFaker.Generate(tilaMaara);

            // 4. Generate Varaus data (Relational Logic)
            var varausFaker = new Faker<Varaus>("fi")
                .RuleFor(v => v.VarausId, f => f.IndexFaker + 1)

                // --- RELATIONAL LINKS ---
                // Randomly picks an ID from the previously generated lists to ensure valid foreign keys
                .RuleFor(v => v.AsiakasId, f => f.PickRandom(Asiakkaat).AsiakasId)
                .RuleFor(v => v.ToimipisteId, f => f.PickRandom(Toimipisteet).ToimipisteId)
                .RuleFor(v => v.TilaId, f => f.PickRandom(Tilat).TilaId)

                .RuleFor(v => v.Tila, f => f.PickRandom<Varaustila>())
                .RuleFor(v => v.Lisätiedot, f => f.Lorem.Sentence())

                // --- TIME LOGIC ---
                // Start date is anytime within the next 30 days
                .RuleFor(v => v.VarausAlkuPvm, f => f.Date.Soon(30))
                // End date is safely calculated to be strictly AFTER the start date to satisfy Luokat.cs
                .RuleFor(v => v.VarausLoppuPvm, (f, v) => v.VarausAlkuPvm.AddHours(f.Random.Int(1, 72)));

            Varaukset = varausFaker.Generate(varausMaara);

            // 5. Generate Palvelu data
            var palveluFaker = new Faker<Palvelu>("fi")
                .RuleFor(p => p.Nimi, f => f.Commerce.ProductName() + " Catering")
                .RuleFor(p => p.Hinta, f => Math.Round(f.Random.Decimal(15m, 150m), 2))
                .RuleFor(p => p.OnPaivakohtainen, f => f.Random.Bool())
                .RuleFor(p => p.Kuvaus, f => f.Lorem.Sentence())
                .RuleFor(p => p.VaadittuHenkilokunta, f => f.Random.Int(1, 5));

            Palvelut = palveluFaker.Generate(palveluMaara);

            // 6. Generate Laite data
            var laiteFaker = new Faker<Laite>("fi")
                .RuleFor(l => l.Nimi, f => f.Commerce.ProductName() + " Projektori")
                .RuleFor(l => l.Hinta, f => Math.Round(f.Random.Decimal(10m, 80m), 2))
                .RuleFor(l => l.OnPaivakohtainen, f => f.Random.Bool())
                .RuleFor(l => l.Kuvaus, f => f.Lorem.Sentence())
                .RuleFor(l => l.VaatiiAsennuksen, f => f.Random.Bool())
                .RuleFor(l => l.Kunto, f => f.PickRandom<LaitteenKunto>())
                .RuleFor(l => l.Määrä, f => f.Random.Int(1, 10));

            Laitteet = laiteFaker.Generate(laiteMaara);

            // 7. Generate Lasku data (Uses the static method from Luokat.cs)
            foreach (var v in Varaukset)
            {
                // Find the room associated with the booking to get the correct daily price
                var tila = Tilat.FirstOrDefault(t => t.TilaId == v.TilaId);
                if (tila != null)
                {
                    // LuoVarauksesta calculates the total sum automatically based on the dates
                    Lasku uusiLasku = Lasku.LuoVarauksesta(v, tila.Hinta);

                    // Randomize the invoice status
                    uusiLasku.Tila = new Faker().PickRandom<LaskunTila>();

                    Laskut.Add(uusiLasku);
                }
            }
        }
    }
}