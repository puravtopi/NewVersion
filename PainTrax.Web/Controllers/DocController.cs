using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml;
using HtmlToOpenXml;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Crypto.IO;
using System.IO;

namespace PainTrax.Web.Controllers
{
    public class DocController : Controller
    {
        public IActionResult Index()
        {
            string htmlContent = "<h1>Test</h1><h1>Test</h1><h1>Test</h1><h1>Test</h1><h1>Test</h1><h1>Test</h1><h1>Test</h1>";
            htmlContent += "<h1>Test</h1><h1>Test</h1><h1>Test</h1><h1>Test</h1><h1>Test</h1><h1>Test</h1><h1>Test</h1>";
            htmlContent += "<h1>Test</h1><h1>Test</h1><h1>Test</h1><h1>Test</h1><h1>Test</h1><h1>Test</h1><h1>Test</h1>";
            htmlContent += "<h1>Test</h1><h1>Test</h1><h1>Test</h1><h1>Test</h1><h1>Test</h1><h1>Test</h1><h1>Test</h1>";
            htmlContent += "<h1>Test</h1><h1>Test</h1><h1>Test</h1><h1>Test</h1><h1>Test</h1><h1>Test</h1><h1>Test</h1>";
            htmlContent += "<h1>Test</h1><h1>Test</h1><h1>Test</h1><h1>Test</h1><h1>Test</h1><h1>Test</h1><h1>Test</h1>";
            htmlContent += "<h1>Test</h1><h1>Test</h1><h1>Test</h1><h1>Test</h1><h1>Test</h1><h1>Test</h1><h1>Test</h1>";
            using (MemoryStream memStream = new MemoryStream())
            {
                using (WordprocessingDocument doc = WordprocessingDocument.Create(memStream, WordprocessingDocumentType.Document))
                {

                    MainDocumentPart mainPart = doc.AddMainDocumentPart();

                    var headerPart = mainPart.AddNewPart<HeaderPart>("First");
                    var restheaderPart = mainPart.AddNewPart<HeaderPart>("Rest");
                    var footerPart = mainPart.AddNewPart<FooterPart>();

                    // Create the main document part content
                    mainPart.Document = new Document();
                    Body body = mainPart.Document.AppendChild(new Body());
                    //    SectionProperties sectionProps = body.AppendChild(new SectionProperties());

                    // Convert HTML to OpenXML and add to the body
                    HtmlConverter converter = new HtmlConverter(mainPart);
                    var generatedBody = converter.Parse(htmlContent);
                    body.Append(generatedBody);

                    var header = new Header(new Paragraph(new Run(new Text("First Page Header Test"))));
                    HeaderReference headerReference = new HeaderReference() { Type = HeaderFooterValues.First, Id = mainPart.GetIdOfPart(headerPart) };
                    var restheader = new Header(new Paragraph(new Run(new Text("Other Page Header "))));
                    HeaderReference restheaderReference = new HeaderReference() { Type = HeaderFooterValues.Default, Id = mainPart.GetIdOfPart(restheaderPart) };
                    var footer = new Footer(new Paragraph(new Run(new Text("Page"), new SimpleField() { Instruction = "PAGE" })));
                    FooterReference footerReference = new FooterReference() { Type = HeaderFooterValues.Default, Id = mainPart.GetIdOfPart(footerPart) };

                    headerPart.Header = header;
                    restheaderPart.Header = restheader;
                    footerPart.Footer = footer;


                    //mainPart.Document.Body.Append(new SectionProperties(headerReference,restheaderReference, footerReference));
                    SectionProperties sectionProps = new DocumentFormat.OpenXml.Wordprocessing.SectionProperties();
                    sectionProps.Append(restheaderReference);

                    sectionProps.Append(headerReference);
                    sectionProps.Append(footerReference);
                    sectionProps.Append(new TitlePage());
                    //SectionProperties sectionProps2 = new SectionProperties();
                    //sectionProps.Append(footerReference);
                    //sectionProps.InsertAt(new HeaderReference() { Type = HeaderFooterValues.First, Id=mainPart.GetIdOfPart(headerPart) }, 0);
                    //sectionProps.InsertAt(new HeaderReference() { Type = HeaderFooterValues.Default, Id = mainPart.GetIdOfPart(restheaderPart) }, 1);

                    mainPart.Document.Body.Append(sectionProps);
                    //body.Append(sectionProps2);




                }


                byte[] fileBytes = memStream.ToArray();
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "output.docx");
            }
        }
    }
}
