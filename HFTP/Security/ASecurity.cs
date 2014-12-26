
using HFTP.Entrust;
using HFTP.Market;
using System;
using System.Collections.Generic;
using System.Diagnostics;
namespace HFTP.Security
{
    /// <summary>
    /// 证券枚举类
    /// </summary>
    public enum Exchange
    {
        NONE,
        SHE,
        SZE,
        HKE
    }

    public enum SecurityCategory
    { 
        NONE,
        STOCK,
        ETF,
        OPTION,
        OTHER   //债券、权证等
    }

    /// <summary>
    /// 证券抽象类
    /// </summary>
    public abstract class ASecurity
    {
        #region 工具函数(static)
        public static Exchange GetExchange(string windcode, ref string tradecode)
        {
            int idx = windcode.IndexOf(".");
            if (idx>=0)
            {
                tradecode = windcode.Substring(0, idx).ToUpper();
                string exchange = windcode.Substring(idx + 1).ToUpper();
                switch (exchange)
                {
                    case "SH":
                        return Exchange.SHE;
                    case "SZ":
                        return Exchange.SZE;
                    case "HK":
                        return Exchange.HKE;
                    default:
                        return Exchange.NONE;
                }
            }
            else
            {
                tradecode = windcode;
                return Exchange.NONE;
            }
        }
        public static SecurityCategory GetSecurityCategory(string code, Exchange exchange)
        {
            string msg = string.Format("未知的证券：{0},{1}", code, exchange);
            string flg1 = code.Substring(0, 1), flg2 = code.Substring(1, 2);

            switch (exchange)
            {
                case Exchange.SHE:
                    #region SHE
                    //<<上海证券交易所证券代码分配规则>>
                    //  修订：http://www.sse.com.cn/aboutus/hotandd/ssenews/c/c_20121024_51225.shtml
                    if (code.Length == 6)
                    {
                        switch (flg1)
                        {
                            case "0":
                            case "1":
                            case "2":
                            case "3":
                            case "4":
                                return SecurityCategory.OTHER;
                            case "5":
                                switch (flg2)
	                            {
                                    case "10":
                                        return SecurityCategory.ETF;
                                    default:
                                        return SecurityCategory.OTHER;
	                            }
                            case "6":
                                return SecurityCategory.STOCK;
                            case "7":
                            case "8":
                            case "9":
                                return SecurityCategory.OTHER;
                            default:
                                break;
                        }
                    }
                    #endregion
                    break;
                case Exchange.SZE:
                    #region SZE
                    if (code.Length == 6)
                    {
                        switch (flg1)
                        {
                            case "0":   //A股
                                return SecurityCategory.STOCK;
                            case "1":   //基金
                                return SecurityCategory.ETF;
                            case "2":   //B股
                                return SecurityCategory.OTHER;
                            case "3":   //创业板
                                return SecurityCategory.STOCK;
                            case "4":   //股转
                            default:
                                break;
                        }
                    }
                    #endregion
                    break;
                case Exchange.HKE:
                    #region HKE
                    //http://www.hkex.com.hk/chi/market/sec_tradinfo/stockcode/documents/scap_c.pdf
                    try
                    {
                        int icode = Convert.ToInt16(code);

                        if (icode >= 2800 && icode <= 2849 || icode >= 3000 && icode <= 3199)
                            return SecurityCategory.ETF;    //港币交易

                        if (icode >= 82800 && icode <= 82849 || icode >= 83000 && icode <= 83199)
                            return SecurityCategory.ETF;    //人民币交易

                        if (icode < 4000)
                            return SecurityCategory.STOCK;  //港股：0001~3999
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    #endregion
                    break;
                default:
                    break;
            }

            MessageManager.GetInstance().Add(MessageType.Error, msg);
            throw new Exception(msg);
        }
        public static ASecurity GetSecurity(SecurityCategory category, string code, Exchange exchange)
        {
            switch (category)
            {
                case SecurityCategory.STOCK:
                    return new Stock(code, exchange);
                case SecurityCategory.ETF:
                    return new ETF(code,exchange);
                default:
                    return null;
            }
        }
        #endregion

        #region 引擎接口(static)
        ////行情
        //protected static AMarket _marketengine;
        //public static AMarket ConnectMarketEngine(MarketVendor vendor)
        //{
        //    if (_marketengine == null || _marketengine.Vendor != vendor)
        //        _marketengine = AMarket.GetInstance(vendor);

        //    return _marketengine;
        //}
        ////委托
        //protected static AEntrust _entrustengine;
        //public static AEntrust ConnectEntrustEngine(EntrustVendor vendor)
        //{
        //    if (_entrustengine == null || _entrustengine.Vendor != vendor)
        //    {
        //        try
        //        {
        //            _entrustengine = AEntrust.GetInstance(vendor);
        //            _entrustengine.Logon();
        //        }
        //        catch (Exception ex)
        //        {
        //            throw ex;
        //        }
        //    }

        //    return _entrustengine;
        //}
        #endregion

        #region 证券属性
        public string name;
        public string code;
        public Exchange exchange = Exchange.NONE;
        public SecurityCategory category = SecurityCategory.NONE;
        #endregion

        #region 交易属性
        public BidAskBook bidaskbook = new BidAskBook();
        public List<EntrustBook> entrustbook = new List<EntrustBook>();
        public List<PositionBook> positionbook = new List<PositionBook>();
        #endregion

        public ASecurity(string code, Exchange exchange, SecurityCategory category)
        {
            this.code = code;
            this.exchange = exchange;
            this.category = category;
        }

        public string GetWindCode()
        {
            string appendix = "";
            switch (this.exchange)
            {
                case Exchange.SHE:
                    appendix = ".SH";
                    break;
                case Exchange.SZE:
                    appendix = ".SZ";
                    break;
                case Exchange.HKE:
                    if (code.ToUpper().Contains("I"))
                        appendix = "HI";
                    else
                        appendix = ".HK";
                    break;
                default:
                    break;
            }

            return this.code + appendix;
        }
        
        public virtual void DebugPrint()
        {
            Debug.Print(string.Format("code={0},name={1},exchange={2},category={3}",this.code, this.name, this.exchange.ToString(),this.category.ToString()));
        }
    }
}
