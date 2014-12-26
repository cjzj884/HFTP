
namespace HFTP.Security
{
    public class TradeCost
    {
        public Exchange exchange = Exchange.NONE;
        public SecurityCategory category = SecurityCategory.NONE;

        public double brokagerate = 0;      //交易佣金率3%%
        public double brokagemin = 0;       //最低佣金额5元
        public double brokageperunit = 0;   //每单位证券费用
        public double stamptaxrate = 0;     //印花税0.1%

        public TradeCost(Exchange exchange, SecurityCategory category)
        {
            this.exchange = exchange;
            this.category = category;

            //初始数据，具体数据应当可以从配置文件读入
            switch (category)
            {
                case SecurityCategory.STOCK:
                    this.brokagerate = 0.0002;
                    this.brokagemin = 0;
                    this.brokageperunit = 0;
                    this.stamptaxrate = 0.001;
                    break;
                case SecurityCategory.ETF:
                    this.brokagerate = 0.0002;
                    this.brokagemin = 0;
                    this.brokageperunit = 0;
                    this.stamptaxrate = 0;
                    break;
                case SecurityCategory.OPTION:
                    this.brokagerate = 0;
                    this.brokagemin = 0;
                    this.brokageperunit = 2;
                    this.stamptaxrate = 0;
                    break;
                default:
                    break;
            }
        }
    }
}
