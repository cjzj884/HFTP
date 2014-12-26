using HFTP.Security;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace HFTP.Market
{
    public class MarketSHE:AMarket
    {
        public MarketSHE()
        {
            this.name = "上交所行情接口";
            this.Vendor = MarketVendor.Exchange;
            loadConfig();
            MessageManager.GetInstance().Add(MessageType.Information, "已开启上交所行情");
        }

        public override void Dispose()
        {
            
        }

        #region 设置参数
        private string filepath_option_contract_target = "";
        private string filepath_option_contract_source = "";
        private string filepath_option_price_history = "";
        private bool flag_option_price_savehistory = false;
        private string filepath_option_price_target = "";
        private string filepath_option_price_source = "";
        private void loadConfig()
        {
            Config config = Config.GetInstance();
            string filename = "";
            #region 个股期权合约
            filename = config.GetParameter(Config.C_FILE_SHE_OPTION_CONTRACT).ToString().Trim();
            filepath_option_contract_source = parseFilePath(Config.C_PATH_SHE_OPTION_CONTRACT_SOURCE, false) + filename;
            filepath_option_contract_target = parseFilePath(Config.C_PATH_SHE_OPTION_CONTRACT_TARGET, true) + filename;            
            #endregion

            #region 个股期权行情
            filename = config.GetParameter(Config.C_FILE_SHE_OPTION_PRICE).ToString().Trim();
            filepath_option_price_source = parseFilePath(Config.C_PATH_SHE_OPTION_PRICE_SOURCE, false) + filename;
            filepath_option_price_target = parseFilePath(Config.C_PATH_SHE_OPTION_PRICE_TARGET, true) + filename;
            filepath_option_price_history = parseFilePath(Config.C_PATH_SHE_OPTION_PRICE_HISTORY, true);
            flag_option_price_savehistory = (bool)config.GetParameter(Config.C_FILE_SHE_OPTION_PRICE_SAVEHIST);
            #endregion
        }
        private string parseFilePath(string key, bool isCreatFolder)
        {
            Config config = Config.GetInstance();
            string path = config.GetParameter(key).ToString().Trim();
            //校验路径
            if (path.LastIndexOf(@"\") != path.Length - 1)
                path += @"\";

            //建文件夹
            if (isCreatFolder)
            {
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
            }

            return path;
        }
        #endregion

        #region 行情接口
        private long _requestId = 1;
        private delegate void AsyncDelegate(long reqId);
        public override string GetTradeTime()
        {
            try
            {
                string filecontent = this.getMarketFileContent();
                if (filecontent == null || filecontent.Trim().Length == 0)
                    return "<NONE>";

                string[] lines = filecontent.Split("\n".ToCharArray());
                string[] fieldshead = lines[0].Split("|".ToCharArray());
                string markettime = fieldshead[6].ToString().Trim();
                return markettime;
            }
            catch (Exception ex)
            {                
                throw ex;
            }
        }
        public override void SubscribeBidAskBook(ASecurity s, int level)
        {
            //异步执行前，保存记录
            long reqId = _requestId++;
            this.htSubscribe.Add(reqId, true);

            //异步调用
            AsyncDelegate dlg;
            switch (s.category)
            {
                case SecurityCategory.STOCK:
                case SecurityCategory.ETF:
                    this.htSubscribedStocks.Add(s.code, s.bidaskbook);
                    this.addSubscribedStocks(s.code);
                    if(!isLoopingDbfFile)
                    {
                        isLoopingDbfFile = true;
                        dlg = new AsyncDelegate(updateStockBidAskBook);
                        dlg.BeginInvoke(reqId, null, null);
                    }
                    break;
                case SecurityCategory.OPTION:
                    this.htSubscribedOptions.Add(s.code, s.bidaskbook);
                    if (!isLoopingMktFile)
                    {
                        isLoopingMktFile = true;
                        dlg = new AsyncDelegate(updateOptionBidAskBook);
                        dlg.BeginInvoke(reqId, null, null);
                    }
                    break;
                default:
                    string msg = string.Format("无法读取该类证券行情：{0}", s.category);
                    MessageManager.GetInstance().Add(MessageType.Error, msg);
                    throw new Exception(msg);
            }
        }

        #region 期权行情-基于mktdt03.txt
        private bool isLoopingMktFile = false;
        private Hashtable htSubscribedOptions = new Hashtable();
        private string getMarketFileContent()
        {
            File.Copy(filepath_option_price_source, filepath_option_price_target, true);
            StreamReader sReader = new StreamReader(filepath_option_price_target, Encoding.Default);
            string filecontent = sReader.ReadToEnd();
            sReader.Close();
            
            return filecontent;
        }
        private void updateOptionBidAskBook(long reqId)
        {
            ///========================================
            ///注意：
            ///     1）该方法只需要执行一次即无限循环，直到闭市自动退出，请勿多次运行
            ///     2）StreamReader读取文件时会锁住文件，建议先复制到本地再进行读取
            ///========================================
            bool isMarketOff = false;
            while (htSubscribe.Contains(reqId) && (bool)htSubscribe[reqId] && !isMarketOff)
            {
                try
                {
                    string filecontent = this.getMarketFileContent();
                    string markettime = "";

                    #region 解释行情
                    string[] lines = null;
                    if (filecontent == null || filecontent.Trim().Length == 0)
                    {
                        MessageManager.GetInstance().Add(MessageType.Error, "上交所期权行情文件出错");
                        break;
                    }
                    lines = filecontent.Split("\n".ToCharArray());

                    #region 文件头
                    string[] fieldshead = lines[0].Split("|".ToCharArray());
                    if (markettime != fieldshead[6].ToString().Trim())
                    {
                        markettime = fieldshead[6].ToString().Trim();

                        //历史备份
                        if (flag_option_price_savehistory)
                        {
                            string filename = markettime.Replace(":", "-").Replace(".", "-") + ".txt";
                            File.Copy(filepath_option_price_target, filepath_option_price_history + filename, true);
                        }
                    }
                    else
                    {
                        Thread.Sleep(100);
                        continue;
                    }
                    #endregion

                    #region 文件体
                    if (!isMarketOff)
                    {
                        string[] fields;
                        string line = "";
                        for (int i = 0; i < lines.Length; i++)
                        {              
                            try
                            {
                                line = lines[i].Trim();
                                if (line != null && line.Length > 0)
                                {
                                    fields = line.Split("|".ToCharArray());

                                    if (fields.Length >= 35)
                                    {                                
                                        #region 更新BidAskBook
                                        string optioncode = fields[1].Trim();
                                        if (htSubscribedOptions.Contains(optioncode))
                                        {
                                            BidAskBook babook = (BidAskBook)htSubscribedOptions[optioncode];
                                            string tradetime = fields[34].Trim();   //HH:MM:SS.000

                                            ///=======================
                                            ///无法用时间戳做数据变更的判断
                                            /// 存在这样的情形，数据变了时间戳没更新
                                            ///=======================
                                            babook.tradetime = tradetime;

                                            #region 交易状态
                                            char[] status = fields[33].Trim().ToUpper().ToCharArray();
                                            switch (status[0])
                                            {
                                                case 'S':
                                                    babook.phase = TradingPhase.START;
                                                    break;
                                                case 'C':
                                                    babook.phase = TradingPhase.CALL;
                                                    break;
                                                case 'T':
                                                    babook.phase = TradingPhase.TRADE;
                                                    break;
                                                case 'B':
                                                    babook.phase = TradingPhase.BREAK;
                                                    break;
                                                case 'E':
                                                    babook.phase = TradingPhase.END;
                                                    break;
                                                case 'V':
                                                    babook.phase = TradingPhase.VOLBREAK;
                                                    break;
                                                case 'P':
                                                    babook.phase = TradingPhase.PAUSE;
                                                    break;
                                                case 'U':
                                                    babook.phase = TradingPhase.UCALL;
                                                    break;
                                                default:
                                                    babook.phase = TradingPhase.NONE;
                                                    break;
                                            }
                                            #endregion

                                            #region 盘口价格
                                            babook.presettle = Utility.ConvertToDouble(fields[5].Trim(), 0);
                                            babook.open = Utility.ConvertToDouble(fields[6].Trim(), 0);
                                            babook.high = Utility.ConvertToDouble(fields[9].Trim(), 0);
                                            babook.low = Utility.ConvertToDouble(fields[10].Trim(), 0);
                                            babook.last = Utility.ConvertToDouble(fields[11].Trim(), 0);
                                            if (babook.phase == TradingPhase.TRADE)//仅在连续竞价阶段更新
                                                babook.lasttrade = babook.last;

                                            babook.bid[0] = Utility.ConvertToDouble(fields[12].Trim(), 0);
                                            babook.bid[1] = Utility.ConvertToDouble(fields[16].Trim(), 0);
                                            babook.bid[2] = Utility.ConvertToDouble(fields[20].Trim(), 0);
                                            babook.bid[3] = Utility.ConvertToDouble(fields[24].Trim(), 0);
                                            babook.bid[4] = Utility.ConvertToDouble(fields[28].Trim(), 0);

                                            babook.bidsize[0] = (int)Utility.ConvertToDouble(fields[13].Trim(), 0);
                                            babook.bidsize[1] = (int)Utility.ConvertToDouble(fields[17].Trim(), 0);
                                            babook.bidsize[2] = (int)Utility.ConvertToDouble(fields[21].Trim(), 0);
                                            babook.bidsize[3] = (int)Utility.ConvertToDouble(fields[25].Trim(), 0);
                                            babook.bidsize[4] = (int)Utility.ConvertToDouble(fields[29].Trim(), 0);

                                            babook.ask[0] = Utility.ConvertToDouble(fields[14].Trim(), 0);
                                            babook.ask[1] = Utility.ConvertToDouble(fields[18].Trim(), 0);
                                            babook.ask[2] = Utility.ConvertToDouble(fields[22].Trim(), 0);
                                            babook.ask[3] = Utility.ConvertToDouble(fields[26].Trim(), 0);
                                            babook.ask[4] = Utility.ConvertToDouble(fields[30].Trim(), 0);

                                            babook.asksize[0] = (int)Utility.ConvertToDouble(fields[15].Trim(), 0);
                                            babook.asksize[1] = (int)Utility.ConvertToDouble(fields[19].Trim(), 0);
                                            babook.asksize[2] = (int)Utility.ConvertToDouble(fields[23].Trim(), 0);
                                            babook.asksize[3] = (int)Utility.ConvertToDouble(fields[27].Trim(), 0);
                                            babook.asksize[4] = (int)Utility.ConvertToDouble(fields[31].Trim(), 0);
                                            #endregion
                                        }
                                        #endregion
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                throw ex;
                            }
                        }
                    }
                    #endregion

                    string marketstatus = fieldshead[8].Trim().ToUpper();
                    char[] tradestatus = marketstatus.ToCharArray();
                    switch (tradestatus[0])
                    {
                        //交易所已闭市
                        case 'E':
                            isMarketOff = true;
                            break;
                        default:
                            break;
                    }
                    #endregion
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
        #endregion

        #region 股票/ETF-基于show2003.dbf
        #region 数据库引擎
        //private OleDbConnection _dbfconn;
        //private OleDbConnection getdbfconn()
        //{
        //    try
        //    {
        //        //if (_dbfconn == null)
        //        //{
        //        //    string connStr = string.Format(@"Provider=Microsoft.Jet.OLEDB.4.0; Data Source={0};Extended Properties=dBASE IV;Persist Security Info=False", filepath_stock_price);
        //        //    _dbfconn = new OleDbConnection(connStr);
        //        //    _dbfconn.Open();
        //        //}

        //        return _dbfconn;
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageManager.GetInstance().Add(MessageType.Error, string.Format("读dbf出错：{0}", ex.Message));
        //        throw ex;
        //    }
        //}
        #endregion
        private bool isLoopingDbfFile = false;
        private Hashtable htSubscribedStocks = new Hashtable();
        private void updateStockBidAskBook(long reqId)
        {
            //bool isMarketOff = false;
            //while (htSubscribe.Contains(reqId) && (bool)htSubscribe[reqId] && !isMarketOff)
            //{
            //    OleDbConnection conn = this.getdbfconn();
            //    string sql = string.Format("SELECT * FROM {0} WHERE 1=1 AND S1 IN ({1})", "show2003", subscribedStocks);
            //    OleDbDataAdapter da = new OleDbDataAdapter(sql, conn);
            //    DataTable tblHQ = new DataTable();
            //    da.Fill(tblHQ);

            //    if (tblHQ.Rows.Count > 0)
            //    {
            //        foreach (DataRow row in tblHQ.Rows)
            //        {
            //            string code = row["S1"].ToString().Trim();
            //        }
            //    }
            //}
        }
        private string subscribedStocks = "'',";
        private void addSubscribedStocks(string code)
        {
            subscribedStocks += "'" + code + "'";
        }
        #endregion
        #endregion

        #region 期权集合
        public override List<Option> GetOptionSet(List<ASecurity> underlyings)
        {
            List<Option> optionlist = new List<Option>();
            if (underlyings != null && underlyings.Count > 0)
            {
                foreach (ASecurity s in underlyings)
                {
                    List<Option> ol = this.GetOptionSet(s);
                    if (ol != null && ol.Count > 0)
                        optionlist.AddRange(ol);
                }
            }

            return optionlist;
        }
        public override List<Option> GetOptionSet(ASecurity underlying)
        {
            try
            {
                this.readOptionContractFile();
                if (this.htOptionSets.Contains(underlying.code))
                    return (List<Option>)htOptionSets[underlying.code];
                else
                    return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void readOptionContractFile()
        {
            try
            {
                if (this.htOptionSets.Count == 0)
                {
                    #region 读取文件
                    //复制到本地
                    File.Copy(filepath_option_contract_source, filepath_option_contract_target, true);
                    
                    //读取该文件
                    string filecontent = "";
                    using (StreamReader sReader = new StreamReader(filepath_option_contract_target, Encoding.Default))
                    {
                        filecontent = sReader.ReadToEnd();
                        sReader.Close();
                    }
                    #endregion

                    #region 解析合约
                    string[] lines = filecontent.Split("\n".ToCharArray());
                    if (lines == null || lines.Length == 0)
                    {
                        MessageManager.GetInstance().Add(MessageType.Error, string.Format("期权合约文件为空"));
                        return;
                    }

                    List<Option> optionlist;
                    foreach (string line in lines)
                    {
                        if (line != null && line.Trim().Length > 0)
                        {
                            string[] fields = line.Split("|".ToCharArray());

                            if (fields.Length >= 34)
                            {
                                //代码
                                Option o = new Option(fields[1].Trim(), Exchange.SHE);
                                o.contractcode = fields[2].Trim();
                                o.name = fields[3].Trim();
                                o.underlyingunits = Convert.ToInt16(fields[9].Trim());
                                o.strike = Convert.ToDouble(fields[10].Trim());
                                o.exercisedate = Utility.ConvertToDateTime(fields[13].Trim(), "yyyyMMdd");
                                o.priceuplimit = Convert.ToDouble(fields[22].Trim());
                                o.pricedownlimit = Convert.ToDouble(fields[23].Trim());
                                o.marginunit = Convert.ToDouble(fields[24].Trim());
                                o.lmtordmaxfloor = Convert.ToInt16(fields[29].Trim());
                                o.ticksize = Convert.ToDouble(fields[32].Trim());
                                o.daystoexercise = (o.exercisedate - DateTime.Today).Days;
                                o.exercisemonth = o.exercisedate.Month;

                                #region 标的
                                string underlyingtype = fields[6].Trim().ToUpper();
                                string underlyingcode = fields[4].Trim();
                                if (this.htUnderlyingSets.Contains(underlyingcode))
                                {
                                    //标的已存在
                                    o.underlying = (ASecurity)htUnderlyingSets[underlyingcode];
                                }
                                else
                                {
                                    //新建标的
                                    ASecurity s = null;
                                    switch (underlyingtype)
                                    {
                                        case "ASH": //股票
                                            s = new Stock(underlyingcode, Exchange.SHE);
                                            break;
                                        case "EBS": //ETF
                                            s = new ETF(underlyingcode, Exchange.SHE);                                            
                                            break;
                                        default:
                                            break;
                                    }

                                    if (s != null)
                                    {
                                        s.name = fields[5].Trim();
                                        htUnderlyingSets.Add(underlyingcode, s);
                                        o.underlying = s;
                                    }
                                }
                                #endregion                      

                                //类型：call or put
                                string type = fields[8].Trim().ToUpper();
                                if (type == "C")
                                    o.type = OptionType.CALL;
                                else
                                    o.type = OptionType.PUT;

                                //交易标志
                                char[] flag = fields[33].Trim().ToUpper().ToCharArray();
                                o.islimitopening = (flag[0] == '0') ? false : true;     //第1位：‘0’表示可开仓，‘1’表示限制卖出开仓（不.包括备兑开仓）和买入开仓。
                                o.istrading = (flag[1] == '0') ? true : false;          //第2位：‘0’表示未连续停牌，‘1’表示连续停牌。
                                o.isnew = (flag[4] == 'A') ? true : false;              //第5位：‘A’表示当日新挂牌的合约，‘E’表示存续的合约

                                //加入列表
                                if (this.htOptionSets.Contains(o.underlying.code))
                                {
                                    optionlist = (List<Option>)this.htOptionSets[o.underlying.code];
                                    optionlist.Add(o);
                                }
                                else
                                {
                                    optionlist = new List<Option>();
                                    optionlist.Add(o);
                                    this.htOptionSets.Add(o.underlying.code, optionlist);
                                }
                            }
                        }
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {                
                throw ex;
            }
        }
        #endregion
    }
}
