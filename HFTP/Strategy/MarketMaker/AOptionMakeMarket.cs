using HFTP.Entrust;
using HFTP.Market;
using HFTP.Security;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
namespace HFTP.Strategy.MarketMaker
{
    public abstract class AOptionMakeMarket
    {
        private static int instanceid = 0;

        public string name;
        public string desc;

        protected bool IsStop = false;
        protected bool IsPause = false;

        protected int c_min_entrust_volume = 5;
        protected QueryPara _queryparam;
        protected AMarket _optionengine;
        protected AEntrust _entrustengine;
        public AOptionMakeMarket()
        {
            instanceid++;

            //行情引擎
            MarketVendor mvendor = (MarketVendor)Config.GetInstance().GetParameter(Config.C_MARKETENGINE_OPTION);
            _optionengine = AMarket.GetInstance(mvendor);
            //委托引擎
            EntrustVendor evendor = (EntrustVendor)Config.GetInstance().GetParameter(Config.C_ENTRUSTENGINE_OPTION);
            _entrustengine = AEntrust.GetInstance(evendor);
            _entrustengine.Logon();

            //参数
            c_min_entrust_volume = (int)Config.GetInstance().GetParameter(Config.C_PARA_MM_MIN_ENTRUST_VOLUME);

            _queryparam = new QueryPara();
            _queryparam.fundcode = Config.GetInstance().GetParameter(Config.C_PARA_MM_FUNDCODE).ToString();
            _queryparam.portfolio = Config.GetInstance().GetParameter(Config.C_PARA_MM_PORTFOLIO).ToString();
        }

        #region 添加/免除合约
        protected List<Option> _options4mm = new List<Option>();
        public virtual void AddOptions(List<ASecurity> underlyinglist)
        {
            try
            {
                if (underlyinglist == null || underlyinglist.Count == 0)
                    return;

                foreach (ASecurity s in underlyinglist)
                {
                    this.AddOptions(s);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public virtual void AddOptions(ASecurity underlying)
        {
            try
            {
                if (underlying.category == SecurityCategory.OPTION)
                {
                    //添加期权合约本身
                    this.AddOptions((Option)underlying);
                }
                else
                { 
                    //添加标的对应的期权合约
                    List<Option> optionlist = _optionengine.GetOptionSet(underlying);
                    this.AddOptions(optionlist);
                }
            }
            catch (Exception ex)
            {                
                throw ex;
            }
        }
        public virtual void AddOptions(List<Option> optionlist)
        {
            if (optionlist == null || optionlist.Count == 0)
                return;

            foreach (Option o in optionlist)
            {
                this.AddOptions(o);
            }
        }
        public virtual void AddOptions(Option o)
        {
            if (o == null)
                return;

            if (!this._options4mm.Contains(o))
                this._options4mm.Add(o);
        }
        public virtual void ExemptOptions(Option exempt)
        {
            if (exempt == null)
                return;

            for (int i = 0; i < _options4mm.Count; i++)
            {
                if (_options4mm[i].code == exempt.code)
                    _options4mm.RemoveAt(i--);
            }
        }
        #endregion

        #region 做市策略
        public virtual void Pause() { this.IsPause = true; }
        public virtual void Resume() { this.IsPause = false; }
        public virtual void Stop() { this.IsStop = true; }
        public virtual void Run()
        {
            //筛选期权合约
            if (_options4mm == null || _options4mm.Count == 0)
            {
                MessageManager.GetInstance().Add(MessageType.Information, string.Format("没有设置需要做市的期权合约"));
                return;
            }

            //做市开始
            MessageManager.GetInstance().Add(MessageType.Information, string.Format("进程{2}-{0}:标的{1}", this.name, _options4mm[0].underlying.name, instanceid));

            //订阅行情
            foreach (Option o in _options4mm)
            {
                _optionengine.SubscribeBidAskBook(o, 5);
            }
            MessageManager.GetInstance().Add(MessageType.Information, string.Format("已订阅{0}个期权合约", _options4mm.Count));

            //暂停1s
            Thread.Sleep(1000);
            MessageManager.GetInstance().Add(MessageType.Information, string.Format("开始做市"));
            
            //开市做市
            while (!IsStop)
            {
                //常发射心跳，避免丢失连接
                _entrustengine.SendHeartbeat();

                #region 做市
                foreach (Option o in _options4mm)
                {
                    switch (o.bidaskbook.phase)
                    {
                        case TradingPhase.CALL:     //开盘集合竞价
                        case TradingPhase.UCALL:    //收盘集合竞价
                        case TradingPhase.VOLBREAK: //熔断集合竞价
                            //集合竞价策略
                            this.makemarket4call(o);
                            break;
                        case TradingPhase.TRADE:    //连续竞价
                            //连续竞价策略
                            this.makemarket4trade(o);
                            break;
                        case TradingPhase.END:      //闭市
                            //退出前做一次委托
                            this.makemarket4trade(o);
                            IsStop = true;
                            MessageManager.GetInstance().Add(MessageType.Information, "已闭市");
                            break;
                        case TradingPhase.PAUSE:    //临时停牌
                            //忽略该合约
                            continue;
                        case TradingPhase.START:    //开市前
                        case TradingPhase.BREAK:    //午休
                            //休息5秒
                            Thread.Sleep(5000);
                            break;
                        default:
                            break;
                    }

                    if (IsPause || IsStop)
                        break;
                }
                #endregion

                #region 暂停
                if (IsPause && !IsStop)
                {
                    MessageManager.GetInstance().Add(MessageType.Information, string.Format("暂停"));

                    int cnt = 0;
                    while (IsPause && !IsStop)
                    {
                        Thread.Sleep(500);

                        //每30秒发射一次心跳，避免丢失连接
                        cnt++;
                        if (cnt >= 30)
                        {
                            cnt = 0;
                            _entrustengine.SendHeartbeat();
                        }
                    }
                }
                #endregion
            }
            MessageManager.GetInstance().Add(MessageType.Information, string.Format("做市已停止"));
        }
        protected abstract void makemarket4call(Option o);  //集合竞价策略
        protected abstract void makemarket4trade(Option o); //连续竞价策略
        protected virtual double getcenterprice(Option o)
        {
            //最后交易价（不包括集合竞价的虚拟成交价和昨收盘）
            if (o.bidaskbook.lasttrade > 0)
                return o.bidaskbook.lasttrade;

            //最后交易价（包括集合竞价的虚拟成交价）
            if (o.bidaskbook.last > 0)
                return o.bidaskbook.last;

            //开盘价
            if (o.bidaskbook.open > 0)
                return o.bidaskbook.open;

            //理论价
            return getBSvalue(o);
        }
        protected virtual bool checkmmstatus(Option o, double skewratio, ref double askpx, ref double bidpx)
        {
            ///skewratio
            ///      =0.5:   上下偏离一致
            ///      >0.5:   Ask偏离中心价更多
            ///     〈0.5:   Bid偏离中心价更多
            try
            {
                double centerpx = this.getcenterprice(o);
                askpx = centerpx * (1 + o.makemarketstat.GetTargetSpreadPct() * skewratio);
                bidpx = centerpx * (1 - o.makemarketstat.GetTargetSpreadPct() * (1 - skewratio));

                //对于小于0.05的价格做调整
                if (bidpx < 0.05)
                {
                    askpx = centerpx + o.makemarketstat.GetTargetLowPxSpread() * skewratio;
                    bidpx = centerpx - o.makemarketstat.GetTargetLowPxSpread() * (1 - skewratio);
                    if (bidpx <= 0)
                        bidpx = o.ticksize;
                }

                int p = 4;
                if (o.ticksize > 0)
                    p = -(int)Math.Log10(o.ticksize);
                askpx = Math.Round(askpx, p);
                bidpx = Math.Round(bidpx, p);

                //买卖价不能相等
                if (bidpx == askpx)
                {
                    MessageManager.GetInstance().Add(MessageType.Warning, string.Format("暂停做市-买卖价相等:{0},价={1}", o.code, bidpx));
                    return false;
                }

                //极小值判断
                if (bidpx <= 0)
                {
                    MessageManager.GetInstance().Add(MessageType.Warning, string.Format("暂停做市-买价低于tick价:{0},bid={1}, tick价={2}", o.code, bidpx, o.ticksize));
                    return false;
                }

                //涨跌停判断
                if (bidpx < o.pricedownlimit)
                {
                    MessageManager.GetInstance().Add(MessageType.Warning, string.Format("暂停做市-买价低于跌停价:{0},bid={1}, 跌停价={2}", o.code, bidpx, o.pricedownlimit));
                    return false;
                }
                else if (askpx > o.priceuplimit)
                {
                    MessageManager.GetInstance().Add(MessageType.Warning, string.Format("暂停做市-卖价高于涨停价:{0},ask={1}, 涨停价={2}", o.code, bidpx, o.priceuplimit));
                    return false;
                }
                
                //是否需要更改委托
                if (o.makemarketstat.lastcenterprice != centerpx)
                    return true;
                else
                {
                    if(centerpx == 0)
                        MessageManager.GetInstance().Add(MessageType.Error, string.Format("期权无中心价：{0},{1}", o.code, o.name));

                    return false;
                }
            }
            catch (Exception ex)
            {                
                throw ex;
            }
        }
        protected virtual void sendorder(Option o, double askpx, double bidpx, QueryPara param)
        {
            if (askpx == bidpx)
            {
                MessageManager.GetInstance().Add(MessageType.Warning, string.Format("买价不能和卖价一样：{0},{1}", o.code, bidpx));
                return;
            }

            //查询委托和持仓信息
            _entrustengine.OptionEntrustQuery(param, o.entrustbook);
            _entrustengine.OptionPositionQuery(param, o.positionbook);

            //撤销现有委托
            List<int> entrustnolist = new List<int>();
            if (o.entrustbook.Count > 0)
            {
                entrustnolist.Clear();
                foreach (EntrustBook eb in o.entrustbook)
                    entrustnolist.Add(eb.entrustno);

                _entrustengine.OptionWithdraw(entrustnolist);
            }

            //查询持仓情况
            int longposition = 0, shortposition = 0;
            if (o.positionbook.Count > 0)
            {
                foreach (PositionBook pb in o.positionbook)
                {
                    if (pb.positiondirection == PostionDerection.LONG)
                        longposition = pb.volume;
                    else if (pb.positiondirection == PostionDerection.SHORT)
                        shortposition = pb.volume;
                }
            }

            //委托参数
            List<EntrustPara> paramlist = new List<EntrustPara>();
            //卖出
            EntrustPara paraAsk = new EntrustPara();
            paraAsk.portfolio = param.portfolio;
            paraAsk.securitycode = param.securitycode;
            paraAsk.exchange = o.exchange;
            paraAsk.volume = c_min_entrust_volume;
            paraAsk.price = askpx;                          //Ask
            paraAsk.entrustdirection = TradeDirection.SELL; //卖出
            if (longposition > paraAsk.volume)              //开/平
                paraAsk.futuredirection = FutureDirection.COVER;
            else
                paraAsk.futuredirection = FutureDirection.OPEN;

            //买入
            EntrustPara paraBid = new EntrustPara();
            paraBid.portfolio = param.portfolio;
            paraBid.securitycode = param.securitycode;
            paraBid.exchange = o.exchange;
            paraBid.volume = c_min_entrust_volume;
            paraBid.price = bidpx;                          //Bid
            paraBid.entrustdirection = TradeDirection.BUY;  //买入
            if (shortposition > paraBid.volume)             //开平
                paraBid.futuredirection = FutureDirection.COVER;
            else
                paraBid.futuredirection = FutureDirection.OPEN; 

            //篮子委托
            paramlist.Add(paraAsk);
            paramlist.Add(paraBid);
            _entrustengine.OptionBasketEntrust(paramlist);
        }
        #endregion

        #region 期权理论价值
        protected virtual double getBSvalue(Option o)
        {
            MessageManager.GetInstance().Add(MessageType.Warning, string.Format("BS模型尚未构建：{0},{1}", o.code, o.name));
            return 0;   
        }
        #endregion
    }
}
