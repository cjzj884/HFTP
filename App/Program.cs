using HFTP;
using HFTP.Entrust;
using HFTP.Market;
using HFTP.Security;
using HFTP.Strategy.MarketMaker;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace App
{
    class Program
    {
        static void Main(string[] args)
        {
            //List<int> intlist1 = new List<int>();
            //intlist1.Add(1);
            //intlist1.Add(2);

            //List<int> intlist2 = new List<int>();
            //intlist2.Add(2);
            //intlist2.Add(3);

            //intlist1.AddRange(intlist2);

            //testMarket1();
            //testMarket2();
            //testMarket3();
            //testMarket4();

            //testHundsun1();
            //testHundsun2();
            //testHundsun3();

            //testMM1();
            testMM2();

            Console.ReadLine();

            MessageManager.GetInstance().DebugPrint();
        }

        #region Market
        static void testMarket1()
        {
            AMarket market = AMarket.GetInstance(MarketVendor.Exchange);
            ASecurity s1 = new ETF("510050", Exchange.SHE);     //50ETF
            ASecurity s2 = new ETF("510180", Exchange.SHE);     //100ETF
            ASecurity s3 = new Stock("601318", Exchange.SHE);   //中国平安
            ASecurity s4 = new Stock("600104", Exchange.SHE);   //上汽集团

            List<ASecurity> seclist = new List<ASecurity>();
            seclist.Add(s1);
            seclist.Add(s2);
            seclist.Add(s3);
            seclist.Add(s4);

            List<Option> optionlist = market.GetOptionSet(seclist);

            //ASecurity s = (ASecurity)market.htUnderlyingSets["510050"];
            //s.name = "更改";

            if (optionlist != null)
            {
                foreach (Option o in optionlist)
                {
                    o.DebugPrint();
                }
            }
            market.Dispose();
        }
        static void testMarket2()
        {
            AMarket market = AMarket.GetInstance(MarketVendor.Exchange);
            Option o1 = new Option("90000528", Exchange.SHE);
            Option o2 = new Option("90000526", Exchange.SHE);
            Option o3 = new Option("90000543", Exchange.SHE);
            market.SubscribeBidAskBook(o1, 5);
            market.SubscribeBidAskBook(o2, 5);
            market.SubscribeBidAskBook(o3, 5);
            while (true)
            {
                o1.bidaskbook.DebugPrint();
                o2.bidaskbook.DebugPrint();
                o3.bidaskbook.DebugPrint();
            }
        }
        static void testMarket3()
        {
            AMarket market = AMarket.GetInstance(MarketVendor.Exchange);
            Option o = new Option("90000528", Exchange.SHE);
            market.SubscribeBidAskBook(o,1);

            while (true)
            {
                o.bidaskbook.DebugPrint();
            }
        }
        static void testMarket4()
        {
            AMarket market = AMarket.GetInstance(MarketVendor.Exchange);
            ETF etf = new ETF("510050", Exchange.SHE);
            Stock s = new Stock("601318", Exchange.SHE);
            market.SubscribeBidAskBook(etf, 2);
            market.SubscribeBidAskBook(s, 2);
            market.Dispose();
        }
        #endregion

        #region Entrust
        static void testHundsun1()
        {
            string user = "738";
            string pwd = "Abc123456";
            string combino = "1101_001";  //11040201

            EntrustHundsun eh = new EntrustHundsun();
            eh.Logon(user, pwd);

            #region 单笔下单
            EntrustPara param = new EntrustPara();
            param.portfolio = combino;
            param.entrustdirection = TradeDirection.SELL;
            param.futuredirection = FutureDirection.OPEN;
            param.exchange = Exchange.SHE;
            param.securitycode = "90000539";
            param.price = 0.54;
            param.volume = 1;
            eh.OptionSingleEntrust(param);
            #endregion


            #region 篮子下单
            List<EntrustPara> paramlist = new List<EntrustPara>();

            EntrustPara param1 = new EntrustPara();
            param1.portfolio = combino;
            param1.entrustdirection = TradeDirection.BUY;
            param1.futuredirection = FutureDirection.OPEN;
            param1.exchange = Exchange.SHE;
            param1.securitycode = "90000539";
            param1.price = 0.525;
            param1.volume = 5;
            EntrustPara param2 = new EntrustPara();
            param2.portfolio = combino;
            param2.entrustdirection = TradeDirection.SELL;
            param2.futuredirection = FutureDirection.OPEN;
            param2.exchange = Exchange.SHE;
            param2.securitycode = "90000539";
            param2.price = 0.535;
            param2.volume = 5;

            paramlist.Add(param1);
            paramlist.Add(param2);
            eh.OptionBasketEntrust(paramlist);
            #endregion
        }
        static void testHundsun2()
        {
            string user = "738";
            string pwd = "Abc123456";

            EntrustHundsun eh = new EntrustHundsun();
            eh.Logon(user, pwd);

            #region 撤单
            List<int> paramlist = new List<int>();
            paramlist.Add(961256);
            paramlist.Add(961257);
            eh.OptionWithdraw(paramlist);
            #endregion
        }
        static void testHundsun3()
        {
            string user = "738";
            string pwd = "Abc123456";
            string acc = "1104";
            string combi = "11040201";
            string opcode="90000229";

            Option o = new Option(opcode, Exchange.SHE);
            EntrustHundsun eh = new EntrustHundsun();
            eh.Logon(user, pwd);

            #region 查询持仓
            QueryPara param = new QueryPara();
            param.fundcode=acc;
            param.portfolio = combi;
            param.securitycode =opcode;

            eh.OptionPositionQuery(param, o.positionbook);
            eh.OptionEntrustQuery(param, o.entrustbook);
            #endregion
        }
        #endregion

        #region MM
        static void testMM1()
        {
            string acc = "1104";
            string combi = "11040201";

            List<ASecurity> underlyinglist = new List<ASecurity>();
            //underlyinglist.Add(new ETF("510050", Exchange.SHE));
            underlyinglist.Add(new ETF("510180", Exchange.SHE));
            underlyinglist.Add(new Stock("601318", Exchange.SHE));
            //underlyinglist.Add(new Stock("600104", Exchange.SHE));

            AMarket mkt = AMarket.GetInstance(MarketVendor.Exchange);
            List<Option> optionlist = mkt.GetOptionSet(underlyinglist);

            foreach (Option o in optionlist)
            {
                QueryPara param = new QueryPara();
                param.fundcode = acc;
                param.portfolio = combi;
                param.securitycode = o.code;

                AEntrust et = AEntrust.GetInstance(EntrustVendor.Hundsun);
                et.OptionEntrustQuery(param, o.entrustbook);
                bool flg = o.makemarketstat.IsValid();

                if (o.entrustbook != null && o.entrustbook.Count > 0)
                {
                    foreach (EntrustBook eb in o.entrustbook)
                    {
                        eb.DebugPrint();
                    }
                }

                //Debug.Print(string.Format("spread, code={2},name={3},价差={0},{1}", o.makemarketstat.currminspreadpct.ToString("P2"), o.makemarketstat.message, o.code,o.name));
            }
        }
        static void testMM2()
        {
            List<ASecurity> underlyinglist = new List<ASecurity>();
            underlyinglist.Add(new ETF("510050", Exchange.SHE));
            underlyinglist.Add(new ETF("510180", Exchange.SHE));
            underlyinglist.Add(new ETF("600104", Exchange.SHE));
            underlyinglist.Add(new ETF("601318", Exchange.SHE));

            AOptionMakeMarket mm = new OptionMakeMarket1();
            mm.AddOptions(underlyinglist);

            mm.Run();
        }
        #endregion
    }
}
