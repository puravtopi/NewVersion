using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Ocsp;
using PainTrax.Web.Helper;
using PainTrax.Web.Models;
using PainTrax.Web.Services;
//using PdfSharp.Pdf.AcroForms;
//using PdfSharp.Pdf.IO;
//using PdfSharp.Pdf;
//using PdfSharp.Drawing;
//using DocumentFormat.OpenXml.Spreadsheet;
//using PdfSharp.Fonts;
using iText.Kernel.Pdf;

using iText.Kernel.Pdf;
using iText.Kernel.Geom;
using iText.Forms;
using iText.Forms.Fields;
namespace PainTrax.Web.Controllers
{
    [SessionCheckFilter]
    public class PdfController : Controller
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<PdfController> _logger;
        public PdfController(ILogger<PdfController> logger, IWebHostEnvironment environment)
        {
            _environment = environment;
            _logger = logger;
        }
        public IActionResult Test()
        {
            try
            {
                string filePath = _environment.WebRootPath + "/Demo_New.pdf";
                string imgPath = _environment.WebRootPath + "/test.png";
                // Create a memory stream to hold the modified PDF
                MemoryStream memoryStream = new MemoryStream();

                using (PdfReader reader = new PdfReader(filePath))
                using (PdfWriter writer = new PdfWriter(memoryStream))
                {
                    PdfDocument pdfDoc = new PdfDocument(reader, writer);

                    // Access the first page
                    PdfPage page = pdfDoc.GetFirstPage();

                    // Update textbox value
                    PdfAcroForm form = PdfAcroForm.GetAcroForm(pdfDoc, true);
                    PdfFormField textbox = form.GetField("demo_fname");
                    textbox.SetValue("Test");

                    // Remove existing image and create a new textbox with the same bounds
                    //PdfArray annots = page.GetPdfObject().GetAsArray(PdfName.Annots);
                    //if (annots != null)
                    //{
                    //    foreach (PdfDictionary annotDict in annots)
                    //    {
                    //        if (PdfName.Widget.Equals(annotDict.GetAsName(PdfName.Subtype)))
                    //        {
                    //            PdfArray rect = annotDict.GetAsArray(PdfName.Rect);
                    //            Rectangle bbox = new Rectangle(rect.GetAsNumber(0).FloatValue(), rect.GetAsNumber(1).FloatValue(),
                    //                    rect.GetAsNumber(2).FloatValue(), rect.GetAsNumber(3).FloatValue());

                    //            // Replace image with textbox
                    //            annotDict.Remove(PdfName.AP);
                    //            annotDict.Put(PdfName.V, new PdfString("New Value")); // Set textbox value
                    //            annotDict.Put(PdfName.Subtype, PdfName.Widget);
                    //            annotDict.Put(PdfName.FT, PdfName.Tx);
                    //            annotDict.Put(PdfName.Rect, new PdfArray(bbox));
                    //        }
                    //    }
                    //}

                    // Close the document
                    pdfDoc.Close();
                }

                // Reset the memory stream position to the beginning
                //memoryStream.Position = 0;

                // Return the modified PDF as a file
                byte[] fileBytes = memoryStream.ToArray();
                return File(fileBytes, "application/pdf", "output.pdf");
            }
            catch (Exception ex)
            {
                SaveLog(ex, "iText");
                return View();
            }

        }

        public IActionResult Index()
        {
            //string filePath = _environment.WebRootPath+ "/Demo_New.pdf";
            //string imgPath = _environment.WebRootPath+ "/test.png";
            //byte[] pdfBytes = System.IO.File.ReadAllBytes(filePath);
            //GlobalFontSettings.FontResolver = new MyFontResolver();

            //using (MemoryStream pdfStream = new MemoryStream(pdfBytes))
            //{
            //    // Open existing PDF document from memory
            //    PdfDocument document = PdfReader.Open(pdfStream, PdfDocumentOpenMode.Modify);

            //    PdfAcroForm form = document.AcroForm;

            //    if (form != null)
            //    {

            //        // Iterate through each field in the form
            //        //  foreach (string fieldName in form.Fields.Names)
            //        foreach (var fieldName in form.Fields.Names)
            //        {
            //            // Get the field object by name
            //            //     PdfTextField textField = form.Fields. .FirstOrDefault(field => field.Name == fieldName);
            //            //  PdfAcroField field = fieldEntry.Value;


            //            PdfAcroField field = document.AcroForm.Fields[fieldName];
            //            // Check if the field is a textbox and its name matches the desired textbox

            //            if (field.Name == "demo_fname")
            //            {
            //                // Set the value of the textbox


            //                field.Value = new PdfString("New Value");

            //                // Get the first page of the document

            //            }

            //            if (field.Name == "demo_sex")
            //            {
            //                // Set the value of the textbox



            //                // Get the first page of the document
            //                PdfPage page = document.Pages[0]; // Assuming you want to add the image annotation to the first page

            //                // Define the position and size for the image annotation
            //                double imageX = field.Elements.GetRectangle("/Rect").X1 + 10; // Adjust as needed
            //                double imageY = field.Elements.GetRectangle("/Rect").Y1 + 10; // Adjust as needed
            //                double imageWidth = 50; // Adjust as needed
            //                double imageHeight = 50; // Adjust as needed

            //                // Add the image annotation to the page
            //                XGraphics gfx = XGraphics.FromPdfPage(page);
            //                XImage image = XImage.FromFile(imgPath); // Replace "image.jpg" with your image path
            //                gfx.DrawImage(image, imageX, imageY, imageWidth, imageHeight);
            //            }

            //        }
            //    }


            //    // Save the modified document to a new memory stream
            //    MemoryStream outputStream = new MemoryStream();
            //    document.Save(outputStream, false);
            //    byte[] updatedPdfBytes = outputStream.ToArray();
            //    return File(updatedPdfBytes, "application/pdf", "updated.pdf");

            //}
            return View();
        }
        #region Private Method
        private void SaveLog(Exception ex, string actionname)
        {
            var msg = "";
            if (ex.InnerException == null)
            {
                _logger.LogError(ex.Message);
                msg = ex.Message;
            }
            else
            {
                _logger.LogError(ex.InnerException.Message);
                msg = ex.InnerException.Message;
            }
            var logdata = new tbl_log
            {
                CreatedDate = DateTime.Now,
                CreatedBy = HttpContext.Session.GetInt32(SessionKeys.SessionCmpUserId),
                Message = msg
            };
            new LogService().Insert(logdata);
        }
        #endregion
    }
}
//    public class MyFontResolver : IFontResolver
//    {
//        public byte[] GetFont(string faceName)
//        {
//            // Implement your font retrieval logic here
//            // This method is not used if you want to ignore a font, so you can leave it unchanged
//            return null;
//        }

//        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
//        {
//            // Implement your logic to resolve typefaces here
//            // You can return default font information for specific font families you want to ignore

//            // Example: Ignore "Courier New" font and use the default font instead
//            if (familyName.Equals("Courier New", StringComparison.OrdinalIgnoreCase))
//            {
//                // Return the default font information (e.g., Arial) for "Courier New"
//                return new FontResolverInfo("Arial");
//            }

//            // Return null if you want to use PDFSharp's default font resolution logic for other font families
//            return null;
//        }
//    }

//}
