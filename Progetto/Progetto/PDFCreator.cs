using DevExpress.Pdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Progetto
{
    public class PDFCreator
    {
        string Nome = "Report";
        int VelocitaMedia, Massima, Minima, LunghezzaPercorso = 1000;
        DateTime Inizio, Fine;

        public PDFCreator(string nome, int Media, int max, int min, int Lunghezza, DateTime start, DateTime finish)
        {
            Nome = nome;
            VelocitaMedia = Media;
            Massima = max;
            Minima = min;
            LunghezzaPercorso = Lunghezza;
            Inizio = start;
            Fine = finish;

        }
        public PDFCreator()
        {

        }
        public void DoSomething()
        {
            using (PdfDocumentProcessor processor = new PdfDocumentProcessor())
            {
                processor.CreateEmptyDocument("C:\\Users\\utente\\Desktop\\report.pdf");

                using (PdfGraphics graph = processor.CreateGraphics())
                {
                    DrawGraphics(graph);
                    processor.RenderNewPage(PdfPaperSize.Letter, graph);
                }
            }
        }
        private void DrawGraphics(PdfGraphics graph)
        {
            //SolidBrush black = (SolidBrush)Brushes.Black;
            //using (Font font = new Font("Times New Roman", 32, FontStyle.Bold))
            //{
            //    graph.DrawString("Nome Percorso"+ Nome, font, black, 180, 150);
            //}
            //using (Font font = new Font("Times New Roman", 32, FontStyle.Bold))
            //{
            //    graph.DrawString("Lunghezza Percorso:"+Convert.ToString(LunghezzaPercorso), font, black, 180, 150);
            //}

           

        }
    }
}

