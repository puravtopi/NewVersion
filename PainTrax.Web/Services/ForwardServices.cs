using MS.Models;
using MS.Services;
using PainTrax.Services;
using PainTrax.Web.Models;
using System.Reflection;

namespace PainTrax.Web.Services
{
    public class ForwardServices : ParentService
    {
        private readonly FUPage1Service _fuPage1services = new FUPage1Service();
        private readonly FUPage2Service _fuPage2services = new FUPage2Service();
        private readonly FUNEService _funeservices = new FUNEService();
        private readonly FUOtherService _fuotherservices = new FUOtherService();
        private readonly PatientIEService _ieService = new PatientIEService();

        #region  IE_to_FU
        public void CopyProperties(object source, object destination, int cmp_id, string tblname = "")
        {
            Type sourceType = source.GetType();
            Type destinationType = destination.GetType();
            List<tbl_ie_fu_select> dataList = null;
            PropertyInfo[] sourceProperties = sourceType.GetProperties();
            PropertyInfo[] destinationProperties = destinationType.GetProperties();
            if (tblname != null)
                dataList = ConvertDataTable<tbl_ie_fu_select>(GetData("select * from tbl_ie_fu_select where tbl_name='" + tblname + "' and cmp_id=" + cmp_id));

            foreach (PropertyInfo sourceProperty in sourceProperties)
            {
                PropertyInfo destinationProperty = Array.Find(destinationProperties, p => p.Name == sourceProperty.Name);
                if (tblname != "")
                {

                    if (destinationProperty != null && destinationProperty.PropertyType == sourceProperty.PropertyType)
                    {
                        if (dataList.Exists(d => d.tbl_column == destinationProperty.Name))
                        {
                            object value = sourceProperty.GetValue(source);
                            destinationProperty.SetValue(destination, value);
                        }
                    }
                }
                else
                {
                    if (destinationProperty != null && destinationProperty.PropertyType == sourceProperty.PropertyType)
                    {
                        object value = sourceProperty.GetValue(source);
                        destinationProperty.SetValue(destination, value);
                    }
                }
            }
        }
        public tbl_fu_page1? GetOnePage1(int ie_id, int fu_id, int cmp_id, int patient_id)
        {
            PatientIEService service = new PatientIEService();
            tbl_ie_page1 ie_Page1 = new tbl_ie_page1();
            tbl_fu_page1 fu_Page1 = new tbl_fu_page1();
            ie_Page1 = _ieService.GetOnePage1(ie_id);
            fu_Page1 = _fuPage1services.GetOne(fu_id);
            if (fu_Page1 == null)
            {
                fu_Page1 = new tbl_fu_page1();
                CopyProperties(ie_Page1, fu_Page1,cmp_id, "page1");
                fu_Page1.ie_id = ie_id;
                fu_Page1.fu_id = fu_id;
                fu_Page1.cmp_id = cmp_id;
                fu_Page1.patient_id = patient_id;
                _fuPage1services.Insert(fu_Page1);
            }
            return fu_Page1;
        }

        public tbl_fu_page2? GetOnePage2(int ie_id, int fu_id, int cmp_id, int patient_id)
        {
            PatientIEService service = new PatientIEService();
            tbl_ie_page2 ie_Page2 = new tbl_ie_page2();
            tbl_fu_page2 fu_Page2 = new tbl_fu_page2();
            ie_Page2 = _ieService.GetOnePage2(ie_id);
            fu_Page2 = _fuPage2services.GetOne(fu_id);
            if (fu_Page2 == null)
            {
                fu_Page2 = new tbl_fu_page2();
                CopyProperties(ie_Page2, fu_Page2,cmp_id, "page2");
                fu_Page2.ie_id = ie_id;
                fu_Page2.fu_id = fu_id;
                fu_Page2.cmp_id = cmp_id;
                fu_Page2.patient_id = patient_id;
                _fuPage2services.Insert(fu_Page2);
            }
            return fu_Page2;
        }

        public tbl_fu_ne? GetOneNE(int ie_id, int fu_id, int cmp_id, int patient_id)
        {
            PatientIEService service = new PatientIEService();
            tbl_ie_ne ie_ne = new tbl_ie_ne();
            tbl_fu_ne fu_ne = new tbl_fu_ne();
            ie_ne = _ieService.GetOneNE(ie_id);
            fu_ne = _funeservices.GetOne(fu_id);
            if (fu_ne == null)
            {
                fu_ne = new tbl_fu_ne();
                CopyProperties(ie_ne, fu_ne,cmp_id, "ne");
                fu_ne.ie_id = ie_id;
                fu_ne.fu_id = fu_id;
                fu_ne.cmp_id = cmp_id;
                fu_ne.patient_id = patient_id;
                _funeservices.Insert(fu_ne);
            }
            return fu_ne;
        }

        public tbl_fu_other? GetOneOther(int ie_id, int fu_id,int cmp_id, int patient_id)
        {
            PatientIEService service = new PatientIEService();
            tbl_ie_other ie_other = new tbl_ie_other();
            tbl_fu_other fu_other = new tbl_fu_other();
            ie_other = _ieService.GetOneOtherPage(ie_id);
            fu_other = _fuotherservices.GetOne(fu_id);
            if (fu_other == null)
            {
                fu_other = new tbl_fu_other();
                CopyProperties(ie_other, fu_other,cmp_id, "other");
                fu_other.ie_id = ie_id;
                fu_other.fu_id = fu_id;
                fu_other.patient_id = patient_id;

                _fuotherservices.Insert(fu_other);
            }
            return fu_other;
        }


        #endregion
    }
}
