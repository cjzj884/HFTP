using HFTP.Security;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using WindQuantLibrary;

namespace HFTP.Market
{
    public class MarketWind : AMarket
    {
        private WindQuantAPI _windapi = new WindQuantAPI();
        public MarketWind()
        {
            this.name = "Wind行情接口";
            this.Vendor = MarketVendor.Wind;
            _windapi.Authorize("", "", true);
            MessageManager.GetInstance().Add(MessageType.Warning, "对于x64的机器需要更换引用的WindQuantLibrary.dll");
        }

        public override void Dispose()
        {
            _windapi.CancelAllRequest();
            _windapi.AuthQuit();
            _windapi.Dispose();
        }

        #region 回报工具
        private bool isCallBackDataValid(QuantEvent quantEvent)
        {
            //检查错误消息
            if (quantEvent.ErrCode < 0)
            {
                string msg = this._windapi.WErr(quantEvent.ErrCode, eLang.eCHN);
                MessageManager.GetInstance().Add(MessageType.Error, msg);
                return false;
            }

            //解释字段
            if (quantEvent.quantData == null)
            {
                MessageManager.GetInstance().Add(MessageType.Error, "QuantEvent没有数据");
                return false;
            }

            return true;
        }
        private void waitCallBackData(long reqid)
        {
            //加入等待列表
            this.htSyncWaiting.Add(reqid, null);

            //等待callback执行
            while (this.htSyncWaiting[reqid] == null)
            {
                Thread.Sleep(100);
            }

            //删除等待队列
            this.htSyncWaiting.Remove(reqid);
        }
        #endregion

        #region 行情接口
        public override string GetTradeTime()
        {
            throw new NotImplementedException();
        }
        public override void SubscribeBidAskBook(ASecurity s, int level)
        {
            try
            {
                string fields = "rt_time,rt_pre_settle,rt_pre_close,rt_open,rt_high,rt_low,rt_last";
                for (int i = 1; i <= Math.Min(level, 5); i++)
                {
                    fields += ",rt_bid?,rt_ask?,rt_bsize?,rt_asize?".Replace("?", i.ToString());
                }

                //注意：snaponly参数，false=持续更新
                long reqid = this._windapi.WSQ(s.GetWindCode(), fields, false, this.callBackOnSubscribeBidAskBook);
                if (reqid < 0)
                {
                    string msg = this._windapi.WErr((eWQErr)reqid, eLang.eCHN);
                    MessageManager.GetInstance().Add(MessageType.Error, msg);
                    throw new Exception(msg);
                }

                //异步执行时，将s对象临时保存于htAsyncObject
                this.htSubscribe.Add(reqid, s.bidaskbook);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private int callBackOnSubscribeBidAskBook(QuantEvent quantEvent)
        {
            //////////////////////////////////
            //异步订阅：htSubscribeObject
            //////////////////////////////////
            if (!this.htSubscribe.Contains(quantEvent.RequestID))
            {
                MessageManager.GetInstance().Add(MessageType.Error, string.Format("没有找到订阅对象:id={0}",quantEvent.RequestID));
                return -1;
            }

            if (!isCallBackDataValid(quantEvent))
                return -1;

            BidAskBook babook = (BidAskBook)this.htSubscribe[quantEvent.RequestID];
            double[] data;
            data = (double[])quantEvent.quantData.MatrixData;

            #region 更新BidAskBook
            for (int i = 0; i < data.Length; i++)
            {
                string field = quantEvent.quantData.ArrWindFields[i].ToLower();
                switch (field)
                {
                    case "rt_time":
                        babook.tradetime = data[i].ToString().Substring(0, 6);
                        break;
                    case "rt_pre_settle":
                        babook.presettle = data[i];
                        break;
                    case "rt_pre_close":
                        babook.preclose = data[i];
                        break;
                    case "rt_open":
                        babook.open = data[i];
                        break;
                    case "rt_high":
                        babook.high = data[i];
                        break;
                    case "rt_low":
                        babook.low = data[i];
                        break;
                    case "rt_last":
                        babook.last = data[i];
                        babook.lasttrade = babook.last;
                        break;
                    default:
                        int idx = -1;
                        if (field.IndexOf("rt_ask")==0)
                        {
                            idx = Convert.ToInt16(field.Substring(6));
                            babook.ask[idx - 1] = data[i];
                        }
                        else if (field.IndexOf("rt_bid") == 0)
                        {
                            idx = Convert.ToInt16(field.Substring(6));
                            babook.bid[idx - 1] = data[i];
                        }
                        else if (field.IndexOf("rt_asize") == 0)
                        {
                            idx = Convert.ToInt16(field.Substring(8));
                            babook.asksize[idx - 1] = (int)data[i];
                        }
                        else if (field.IndexOf("rt_bsize") == 0)
                        {
                            idx = Convert.ToInt16(field.Substring(8));
                            babook.bidsize[idx - 1] = (int)data[i];
                        }
                        break;
                }
            }
            #endregion

            babook.DebugPrint();
            return (int)quantEvent.ErrCode;
        }        
        #endregion

        #region 期权集合
        public override List<Option> GetOptionSet(List<ASecurity> underlyings)
        {
            try
            {
                List<Option> optionlist = null;
                if (underlyings.Count > 0)
                {
                    optionlist = new List<Option>();
                    foreach (ASecurity s in underlyings)
                    {
                        List<Option> sublist = this.GetOptionSet(s);
                        if (sublist != null && sublist.Count > 0)
                            optionlist.AddRange(sublist);
                    }
                }

                return optionlist;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public override List<Option> GetOptionSet(ASecurity underlying)
        {
            try
            {
                long reqid = this._windapi.WSET("OptionChain", "date=" + DateTime.Today.ToString("yyyyMMdd") + ";us_code=" + underlying.GetWindCode() + ";option_var=;month=全部;call_put=全部", this.callBackOnGetOptionSet);
                if (reqid < 0)
                {
                    string msg = this._windapi.WErr((eWQErr)reqid, eLang.eCHN);
                    MessageManager.GetInstance().Add(MessageType.Error, msg);
                    throw new Exception(msg);
                }

                //等待回报数据
                this.waitCallBackData(reqid);
                
                //读取数据
                if (this.htOptionSets.Contains(underlying.code))
                    return (List<Option>)this.htOptionSets[underlying.code];
                else
                    return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private int callBackOnGetOptionSet(QuantEvent quantEvent)
        {
            //////////////////////////////////
            //同步调用：htSyncWaiting
            //////////////////////////////////

            //检查等待列表，首次未找到则等待一会儿
            if (!this.htSyncWaiting.Contains(quantEvent.RequestID))
                Thread.Sleep(100);
            if (!this.htSyncWaiting.Contains(quantEvent.RequestID))
                return -1;

            if (!isCallBackDataValid(quantEvent))
            {
                //解除主进程等待
                this.htSyncWaiting[quantEvent.RequestID] = quantEvent.ErrCode;

                //退出
                return -1;
            }

            object[] data;
            data = (object[])quantEvent.quantData.MatrixData;

            List<Option> optionlist = new List<Option>();
            for (int i = 0; i < data.Length / 13; i++)
            {
                string underlyingwindcode = data[0 + i * 13].ToString();
                string underlyingname = data[1 + i * 13].ToString();
                string optionwindcode = data[3 + i * 13].ToString();
                //转换Wind代码和交易代码
                string optioncode = optionwindcode; 
                Exchange exchange = ASecurity.GetExchange(optionwindcode, ref optioncode);
                string underlyingcode = underlyingwindcode;
                ASecurity.GetExchange(underlyingwindcode, ref underlyingcode);

                //读取期权信息
                Option o = new Option(optioncode, exchange);
                #region 期权信息
                o.name = data[4 + i * 13].ToString();
                o.strike = Convert.ToDouble(data[6 + i * 13]);
                o.exercisedate = Utility.ConvertToDateTime(data[10 + i * 13].ToString(), "");
                o.daystoexercise = (o.exercisedate - DateTime.Today).Days;
                o.yearstoexercise = o.daystoexercise / 365.0;
                string typename = data[8 + i * 13].ToString();
                if (typename == "认购")
                    o.type = OptionType.CALL;
                else
                    o.type = OptionType.PUT;
                #endregion

                #region 标的信息
                SecurityCategory category = ASecurity.GetSecurityCategory(underlyingcode, exchange);
                o.underlying = ASecurity.GetSecurity(category, underlyingcode, exchange);
                o.underlying.name = underlyingname;
                #endregion

                optionlist.Add(o);
            }

            if (!htOptionSets.Contains(optionlist[0].underlying.code))
                htOptionSets.Add(optionlist[0].underlying.code, optionlist);
            else
                htOptionSets[optionlist[0].underlying.code] = optionlist;

            //解除主进程等待
            this.htSyncWaiting[quantEvent.RequestID] = quantEvent.ErrCode;
            return (int)quantEvent.ErrCode;
        }
        #endregion
    }
}
