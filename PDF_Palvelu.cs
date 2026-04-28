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
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("LASKU").FontSize(24).Bold().FontColor(Colors.Blue.Medium);
                            col.Item().Text($"Laskun numero: {lasku.LaskuId}");
                            col.Item().Text($"Varauksen numero: {varaus.VarausId}");
                            col.Item().Text($"Päiväys: {DateTime.Now:dd.MM.yyyy}");
                            col.Item().Text($"Eräpäivä: {lasku.Eräpäivä:dd.MM.yyyy}").SemiBold();
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
                                columns.RelativeColumn(3); // Tuote/Palvelu
                                columns.RelativeColumn(1); // Määrä
                                columns.RelativeColumn(1); // Yksikkö
                                columns.RelativeColumn(2); // hinta
                                columns.RelativeColumn(2); // Yhteensä
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(HeaderStyle).Text("Kuvaus");
                                header.Cell().Element(HeaderStyle).AlignRight().Text("Määrä");
                                header.Cell().Element(HeaderStyle).Text("Yks.");
                                header.Cell().Element(HeaderStyle).AlignRight().Text("á Hinta");
                                header.Cell().Element(HeaderStyle).AlignRight().Text("Yhteensä");
                            });

                            // 1. Tilan vuokra
                            int paivat = varaus.Kestopvm();
                            table.Cell().Element(CellStyle).Text(tila.TilanNimi);
                            table.Cell().Element(CellStyle).AlignRight().Text(paivat.ToString());
                            table.Cell().Element(CellStyle).Text("pv");
                            table.Cell().Element(CellStyle).AlignRight().Text($"{tila.Hinta:N2} €");
                            table.Cell().Element(CellStyle).AlignRight().Text($"{tila.Hinta * paivat:N2} €");

                            // 2. Palvelut
                            foreach (var p in varaus.VaratutPalvelut)
                            {
                                decimal kpl = p.OnPaivakohtainen ? paivat : 1;
                                table.Cell().Element(CellStyle).Text(p.Nimi);
                                table.Cell().Element(CellStyle).AlignRight().Text(kpl.ToString());
                                table.Cell().Element(CellStyle).Text(p.OnPaivakohtainen ? "pv" : "kpl");
                                table.Cell().Element(CellStyle).AlignRight().Text($"{p.Hinta:N2} €");
                                table.Cell().Element(CellStyle).AlignRight().Text($"{(p.Hinta * kpl):N2} €");
                            }

                            // 3. Laitteet
                            foreach (var l in varaus.VaratutLaitteet)
                            {
                                decimal kpl = l.OnPaivakohtainen ? (paivat * l.Määrä) : l.Määrä;
                                table.Cell().Element(CellStyle).Text(l.Nimi);
                                table.Cell().Element(CellStyle).AlignRight().Text(kpl.ToString());
                                table.Cell().Element(CellStyle).Text("kpl");
                                table.Cell().Element(CellStyle).AlignRight().Text($"{l.Hinta:N2} €");
                                table.Cell().Element(CellStyle).AlignRight().Text($"{(l.Hinta * kpl):N2} €");
                            }

                            // YHTEENSÄ
                            table.Footer(footer =>
                            {
                                footer.Cell().ColumnSpan(5).PaddingTop(10).AlignRight().Text($"Loppusumma: {lasku.Summa:N2} €").FontSize(14).Bold();
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
        public static void LuoRaporttiPDF(List<Varaus> varaukset, DateTime alku, DateTime loppu, string tiedostoPolku)
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
                            columns.ConstantColumn(40); // ID
                            columns.RelativeColumn(2);  // Asiakas
                            columns.RelativeColumn(2);  // Toimipiste
                            columns.ConstantColumn(70); // Alku
                            columns.ConstantColumn(70); // Loppu
                            columns.ConstantColumn(50); // Tila
                            columns.RelativeColumn(1); // Summa
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(HeaderStyle).Text("ID");
                            header.Cell().Element(HeaderStyle).Text("Asiakas ID");
                            header.Cell().Element(HeaderStyle).Text("Toimipiste ID");
                            header.Cell().Element(HeaderStyle).Text("Alkaa");
                            header.Cell().Element(HeaderStyle).Text("Päättyy");
                            header.Cell().Element(HeaderStyle).Text("Tila");
                            header.Cell().Element(HeaderStyle).AlignRight().Text("Yhteensä");
                        });

                        foreach (var v in varaukset)
                        {
                            table.Cell().Element(CellStyle).Text(v.VarausId.ToString());
                            table.Cell().Element(CellStyle).Text(v.AsiakasId.ToString());
                            table.Cell().Element(CellStyle).Text(v.ToimipisteId.ToString());
                            table.Cell().Element(CellStyle).Text(v.VarausAlkuPvm.ToShortDateString());
                            table.Cell().Element(CellStyle).Text(v.VarausLoppuPvm.ToShortDateString());
                            table.Cell().Element(CellStyle).Text(v.Tila.ToString());
                            table.Cell().Element(CellStyle).AlignRight().Text($"{v.LaskeVarauksenYhteishinta(0):N2} €"); // Huom: tilan hinta pitäisi hakea tässä
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
    