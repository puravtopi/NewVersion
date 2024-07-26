using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace PainTrax.Web.Helper
{
    public class EnumHelper
    {
        public enum CaseType
        {
            [Display(Name = "NF")]
            NF = 1,
            [Display(Name = "MM")]
            MM = 2,
            [Display(Name = "Lien")]
            Lien = 3,
            [Display(Name = "WC")]
            WC = 4,
            [Display(Name = "OON")]
            OON = 5,
            [Display(Name = "LOP")]
            LOP = 6
        }

        public enum VisitType
        {
            [Display(Name = "FU")]
            FU = 1,
            [Display(Name = "Postop FU")]
            ProFU = 2,
            [Display(Name = "Preop H&P")]
            PriFU = 3,
            [Display(Name = "Tele FU")]
            TeleFU = 4
        }

        public enum Gender
        {
            [Display(Name = "")]
            na = 0,
            [Display(Name = "Male")]
            male = 1,
            [Display(Name = "Female")]
            female = 2,
            [Display(Name = "Other")]
            Other = 3

        }

        public enum MrMrs
        {
            [Display(Name = "Mr.")]
            male = 1,
            [Display(Name = "Ms.")]
            female = 2,
            [Display(Name = "Other")]
            Other = 3

        }

        public enum Study1
        {
            [Display(Name = "MRI")]
            MRI = 1,
            [Display(Name = "CT-Scan")]
            CTScan = 2,
            [Display(Name = "X-Rays")]
            XRays = 3,
            [Display(Name = "TM Flow")]
            TMFlow = 4
        }

        public enum Study2
        {
            [Display(Name = "MRI")]
            MRI = 1,
            [Display(Name = "CT-Scan")]
            CTScan = 2,
            [Display(Name = "X-Rays")]
            XRays = 3,
            [Display(Name = "UE NCV/EMG")]
            UENCV_EMG = 4,
            [Display(Name = "LE NCV/EMG")]
            LENCV_EMG = 5,
            [Display(Name = "TM Flow")]
            TMFlow = 6
        }

        public static string GetDisplayName(Enum enumValue)
        {
            return enumValue.GetType()
                            .GetMember(enumValue.ToString())
                            .First()
                            .GetCustomAttribute<DisplayAttribute>()
                            .GetName();
        }

    }
}
