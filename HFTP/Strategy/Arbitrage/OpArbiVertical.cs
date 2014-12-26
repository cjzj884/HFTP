using HFTP.Security;
using System;

namespace HFTP.Strategy.Arbitrage
{
    public class OpArbiVertical: AOptionArbitrage
    {
        public OpArbiVertical(Option o1,Option o2)
        {
            this.name = "期权价差组合套利";
            this.desc = "";
            this._optionlist.Add(o1);
            this._optionlist.Add(o2);
            this.isvalid = this.validate();
        }

        protected override bool validate()
        {
            if (!base.validate())
                return false;

            bool flg = true;
            //检查：期权类型一致
            if (this._optionlist[0].type != this._optionlist[1].type)
            {
                MessageManager.GetInstance().Add(MessageType.Error, string.Format("期权合约类型不一致：{0}：{1},{2}", this.name, this._optionlist[0].type, this._optionlist[1].type));
                flg = flg && false;
            }

            //检查：行权日一致
            if (this._optionlist[0].exercisedate != this._optionlist[1].exercisedate)
            {
                MessageManager.GetInstance().Add(MessageType.Error, string.Format("期权合约行权日不一致：{0}：{1},{2}", this.name, this._optionlist[0].exercisedate.ToString("yyyy-MM-dd"), this._optionlist[1].exercisedate.ToString("yyyy-MM-dd")));
                flg = flg && false;
            }

            //检查：行权价K2>K1
            if (this._optionlist[0].strike >= this._optionlist[1].strike)
            {
                MessageManager.GetInstance().Add(MessageType.Error, string.Format("期权合约行权价不符合要求：{0}：{1},{2}", this.name, this._optionlist[0].strike.ToString("N2"), this._optionlist[1].strike.ToString("N2")));
                flg = flg && false;
            }

            return flg;
        }
        
        protected override void tradeCallType()
        {
            throw new NotImplementedException();
        }

        protected override void tradePutType()
        {
            throw new NotImplementedException();
        }
    }
}
