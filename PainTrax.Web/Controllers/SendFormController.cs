using Microsoft.AspNetCore.Mvc;
using PainTrax.Web.Helper;
using PainTrax.Web.Models;
using PainTrax.Web.Services;

namespace PainTrax.Web.Controllers
{
    public class SendFormController : Controller
    {
        private readonly CommonService _commonservices = new CommonService();
        private Microsoft.AspNetCore.Hosting.IHostingEnvironment Environment;
        private readonly ILogger<HomeController> _logger;

        public SendFormController(Microsoft.AspNetCore.Hosting.IHostingEnvironment environment, ILogger<HomeController> logger)
        {
            Environment = environment;
            _logger = logger;
        }

        public IActionResult AuthoAckno(string id = "Tmy87eGD4pBwqmBnU1kYZhtyTy9oAonqlCLlpQzjACM=", string type = "IE")       
        {
            var _id = EncryptionHelper.Decrypt(id);

            if (_commonservices.isSignExist(_id, type) == false)
            {
                var data = _commonservices.getPatientDetails(_id, type);

                if (data == null)
                {
                    data = new Models.PatientDetails()
                    {
                        fname = "",
                        lname = ""

                    };
                }

                data.id = _id;
                data.type = type;
                data.isExist = false;
                return View(data);
            }
            else
            {
                var data = new Models.PatientDetails()
                {
                    isExist = true
                };
                return View(data);
            }

        }

        [HttpPost]
        public IActionResult SaveSign([FromBody] tbl_ie_sign model)
        {
            if (string.IsNullOrEmpty(model.signatureData))
                return BadRequest("Invalid signature data.");
            try
            {

                var base64Data = ExtractBase64Data(model.signatureData);

                if (base64Data == null || !IsBase64String(base64Data))
                {
                    return BadRequest("Invalid Base-64 string.");
                }


                var imageData = Convert.FromBase64String(base64Data);
                var signaturesDir = Path.Combine(Environment.WebRootPath, "signatures", "authoacko");


                if (!Directory.Exists(signaturesDir))
                {
                    Directory.CreateDirectory(signaturesDir);
                }
                var filename = $"{model.type + "_" + model.id}.jpeg";
                var savePath = Path.Combine(signaturesDir, filename);

                System.IO.File.WriteAllBytes(savePath, imageData);


                string ie_id = "", fu_id = "";

                if (model.type == "IE")
                    ie_id = model.id.ToString();
                else
                    fu_id = model.id.ToString();

                var id = _commonservices.InsertSign(filename, base64Data, ie_id, fu_id);


                return Ok(new { id = id });
            }
            catch (Exception ex)
            {
                SaveLog(ex, "SaveSign");
                return Ok(new { id = 0 });
            }
        }
        private string ExtractBase64Data(string signatureData)
        {
            // Example method to extract Base64 data from a comma-separated string
            // Adjust as per your actual data structure
            return signatureData.Contains(",")
                   ? signatureData.Split(',')[1].Trim()
                   : signatureData.Trim();
        }

        private bool IsBase64String(string base64)
        {
            Span<byte> buffer = new Span<byte>(new byte[base64.Length]);
            return Convert.TryFromBase64String(base64, buffer, out _);
        }

        #region private Method
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
                Message = msg,
            };
            new LogService().Insert(logdata);
        }
        #endregion 
    }
}
