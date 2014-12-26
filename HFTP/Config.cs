using HFTP.Entrust;
using HFTP.Market;
using HFTP.Security;
using System;
using System.Collections;
using System.Collections.Generic;

namespace HFTP
{
    public class Config
    {
        private static Config _instance = null;
        public static Config GetInstance()
        {
            if (_instance == null)
                _instance = new Config();

            return _instance;
        }

        //交易成本
        private List<TradeCost> tradecostlist = new List<TradeCost>();

        //各类参数
        private Hashtable htpara = new Hashtable();

        /// <summary>
        /// 参数名
        /// </summary>
        #region 上交所行情配置
        //行情
        public const string C_PATH_SHE_OPTION_PRICE_TARGET = "SHE_OPTION_PRICE_TARGET";
        public const string C_PATH_SHE_OPTION_PRICE_SOURCE = "SHE_OPTION_PRICE_SOURCE";
        public const string C_PATH_SHE_OPTION_PRICE_HISTORY = "SHE_OPTION_PRICE_HISTORY";
        public const string C_FILE_SHE_OPTION_PRICE = "SHE_OPTION_PRICE_FILE";
        public const string C_FILE_SHE_OPTION_PRICE_SAVEHIST = "SHE_OPTION_PRICE_SAVEHIST";
        //合约
        public const string C_PATH_SHE_OPTION_CONTRACT_TARGET = "SHE_OPTION_CONTRACT_TARGET";
        public const string C_PATH_SHE_OPTION_CONTRACT_SOURCE = "SHE_OPTION_CONTRACT_SOURCE";
        public const string C_FILE_SHE_OPTION_CONTRACT = "SHE_OPTION_PRICE_CONTRACT";
        #endregion

        #region 恒生系统配置
        public const string C_PATH_HUNDSUN_CONFIG = "HUNDSUN_CONFIG";
        public const string C_PARA_HUNDSUN_USER = "HUNDSUN_USER";
        public const string C_PARA_HUNDSUN_PASSWORD = "HUNDSUN_PASSWORD";
        #endregion

        #region 行情/委托引擎配置
        public const string C_MARKETENGINE_OPTION = "MARKETENGINE_OPTION";
        public const string C_MARKETENGINE_OTHER = "MARKETENGINE_OTHER";
        public const string C_ENTRUSTENGINE_OPTION = "ENTRUSTENGINE_OTHER";
        #endregion

        #region 期权做市商配置
        public const string C_PARA_MM_FUNDCODE = "MM_FUNDCODE";
        public const string C_PARA_MM_PORTFOLIO = "MM_PORTFOLIO";
        //最大价差目标[0,1]：设置为最大允许范围的%, e.g. 0.7=允许的最大价差的70%
        public const string C_PARA_MM_MAX_SPREAD_TARGET = "MM_MIN_SPREAD_TARGET";
        //最小下单张数
        public const string C_PARA_MM_MIN_ENTRUST_VOLUME = "MIN_ENTRUST_VOLUME";
        #endregion

        public Config()
        {
            this.loadTradeCost();
            this.loadParameters();
        }

        private void loadTradeCost()
        {
            //上交所
            this.tradecostlist.Add(new TradeCost(Exchange.SHE, SecurityCategory.STOCK));
            this.tradecostlist.Add(new TradeCost(Exchange.SHE, SecurityCategory.ETF));
            this.tradecostlist.Add(new TradeCost(Exchange.SHE, SecurityCategory.OPTION));

            //深交所
            this.tradecostlist.Add(new TradeCost(Exchange.SZE, SecurityCategory.STOCK));
            this.tradecostlist.Add(new TradeCost(Exchange.SZE, SecurityCategory.ETF));
            this.tradecostlist.Add(new TradeCost(Exchange.SZE, SecurityCategory.OPTION));
        }
        private void loadParameters()
        {
            #region 上交所行情配置
            //行情
            //htpara.Add(C_FILE_SHE_OPTION_PRICE, @"mktdt03.txt");
            //TODO: 以下为测试数据
            htpara.Add(C_FILE_SHE_OPTION_PRICE, @"mktdt73.txt");
            MessageManager.GetInstance().Add(MessageType.Warning, string.Format("交易所行情文件临时改为mktdt73"));

            htpara.Add(C_PATH_SHE_OPTION_PRICE_TARGET, @"D:\Project\Data\Option");
            htpara.Add(C_PATH_SHE_OPTION_PRICE_SOURCE, @"\\10.87.50.32\hq2\ggqq");
            htpara.Add(C_PATH_SHE_OPTION_PRICE_HISTORY, @"D:\Project\Data\Option\History");
            htpara.Add(C_FILE_SHE_OPTION_PRICE_SAVEHIST, true);
            //合约
            htpara.Add(C_PATH_SHE_OPTION_CONTRACT_SOURCE, @"\\10.87.50.32\hq2\ggqq");
            htpara.Add(C_PATH_SHE_OPTION_CONTRACT_TARGET, @"D:\Project\Data\Option");
            htpara.Add(C_FILE_SHE_OPTION_CONTRACT, @"reff03<MMdd>.txt".Replace("<MMdd>", DateTime.Today.ToString("MMdd")));
            //TODO: 以下为测试数据
            //htpara.Add(C_FILE_SHE_OPTION_CONTRACT, @"reff031219.txt");
            //MessageManager.GetInstance().Add(MessageType.Warning, string.Format("期权合约文件为测试文件：{0}", "reff031226.txt"));
            #endregion
            
            #region 恒生系统配置
            htpara.Add(C_PATH_HUNDSUN_CONFIG, @"t2sdk.ini");
            htpara.Add(C_PARA_HUNDSUN_USER, @"738");
            htpara.Add(C_PARA_HUNDSUN_PASSWORD, @"Abc123456");
            #endregion

            #region 行情/委托引擎配置
            htpara.Add(C_MARKETENGINE_OPTION, MarketVendor.Exchange);
            htpara.Add(C_MARKETENGINE_OTHER, MarketVendor.Wind);
            htpara.Add(C_ENTRUSTENGINE_OPTION, EntrustVendor.Hundsun);
            #endregion

            #region 期权做市商配置
            htpara.Add(C_PARA_MM_FUNDCODE, @"1104");
            htpara.Add(C_PARA_MM_PORTFOLIO, @"11040201");
            htpara.Add(C_PARA_MM_MAX_SPREAD_TARGET, 0.2);
            htpara.Add(C_PARA_MM_MIN_ENTRUST_VOLUME, 5);
            #endregion
        }

        public TradeCost GetTradeCost(Exchange exchange, SecurityCategory category)
        {
            return this.tradecostlist.Find(delegate(TradeCost c) { return c.exchange == exchange && c.category == category; });
        }

        public object GetParameter(string key)
        {
            if (htpara.Contains(key))
                return htpara[key];

            return null;
        }
    }
}
