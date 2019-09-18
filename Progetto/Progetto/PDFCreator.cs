using DevExpress.Pdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using DevExpress.XtraSpreadsheet.Services;
using DevExpress.XtraSpreadsheet.Services.Implementation;
using DevExpress.Spreadsheet;
using DevExpress.Xpf.Charts;
using DevExpress.XtraReports.UI;
using DevExpress.XtraPrinting;
using System.IO;

namespace Progetto
{
    public class PDFCreator
    {
        string Nome = "";
        double VelocitaMedia, Massima, Minima, LunghezzaPercorso = 1000;
        string Inizio, Fine, TotalTime;

        public PDFCreator(string nome, double Media, double max, double min, double Lunghezza, string start, string finish, string totalTime)
        {
            Nome = nome;
            VelocitaMedia = Media;
            Massima = max;
            Minima = min;
            LunghezzaPercorso = Lunghezza;
            Inizio = start;
            Fine = finish;
            TotalTime = totalTime;

        }

        public void CreaPDF(ChartControl Chart)
        {
            string pathPDF = $"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\\{(Nome == "" || Nome == null ? "Route" : Nome)}.pdf";
            string pathIMG = $"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\\##%%R3P0rT%%##.png";
            using (PdfDocumentProcessor processor = new PdfDocumentProcessor())
            {

                ImageExportOptions options = new ImageExportOptions();
                options.Resolution = 400;

                Chart.ExportToImage(pathIMG, options, PrintSizeMode.ProportionalZoom);
                // Create an empty document. 


                using (Image img = Image.FromFile(pathIMG))
                {
                    processor.CreateEmptyDocument(pathPDF);

                    PdfRectangle pageBounds = PdfPaperSize.A4;
                    // Create and draw PDF graphics. 
                    using (PdfGraphics graph = processor.CreateGraphics())
                    {
                        DrawGraphics(graph, pageBounds, img);

                        // Render a page with graphics. 
                        processor.RenderNewPage(PdfPaperSize.A4, graph, 72, 72);
                        Console.WriteLine("PDF Generato");
                    }
                }
            }

            File.Delete(pathIMG);

            void DrawGraphics(PdfGraphics graph, PdfRectangle pageBounds, Image img)
            {
                //2480 x 3508
                //595 x 842
                //int Orr = 200;
                double margine = 50;
                double x = pageBounds.Width - (2 * margine);
                double y = pageBounds.Height - (2 * margine);
                double ydiv = (y / 2) / 6;
                double xdiv = x / 3;

                graph.TranslateTransform(0, 0);
                // Draw text lines on the page. 
                SolidBrush black = (SolidBrush)Brushes.Black;
                SolidBrush red = (SolidBrush)Brushes.Red;

                using (Font font1 = new Font("Arial", 20, FontStyle.Bold))
                {
                    graph.DrawString((Nome == "" || Nome == null ? "Route" : Nome), font1, red, (float)((x / 2) + ((margine / 2) - 5)), (float)margine - 10);
                }

                using (Font font2 = new Font("Arial", 11))
                {
                    graph.DrawImage(img, new RectangleF((float)(margine), (float)(margine * 1.5), (float)(PdfPaperSize.A4.Width - 2 * margine), (float)((y / 2) - 2 * margine)));
                    graph.DrawString($"Velocità media: {Math.Round(VelocitaMedia, 2)} Km/h", font2, black, (float)(margine), (float)(y / 2));
                    graph.DrawString($"Velocità massima: {Math.Round(Massima, 1)} Km/h", font2, black, (float)(xdiv + margine), (float)((y / 2)));
                    graph.DrawString($"Velocità minima: {Math.Round(Minima, 1)} Km/h", font2, black, (float)((2 * xdiv) + margine), (float)(y / 2));
                    graph.DrawString($"Partenza: {Inizio}", font2, black, (float)(margine), (float)((y / 2) + ydiv));
                    graph.DrawString($"Arrivo: {Fine}", font2, black, (float)(xdiv + margine), (float)((y / 2) + ydiv));
                    graph.DrawString($"Durata del viaggio: {TotalTime}", font2, black, (float)((2 * xdiv) + margine), (float)((y / 2) + ydiv));
                    graph.DrawString($"Lunghezza del percorso: {Math.Round(LunghezzaPercorso, 2)} Km", font2, black, (float)margine, (float)((y / 2) + ydiv * 2));
                    graph.DrawString($"Punti di stazionamento: ", font2, black, (float)margine, (float)((y / 2) + ydiv * 3));
                }
            }
        }
    }
}
