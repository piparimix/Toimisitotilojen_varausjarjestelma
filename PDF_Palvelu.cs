using System;
using System.Collections.Generic;
using System.Text;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Toimistotilojen_varausjarjestelma
{
    class PDF_Palvelu
    {
        private const string YrityksenNimi = "VuokraToimistot Oy";
        private const string YrityksenOsoite = "Toimistokuja 1, 00100 Helsinki";
        public static void LuoLaskuPDF(Lasku lasku, Varaus varaus, Asiakas asiakas, Toimipiste toimipiste, Tila tila, string tiedostoPolku)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Arial));

                    // --- YLÄTUNNISTE ---
                    page.Header().PaddingBottom(20).Row(row =>
                    {
                        // Vasen puoli: Laskun tekstit
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("LASKU").FontSize(24).Bold().FontColor(Colors.Blue.Medium);
                            col.Item().Text($"Laskun numero: {lasku.LaskuId}");
                            col.Item().Text($"Varauksen numero: {varaus.VarausId}");
                            col.Item().Text($"Päiväys: {DateTime.Now:dd.MM.yyyy}");
                            col.Item().Text($"Eräpäivä: {lasku.Eräpäivä:dd.MM.yyyy}").SemiBold();
                        });

                        // Oikea puoli: Viivakoodi
                        row.ConstantItem(150).AlignRight().Column(col =>
                        {
                            // Generate the barcode using the invoice ID
                            byte[] barcodeImage = Barcode_palvelu.GetBarcodeBytes(lasku.LaskuId.ToString());

                            // Add the barcode image to the PDF
                            col.Item().Image(barcodeImage);
                        });
                    });
                    // --- OSOITETIEDOT ---
                    page.Content().Column(col =>
                    {
                        col.Item().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(20).Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Laskuttaja").FontSize(9).FontColor(Colors.Grey.Medium);
                                c.Item().Text(YrityksenNimi).Bold();
                                c.Item().Text(YrityksenOsoite);
                                c.Item().Text(toimipiste.ToimipisteenNimi);
                            });

                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Vastaanottaja").FontSize(9).FontColor(Colors.Grey.Medium);
                                c.Item().Text($"{asiakas.Etunimi} {asiakas.Sukunimi}").Bold();

                                // Check the customer type and add the Y-tunnus if it's a company or organization
                                if (asiakas.Tyyppi == Asiakastyyppi.Yritys || asiakas.Tyyppi == Asiakastyyppi.Organisaatio)
                                {
                                    if (!string.IsNullOrWhiteSpace(asiakas.YTunnus))
                                    {
                                        c.Item().Text($"Y-tunnus: {asiakas.YTunnus}");
                                    }
                                }

                                c.Item().Text(asiakas.Osoite);
                                c.Item().Text(asiakas.Tyyppi.ToString());
                            });
                        });

                        col.Item().PaddingTop(20).Text($"Varaus: {toimipiste.Kaupunki}, {tila.TilanNimi} ({varaus.VarausAlkuPvm:dd.MM.yyyy} - {varaus.VarausLoppuPvm:dd.MM.yyyy})");

                        // --- LASKURIVIT ---
                        col.Item().PaddingTop(10).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(4); // Tuote/Palvelu
                                columns.RelativeColumn(2); // Määrä + Yksikkö (Combined)
                                columns.RelativeColumn(2); // á Hinta
                                columns.RelativeColumn(2); // Yhteensä
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(HeaderStyle).Text("Kuvaus");
                                header.Cell().Element(HeaderStyle).AlignRight().Text("Määrä"); // Single header
                                header.Cell().Element(HeaderStyle).AlignRight().Text("Hinta"); // Or "Yksikköhinta"
                                header.Cell().Element(HeaderStyle).AlignRight().Text("Yhteensä");
                            });

                            // 1. Tilan vuokra
                            int paivat = varaus.Kestopvm();
                            table.Cell().Element(CellStyle).Text(tila.TilanNimi);
                            table.Cell().Element(CellStyle).AlignRight().Text($"{paivat} pv"); // Combined text with a space
                            table.Cell().Element(CellStyle).AlignRight().Text($"{tila.Hinta:N2} €");
                            table.Cell().Element(CellStyle).AlignRight().Text($"{tila.Hinta * paivat:N2} €");

                            // 2. Palvelut
                            foreach (var p in varaus.VaratutPalvelut)
                            {
                                decimal kpl = p.OnPaivakohtainen ? paivat : 1;
                                string yksikko = p.OnPaivakohtainen ? "pv" : "kpl";

                                table.Cell().Element(CellStyle).Text(p.Nimi);
                                table.Cell().Element(CellStyle).AlignRight().Text($"{kpl} {yksikko}"); // Combined
                                table.Cell().Element(CellStyle).AlignRight().Text($"{p.Hinta:N2} €");
                                table.Cell().Element(CellStyle).AlignRight().Text($"{(p.Hinta * kpl):N2} €");
                            }

                            // 3. Laitteet
                            foreach (var l in varaus.VaratutLaitteet)
                            {
                                decimal kpl = l.OnPaivakohtainen ? (paivat * l.Määrä) : l.Määrä;

                                table.Cell().Element(CellStyle).Text(l.Nimi);
                                table.Cell().Element(CellStyle).AlignRight().Text($"{kpl} kpl"); // Combined
                                table.Cell().Element(CellStyle).AlignRight().Text($"{l.Hinta:N2} €");
                                table.Cell().Element(CellStyle).AlignRight().Text($"{(l.Hinta * kpl):N2} €");
                            }

                            // YHTEENSÄ
                            table.Footer(footer =>
                            {
                                footer.Cell().ColumnSpan(4).PaddingTop(10).AlignRight().Text($"Loppusumma: {lasku.Summa:N2} €").FontSize(14).Bold();
                            });
                        });

                        if (!string.IsNullOrWhiteSpace(varaus.Lisätiedot))
                        {
                            col.Item().PaddingTop(30).Column(c => {
                                c.Item().Text("Lisätiedot:").Bold();
                                c.Item().Text(varaus.Lisätiedot).Italic();
                            });
                        }
                    });

                    page.Footer().AlignRight().Text(x =>
                    {
                        x.Span("Sivu ");
                        x.CurrentPageNumber();
                    });
                });
            })
            .GeneratePdf(tiedostoPolku);
        }
        public static void LuoRaporttiPDF(List<Varaus> varaukset, List<Asiakas> asiakkaat, List<Toimipiste> toimipisteet, List<Tila> tilat, DateTime alku, DateTime loppu, string tiedostoPolku)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(1, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    page.Header().Text($"Varausraportti: {alku:dd.MM.yyyy} - {loppu:dd.MM.yyyy}").FontSize(18).Bold();

                    page.Content().PaddingTop(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(30); // ID
                            columns.RelativeColumn(2);  // Asiakas
                            columns.RelativeColumn(2);  // Toimipiste
                            columns.RelativeColumn(3);  // Tila (Wider to prevent awkward wrapping of long names)
                            columns.ConstantColumn(65); // Alku
                            columns.ConstantColumn(65); // Loppu
                            columns.ConstantColumn(95); // Varaustila (Wider for the formatted text)
                            columns.RelativeColumn(1);  // Summa
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(HeaderStyle).Text("ID");
                            header.Cell().Element(HeaderStyle).Text("Asiakas");
                            header.Cell().Element(HeaderStyle).Text("Toimipiste");
                            header.Cell().Element(HeaderStyle).Text("Tila");
                            header.Cell().Element(HeaderStyle).Text("Alkaa");
                            header.Cell().Element(HeaderStyle).Text("Päättyy");
                            header.Cell().Element(HeaderStyle).Text("Varaustila");
                            header.Cell().Element(HeaderStyle).AlignRight().Text("Yhteensä");
                        });

                        foreach (var v in varaukset)
                        {
                            // Find the related objects
                            var asiakas = asiakkaat.FirstOrDefault(a => a.AsiakasId == v.AsiakasId);
                            var toimipiste = toimipisteet.FirstOrDefault(t => t.ToimipisteId == v.ToimipisteId);
                            var tila = tilat.FirstOrDefault(t => t.TilaId == v.TilaId);

                            // Safely extract names
                            string asiakasNimi = asiakas != null ? $"{asiakas.Etunimi} {asiakas.Sukunimi}" : "Tuntematon";
                            string toimipisteNimi = toimipiste != null ? toimipiste.ToimipisteenNimi : "Tuntematon";
                            string tilaNimi = tila != null ? tila.TilanNimi : "Tuntematon";
                            decimal tilanHinta = tila != null ? tila.Hinta : 0;

                            // Format the enum to look natural
                            string tilaTeksti = v.Tila switch
                            {
                                Varaustila.OdottaaVahvistusta => "Odottaa vahvistusta",
                                Varaustila.Vahvistettu => "Vahvistettu",
                                Varaustila.Peruttu => "Peruttu",
                                _ => v.Tila.ToString()
                            };

                            table.Cell().Element(CellStyle).Text(v.VarausId.ToString());
                            table.Cell().Element(CellStyle).Text(asiakasNimi);
                            table.Cell().Element(CellStyle).Text(toimipisteNimi);
                            table.Cell().Element(CellStyle).Text(tilaNimi);

                            // Force the standard Finnish date format regardless of OS culture settings
                            table.Cell().Element(CellStyle).Text(v.VarausAlkuPvm.ToString("dd.MM.yyyy"));
                            table.Cell().Element(CellStyle).Text(v.VarausLoppuPvm.ToString("dd.MM.yyyy"));

                            table.Cell().Element(CellStyle).Text(tilaTeksti);
                            table.Cell().Element(CellStyle).AlignRight().Text($"{v.LaskeVarauksenYhteishinta(tilanHinta):N2} €");
                        }
                    });
                });
            })
            .GeneratePdf(tiedostoPolku);
        }


        // --- APUTYYLIT ---
        static IContainer HeaderStyle(IContainer container)
        {
            return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
        }

        static IContainer CellStyle(IContainer container)
        {
            return container.PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten3);
        }
    }
}
    