using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HFTP
{
    public class Utility
    {
        public static DateTime C_NULL_DATE = new DateTime(1900, 1, 1);

        public static DateTime ConvertToDateTime(string date, string pattern)
        {
            try
            {
                string strdate = date;
                switch (pattern)
                {
                    case "yyyyMMdd":
                        strdate = date.Substring(0, 4) + "-" + date.Substring(4, 2) + "-" + date.Substring(6);
                        break;
                    default:
                        break;
                }

                return Convert.ToDateTime(strdate);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static double ConvertToDouble(string value, double defaultvalue)
        {
            try
            {
                value = value.Trim();
                if (value.Length == 0)
                    return defaultvalue;
                else
                    return Convert.ToDouble(value);
            }
            catch (Exception)
            {
                return defaultvalue;
            }
        }
    }
}
