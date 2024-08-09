using Microsoft.AspNetCore.Mvc.Rendering;
using PainTrax.Web.Services;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace PainTrax.Web.Helper
{
    public class Common
    {
        private readonly DesinationServices _designationservices = new DesinationServices();
        private readonly GroupsService _groupservices = new GroupsService();
        private readonly LocationsService _locationservices = new LocationsService();
        private readonly CaseTypeService _caseTypeService = new CaseTypeService();
        private readonly UserService _userservices = new UserService();
        private readonly CommonService _commonService = new CommonService();
        private readonly AccidentTypeService _accidentTypeService = new AccidentTypeService();
        private readonly StateService _stateService = new StateService();
        private readonly ReferringPhysicianService _physicianService = new ReferringPhysicianService();

        public List<SelectListItem> GetDesignation(int cmp_id)
        {

            string cnd = " and cmp_id=" + cmp_id;

            var dgdata = _designationservices.GetAll(cnd);
            var dglist = new List<SelectListItem>();
            dglist.Add(new SelectListItem
            {
                Text = "--Select Designation--",
                Value = ""
            });

            foreach (var item in dgdata)
            {
                dglist.Add(new SelectListItem
                {
                    Text = item.title,
                    Value = item.id.ToString()
                });
            }


            return dglist;
        }

        public List<SelectListItem> GetGroups(int cmp_id)
        {
            string cnd = " and cmp_id=" + cmp_id;
            var gpdata = _groupservices.GetAll(cnd);
            var gplist = new List<SelectListItem>();
            gplist.Add(new SelectListItem
            {
                Text = "--Select Group--",
                Value = ""
            });

            foreach (var item in gpdata)
            {
                gplist.Add(new SelectListItem
                {
                    Text = item.Title,
                    Value = item.Id.ToString()
                });
            }

            return gplist;
        }

        public List<SelectListItem> GetLocations(int cmp_id)
        {
            string cnd = " and cmp_id=" + cmp_id;
            var data = _locationservices.GetAll(cnd);
            var list = new List<SelectListItem>();


            list.Add(new SelectListItem
            {
                Text = "--Select Location--",
                Value = ""
            });


            foreach (var item in data)
            {
                list.Add(new SelectListItem
                {
                    Text = item.location,
                    Value = item.id.ToString()
                });
            }

            return list;
        }
        public List<SelectListItem> GetCaseType(int cmp_id)
        {
            string cnd = " and cmp_id=" + cmp_id;
            var data = _caseTypeService.GetAll(cnd);
            var list = new List<SelectListItem>();


            list.Add(new SelectListItem
            {
                Text = "--Select CaseType--",
                Value = ""
            });


            foreach (var item in data)
            {
                list.Add(new SelectListItem
                {
                    Text = item.casetype,
                    Value = item.casetype
                });
            }

            return list;
        }

        public List<SelectListItem> GetAccidenttype(int cmp_id)
        {
            string cnd = " and cmp_id=" + cmp_id;
            var data = _accidentTypeService.GetAll(cnd);
            var list = new List<SelectListItem>();


            list.Add(new SelectListItem
            {
                Text = "--Select Accident Type--",
                Value = ""
            });


            foreach (var item in data)
            {
                list.Add(new SelectListItem
                {
                    Text = item.accidenttype,
                    Value = item.accidenttype
                });
            }

            return list;
        }

        public List<SelectListItem> GetUsers(int cmp_id)
        {
            string cnd = " and cmp_id=" + cmp_id;
            var data = _userservices.GetAll(cnd);
            var list = new List<SelectListItem>();

            list.Add(new SelectListItem
            {
                Text = "--Select User--",
                Value = ""
            });

            foreach (var item in data)
            {
                list.Add(new SelectListItem
                {
                    Text = item.fname + ' ' + item.lname,
                    Value = item.Id.ToString()
                });
            }

            return list;
        }
        public List<SelectListItem> GetState(int cmp_id)
        {
            // string cnd = " and cmp_id=" + cmp_id;
            string cnd = " ";
            var data = _stateService.GetAll(cnd);
            var list = new List<SelectListItem>();


            list.Add(new SelectListItem
            {
                Text = "--Select State--",
                Value = ""
            });


            foreach (var item in data)
            {
                list.Add(new SelectListItem
                {
                    Text = item.state_name,
                    Value = item.state_name
                });
            }

            return list;
        }
        public List<SelectListItem> GetPhysician(int cmp_id)
        {
            string cnd = " and cmp_id=" + cmp_id;
            var data = _physicianService.GetAll(cnd);
            var list = new List<SelectListItem>();


            list.Add(new SelectListItem
            {
                Text = "--Select Physician--",
                Value = ""
            });


            foreach (var item in data)
            {
                list.Add(new SelectListItem
                {
                    Text = item.physicianname,
                    Value = item.Id.ToString(),
                });
            }

            return list;
        }

        public List<CheckBoxItem> GetLocationsCheckBoxList(int cmp_id, string loc_ids = "")
        {
            string cnd = " and cmp_id=" + cmp_id;
            var data = _locationservices.GetAll(cnd);
            var list = new List<CheckBoxItem>();


            foreach (var item in data)
            {
                list.Add(new CheckBoxItem
                {
                    Item = item.location,
                    Id = item.id.Value,
                    IsChecked = loc_ids.Contains(item.id.ToString())
                });
            }

            return list;
        }

        public List<CheckBoxItem> GetPagesCheckBoxList(string pages_ids = "")
        {

            var data = _commonService.GetPagesAll("");
            var list = new List<CheckBoxItem>();


            foreach (var item in data)
            {
                list.Add(new CheckBoxItem
                {
                    Item = item.page_name,
                    Id = item.id,
                    IsChecked = pages_ids.Contains(item.id.ToString())
                });
            }

            return list;
        }

        public List<CheckBoxItem> GetReportsCheckBoxList(string pages_ids = "")
        {

            var data = _commonService.GetReportsAll("");
            var list = new List<CheckBoxItem>();


            foreach (var item in data)
            {
                list.Add(new CheckBoxItem
                {
                    Item = item.report_name,
                    Id = item.id,
                    IsChecked = pages_ids.Contains(item.id.ToString())
                });
            }

            return list;
        }

        public List<CheckBoxItem> GetRolsCheckBoxList(string pages_ids = "")
        {

            var data = _commonService.GetRoleAll("");
            var list = new List<CheckBoxItem>();


            foreach (var item in data)
            {
                list.Add(new CheckBoxItem
                {
                    Item = item.name,
                    Id = item.id,
                    IsChecked = pages_ids.Contains(item.id.ToString())
                });
            }

            return list;
        }

        public List<SelectListItem> GetSOAPSettingList(string colNames)
        {

            var data = GetPage1Feilds();
            var list = new List<SelectListItem>();


            foreach (var item in data)
            {
                list.Add(new SelectListItem
                {
                    Value = item.Value,
                    Text = item.Text,
                    Selected = colNames.Contains(item.Value)
                });
            }

            return list;
        }

        public List<SelectListItem> GetPage2SettingList(string colNames)
        {

            var data = GetPage2Feilds();
            var list = new List<SelectListItem>();


            foreach (var item in data)
            {
                list.Add(new SelectListItem
                {
                    Value = item.Value,
                    Text = item.Text,
                    Selected = colNames.Contains(item.Value)
                });
            }

            return list;
        }
        public List<SelectListItem> GetPageNESettingList(string colNames)
        {

            var data = GetPageNEFeilds();
            var list = new List<SelectListItem>();


            foreach (var item in data)
            {
                list.Add(new SelectListItem
                {
                    Value = item.Value,
                    Text = item.Text,
                    Selected = colNames.Contains(item.Value)
                });
            }

            return list;
        }

        public List<SelectListItem> GetPageOtherSettingList(string colNames)
        {

            var data = GetPageOtherFeilds();
            var list = new List<SelectListItem>();


            foreach (var item in data)
            {
                list.Add(new SelectListItem
                {
                    Value = item.Value,
                    Text = item.Text,
                    Selected = colNames.Contains(item.Value)
                });
            }

            return list;
        }

        private static string Encrypt(string clearText)
        {
            string encryptionKey = "PAINTRAX2023";
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(encryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return clearText;
        }

        private static string Decrypt(string cipherText)
        {
            string encryptionKey = "PAINTRAX2023";
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(encryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return cipherText;
        }

        public static DateTime? ConvertDate(string date)
        {
            if (string.IsNullOrEmpty(date) == false)
            {
                return Convert.ToDateTime(date.Replace("-", "/"));
            }
            else
                return null;
        }

        public static string commonDate(DateTime? date, string dateformat = "MM/dd/yyyy")
        {
            if (date != null)
                return Convert.ToDateTime(date).ToString(dateformat).Replace('-', '/');
            else
                return "";
        }

        public static string? GetMrMrsFromSex(string sex)
        {
            if (string.IsNullOrEmpty(sex) == false)
            {
                return sex == "1" ? "Mr." : "Ms.";
            }
            else
                return "";
        }

        public static string? GetGenderFromSex(string sex)
        {
            if (string.IsNullOrEmpty(sex) == false)
            {
                return sex == "1" ? "male" : "female";
            }
            else
                return "";
        }

        public static string? GetGenderFromMrMrs(string sex)
        {
            if (string.IsNullOrEmpty(sex) == false)
            {
                return sex == "Mr." ? "1" : "2";
            }
            else
                return "";
        }

        public static string FirstCharToUpper(string input)
        {
            if (String.IsNullOrEmpty(input))
                throw new ArgumentException("ARGH!");
            return input.First().ToString().ToUpper() + input.Substring(1);
        }

        public static string ReplceCommowithAnd(string input)
        {
            if (!string.IsNullOrEmpty(input))
            {
                var lastComma = input.LastIndexOf(',');
                if (lastComma != -1)
                    input = input.Remove(lastComma, 1).Insert(lastComma, " and ");

            }
            return input;
        }


        private List<SelectListItem> GetPage1Feilds()
        {

            List<SelectListItem> returnstatus = new List<SelectListItem>
            {


                new SelectListItem{ Text="History", Value = "history",Selected=false },
                new SelectListItem{ Text="Body Parts", Value = "bodypart",Selected=false },
                new SelectListItem{ Text="CE", Value = "cc",Selected=false },
                new SelectListItem{ Text="PE", Value = "pe",Selected=false },
                new SelectListItem{ Text="Assessment", Value = "assessment",Selected=false },
                new SelectListItem{ Text="Plan", Value = "plan",Selected=false },
                new SelectListItem{ Text="Occupation", Value = "occupation",Selected=false },
                new SelectListItem{ Text="Degree of Disability", Value = "dd",Selected=false },
                new SelectListItem{ Text="Work Status", Value = "work_status",Selected=false },
                new SelectListItem{ Text="PMH", Value = "pmh",Selected=false },
                new SelectListItem{ Text="PSH", Value = "psh",Selected=false },
                new SelectListItem{ Text="Allergies", Value = "allergies",Selected=false },
                new SelectListItem{ Text="Medication", Value = "medication",Selected=false },
                new SelectListItem{ Text="Family History", Value = "family_history",Selected=false },
                new SelectListItem{ Text="Social History", Value = "social_history",Selected=false }


                          };
            return returnstatus;

        }

        private List<SelectListItem> GetPage2Feilds()
        {

            List<SelectListItem> returnstatus = new List<SelectListItem>
            {


                new SelectListItem{ Text="ROS", Value = "ros",Selected=false },
                new SelectListItem{ Text="AOD", Value = "aod",Selected=false },
                new SelectListItem{ Text="Other", Value = "other",Selected=false },
                                         };
            return returnstatus;

        }

        private List<SelectListItem> GetPageNEFeilds()
        {

            List<SelectListItem> returnstatus = new List<SelectListItem>
            {


                new SelectListItem{ Text="Neurological Exam", Value = "neurological_exam",Selected=false },
                new SelectListItem{ Text="Sensory", Value = "sensory",Selected=false },
                new SelectListItem{ Text="Manual muscle strength testing", Value = "manual_muscle_strength_testing",Selected=false },
                new SelectListItem{ Text="DTR", Value = "other_content",Selected=false },
                                         };
            return returnstatus;

        }
        private List<SelectListItem> GetPageOtherFeilds()
        {

            List<SelectListItem> returnstatus = new List<SelectListItem>
            {
                new SelectListItem{ Text="Treatment", Value = "treatment_details",Selected=false },
                new SelectListItem{ Text="FOLLOW UP", Value = "followup_duration",Selected=false },
                new SelectListItem{ Text="FOLLOW Date", Value = "followup_date",Selected=false },

            };
            return returnstatus;

        }
    }


    public class CommonSp
    {
        #region Common
        //Common
        public const string CheckExecuteStatus = "sp_check_execute_status";
        public const string GetAllProceduress = "sp_GetAllProceduress";
        public const string SavePatientProceduresDetails = "sp_SavePatientProceduresDetails";
        #endregion
    }


}
