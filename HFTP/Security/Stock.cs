using System;

namespace HFTP.Security
{
    public class Stock:ASecurity
    {
        #region 股票属性

        #endregion

        public Stock(string code, Exchange exchange) : base(code, exchange, SecurityCategory.STOCK) { }

        #region 证券接口
        #endregion
    }
}
