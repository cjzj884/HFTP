using HFTP.Entrust;
using HFTP.Security;
using System;
using System.Diagnostics;

namespace HFTP.Strategy.MarketMaker
{
    public class OptionMakeMarket1: AOptionMakeMarket
    {
        public OptionMakeMarket1()
        {
            this.name = "跟随报价策略1";
        }

        protected override void makemarket4call(Option o)
        {
            try
            {
                //集合竞价做市策略
                //同连续竞价
                this.makemarket4trade(o);
            }
            catch (Exception ex)
            {                
                throw ex;
            }
        }

        protected override void makemarket4trade(Option o)
        {
            try
            {
                //连续竞价做市策略
                double askpx = 0, bidpx = 0;
                if (checkmmstatus(o, 0.5, ref askpx, ref bidpx))
                {
                    this._queryparam.securitycode = o.code;
                    this.sendorder(o, askpx, bidpx, this._queryparam);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
