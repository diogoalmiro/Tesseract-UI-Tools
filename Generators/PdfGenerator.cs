﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using GroupDocs.Parser;

namespace Tesseract_UI_Tools.Generators
{
    public class PdfGenerator : ATiffPagesGenerator
    {
        public static new string[] FORMATS = new string[] { "pdf" };

        public PdfGenerator(string FilePath) : base(FilePath){
            PdfDocument doc = PdfSharp.Pdf.IO.PdfReader.Open(FilePath);
            if (doc.Tag != null && (string)doc.Tag == PDG_TAG)
            {
                CanRun = false;
            }
            doc.Close();
        }

        public override string[] GenerateTIFFs(string FolderPath, bool Overwrite = false, IProgress<float>? Progress = null, BackgroundWorker? worker = null)
        {
            if (!CanRun) throw new Exception("Attempting to run a File already generated by Tesseract UI Tools");
            Parser PDFParser = new Parser(FilePath);

            List<string> Result = new List<string>();
            var Images = PDFParser.GetImages();
            int TotalI = Images.Count();
            int CurrI = 0;
            foreach ( GroupDocs.Parser.Data.PageImageArea Image in Images )
            {
                using (Bitmap Tiff = new System.Drawing.Bitmap(Image.GetImageStream()))
                {
                    if (worker != null && worker.CancellationPending) break;
                    if (Progress != null) Progress.Report((float)(CurrI) / TotalI);
                    
                    if (Image.Rotation.CompareTo(90.0) == 0) Tiff.RotateFlip(RotateFlipType.Rotate90FlipNone);
                    if (Image.Rotation.CompareTo(180.0) == 0) Tiff.RotateFlip(RotateFlipType.Rotate180FlipNone);
                    if (Image.Rotation.CompareTo(270.0) == 0) Tiff.RotateFlip(RotateFlipType.Rotate270FlipNone);
                    string Out = Path.Combine(FolderPath, TiffPage(CurrI));
                    Tiff.Save(Out, System.Drawing.Imaging.ImageFormat.Tiff);
                    Result.Add(Out);
                    CurrI++;
                }
            }

            return Result.ToArray();
        }
    }
}
