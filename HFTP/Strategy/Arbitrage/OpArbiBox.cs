using HFTP.Security;
using System;

namespace HFTP.Strategy.Arbitrage
{
    public class OpArbiBox: AOptionArbitrage
    {
        public OpArbiBox(Option call1, Option call2, Option put1, Option put2)
        {
            this.name = "期权盒式组合套利";
            this.desc = "";            
            this._optionlist.Add(call1);
            this._optionlist.Add(call2);
            this._optionlist.Add(put1);
            this._optionlist.Add(put2);
            this.isvalid = this.validate();
        }

        protected override bool validate()
        {
            if (!base.validate())
                return false;

            bool flg = true;
            foreach (Option o in this._optionlist)
            {
                //检查：期权类型一致
                if (!(_optionlist[0].type == OptionType.CALL && _optionlist[1].type == OptionType.CALL 
                    && _optionlist[2].type == OptionType.PUT && _optionlist[3].type == OptionType.PUT))
                {
                    MessageManager.GetInstance().Add(MessageType.Error, string.Format("期权合约类型不符合要求：{0}：{1},{2},{3},{4}", this.name
                        , _optionlist[0].type, _optionlist[1].type, _optionlist[2].type, _optionlist[3].type));
                    flg = flg && false;
                }

                //检查：行权日一致
                if (o.exercisedate != this._optionlist[0].exercisedate)
                {
                    MessageManager.GetInstance().Add(MessageType.Error, string.Format("期权合约行权日不一致：{0}：{1},{2}", this.name
                        , o.exercisedate.ToString("yyyy-MM-dd"), this._optionlist[0].exercisedate.ToString("yyyy-MM-dd")));
                    flg = flg && false;
                }
            }

            //检查：行权价Call:K2>K1,Put:K2>K1
            if (!(_optionlist[0].strike < _optionlist[1].strike && _optionlist[2].strike < _optionlist[3].strike))
            {
                MessageManager.GetInstance().Add(MessageType.Error, string.Format("期权合约行权价不符合要求：{0}：{1},{2},{3},{4}", this.name
                    , _optionlist[0].strike.ToString("N2"), _optionlist[1].strike.ToString("N2")
                    , _optionlist[2].strike.ToString("N2"), _optionlist[3].strike.ToString("N2")));
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
