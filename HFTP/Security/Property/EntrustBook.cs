using HFTP.Entrust;
using System.Diagnostics;

namespace HFTP.Security
{
    public class EntrustBook
    {
        public int entrustno = 0;
        public int batchno = 0;
        public string code = "";
        public double price = 0;
        public double volume = 0;
        public TradeDirection tradedirection = TradeDirection.NONE;
        public FutureDirection futuredirection = FutureDirection.NONE;
        public string message = "";

        public void DebugPrint()
        {
            Debug.Print(string.Format("batch={0},entrustno={1},code={2},px={3},vol={4}", batchno, entrustno, code, price, volume));
        }
    }
}
