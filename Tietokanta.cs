using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;

namespace Toimistotilojen_varausjarjestelma
{
    class Tietokanta
    {
        // Yhteysmerkkijonat haetaan App.config-tiedostosta, mikä mahdollistaa helpon konfiguroinnin ilman koodin muuttamista.
        private readonly string local = ConfigurationManager.ConnectionStrings["LocalConnection"].ConnectionString;
        private readonly string localWithDb = ConfigurationManager.ConnectionStrings["LocalWithDbConnection"].ConnectionString;

        public async Task LuoTietokantaAsync()
        {
            MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder(localWithDb);
            string dbName = builder.Database;
            using (MySqlConnection conn = new MySqlConnection(local))
            {
                await conn.OpenAsync();
                MySqlCommand cmd = new MySqlCommand($"CREATE DATABASE IF NOT EXISTS {dbName}", conn);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task PoistaTietokantaAsync()
        {
            MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder(localWithDb);
            string dbName = builder.Database;
            using (MySqlConnection conn = new MySqlConnection(local))
            {
                await conn.OpenAsync();
                MySqlCommand cmd = new MySqlCommand($"DROP DATABASE IF EXISTS {dbName}", conn);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task PoistaVanhaTietokantaAsync()
        {
            using (MySqlConnection conn = new MySqlConnection(local))
            {
                await conn.OpenAsync();
                MySqlCommand cmd = new MySqlCommand($"DROP DATABASE IF EXISTS Laskutusapp", conn);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task LuoTaulutAsync()
        {
            using (MySqlConnection conn = new MySqlConnection(localWithDb))
            {
                await conn.OpenAsync();
                try
                {
                    MySqlCommand cmd = new MySqlCommand(
                        // Asiakas-taulussa on tieto asiakkaan etunimestä, sukunimestä, sähköpostista, puhelinnumerosta,
                        // osoitteesta, tyypistä (yritys, organisaatio tai yksityishenkilö) ja laskutustavasta (sähköposti tai paperi).
                        "CREATE TABLE IF NOT EXISTS ASIAKAS(" +
                        "AsiakasId INT NOT NULL AUTO_INCREMENT PRIMARY KEY," +
                        "Etunimi VARCHAR(100) NOT NULL," +
                        "Sukunimi VARCHAR(100) NOT NULL," +
                        "sahkoposti VARCHAR(254) NOT NULL," +
                        "Puhelinnro VARCHAR(20) NOT NULL," +
                        "Osoite VARCHAR(100) NOT NULL," +
                        "YTunnus VARCHAR(20)," +
                        "Tyyppi ENUM('Yritys','Organisaatio', 'Yksityishenkilö') NOT NULL," +
                        "Laskutustapa ENUM('Sähköposti','Paperi') NOT NULL," +
                        "itime TIMESTAMP," +
                        "iby VARCHAR(20)," +
                        "utime TIMESTAMP," +
                        "uby VARCHAR(20));" +

                        // Toimipiste-taulussa on tieto toimipisteen kaupungista, nimestä, osoitteesta, postitoimipaikasta, postinumerosta ja puhelinnumerosta.
                        "CREATE TABLE IF NOT EXISTS Toimipiste(" +
                        "ToimipisteID INT NOT NULL AUTO_INCREMENT PRIMARY KEY," +
                        "Kaupunki VARCHAR(100) NOT NULL," +
                        "Nimi VARCHAR(100) NOT NULL," +
                        "Osoite VARCHAR(100) NOT NULL," +
                        "Postitoimipaikka VARCHAR(100) NOT NULL," +
                        "Postinro CHAR(5) NOT NULL," +
                        "Puhelinnro VARCHAR(20) NOT NULL," +
                        "itime TIMESTAMP," +
                        "iby VARCHAR(20)," +
                        "utime TIMESTAMP," +
                        "uby VARCHAR(20));" +

                        // Tila-taulussa on tieto tilan nimestä, kapasiteetista, lisätiedoista ja hinnasta.
                        // Lisäksi siinä on kentät tietueen luonti- ja päivitysaikojen sekä käyttäjien tallentamiseen.
                        "CREATE TABLE IF NOT EXISTS Tila(" +
                        "TilaId INT NOT NULL AUTO_INCREMENT PRIMARY KEY," +
                        "TilanNimi VARCHAR (50) NOT NULL," +
                        "Kapasiteetti INT NOT NULL," +
                        "Lisätiedot VARCHAR(200) NOT NULL," +
                        "Hinta DECIMAL (8,2), " +
                        "itime TIMESTAMP," +
                        "iby VARCHAR(20)," +
                        "utime TIMESTAMP," +
                        "uby VARCHAR(20));" +

                        // VuokraKohde-taulussa on tieto vuokrattavan kohteen nimestä, hinnasta, siitä onko hinta päiväkohtainen,
                        // kuvauksesta sekä tietueen luonti- ja päivitysaikojen ja käyttäjien tallentamisesta.
                        "CREATE TABLE IF NOT EXISTS VuokraKohde(" +
                        "Id INT NOT NULL AUTO_INCREMENT PRIMARY KEY, " +
                        "Nimi VARCHAR(255) NOT NULL, " +
                        "Hinta DECIMAL(8,2) NOT NULL, " +
                        "OnPaivakohtainen BOOLEAN NOT NULL, " +
                        "Kuvaus TEXT, " +
                        "itime TIMESTAMP, " +
                        "iby VARCHAR(20), " +
                        "utime TIMESTAMP, " +
                        "uby VARCHAR(20));" +

                        // Palvelu-taulussa on tieto siitä, kuinka monta henkilökuntaa vuokrattava kohde vaatii,
                        // sekä tietueen luonti- ja päivitysaikojen ja käyttäjien tallentamisesta.
                        "CREATE TABLE IF NOT EXISTS Palvelu(" +
                        "Id INT NOT NULL PRIMARY KEY, " +
                        "VaadittuHenkilokunta INT NOT NULL, " +
                        "itime TIMESTAMP, " +
                        "iby VARCHAR(20), " +
                        "utime TIMESTAMP, " +
                        "uby VARCHAR(20), " +
                        "FOREIGN KEY (Id) REFERENCES VuokraKohde(Id) ON DELETE CASCADE);" +

                        // Laite-taulussa on tieto siitä, vaatiiko vuokrattava kohde asennuksen,
                        // sekä tietueen luonti- ja päivitysaikojen ja käyttäjien tallentamisesta.
                        "CREATE TABLE IF NOT EXISTS Laite(" +
                        "Id INT NOT NULL PRIMARY KEY, " +
                        "VaatiiAsennuksen BOOLEAN NOT NULL, " +
                        "Kunto ENUM('Erinomainen','Hyvä','Tyydyttävä','Huono','Rikkinäinen') NOT NULL, " +
                        "Maara INT NOT NULL DEFAULT 1, " +
                        "itime TIMESTAMP, " +
                        "iby VARCHAR(20), " +
                        "utime TIMESTAMP, " +
                        "uby VARCHAR(20), " +
                        "FOREIGN KEY (Id) REFERENCES VuokraKohde(Id) ON DELETE CASCADE);" +

                        // Varaus-taulussa on tieto varauksen tilasta (vahvistettu, peruttu tai odottaa vahvistusta), lisätiedoista,
                        // varauksen alkamis- ja päättymispäivämääristä sekä tietueen luonti- ja päivitysaikojen ja käyttäjien tallentamisesta.
                        "CREATE TABLE IF NOT EXISTS Varaus (" +
                        "VarausId INT NOT NULL AUTO_INCREMENT PRIMARY KEY," +
                        "AsiakasId INT NOT NULL," +
                        "ToimipisteId INT NOT NULL," +
                        "TilaId INT NOT NULL," +
                        "Varaustila ENUM('Vahvistettu','Peruttu','OdottaaVahvistusta') NOT NULL," +
                        "Lisatiedot TEXT," +
                        "VarausAlkuPvm DATETIME NOT NULL," +
                        "VarausLoppuPvm DATETIME NOT NULL," +
                        "itime TIMESTAMP DEFAULT CURRENT_TIMESTAMP," +
                        "iby VARCHAR(20)," +
                        "utime TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP," +
                        "uby VARCHAR(20)," +
                        "FOREIGN KEY(AsiakasId) REFERENCES ASIAKAS(AsiakasId)," +
                        "FOREIGN KEY(ToimipisteId) REFERENCES Toimipiste(ToimipisteID)," +
                        "FOREIGN KEY(TilaId) REFERENCES Tila(TilaId));" +

                        // Lasku-taulussa on tieto laskun eräpäivästä, summasta, tilasta (avoinna, maksettu tai erääntynyt)
                        // sekä tietueen luonti- ja päivitysaikojen ja käyttäjien tallentamisesta.
                        "CREATE TABLE IF NOT EXISTS Lasku(" +
                        "LaskuId INT NOT NULL AUTO_INCREMENT PRIMARY KEY," +
                        "VarausId INT NOT NULL," +
                        "Erapaiva DATETIME NOT NULL," +
                        "Summa DECIMAL(8,2) NOT NULL," +
                        "laskuntila ENUM('Avoinna','Maksettu','Erääntynyt') NOT NULL," +
                        "itime TIMESTAMP," +
                        "iby VARCHAR(20)," +
                        "utime TIMESTAMP," +
                        "uby VARCHAR(20)," +
                        "FOREIGN KEY(VarausId) REFERENCES Varaus(VarausId));" +

                        // VarausPalvelu-taulussa on tieto varauksen ja palvelun välisestä monesta moneen -suhteesta,
                        // jossa on viiteavain varaukseen ja viiteavain palveluun.
                        "CREATE TABLE IF NOT EXISTS VarausPalvelu(" +
                        "VarausId INT NOT NULL," +
                        "PalveluId INT NOT NULL," +
                        "PRIMARY KEY(VarausId, PalveluId)," +
                        "FOREIGN KEY(VarausId) REFERENCES Varaus(VarausId) ON DELETE CASCADE," +
                        "FOREIGN KEY(PalveluId) REFERENCES Palvelu(Id) ON DELETE CASCADE);" +

                        // VarausLaite-taulussa on tieto varauksen ja laitteen välisestä monesta moneen -suhteesta,
                        // jossa on viiteavain varaukseen ja viiteavain laittee
                        "CREATE TABLE IF NOT EXISTS VarausLaite(" +
                        "VarausId INT NOT NULL," +
                        "LaiteId INT NOT NULL," +
                        "PRIMARY KEY (VarausId, LaiteId)," +
                        "FOREIGN KEY (VarausId) REFERENCES Varaus(VarausId) ON DELETE CASCADE," +
                        "FOREIGN KEY (LaiteId) REFERENCES Laite(Id) ON DELETE CASCADE);", conn);

                    await cmd.ExecuteNonQueryAsync();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Virhe taulujen luonnissa: " + ex.Message);
                    throw;
                }
            }
        }

        // Testi Data
        // TallennaAsiakkaatAsync-metodi tallentaa listan asiakkaita tietokantaan.
        public async Task TallennaAsiakkaatAsync(List<Asiakas> asiakkaat)
        {
            using (MySqlConnection conn = new MySqlConnection(localWithDb))
            {
                await conn.OpenAsync();
                using (MySqlTransaction tr = await conn.BeginTransactionAsync())
                {
                    try
                    {
                        string sql = "INSERT INTO ASIAKAS (Etunimi, Sukunimi, sahkoposti, Puhelinnro, Osoite, YTunnus, Tyyppi, Laskutustapa) " +
                                     "VALUES (@etunimi, @sukunimi, @sahkoposti, @puhelin, @osoite, @ytunnus, @tyyppi, @laskutustapa)";

                        using (MySqlCommand cmd = new MySqlCommand(sql, conn, tr))
                        {
                            foreach (var a in asiakkaat)
                            {
                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("@etunimi", a.Etunimi);
                                cmd.Parameters.AddWithValue("@sukunimi", a.Sukunimi);
                                cmd.Parameters.AddWithValue("@sahkoposti", a.Sähköposti);
                                cmd.Parameters.AddWithValue("@puhelin", a.Puhelin);
                                cmd.Parameters.AddWithValue("@osoite", a.Osoite);
                                cmd.Parameters.AddWithValue("@ytunnus", a.YTunnus);
                                cmd.Parameters.AddWithValue("@tyyppi", a.Tyyppi.ToString());
                                cmd.Parameters.AddWithValue("@laskutustapa", a.Laskutustapa.ToString());
                                await cmd.ExecuteNonQueryAsync();
                            }
                        }
                        await tr.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        await tr.RollbackAsync();
                        System.Diagnostics.Debug.WriteLine("Virhe asiakkaiden tallennuksessa: " + ex.Message);
                        throw;
                    }
                }
            }
        }

        // TallennaToimipisteetAsync-metodi tallentaa listan toimipisteitä tietokantaan.
        public async Task TallennaToimipisteetAsync(List<Toimipiste> toimipisteet)
        {
            using (MySqlConnection conn = new MySqlConnection(localWithDb))
            {
                await conn.OpenAsync();
                using (MySqlTransaction tr = await conn.BeginTransactionAsync())
                {
                    try
                    {
                        string sql = "INSERT INTO Toimipiste (Kaupunki, Nimi, Osoite, Postitoimipaikka, Postinro, Puhelinnro) " +
                                     "VALUES (@kaupunki, @nimi, @osoite, @postitmp, @postinro, @puhelin)";

                        using (MySqlCommand cmd = new MySqlCommand(sql, conn, tr))
                        {
                            foreach (var t in toimipisteet)
                            {
                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("@kaupunki", t.Kaupunki);
                                cmd.Parameters.AddWithValue("@nimi", t.ToimipisteenNimi);
                                cmd.Parameters.AddWithValue("@osoite", t.Osoite);
                                cmd.Parameters.AddWithValue("@postitmp", t.Postitoimipaikka);
                                cmd.Parameters.AddWithValue("@postinro", t.Postinumero.ToString());
                                cmd.Parameters.AddWithValue("@puhelin", t.Puhelinnumero);
                                await cmd.ExecuteNonQueryAsync();
                            }
                        }
                        await tr.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        await tr.RollbackAsync();
                        System.Diagnostics.Debug.WriteLine("Virhe toimipisteiden tallennuksessa: " + ex.Message);
                        throw;
                    }
                }
            }
        }

        // TallennaTilatAsync-metodi tallentaa listan tiloja tietokantaan.
        public async Task TallennaTilatAsync(List<Tila> tilat)
        {
            using (MySqlConnection conn = new MySqlConnection(localWithDb))
            {
                await conn.OpenAsync();
                using (MySqlTransaction tr = await conn.BeginTransactionAsync())
                {
                    try
                    {
                        string sql = "INSERT INTO Tila (TilanNimi, Kapasiteetti, Lisätiedot, Hinta) " +
                                     "VALUES (@nimi, @kapasiteetti, @lisatiedot, @hinta)";

                        using (MySqlCommand cmd = new MySqlCommand(sql, conn, tr))
                        {
                            foreach (var t in tilat)
                            {
                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("@nimi", t.TilanNimi);
                                cmd.Parameters.AddWithValue("@kapasiteetti", t.Kapasiteetti);
                                cmd.Parameters.AddWithValue("@lisatiedot", t.Lisätiedot);
                                cmd.Parameters.AddWithValue("@hinta", t.Hinta);
                                await cmd.ExecuteNonQueryAsync();
                            }
                        }
                        await tr.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        await tr.RollbackAsync();
                        System.Diagnostics.Debug.WriteLine("Virhe tilojen tallennuksessa: " + ex.Message);
                        throw;
                    }
                }
            }
        }

        // TallennaVarauksetAsync-metodi tallentaa listan varauksia tietokantaan.
        public async Task TallennaVarauksetAsync(List<Varaus> varaukset)
        {
            using (MySqlConnection conn = new MySqlConnection(localWithDb))
            {
                await conn.OpenAsync();
                using (MySqlTransaction tr = await conn.BeginTransactionAsync())
                {
                    try
                    {
                        string sql = "INSERT INTO Varaus (AsiakasId, ToimipisteId, TilaId, Varaustila, Lisatiedot, VarausAlkuPvm, VarausLoppuPvm) " +
                                     "VALUES (@asiakasId, @toimipisteId, @tilaId, @tila, @lisatiedot, @alku, @loppu)";

                        using (MySqlCommand cmd = new MySqlCommand(sql, conn, tr))
                        {
                            foreach (var v in varaukset)
                            {
                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("@asiakasId", v.AsiakasId);
                                cmd.Parameters.AddWithValue("@toimipisteId", v.ToimipisteId);
                                cmd.Parameters.AddWithValue("@tilaId", v.TilaId);
                                cmd.Parameters.AddWithValue("@tila", v.Tila.ToString());
                                cmd.Parameters.AddWithValue("@lisatiedot", v.Lisätiedot);
                                cmd.Parameters.AddWithValue("@alku", v.VarausAlkuPvm);
                                cmd.Parameters.AddWithValue("@loppu", v.VarausLoppuPvm);
                                await cmd.ExecuteNonQueryAsync();
                            }
                        }
                        await tr.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        await tr.RollbackAsync();
                        System.Diagnostics.Debug.WriteLine("Virhe varausten tallennuksessa: " + ex.Message);
                        throw;
                    }
                }
            }
        }

        // TallennaPalvelutAsync-metodi tallentaa listan palveluita tietokantaan.
        public async Task TallennaPalvelutAsync(List<Palvelu> palvelut)
        {
            using (MySqlConnection conn = new MySqlConnection(localWithDb))
            {
                await conn.OpenAsync();
                using (MySqlTransaction tr = await conn.BeginTransactionAsync())
                {
                    try
                    {
                        string sqlVuokraKohde = "INSERT INTO VuokraKohde (Nimi, Hinta, OnPaivakohtainen, Kuvaus) VALUES (@nimi, @hinta, @paivakohtainen, @kuvaus); SELECT LAST_INSERT_ID();";
                        string sqlPalvelu = "INSERT INTO Palvelu (Id, VaadittuHenkilokunta) VALUES (@id, @henkilokunta);";

                        foreach (var p in palvelut)
                        {
                            long lastId;
                            using (MySqlCommand cmd = new MySqlCommand(sqlVuokraKohde, conn, tr))
                            {
                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("@nimi", p.Nimi);
                                cmd.Parameters.AddWithValue("@hinta", p.Hinta);
                                cmd.Parameters.AddWithValue("@paivakohtainen", p.OnPaivakohtainen);
                                cmd.Parameters.AddWithValue("@kuvaus", p.Kuvaus);

                                lastId = Convert.ToInt64(await cmd.ExecuteScalarAsync());
                                p.Id = (int)lastId;
                            }

                            using (MySqlCommand cmd = new MySqlCommand(sqlPalvelu, conn, tr))
                            {
                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("@id", lastId);
                                cmd.Parameters.AddWithValue("@henkilokunta", p.VaadittuHenkilokunta);
                                await cmd.ExecuteNonQueryAsync();
                            }
                        }
                        await tr.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        await tr.RollbackAsync();
                        System.Diagnostics.Debug.WriteLine("Virhe palveluiden tallennuksessa: " + ex.Message);
                        throw;
                    }
                }
            }
        }

        // TallennaLaitteetAsync-metodi tallentaa listan laitteita tietokantaan.
        public async Task TallennaLaitteetAsync(List<Laite> laitteet)
        {
            using (MySqlConnection conn = new MySqlConnection(localWithDb))
            {
                await conn.OpenAsync();
                using (MySqlTransaction tr = await conn.BeginTransactionAsync())
                {
                    try
                    {
                        string sqlVuokraKohde = "INSERT INTO VuokraKohde (Nimi, Hinta, OnPaivakohtainen, Kuvaus) VALUES (@nimi, @hinta, @paivakohtainen, @kuvaus); SELECT LAST_INSERT_ID();";
                        string sqlLaite = "INSERT INTO Laite (Id, VaatiiAsennuksen, Kunto, Maara) VALUES (@id, @asennus, @kunto, @maara);";

                        foreach (var l in laitteet)
                        {
                            long lastId;
                            using (MySqlCommand cmd = new MySqlCommand(sqlVuokraKohde, conn, tr))
                            {
                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("@nimi", l.Nimi);
                                cmd.Parameters.AddWithValue("@hinta", l.Hinta);
                                cmd.Parameters.AddWithValue("@paivakohtainen", l.OnPaivakohtainen);
                                cmd.Parameters.AddWithValue("@kuvaus", l.Kuvaus);
                                lastId = Convert.ToInt64(await cmd.ExecuteScalarAsync());
                                l.Id = (int)lastId;
                            }

                            using (MySqlCommand cmd = new MySqlCommand(sqlLaite, conn, tr))
                            {
                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("@id", lastId);
                                cmd.Parameters.AddWithValue("@asennus", l.VaatiiAsennuksen);
                                cmd.Parameters.AddWithValue("@kunto", l.Kunto.ToString());
                                cmd.Parameters.AddWithValue("@maara", l.Määrä);
                                await cmd.ExecuteNonQueryAsync();
                            }
                        }
                        await tr.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        await tr.RollbackAsync();
                        System.Diagnostics.Debug.WriteLine("Virhe laitteiden tallennuksessa: " + ex.Message);
                        throw;
                    }
                }
            }
        }

        // TallennaLaskutAsync-metodi tallentaa listan laskuja tietokantaan.
        public async Task TallennaLaskutAsync(List<Lasku> laskut)
        {
            using (MySqlConnection conn = new MySqlConnection(localWithDb))
            {
                await conn.OpenAsync();
                using (MySqlTransaction tr = await conn.BeginTransactionAsync())
                {
                    try
                    {
                        string sql = "INSERT INTO Lasku (VarausId, Erapaiva, Summa, laskuntila) VALUES (@varausId, @erapaiva, @summa, @tila)";

                        using (MySqlCommand cmd = new MySqlCommand(sql, conn, tr))
                        {
                            foreach (var l in laskut)
                            {
                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("@varausId", l.VarausId);
                                cmd.Parameters.AddWithValue("@erapaiva", l.Eräpäivä);
                                cmd.Parameters.AddWithValue("@summa", l.Summa);
                                cmd.Parameters.AddWithValue("@tila", l.Tila.ToString());
                                await cmd.ExecuteNonQueryAsync();
                            }
                        }
                        await tr.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        await tr.RollbackAsync();
                        System.Diagnostics.Debug.WriteLine("Virhe laskujen tallennuksessa: " + ex.Message);
                        throw;
                    }
                }
            }
        } // testi data loppuu tähän


        // Tästä eteenpäin voidaan toteuttaa muita tietokantaoperaatioita, kuten tietojen hakua, päivitystä ja poistoa tarpeen mukaan.
        // Metodi pitää toteuttaa asynkronisesti ja käyttää parametrisoituja SQL-kyselyitä tietoturvan varmistamiseksi.
        // Metodi pitää kommentoida siten että se kertoo selkeästi mitä se tekee ja missä sitä käytetään, jotta koodin ylläpito ja laajentaminen on helpompaa tulevaisuudessa.










    }
}