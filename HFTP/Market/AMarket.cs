using HFTP.Security;
using System;
using System.Collections;
using System.Collections.Generic;

namespace HFTP.Market
{
    public enum MarketVendor
    { 
        NONE,
        Exchange,   //交易所接口
        Wind        //Wind接口
    }

    public abstract class AMarket
    {
        #region 静态
        private static Hashtable _htmarketengines = new Hashtable();
        public static AMarket GetInstance(MarketVendor vendor)
        {
            try
            {
                if (_htmarketengines.Contains(vendor))
                    return (AMarket)_htmarketengines[vendor];

                AMarket marketengine = null;
                switch (vendor)
                {
                    case MarketVendor.Exchange:
                        marketengine = new MarketSHE();
                        break;
                    case MarketVendor.Wind:
                        marketengine = new MarketWind();
                        break;
                    default:
                        break;
                }

                if (marketengine == null)
                {
                    MessageManager.GetInstance().Add(MessageType.Error, string.Format("未找到行情接口：{0}", vendor.ToString()));
                    return null;
                }

                _htmarketengines.Add(vendor, marketengine);
                return marketengine;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        #endregion

        public MarketVendor Vendor;
        public string name;
        public virtual void Dispose() { }

        /// <summary>
        /// 同步调用时，将reqID存放于该表中供调度
        /// </summary>
        protected Hashtable htSyncWaiting = new Hashtable();
        /// <summary>
        /// 异步调用时，将reqID及订阅对象放于该表中供调度
        /// </summary>
        protected Hashtable htSubscribe = new Hashtable();

        #region 行情接口
        public abstract string GetTradeTime();
        public abstract void SubscribeBidAskBook(ASecurity s, int level);
        #endregion

        #region 期权集合
        public Hashtable htOptionSets = new Hashtable();
        public Hashtable htUnderlyingSets = new Hashtable();
        public abstract List<Option> GetOptionSet(ASecurity underlying);
        public abstract List<Option> GetOptionSet(List<ASecurity> underlyings);
        public Option GetOption(string code, ASecurity underlying)
        {
            Option o = null;
            if (!htUnderlyingSets.Contains(underlying.code))
                this.GetOptionSet(underlying);

            if (htOptionSets.Contains(underlying.code))
            {
                List<Option> oplist = (List<Option>)htOptionSets[underlying.code];
                o = oplist.Find(delegate(Option of) { return of.code == code; });
            }

            return o;
        }
        #endregion
    }
}
