using HFTP.Security;
using System;
using System.Diagnostics;
namespace HFTP.Statistic
{
    public class OptionMakeMarketStat
    {
        private const double C_SPREAD_PCT_1 = 0.1;   //近月
        private const double C_SPREAD_PCT_2 = 0.2;   //远月
        private const double C_SPREAD_LOWPX = 0.025;       //低价
        private const double C_INFINITE = 9999;

        #region 做市指标
        public double lastcenterprice = 0;
        private double validmaxspreadpct = C_SPREAD_PCT_1;  //做市商考核标准允许的最大价差比率
        private double targetpct = 0.7;                     //策略中达到做市商标准的程度：0.7=七成
        public double GetTargetSpreadPct()
        {
            return validmaxspreadpct * targetpct;
        }
        public double GetTargetLowPxSpread()
        {
            return C_SPREAD_LOWPX * targetpct;
        }
        #endregion

        #region 做市校验
        private double currmaxbidprice = 0;
        private double currminaskprice = C_INFINITE;
        private double currminspread = 0;
        private double currminspreadpct = 0;
        private string message = "";
        #endregion

        private Option option = null;
        public OptionMakeMarketStat(Option o)
        {
            option = o;
            this.targetpct = (double)Config.GetInstance().GetParameter(Config.C_PARA_MM_MAX_SPREAD_TARGET);

            if (option.daystoexercise < 62)
            {
                //近月合约
                validmaxspreadpct = C_SPREAD_PCT_1;
            }
            else
            {
                //季月合约
                validmaxspreadpct = C_SPREAD_PCT_2;
            }
        }
        public bool IsValid()
        {
            if (option == null )
            {
                message = "期权未定义";
                return false;
            }

            if (!option.istrading || option.islimitopening)
            {
                message = "豁免做市义务：期权停牌或限制交易";
                return true;
            }

            if (option.bidaskbook.bid[0] >= option.priceuplimit)
            {
                message = "豁免做市义务：期权涨停";
                return true;
            }

            if (option.bidaskbook.ask[0] <= option.pricedownlimit && option.bidaskbook.ask[0] > 0)
            {
                message = "豁免做市义务：期权跌停";
                return true;
            }

            if (option.entrustbook == null || option.entrustbook.Count == 0)
            {
                message = "无委托";
                return false;
            }

            //计算最小价差
            foreach (EntrustBook eb in option.entrustbook)
            {
                if (eb.price <= 0)
                    continue;

                if (eb.tradedirection == Entrust.TradeDirection.BUY)
                {
                    currmaxbidprice = Math.Max(eb.price, currmaxbidprice);
                }
                else if (eb.tradedirection == Entrust.TradeDirection.SELL)
                {
                    currminaskprice = Math.Min(eb.price, currminaskprice);
                }
            }

            if (currminaskprice * currmaxbidprice == 0 || currminaskprice == C_INFINITE)
            {
                message = "仅有单向委托";
                return false;
            }

            currminspread = currminaskprice - currmaxbidprice;
            currminspreadpct = currminspread / currmaxbidprice;

            ///======================================
            ///做市评价标准
            ///     近月合约：10%
            ///     季月合约：20%
            ///     小额合约：买价小于0.05，价差小于0.025
            ///豁免情况
            ///     涨跌停/停牌
            ///======================================
            if (currmaxbidprice < 0.05)
            {
                #region 低价合约
                if (currminspread >= 0.025)
                {
                    message = string.Format("低价合约：价差>=0.025,{0},{1}", option.name, currminspread.ToString("N4"));
                    return false;
                }
                #endregion
            }
            else
            {
                if (currminspreadpct > validmaxspreadpct)
                {
                    message = string.Format("合约：价差>={2},{0},{1}", option.name, currminspreadpct.ToString("P2"), validmaxspreadpct.ToString("P2"));
                    return false;
                }
            }

            message = "符合做市标准";
            return true;
        }

        public void DebugPrint()
        {
            Debug.Print(string.Format("{0},{1}", option.code, message));
        }
    }
}
