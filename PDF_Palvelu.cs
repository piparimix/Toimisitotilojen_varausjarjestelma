using System;
using System.Collections.Generic;
using System.Text;

namespace Toimistotilojen_varausjarjestelma
{
    class PDF_Palvelu
    {
        // Laskuttajan tiedot.
        // nimi, osoite, postinumero, kaupunki

        // Asiakkaan tiedot.
        // nimi, osoite, postinumero, kaupunki, tyyppi (yritys/henkilö/organisaatio), y-tunnus (yritys/organisaatio), puhelin, sähköposti

        // Laskun ja varauksen perustiedot.
        // laskun numero, eräpäivä, varausnumero, toimipisteen nimi, toimipisteen osoite,
        // toimipisteen postinumero, toimipisteen kaupunki, varauksen aloituspvm, varauksen lopetuspvm, varauksen kesto päivinä

        // Laskurivit (Tila).
        // tilan nimi, tilan päivähinta, varauksen kesto päivinä, tilan yhteishinta

        // Laskurivit (Palvelut ja Laitteet).
        // tuotteen nimi, yksikköhinta, veloitusperuste (päiväkohtainen vai kerta), tuotteen yhteishinta

        // Yhteenveto.
        // laskun loppusumma, laskutustapa (sähköposti/paperi)

    }
}
