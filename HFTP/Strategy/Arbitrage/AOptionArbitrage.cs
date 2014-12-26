using HFTP.Security;
using System.Collections.Generic;

namespace HFTP.Strategy.Arbitrage
{
    public abstract class AOptionArbitrage
    {
        public string name;
        public string desc;
        public bool isvalid = false;
        public double requiredrate = 0.06;    //套利最低年化收益率要求;

        protected List<Option> _optionlist = new List<Option>();

        protected virtual bool validate()
        {
            //检查：有期权合约存在
            if (_optionlist.Count == 0)
            {
                MessageManager.GetInstance().Add(MessageType.Error, string.Format("找不到期权合约：{0}", this.name));
                return false;
            }

            bool flg = true;
            //检查：各期权合约不为空，含有K和T等关键信息
            foreach (Option o in _optionlist)
            {
                if (o == null) 
                {
                    MessageManager.GetInstance().Add(MessageType.Error, string.Format("期权合约为空：{0}", this.name));
                    flg = flg && false;
                }

                //检查T
                if (o.exercisedate == Utility.C_NULL_DATE)
                {
                    MessageManager.GetInstance().Add(MessageType.Error, string.Format("期权合约缺少行权日期：{0},{1},{2}", this.name, o.code, o.name));
                    flg = flg && false;
                }
                //检查K
                if (o.strike == 0)
                {
                    MessageManager.GetInstance().Add(MessageType.Error, string.Format("期权合约缺少行权价：{0},{1},{2}", this.name, o.code, o.name));
                    flg = flg && false;
                }
                //检查：标的一致
                if (o.underlying == null || _optionlist[0].underlying == null || o.underlying.code != _optionlist[0].underlying.code)
                {
                    MessageManager.GetInstance().Add(MessageType.Error, string.Format("期权合约标的不一致：{0}：{1},{2}", this.name, o.underlying.code, this._optionlist[0].underlying.code));
                    flg = flg && false;
                }
            }

            //排序:
            //  Call<Put...按照OptionType定义的顺序
            //  K1<K2<K3...按照数值大小
            if (flg)
                _optionlist.Sort(delegate(Option op1, Option op2) {
                    if (op1.type != op2.type)
                        return op1.type.CompareTo(op2.type);
                    else
                        return op1.strike.CompareTo(op2.strike);
                });

            //返回校验结果
            return flg;
        }

        public virtual void Run()
        {
            //期权校验失败退出
            if (!isvalid)
            {
                MessageManager.GetInstance().Add(MessageType.Error, "期权合约校验失败");
                return;
            }

            //尝试交易
            this.tradeCallType();
            this.tradePutType();
        }

        protected abstract void tradeCallType();    //由看涨期权组成的套利
        protected abstract void tradePutType();     //由看跌期权组成的套利
    }
}
