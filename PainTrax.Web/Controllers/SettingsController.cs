using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PainTrax.Web.Helper;
using PainTrax.Web.Models;
using PainTrax.Web.Services;

namespace PainTrax.Web.Controllers
{
    public class SettingsController : Controller
    {
        #region Variables
        private readonly IMapper _mapper;
        private readonly SettingsService _services = new SettingsService();
        private readonly ILogger<SettingsController> _logger;
        private readonly Common _commonservices = new Common();
        #endregion

        public IActionResult Index()
        {
            var data = new tbl_settings();
            try
            {
              
                int? cmpid = HttpContext.Session.GetInt32(SessionKeys.SessionCmpId);

                ViewBag.locList = _commonservices.GetLocations(cmpid.Value);
                data = _services.GetOne(cmpid.Value);
            }
            catch(Exception ex)
            {
                SaveLog(ex, "Index");
            }           

            return View(data);
        }

        [HttpPost]
        public IActionResult Index(tbl_settings model)
        {           
            try
            {              
                int? cmpid = HttpContext.Session.GetInt32(SessionKeys.SessionCmpId);
                model.cmp_id = cmpid.Value;
                _services.Update(model);
                
                ViewBag.locList = _commonservices.GetLocations(cmpid.Value);
                HttpContext.Session.SetInt32(SessionKeys.SessionLocationId, model.location);
                HttpContext.Session.SetInt32(SessionKeys.SessionPageSize, model.page_size);
                return View(model);
            }
            catch (Exception ex)
            {
                SaveLog(ex, "Index");
            }
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
