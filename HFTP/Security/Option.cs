using HFTP.Entrust;
using HFTP.Market;
using HFTP.Statistic;
using System;
using System.Diagnostics;

namespace HFTP.Security
{
    public enum OptionType
    { 
        CALL,
        PUT
    }

    public class Option:ASecurity
    {
        #region 期权合约属性
        public string contractcode = "";    //期权合约代码
        public OptionType type;             //类型：call put
        public ASecurity underlying;        //标的
        public double strike = 0;           //行权价
        public DateTime exercisedate = Utility.C_NULL_DATE;   //行权日
        public int exercisemonth = 0;
        public int daystoexercise = 0;      //剩余天数
        public double yearstoexercise = 0;  //剩余年数

        public int underlyingunits = 0;     //合约单位: ETF=10000,STOCK=1000
        public double ticksize = 0;         //最小报价单位：ETF=0.0001,STOCK=0.001
        public double priceuplimit = 0;     //涨停价
        public double pricedownlimit = 0;   //跌停价
        public double marginunit = 0;       //单位合约保证金, 1单位合约=10000份ETF
        public double lmtordmaxfloor = 0;   //单笔限价单数量上限
        public bool istrading = true;       //是否交易, false=停牌
        public bool islimitopening = false; //是否限制开仓, true=限制
        public bool isnew = false;          //是否新挂合约, true=新合约
        #endregion

        #region 期权做市商统计
        public OptionMakeMarketStat makemarketstat;
        #endregion

        public Option(string code, Exchange exchange) : base(code, exchange, SecurityCategory.OPTION) 
        {
            this.makemarketstat = new OptionMakeMarketStat(this);
        }
        
        public override void DebugPrint()
        {
            //base.DebugPrint();
            Debug.Print(string.Format("code={0},type={1},K={2},T={3}, underlying={4},type={5}", this.contractcode, this.type, this.strike, this.daystoexercise.ToString("N0"), this.underlying.name, this.underlying.category));
        }
    }
}
