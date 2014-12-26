using HFTP.Security;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HFTP.Entrust
{
    #region 结构和枚举
    public enum EntrustVendor
    {
        NONE,
        Hundsun,    //恒生接口
        Wind        //Wind接口
    }
    public enum TradeDirection
    {
        NONE = 0,
        BUY = 1,
        SELL = 2
    }
    public enum FutureDirection
    {
        NONE = 0,
        OPEN = 1,
        COVER = 2
    }
    public enum PostionDerection
    {
        NONE,
        LONG,
        SHORT
    }
    public class EntrustPara
    {
        public string portfolio;      //组合代码
        public Exchange exchange;     //市场代码
        public string securitycode;    //证券代码
        public double volume;
        public double price;

        public TradeDirection entrustdirection = TradeDirection.NONE;   //买卖
        public FutureDirection futuredirection = FutureDirection.NONE;     //开平
    }
    public class QueryPara
    {
        public string fundcode; //基金代码
        public string portfolio;     //组合代码
        public string securitycode;
    }
    #endregion

    public abstract class AEntrust
    {
        #region 静态
        private static Hashtable _htentrustengines = new Hashtable();
        public static AEntrust GetInstance(EntrustVendor vendor)
        {
            try
            {
                if (_htentrustengines.Contains(vendor))
                    return (AEntrust)_htentrustengines[vendor];

                AEntrust entrustengine = null;
                switch (vendor)
                {
                    case EntrustVendor.Hundsun:
                        entrustengine = new EntrustHundsun();
                        break;
                    case EntrustVendor.Wind:
                        break;
                    default:
                        break;
                }

                if (entrustengine == null)
                {
                    MessageManager.GetInstance().Add(MessageType.Error, string.Format("未找到委托接口：{0}", vendor.ToString()));
                    return null;
                }

                _htentrustengines.Add(vendor, entrustengine);
                return entrustengine;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        #endregion

        public string name;
        public EntrustVendor Vendor = EntrustVendor.NONE;
        public abstract void SendHeartbeat();
        public abstract string GetToken();
        public abstract void Logon();
        public abstract void Logon(string user, string pwd);
        public abstract void OptionEntrustQuery(QueryPara param, List<EntrustBook> entrustbook);
        public abstract void OptionPositionQuery(QueryPara param, List<PositionBook> positionbook);
        public abstract void OptionWithdraw(List<int> entrustnolist);
        public abstract void OptionSingleEntrust(EntrustPara param);
        public abstract void OptionBasketEntrust(List<EntrustPara> paramlist);
    }
}
