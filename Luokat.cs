using System;
using System.Collections.Generic;
using System.Text;

namespace Toimistotilojen_varausjarjestelma
{
    // Toimipiste-luokka määrittelee toimipisteen, joka sisältää perustiedot, kuten toimipisteen nimen, kaupungin ja osoitteen.
    public class Toimipiste    {
        // ToimipisteId on toimipisteen yksilöivä tunniste
        public int ToimipisteId { get; set; }

        // ToimipisteenNimi on toimipisteen nimi, joka on pakko alustaa oliota luotaessa.
        public required string ToimipisteenNimi { get; set; }

        // Kaupunki on toimipisteen sijaintikaupunki, joka on pakko alustaa oliota luotaessa.
        public required string Kaupunki { get; set; }

        // Osoite on toimipisteen katuosoite, joka on pakko alustaa oliota luotaessa.
        public required string Osoite { get; set; }
        public required string Postitoimipaikka { get; set; }
        public required int Postinumero { get; set; }
        public required string Puhelinnumero { get; set; }

        // Oletetaan, että Toimipisteellä on useita Tiloja
        public List<Tila> Tilat { get; set; } = new List<Tila>();

        // Oletetaan, että Toimipisteellä on useita Palveluita
        public List<Palvelu> Palvelut { get; set; } = new List<Palvelu>();

        // Oletetaan, että Toimipisteellä on useita Laitteita
        public List<Laite> Laitteet { get; set; } = new List<Laite>();
    }

    // Tila-luokka määrittelee tilat, joita asiakas voi varata, kuten neuvotteluhuoneet, työpisteet tai tapahtumatilat
    public class Tila
    {
        // TilaId on tilan yksilöivä tunniste
        public int TilaId { get; set; }

        // Yksityinen taustamuuttuja (backing field), johon kapasiteetin varsinainen arvo tallennetaan.
        private int _kapasiteetti;
        public string Lisätiedot { get; set; } = string.Empty;

        // Julkinen ominaisuus kapasiteetin lukemiseen ja asettamiseen. 'required' tarkoittaa, että tämä on pakko alustaa oliota luotaessa. 
        public required int Kapasiteetti
        {
            get { return _kapasiteetti; }
            set
            {
                if (value < 1 || value > 8)
                {
                    // Kapasiteetti ei saa olla alle 1 tai yli 8, koska tilat on suunniteltu enintään 8 henkilölle.
                    throw new ArgumentException("Kapasiteetti on virheellinen!");
                }
                _kapasiteetti = value;
            }
        }

        // TilanNimi on tilan nimi, joka on pakko alustaa oliota luotaessa.
        public required string TilanNimi { get; set; }

        // Yksityinen taustamuuttuja (backing field), johon hinnan varsinainen arvo tallennetaan.
        private decimal _hinta;

        // Julkinen ominaisuus hinnan lukemiseen ja asettamiseen. Hinta ei saa olla negatiivinen.
        public decimal Hinta
        {
            get { return _hinta; }
            set
            {
                if (value < 0)
                {
                    // Hinta ei saa olla negatiivinen, koska se ei olisi loogista varauksen hinnoittelussa.
                    throw new ArgumentException("Hinta ei voi olla negatiivinen!");
                }
                _hinta = value;
            }
        }

    }

    // Varaustila määrittelee varauksen tilan, joka voi olla vahvistettu, peruttu tai odottaa vahvistusta
    public enum Varaustila
    {
        Vahvistettu,
        Peruttu,
        OdottaaVahvistusta
    }

    // Varaus-luokka määrittelee varauksen, joka sisältää varauksen perustiedot, kuten varauksen aloitus- ja lopetuspvm, varatut palvelut ja laitteet sekä laskukaavan varauksen kokonaishinnan laskemiseksi
    public class Varaus
    {
        // VarausId on varauksen yksilöivä tunniste
        public int VarausId { get; set; }
        public string Lisätiedot { get; set; } = string.Empty;

        // AsiakasId on varauksen tekijän asiakastiedot, joka viittaa Asiakas-luokan AsiakasId:hen
        public int AsiakasId { get; set; }

        // ToimipisteId on varauksen kohteena olevan toimipisteen tunniste, joka viittaa Toimipiste-luokan ToimipisteId:hen
        public int ToimipisteId { get; set; }

        // TilaId on varauksen kohteena olevan tilan tunniste, joka viittaa Tila-luokan TilaId:hen
        public int TilaId { get; set; }

        // Varaustila määrittelee varauksen tilan, joka on pakko alustaa oliota luotaessa. Se voi olla vahvistettu, peruttu tai odottaa vahvistusta.
        public required Varaustila Tila { get; set; }

        // LuontiPvm tallentaa varauksen luontipvm, ja se asetetaan automaattisesti varauksen luotaessa nykyhetkeen.
        public DateTime LuontiPvm { get; set; } = DateTime.Now;

        // Yksityinen taustamuuttuja (backing field), johon varauksen aloituspvm tallennetaan.
        private DateTime _varausAlkuPvm;

        // Yksityinen taustamuuttuja (backing field), johon varauksen lopetuspvm tallennetaan.
        private DateTime _varausLoppuPvm;

        // VarausAlkuPvm ei saa olla menneisyydessä, ja se on pakko alustaa oliota luotaessa.
        public required DateTime VarausAlkuPvm
        {
            get { return _varausAlkuPvm; }
            set
            {
                _varausAlkuPvm = value;
            }
        }

        // VarausLoppuPvm ei saa olla ennen aloituspvm, ja se on pakko alustaa oliota luotaessa
        public required DateTime VarausLoppuPvm
        {
            get { return _varausLoppuPvm; }
            set
            {
                if (value < _varausAlkuPvm)
                {
                    throw new ArgumentException("Varaus loppuu ennen alkamista!");
                }
                _varausLoppuPvm = value;
            }
        }

        // VaratutPalvelut on lista varauksen yhteydessä varatuista palveluista, ja se on oletuksena tyhjä lista.
        public List<Palvelu> VaratutPalvelut { get; set; } = new List<Palvelu>();

        // VaratutLaitteet on lista varauksen yhteydessä varatuista laitteista, ja se on oletuksena tyhjä lista.
        public List<Laite> VaratutLaitteet { get; set; } = new List<Laite>();

        // Kestopvm-metodi laskee varauksen keston päivinä aloitus- ja lopetuspvm:n perusteella
        public int Kestopvm()
        {
            // Lasketaan päivien erotus. Jos varaus on saman päivän aikana, tulos on vähintään 1 päivä.
            int paivat = (VarausLoppuPvm.Date - VarausAlkuPvm.Date).Days;
            if (paivat <= 0)
            {
                return 1;
            }
            else
            {
                return paivat;
            }
        }

        // LaskeVarauksenYhteishinta-metodi laskee varauksen kokonaishinnan tilan päivähinnan, varattujen palveluiden ja laitteiden hintojen perusteella
        public decimal LaskeVarauksenYhteishinta(decimal tilanPaivahinta)
        {
            // Haetaan varauksen kesto päivinä Kestopvm-metodilla
            int paivat = Kestopvm();

            // Lasketaan tilan hinta kertomalla tilan päivähinta varauksen kestolla päivinä. Tämä on varauksen perushinta ilman palveluita ja laitteita.
            decimal summa = tilanPaivahinta * paivat;

            // Käydään läpi varatut palvelut, ja jos palvelu on päiväkohtainen, sen hinta kerrotaan varauksen kestolla päivinä, muuten lisätään vain palvelun hinta.
            foreach (var p in VaratutPalvelut)
            {
                if (p.OnPaivakohtainen == true)
                {
                    summa = summa + (p.Hinta * paivat);
                }
                else
                {
                    summa = summa + p.Hinta;
                }
            }

            // Käydään läpi varatut laitteet, ja jos laite on päiväkohtainen, sen hinta kerrotaan varauksen kestolla päivinä, muuten lisätään vain laitteen hinta.
            foreach (var l in VaratutLaitteet)
            {
                if (l.OnPaivakohtainen == true)
                {
                    summa = summa + (l.Hinta * paivat);
                }
                else
                {
                    summa = summa + l.Hinta;
                }
            }
            // Paluuarvo on varauksen kokonaishinta, joka sisältää tilan hinnan, varattujen palveluiden ja laitteiden hinnat.
            return summa;
        }
    }

    // Asiakastyyppi määrittelee, onko asiakas yritys, organisaatio vai yksityishenkilö
    public enum Asiakastyyppi
    {
        Yritys,
        Organisaatio,
        Yksityishenkilö
    }

    // LaskunTyyppi määrittelee, haluaako asiakas saada laskun sähköpostitse vai paperisena
    public enum LaskunTyyppi
    {
        Sähköposti,
        Paperi
    }

    // Asiakas-luokka määrittelee asiakkaan, joka sisältää asiakkaan perustiedot, asiakastyypin ja laskutustavan
    public class Asiakas
    {
        public int AsiakasId { get; set; }
        public required string Etunimi { get; set; }
        public required string Sukunimi { get; set; }
        public required string Sähköposti { get; set; }
        public required string Puhelin { get; set; }
        public required Asiakastyyppi Tyyppi { get; set; }
        public required LaskunTyyppi Laskutustapa { get; set; }
        public required string Osoite { get; set; }
    }

    // LaskunTila määrittelee laskun tilan, joka voi olla avoinna, maksettu tai erääntynyt
    public enum LaskunTila
    {
        Avoinna,
        Maksettu,
        Erääntynyt
    }

    // Lasku-luokka määrittelee laskun, joka sisältää laskutustiedot, kuten laskun numeron, summan ja eräpäivän.
    // Lasku-luokka sisältää myös LuoVarauksesta-metodin, joka luo laskun varauksesta, ja se käyttää varauksen LaskeVarauksenYhteishinta-metodia laskemaan laskun summan tilan päivähinnan perusteella.
    public class Lasku
    {
        public int LaskuId { get; set; }
        public required int VarausId { get; set; }
        public required DateTime Eräpäivä { get; set; }
        private Decimal _summa;
        public required Decimal Summa
        {
            get { return _summa; }
            set
            {
                if (value < 0)
                {
                    // Summa ei saa olla negatiivinen, koska se ei olisi loogista laskutuksessa.
                    throw new ArgumentException("Summa ei voi olla negatiivinen!");
                }
                _summa = value;
            }
        }
        public required LaskunTila Tila { get; set; }

        // LuoVarauksesta-metodi luo laskun varauksesta, ja se käyttää varauksen LaskeVarauksenYhteishinta-metodia laskemaan laskun summan tilan päivähinnan perusteella.
        public static Lasku LuoVarauksesta(Varaus varaus, decimal tilanHinta)
        {
            return new Lasku
            {
                VarausId = varaus.VarausId,
                Summa = varaus.LaskeVarauksenYhteishinta(tilanHinta),
                Tila = LaskunTila.Avoinna,
                Eräpäivä = DateTime.Now.AddDays(14)
            };
        }
    }

    // VuokraKohde-luokka on abstrakti luokka, joka määrittelee yhteiset ominaisuudet palveluille ja laitteille, joita asiakas voi varata tilan lisäksi.
    public abstract class VuokraKohde
    {
        public int Id { get; set; }
        public required string Nimi { get; set; }
        private decimal _hinta;
        public required Decimal Hinta
        {
            get { return _hinta; }
            set
            {
                if (value < 0)
                {
                    // Hinta ei saa olla negatiivinen, koska se ei olisi loogista varauksen hinnoittelussa.
                    throw new ArgumentException("Hinta ei voi olla negatiivinen!");
                }
                _hinta = value;
            }
        }

        // OnPaivakohtainen määrittelee, onko palvelu tai laite päiväkohtainen, eli lasketaanko sen hinta varauksen kestolla päivinä vai lisätäänkö vain kerran varauksen kokonaishintaan.
        public bool OnPaivakohtainen { get; set; }
        public string Kuvaus { get; set; } = string.Empty;
    }

    // Palvelu-luokka perii VuokraKohde-luokan ominaisuudet, ja se edustaa palveluita, joita asiakas voi varata tilan lisäksi, kuten catering, siivous tai AV-tuki.
    public class Palvelu : VuokraKohde
    {
        public int VaadittuHenkilokunta { get; set; }
    }

    public enum LaitteenKunto
    {
        Erinomainen,
        Hyvä,
        Tyydyttävä,
        Huono,
        Rikkinäinen
    }
    // Laite-luokka perii VuokraKohde-luokan ominaisuudet, ja se edustaa laitteita, joita asiakas voi varata tilan lisäksi, kuten projektori, äänentoistojärjestelmä tai tietokone.
    public class Laite : VuokraKohde
    {
        public bool VaatiiAsennuksen { get; set; }
        public int Määrä { get; set; }
        public required LaitteenKunto Kunto { get; set; }
    }
}