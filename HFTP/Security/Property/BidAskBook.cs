
using System;
using System.Diagnostics;
namespace HFTP.Security
{
    public enum TradingPhase
    { 
        NONE,
        START,          //开始前准备
        CALL,           //开盘集合竞价
        UCALL,          //收盘集合竞价
        TRADE,          //连续竞价
        BREAK,          //休市
        END,            //闭市
        VOLBREAK,       //波动性中断
        PAUSE           //临时停牌
    }

    public class BidAskBook
    {
        public TradingPhase phase = TradingPhase.NONE;

        public string tradetime = "";   //hhmmss
        public double preclose = 0;     //前收盘
        public double presettle = 0;    //前结算
        public double open = 0;
        public double high = 0;
        public double low = 0;
        public double last = 0;         //集合竞价时：last会根据模拟价变化
        public double lasttrade = 0;    //集合竞价时：保留前一交易价格；开盘时为0，不等于昨收盘

        public double[] bid = new double[] { 0, 0, 0, 0, 0 };
        public int[] bidsize = new int[] { 0, 0, 0, 0, 0 };
        public double[] ask = new double[] { 0, 0, 0, 0, 0 };
        public int[] asksize = new int[] { 0, 0, 0, 0, 0 };
        
        public void DebugPrint()
        {
            Debug.Print("=======================================");
            Debug.Print(string.Format("time={0}", this.tradetime));

            //Debug.Print(string.Format("{0},{1},{2},{3},{4},{5}", this.preclose, this.presettle, this.open, this.high, this.low, this.last));
            //Debug.Print(string.Format("A5={0},{1}", this.ask[4], this.asksize[4]));
            //Debug.Print(string.Format("A4={0},{1}", this.ask[3], this.asksize[3]));
            //Debug.Print(string.Format("A3={0},{1}", this.ask[2], this.asksize[2]));
            //Debug.Print(string.Format("A2={0},{1}", this.ask[1], this.asksize[1]));
            Debug.Print(string.Format("A1={0},{1}", this.ask[0], this.asksize[0]));

            Debug.Print(string.Format("B1={0},{1}", this.bid[0], this.bidsize[0]));
            //Debug.Print(string.Format("B2={0},{1}", this.bid[1], this.bidsize[1]));
            //Debug.Print(string.Format("B3={0},{1}", this.bid[2], this.bidsize[2]));
            //Debug.Print(string.Format("B4={0},{1}", this.bid[3], this.bidsize[3]));
            //Debug.Print(string.Format("B5={0},{1}", this.bid[4], this.bidsize[4]));
        }
    }
}
