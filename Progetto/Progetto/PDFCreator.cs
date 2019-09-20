using DevExpress.Pdf;
using DevExpress.Xpf.Charts;
using DevExpress.XtraPrinting;
using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;

namespace Progetto
{
    public class PDFCreator
    {
        private string _nome;
        private double _velocitaMedia, _massima, _minima, _lunghezzaPercorso;
        private string _inizio, _fine, _totalTime;
        ObservableCollection<string> _puntiStazionamento;

        public PDFCreator(string nome, double Media, double max, double min, double Lunghezza, string start, string finish, string totalTime, ObservableCollection<string> stationaryPoints)
        {
            _nome = nome;
            _velocitaMedia = Media;
            _massima = max;
            _minima = min;
            _lunghezzaPercorso = Lunghezza;
            _inizio = start;
            _fine = finish;
            _totalTime = totalTime;
            _puntiStazionamento = stationaryPoints;
        }

        public void CreaPDF(ChartControl Chart)
        {
            try
            {
                string pathPDF = $"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\\{(_nome == "" || _nome == null ? "Route" : _nome)}.pdf";
                string pathIMG = $"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\\##%%R3P0rT%%##.png";
                using (PdfDocumentProcessor processor = new PdfDocumentProcessor())
                {

                    ImageExportOptions options = new ImageExportOptions
                    {
                        Resolution = 400
                    };

                    Chart.ExportToImage(pathIMG, options, PrintSizeMode.ProportionalZoom);
                    // Create an empty document. 


                    using (Image img = Image.FromFile(pathIMG))
                    {
                        processor.CreateEmptyDocument(pathPDF);

                        PdfRectangle pageBounds = PdfPaperSize.A4;
                        // Create and draw PDF graphics. 
                        using (PdfGraphics graph = processor.CreateGraphics())
                        {
                            DrawGraphics(graph, pageBounds, img, processor);

                            // Render a page with graphics. 
                            //processor.RenderNewPage(PdfPaperSize.A4, graph, 72, 72);
                            Console.WriteLine("PDF Generato");
                        }
                    }
                }

                File.Delete(pathIMG);

                void DrawGraphics(PdfGraphics graph, PdfRectangle pageBounds, Image img, PdfDocumentProcessor processor)
                {
                    //2480 x 3508
                    //595 x 842
                    //int Orr = 200;
                    double margine = 50;
                    double x = pageBounds.Width - (2 * margine);
                    double y = pageBounds.Height - (2 * margine);
                    double ydiv = (y / 2) / 6;
                    double xdiv = x / 3;
                    int nPage = 1;

                    graph.TranslateTransform(0, 0);
                    // Draw text lines on the page. 
                    SolidBrush black = (SolidBrush)Brushes.Black;
                    SolidBrush red = (SolidBrush)Brushes.Red;

                    using (Font font1 = new Font("Arial", 20, FontStyle.Bold))
                    {
                        graph.DrawString((_nome == "" || _nome == null ? "Route" : _nome), font1, red, (float)((x / 2) + ((margine / 2) - 5)), (float)margine - 10);
                    }

                    using (Font font2 = new Font("Arial", 11))
                    {
                        graph.DrawImage(img, new RectangleF((float)(margine), (float)(margine * 1.5), (float)(PdfPaperSize.A4.Width - 2 * margine), (float)((y / 2) - 2 * margine)));
                        graph.DrawString($"Velocità media: {Math.Round(_velocitaMedia, 2)} Km/h", font2, black, (float)(margine), (float)(y / 2));
                        graph.DrawString($"Velocità massima: {Math.Round(_massima, 1)} Km/h", font2, black, (float)(xdiv + margine), (float)((y / 2)));
                        graph.DrawString($"Velocità minima: {Math.Round(_minima, 1)} Km/h", font2, black, (float)((2 * xdiv) + margine), (float)(y / 2));
                        graph.DrawString($"Partenza: {_inizio}", font2, black, (float)(margine), (float)((y / 2) + ydiv));
                        graph.DrawString($"Arrivo: {_fine}", font2, black, (float)(xdiv + margine), (float)((y / 2) + ydiv));
                        graph.DrawString($"Durata del viaggio: {_totalTime}", font2, black, (float)((2 * xdiv) + margine), (float)((y / 2) + ydiv));
                        graph.DrawString($"Lunghezza del percorso: {Math.Round(_lunghezzaPercorso, 2)} Km", font2, black, (float)margine, (float)((y / 2) + ydiv * 2));
                        if (_puntiStazionamento.Count != 0)
                        {
                            graph.DrawString($"Punti di stazionamento: ", font2, black, (float)margine, (float)((y / 2) + ydiv * 3));
                        }
                    }

                    if (_puntiStazionamento.Count != 0)
                    {
                        using (Font font3 = new Font("Arial", 8))
                        {
                            //for (int i = 0; i < _puntiStazionamento.Count - 1; i++)
                            //{
                            //    graph.DrawString(_puntiStazionamento[i], font3, black, (float)margine, (float)((y / 2) + ydiv * (3.3 + (0.3 * i))));
                            //}

                            int z = 0;
                            float position = 0;
                            for (int i = 0; i < _puntiStazionamento.Count; i++)
                            {
                                bool pageStart = true;
                                if (position < (pageBounds.Height - margine) && nPage == 1)
                                {
                                    z = i;
                                    position = (float)((y / 2) + ydiv * (3.3 + (0.3 * z)));
                                }
                                else if (position < (pageBounds.Height - margine) && nPage != 1)
                                {
                                    z++;
                                    pageStart = true;
                                    position = (float)(margine + ydiv * (0.3 * z));
                                }
                                else if (pageStart)
                                {
                                    //processor.InsertNewPage(nPage + 1, pageBounds);
                                    processor.RenderNewPage(PdfPaperSize.A4, graph, 72, 72);
                                    graph = processor.CreateGraphics();
                                    nPage++;
                                    z = 0;
                                    pageStart = false;
                                    position = (float)(margine + ydiv * (0.3 * z));
                                }
                                graph.DrawString(_puntiStazionamento[i], font3, black, (float)margine, position);
                            }
                            processor.RenderNewPage(PdfPaperSize.A4, graph, 72, 72);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
