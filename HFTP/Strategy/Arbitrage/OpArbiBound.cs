using HFTP.Security;
using System;
using System.Diagnostics;

namespace HFTP.Strategy.Arbitrage
{
    public class OpArbiBound:AOptionArbitrage
    {
        public OpArbiBound(Option o)
        {
            this.name = "单一期权边界套利";
            this.desc = "";
            this._optionlist.Add(o);
            this.isvalid = this.validate();
        }

        protected override bool validate()
        {
            //基础校验
            return base.validate();
        }

        protected override void tradeCallType()
        {
            //看涨期权理论价格：S-Kexp(-rT)<C<S
            Option o = this._optionlist[0];

            //double cost = 0, ret = 0, annualyield = 0;
            //  当C>S,卖C买S
            if (o.bidaskbook.bid[0] - o.underlying.bidaskbook.ask[0]>0)
            {
                ////成本: 卖出期权保证金+买入现货金额+期权交易费+现货交易费-权利金
                //cost = o.contractinfo.marginunit + o.underlying.bidaskbook.ask[0]
                //    + o.tradecost.brokageperunit + o.underlying.tradecost.brokagerate * o.underlying.bidaskbook.ask[0]
                //    - o.bidaskbook.bid[0];

                ////收入: K+(C-S)exp(rT) or  St+(C-S)exp(rT)
                //ret = o.strike + (o.bidaskbook.bid[0] - o.underlying.bidaskbook.ask[0]);

                ////年化收益率
                //annualyield = ret / cost / 365 * o.daystoexercise;

                //if (annualyield > this.requiredrate)
                //{
                //    Debug.Print(string.Format("{0}: Short C @{1}, Long S @{2}", o.name, o.bidaskbook.bid[0], o.underlying.bidaskbook.bid[0]));
                //    //o.Entrust();
                //}
            }

            //  当C<S-Kexp(-rT),买C卖S
            if (o.bidaskbook.ask[0] - o.underlying.bidaskbook.bid[0] + o.strike < 0)
            {
                //Debug.Print(string.Format("{0}: Long C @{1}, Short S @{2}", o.name, o.bidaskbook.ask[0], o.underlying.bidaskbook.bid[0]));
                return;
            }
        }

        protected override void tradePutType()
        {
            //看跌期权理论价格：Kexp(-rT)-S<P<K； 

        }
    }
}
