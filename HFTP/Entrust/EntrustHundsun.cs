using HFTP.Security;
using hundsun.mcapi;
using hundsun.t2sdk;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace HFTP.Entrust
{
    public unsafe class EntrustHundsun : AEntrust
    {
        private CT2Connection connMain;
        private CT2Connection connSub;
        private string token;

        public EntrustHundsun()
        {
            this.name = "恒生UFX委托接口";
            this.Vendor = EntrustVendor.Hundsun;
            MessageManager.GetInstance().Add(MessageType.Warning, "对于x64的机器需要更换引用的ext_t2sdkEx.dll");

            try
            {
                CT2Configinterface config = new CT2Configinterface();
                config.Load(Config.GetInstance().GetParameter(Config.C_PATH_HUNDSUN_CONFIG).ToString());

                #region 打开连接
                //主连接
                connMain = new CT2Connection(config);
                connMain.Create2BizMsg(new UFXMainCallback());
                int ret = connMain.Connect(5000);
                if (ret != 0)
                {
                    MessageManager.GetInstance().Add(MessageType.Error, string.Format("连接{0}失败：main-{1}", config.GetString("t2sdk", "servers", ""), connMain.GetErrorMsg(ret)));
                    return;
                }

                //子连接
                connSub = new CT2Connection(config);
                connSub.Create(null);
                ret = connSub.Connect(5000);
                if (ret != 0)
                {
                    MessageManager.GetInstance().Add(MessageType.Error, string.Format("连接{0}失败：sub-{1}", config.GetString("t2sdk", "servers", ""), connSub.GetErrorMsg(ret)));
                }
                #endregion
            }
            catch (Exception ex)
            {
                MessageManager.GetInstance().Add(MessageType.Error, ex.Message+"请检查license.dat文件是否存在。");
                throw ex;
            }
        }
        
        #region 结构和枚举
        public enum OptionFunction
        {
            //恒生现有的期权做市的功能号是给股指期权的
            Heartbeat=10000,          //心跳
            Logon = 10001,            //登录
            SingleEntrust = 91005,    //期权单个委托
            BasketEntrust = 91091,    //期权篮子委托
            Withdraw = 91106,         //期权委托撤单
            EntrustQuery = 32004,     //期权委托查询
            PositionQuery = 31004     //期权持仓查询
        }        
        #endregion

        #region 登陆
        private bool islogon()
        {
            if (this.token == null || this.token.Length == 0)
            {
                return false;
            }
            else
                return true;
        }
        public override string GetToken()
        {
            if (this.token != null && this.token.Length > 0)
                return this.token;
            else
                return "<NONE>";
        }
        public override void Logon()
        {
            string user = Config.GetInstance().GetParameter(Config.C_PARA_HUNDSUN_USER).ToString();
            string pass = Config.GetInstance().GetParameter(Config.C_PARA_HUNDSUN_PASSWORD).ToString();
            this.Logon(user, pass);
        }
        public override void Logon(string user, string pwd)
        {
            //无需重复登陆
            if (islogon())
                return;

            #region packer
            CT2Packer packer = new CT2Packer(2);
            packer.BeginPack();

            //字段名
            packer.AddField("operator_no", Convert.ToSByte('S'), 16, 4);
            packer.AddField("password", Convert.ToSByte('S'), 16, 4);
            packer.AddField("mac_address", Convert.ToSByte('S'), 32, 4);
            packer.AddField("ip_address", Convert.ToSByte('S'), 32, 4);
            packer.AddField("hd_volserial", Convert.ToSByte('S'), 10, 4);
            packer.AddField("op_station", Convert.ToSByte('S'), 255, 4);
            packer.AddField("authorization_id", Convert.ToSByte('S'), 64, 4);
            packer.AddField("login_time", Convert.ToSByte('S'), 6, 4);
            packer.AddField("verification_code", Convert.ToSByte('S'), 32, 4);

            //参数值
            packer.AddStr(user);
            packer.AddStr(pwd);
            packer.AddStr("mac");       //TODO: 使用真实数据替代
            packer.AddStr("ip");
            packer.AddStr("vol");
            packer.AddStr("op");
            packer.AddStr("");
            packer.AddStr("");
            packer.AddStr("");

            packer.EndPack();
            #endregion
            MessageManager.GetInstance().Add(MessageType.Warning, string.Format("Logon：mac,ip等信息为虚拟的"));

            int retcode = this.sendpacker(OptionFunction.Logon, packer, false);

            #region unpacker
            CT2UnPacker unpacker = getCallbackData(retcode);
            if (unpacker != null)
            {
                this.token = unpacker.GetStr("user_token");
                MessageManager.GetInstance().Add(MessageType.Information, string.Format("恒生系统登陆成功：{0}", this.token));
            }
            else
            {
                this.token = null;
                MessageManager.GetInstance().Add(MessageType.Error, string.Format("恒生系统登陆失败"));
            }
            #endregion
        }
        public override void SendHeartbeat()
        {
            //无需重复登陆
            if (!islogon())
                return;

            #region packer
            CT2Packer packer = new CT2Packer(2);
            packer.BeginPack();
            packer.AddField("operator_no", Convert.ToSByte('S'), 16, 4);
            packer.AddStr(this.token);
            packer.EndPack();
            #endregion
            int retcode = this.sendpacker(OptionFunction.Logon, packer, false);
        }
        #endregion
        

        #region 委托及查询
        private int sendpacker(OptionFunction function, CT2Packer packer, bool IsAsync = true)
        {
            try
            {                
                CT2BizMessage BizMessage = new CT2BizMessage();     //构造消息
                BizMessage.SetFunction((int)function);              //设置功能号
                BizMessage.SetPacketType(0);                        //设置消息类型为请求

                unsafe
                {
                    BizMessage.SetContent(packer.GetPackBuf(), packer.GetPackLen());
                }

                /************************************************************************/
                /* 此处使用异步发送 同步发送可以参考下面注释代码
                 * connection.SendBizMsg(BizMessage, 0);
                 * 1=异步，0=同步
                /************************************************************************/
                int iRet = this.connMain.SendBizMsg(BizMessage, (IsAsync) ? 1 : 0);
                if (iRet < 0)
                {
                    MessageManager.GetInstance().Add(MessageType.Error, string.Format("发送错误：{0}", connMain.GetErrorMsg(iRet)));
                }
                packer.Dispose();
                BizMessage.Dispose();

                return iRet;
            }
            catch (Exception ex)
            {                
                throw ex;
            }
        }
        private CT2UnPacker getCallbackData(int retcode)
        {
            //外部所指向的消息对象的内存由SDK内部管理，外部切勿释放
            CT2BizMessage lpMsg; 
            this.connMain.RecvBizMsg(retcode, out lpMsg, 5000, 1);

            int errcode = lpMsg.GetErrorNo();      //获取返回码
            int function = lpMsg.GetFunction();    //读取功能号
            if (errcode != 0)
            {
                MessageManager.GetInstance().Add(MessageType.Error, string.Format("操作失败：{0}", lpMsg.GetErrorInfo()));
                return null;
            }

            CT2UnPacker unpacker = null;
            unsafe
            {
                int iLen = 0;
                void* lpdata = lpMsg.GetContent(&iLen);
                unpacker = new CT2UnPacker(lpdata, (uint)iLen);
            }

            int code = unpacker.GetInt("ErrorCode");
            if (code != 0)
            {
                string msg = unpacker.GetStr("ErrorMsg");
                MessageManager.GetInstance().Add(MessageType.Error, string.Format("操作失败:{0}", msg));
                return null;
            }

            unpacker.SetCurrentDatasetByIndex(1);
            return unpacker;
        }        
        private string getMarketNo(Exchange exchange)
        {
            switch (exchange)
            {
                case Exchange.SHE:
                    return "1";
                case Exchange.SZE:
                    return "2";
                default:
                    return "0";
            }
        }
        public override void OptionSingleEntrust(EntrustPara param)
        {
            try
            {
                if (!islogon())
                    return;

                #region packer
                CT2Packer packer = new CT2Packer(2);
                packer.BeginPack();
                packer.AddField("user_token", Convert.ToSByte('S'), 512, 4);
                packer.AddField("combi_no", Convert.ToSByte('S'), 8, 4);
                packer.AddField("market_no", Convert.ToSByte('S'), 3, 4);
                packer.AddField("stock_code", Convert.ToSByte('S'), 16, 4);
                packer.AddField("entrust_direction", Convert.ToSByte('S'), 3, 4);
                packer.AddField("futures_direction", Convert.ToSByte('S'), 1, 4);
                packer.AddField("price_type", Convert.ToSByte('S'), 1, 4);
                packer.AddField("entrust_price", Convert.ToSByte('F'), 11, 4);
                packer.AddField("entrust_amount", Convert.ToSByte('F'), 16, 2);
                packer.AddField("covered_flag", Convert.ToSByte('S'), 1, 2);

                packer.AddStr(this.token);
                packer.AddStr(param.portfolio);
                packer.AddStr(this.getMarketNo(param.exchange));
                packer.AddStr(param.securitycode);
                packer.AddStr(((int)param.entrustdirection).ToString());
                packer.AddStr(((int)param.futuredirection).ToString());
                packer.AddStr("0");                   //0=限价
                packer.AddDouble(param.price);
                packer.AddDouble(param.volume);
                packer.AddStr("0");                   //covered_flag，备兑标志，0=非备兑

                packer.EndPack();
                #endregion

                int retcode = this.sendpacker(OptionFunction.SingleEntrust, packer);

                MessageManager.GetInstance().Add(MessageType.TradeNotice, string.Format("单笔委托:code={0}", param.securitycode));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public override void OptionBasketEntrust(List<EntrustPara> paramlist)
        {
            try
            {
                if (!islogon())
                    return;

                if (paramlist == null || paramlist.Count == 0)
                    return;

                string codelist = "|";
                #region packer
                CT2Packer packer = new CT2Packer(2);
                packer.BeginPack();
                packer.AddField("user_token", Convert.ToSByte('S'), 512, 4);
                packer.AddField("combi_no", Convert.ToSByte('S'), 8, 4);
                packer.AddField("market_no", Convert.ToSByte('S'), 3, 4);
                packer.AddField("stock_code", Convert.ToSByte('S'), 16, 4);
                packer.AddField("entrust_direction", Convert.ToSByte('S'), 3, 4);
                packer.AddField("futures_direction", Convert.ToSByte('S'), 1, 4);
                packer.AddField("price_type", Convert.ToSByte('S'), 1, 4);
                packer.AddField("entrust_price", Convert.ToSByte('F'), 11, 4);
                packer.AddField("entrust_amount", Convert.ToSByte('F'), 16, 2);
                packer.AddField("covered_flag", Convert.ToSByte('S'), 1, 2);

                foreach (EntrustPara param in paramlist)
                {
                    packer.AddStr(this.token);
                    packer.AddStr(param.portfolio);
                    packer.AddStr(this.getMarketNo(param.exchange));
                    packer.AddStr(param.securitycode);
                    packer.AddStr(((int)param.entrustdirection).ToString());
                    packer.AddStr(((int)param.futuredirection).ToString());
                    packer.AddStr("0");                   //0=限价
                    packer.AddDouble(param.price);
                    packer.AddDouble(param.volume);
                    packer.AddStr("0");                   //covered_flag，备兑标志，0=非备兑

                    codelist += param.securitycode + "|";
                }

                packer.EndPack();
                #endregion

                int retcode = this.sendpacker(OptionFunction.BasketEntrust, packer);

                MessageManager.GetInstance().Add(MessageType.TradeNotice, string.Format("篮子委托：共{0}笔,{1}", paramlist.Count, codelist));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public override void OptionEntrustQuery(QueryPara param, List<EntrustBook> entrustbook)
        {
            try
            {
                if (!islogon())
                    return;

                #region packer
                CT2Packer packer = new CT2Packer(2);
                packer.BeginPack();
                packer.AddField("user_token", Convert.ToSByte('S'), 512, 4);
                packer.AddField("account_code", Convert.ToSByte('S'), 32, 4);
                packer.AddField("combi_no", Convert.ToSByte('S'), 8, 4);
                packer.AddField("stock_code", Convert.ToSByte('S'), 16, 4);

                packer.AddStr(this.token);
                packer.AddStr(param.fundcode);
                packer.AddStr(param.portfolio);
                packer.AddStr(param.securitycode);

                packer.EndPack();
                #endregion

                int retcode = this.sendpacker(OptionFunction.EntrustQuery, packer, false);

                #region unpacker
                CT2UnPacker unpacker = getCallbackData(retcode);
                if (unpacker != null)
                {
                    //更新委托列表
                    entrustbook.Clear();
                    while (unpacker.IsEOF() == 0)
                    {
                        EntrustBook enbook = new EntrustBook();
                        enbook.code = unpacker.GetStr("stock_code");
                        enbook.batchno = unpacker.GetInt("batch_no");
                        enbook.entrustno = unpacker.GetInt("entrust_no");
                        enbook.price = unpacker.GetDouble("entrust_price");
                        enbook.message = unpacker.GetStr("withdraw_cause");

                        //委托
                        int entrustdirction = unpacker.GetInt("entrust_direction");
                        if (entrustdirction == 1)
                            enbook.tradedirection = TradeDirection.BUY;
                        else
                            enbook.tradedirection = TradeDirection.SELL;
                        
                        //开平
                        int futuredirection = unpacker.GetInt("futures_direction");
                        if(futuredirection == 1)
                            enbook.futuredirection = FutureDirection.OPEN;
                        else
                            enbook.futuredirection = FutureDirection.COVER;

                        //剩余数量
                        int entrustvol = unpacker.GetInt("entrust_amount");
                        int dealvol = unpacker.GetInt("deal_amount");
                        enbook.volume = entrustvol - dealvol;

                        //委托状态
                        string entruststate = unpacker.GetStr("entrust_state"); //委托状态
                        switch (entruststate)
                        {
                            case "1":    //未报
                            case "4":    //已报
                            case "6":    //部成
                                if (enbook.price > 0 && enbook.volume > 0)
                                    entrustbook.Add(enbook);
                                break;
                            case "5":    //废单                                    
                            case "7":    //已成
                            case "8":    //部撤                            
                            case "9":    //已撤
                            case "a":    //待撤
                            case "A":    //未撤
                            case "B":    //待撤
                            case "C":    //正撤
                            case "D":    //撤认
                            case "E":    //撤废
                            case "F":    //已撤
                                break;
                            default:
                                break;
                        }

                        unpacker.Next();
                    }
                }
                else
                {
                    MessageManager.GetInstance().Add(MessageType.Error, string.Format("委托查询失败：{0}", param.securitycode));
                }
                #endregion
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public override void OptionPositionQuery(QueryPara param, List<PositionBook> positionbook)
        {
            try
            {
                if (!islogon())
                    return;

                #region packer
                CT2Packer packer = new CT2Packer(2);
                packer.BeginPack();
                packer.AddField("user_token", Convert.ToSByte('S'), 512, 4);
                packer.AddField("account_code", Convert.ToSByte('S'), 32, 4);
                packer.AddField("combi_no", Convert.ToSByte('S'), 8, 4);
                packer.AddField("stock_code", Convert.ToSByte('S'), 16, 4);

                packer.AddStr(this.token);
                packer.AddStr(param.fundcode);
                packer.AddStr(param.portfolio);
                packer.AddStr(param.securitycode);

                packer.EndPack();
                #endregion

                int retcode = this.sendpacker(OptionFunction.PositionQuery, packer, false);

                #region unpacker
                CT2UnPacker unpacker = getCallbackData(retcode);
                if (unpacker != null)
                {
                    //更新持仓
                    positionbook.Clear();
                    while (unpacker.IsEOF()==0)
                    {
                        PositionBook pbook = new PositionBook();
                        pbook.code = unpacker.GetStr("stock_code");
                        int pflag = unpacker.GetInt("position_flag");
                        if (pflag == 1)
                            pbook.positiondirection = PostionDerection.LONG;
                        else
                            pbook.positiondirection = PostionDerection.SHORT;
                        pbook.volume = unpacker.GetInt("enable_amount");
                        
                        //可能存在volume<=0的bug，所以筛选出为正的
                        if (pbook.volume > 0)
                            positionbook.Add(pbook);

                        unpacker.Next();
                    }
                }
                else
                {
                    MessageManager.GetInstance().Add(MessageType.Error, string.Format("持仓查询失败：{0}", param.securitycode));
                }
                #endregion
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public override void OptionWithdraw(List<int> entrustnolist)
        {
            try
            {
                if (!islogon())
                    return;

                if (entrustnolist == null || entrustnolist.Count == 0)
                    return;

                #region packer
                CT2Packer packer = new CT2Packer(2);
                packer.BeginPack();
                packer.AddField("user_token", Convert.ToSByte('S'), 512, 4);
                packer.AddField("entrust_no", Convert.ToSByte('I'), 8, 4);

                string entrustlist = "";
                foreach (int entrustno in entrustnolist)
                {
                    packer.AddStr(this.token);
                    packer.AddInt(entrustno);
                    entrustlist += entrustno + "|";
                }

                packer.EndPack();
                #endregion

                int retcode = this.sendpacker(OptionFunction.Withdraw, packer);

                MessageManager.GetInstance().Add(MessageType.TradeNotice, string.Format("委托撤单：共{0}笔,|{1}", entrustnolist.Count, entrustlist));
            }
            catch (Exception ex)
            {                
                throw ex;
            }
        }
        #endregion

        #region 订阅交易所回报
        public void Subcribe(string user, string pwd)
        {
            //订阅报单至交易所后的反馈信息
            //该命令和logon一起发出

            //subcallback = new UFXSubCallback(this);
            //subcribe = this.connMain.NewSubscriber(subcallback, "ufx_demo", 50000, 2000, 100);
            //if (subcribe == null)
            //{
            //    Debug.Print(string.Format("订阅创建失败 {0}", connMain.GetMCLastError()));
            //    return;
            //}
            //CT2SubscribeParamInterface args = new CT2SubscribeParamInterface();
            //args.SetTopicName("ufx_topic");
            //args.SetReplace(false);
            //args.SetFilter("operator_no", user);

            //CT2Packer req = new CT2Packer(2);
            //req.BeginPack();
            //req.AddField("login_operator_no", Convert.ToSByte('S'), 16, 4);
            //req.AddField("password", Convert.ToSByte('S'), 16, 4);

            //req.AddStr(user);
            //req.AddStr(pwd);

            //req.EndPack();

            //CT2UnPacker unpacker = null;
            //int ret = subcribe.SubscribeTopicEx(args, 50000, out unpacker, req);
            //req.Dispose();
            //if (ret > 0)
            //{
            //    Debug.Print("订阅成功");
            //    subcribeid = ret;
            //}
            //else
            //{
            //    if (unpacker != null)
            //    {
            //        Debug.Print("订阅失败");
            //        this.ShowUnPacker(unpacker);
            //    }
            //}
        }
        #endregion
    }

    public unsafe class UFXMainCallback : CT2CallbackInterface
    {
        public static void ShowUnPacker(CT2UnPacker lpUnPack)
        {
            int count = lpUnPack.GetDatasetCount();
            for (int k = 0; k < count; k++)
            {
                Debug.Print(string.Format("第[{0}]个数据集", k));
                lpUnPack.SetCurrentDatasetByIndex(k);
                String strInfo = string.Format("记录行数：           {0}", lpUnPack.GetRowCount());
                Debug.Print(strInfo);
                strInfo = string.Format("列行数：			 {0}", lpUnPack.GetColCount());
                Debug.Print(strInfo);
                while (lpUnPack.IsEOF() == 0)
                {
                    for (int i = 0; i < lpUnPack.GetColCount(); i++)
                    {
                        String colName = lpUnPack.GetColName(i);
                        sbyte colType = lpUnPack.GetColType(i);
                        if (!colType.Equals('R'))
                        {
                            String colValue = lpUnPack.GetStrByIndex(i);
                            String str = string.Format("{0}:			[{1}]", colName, colValue);
                            Debug.Print(str);
                        }
                        else
                        {
                            int colLength = 0;
                            unsafe
                            {
                                void* colValue = (char*)lpUnPack.GetRawByIndex(i, &colLength);
                                string str = string.Format("{0}:			[{1}]({2})", colName, Marshal.PtrToStringAuto(new IntPtr(colValue)), colLength);
                            }
                        }
                    }
                    lpUnPack.Next();
                }
            }
        }

        public override void OnClose(CT2Connection lpConnection)
        { }

        public override void OnConnect(CT2Connection lpConnection)
        { }

        public override void OnReceivedBiz(CT2Connection lpConnection, int hSend, string lppStr, CT2UnPacker lppUnPacker, int nResult)
        { }

        public override void OnReceivedBizEx(CT2Connection lpConnection, int hSend, CT2RespondData lpRetData, string lppStr, CT2UnPacker lppUnPacker, int nResult)
        { }

        //数据
        public override void OnReceivedBizMsg(CT2Connection lpConnection, int hSend, CT2BizMessage lpMsg)
        {
            int errcode = lpMsg.GetErrorNo();      //获取错误代码
            int retcode = lpMsg.GetReturnCode();   //获取返回码
            int function = lpMsg.GetFunction();
            if (errcode != 0)
            {
                MessageManager.GetInstance().Add(MessageType.Error, string.Format("异步接收出错：", lpMsg.GetErrorInfo()));
                return;
            }

            string msg = "", entrustlist = "", batchno = "";
            uint cnt = 0;
            CT2UnPacker unpacker = null;
            unsafe
            {
                int iLen = 0;
                void* lpdata = lpMsg.GetContent(&iLen);
                unpacker = new CT2UnPacker(lpdata, (uint)iLen);
            }

            errcode = unpacker.GetInt("ErrorCode");
            if (errcode != 0)
            {
                msg = unpacker.GetStr("ErrorMsg");
                MessageManager.GetInstance().Add(MessageType.Error, string.Format("操作失败：{0}", msg));
                return;
            }

            //回报数据存于第1个数据集，第0个为错误消息
            unpacker.SetCurrentDatasetByIndex(1);
            cnt = unpacker.GetRowCount();
            while (unpacker.IsEOF() == 0)
            {
                entrustlist += unpacker.GetInt("entrust_no") + "|";
                batchno = unpacker.GetInt("batch_no").ToString();
                unpacker.Next();
            }

            //返回消息
            switch (function)
            {
                case (int)EntrustHundsun.OptionFunction.SingleEntrust:
                    MessageManager.GetInstance().Add(MessageType.TradeNotice, string.Format("单笔委托回报：成功，共1笔，|{0}", entrustlist));
                    break;
                case (int)EntrustHundsun.OptionFunction.BasketEntrust:
                    MessageManager.GetInstance().Add(MessageType.TradeNotice, string.Format("篮子委托回报：成功，批号={0}", batchno));
                    break;
                case (int)EntrustHundsun.OptionFunction.Withdraw:
                    MessageManager.GetInstance().Add(MessageType.TradeNotice, string.Format("委托撤单回报：成功，共{0}笔,|{1}", cnt, entrustlist));
                    break;
                default:
                    break;
            }
        }

        public override void OnRegister(CT2Connection lpConnection)
        { }

        public override void OnSafeConnect(CT2Connection lpConnection)
        { }

        public override void OnSent(CT2Connection lpConnection, int hSend, void* lpData, int nLength, int nQueuingData)
        { }
    }

    public unsafe class UFXSubCallback : CT2SubCallbackInterface
    {
        //回报
        public override void OnReceived(CT2SubscribeInterface lpSub, int subscribeIndex, void* lpData, int nLength, tagSubscribeRecvData lpRecvData)
        {
            try
            {
                Debug.Print("/*********************************收到主推数据 begin***************************/");
                string strInfo = string.Format("附加数据长度：       {0}", lpRecvData.iAppDataLen);
                Debug.Print(strInfo);
                if (lpRecvData.iAppDataLen > 0)
                {
                    unsafe
                    {
                        strInfo = string.Format("附加数据：           {0}", Marshal.PtrToStringAuto(new IntPtr(lpRecvData.lpAppData)));
                        Debug.Print(strInfo);
                    }
                }
                Debug.Print("过滤字段部分：\n");
                if (lpRecvData.iFilterDataLen > 0)
                {
                    CT2UnPacker lpUnpack = new CT2UnPacker(lpRecvData.lpFilterData, (uint)lpRecvData.iFilterDataLen);
                    //ufxengine.ShowUnPacker(lpUnpack);
                    lpUnpack.Dispose();
                }
                CT2UnPacker lpUnPack1 = new CT2UnPacker((void*)lpData, (uint)nLength);
                if (lpUnPack1 != null)
                {
                    //ufxengine.ShowUnPacker(lpUnPack1);
                    lpUnPack1.Dispose();
                }
                Debug.Print("/*********************************收到主推数据 end ***************************/");
            }
            catch (System.Exception ex)
            {                
                throw ex;
            }
        }

        public override void OnRecvTickMsg(CT2SubscribeInterface lpSub, int subscribeIndex, string TickMsgInfo)
        {}
    }
}
