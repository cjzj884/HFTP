using HFTP;
using HFTP.Entrust;
using HFTP.Market;
using HFTP.Security;
using HFTP.Strategy.MarketMaker;
using System;
using System.Collections;
using System.Threading;
using System.Windows.Forms;

namespace OptionMMCenter
{
    public partial class FormOptionMM : Form
    {
        public FormOptionMM()
        {
            InitializeComponent();
        }

        private void btnHundsun_Click(object sender, EventArgs e)
        {
            AEntrust engine = AEntrust.GetInstance(EntrustVendor.Hundsun);
            engine.Logon();
            textBoxCheckHundsun.Text = engine.GetToken();
        }

        private void btnCheckExchange_Click(object sender, EventArgs e)
        {
            AMarket engine = AMarket.GetInstance(MarketVendor.Exchange);
            textBoxExchange.Text = engine.GetTradeTime();
        }

        private Hashtable htUnderlyings = new Hashtable();
        private void FormOptionMM_Load(object sender, EventArgs e)
        {
            htUnderlyings.Add("510050", new ETF("510050", Exchange.SHE));
            htUnderlyings.Add("510180", new ETF("510180", Exchange.SHE));
            htUnderlyings.Add("601318", new Stock("601318", Exchange.SHE));
            htUnderlyings.Add("600104", new Stock("600104", Exchange.SHE));

            checkedListBoxUnderlying.Items.Add("510050", true);
            checkedListBoxUnderlying.Items.Add("510180", false);
            checkedListBoxUnderlying.Items.Add("601318", false);
            checkedListBoxUnderlying.Items.Add("600104", false);
        }

        private delegate void MMRunDelegate();
        private Hashtable htmarketmakers = new Hashtable();

        #region ShowMessage
        private bool isMMruning = false;
        private delegate void ShowMsgDelegate(string sysmsg, string trademsg);
        private void readMsg()
        {
            ShowMsgDelegate msgdlg = new ShowMsgDelegate(showMsg);
            while (isMMruning)
            {
                string sysmsg = "", trademsg = "";
                MessageManager.GetInstance().GetNewMessages(ref sysmsg, ref trademsg);
                this.BeginInvoke(msgdlg, sysmsg, trademsg);
                Thread.Sleep(1000);
            }
        }
        private void showMsg(string sysmsg, string trademsg)
        {
            this.richTextBoxSystemMsg.AppendText(sysmsg);
            this.richTextBoxSystemMsg.ScrollToCaret();

            this.richTextBoxTradeMsg.AppendText(trademsg);
            this.richTextBoxTradeMsg.ScrollToCaret();
        }
        #endregion

        private void btnRunMM_Click(object sender, EventArgs e)
        {
            foreach (string code in checkedListBoxUnderlying.CheckedItems)
            {
                ASecurity s = null;
                if (htUnderlyings.Contains(code))
                    s = (ASecurity)htUnderlyings[code];

                AOptionMakeMarket omm = new OptionMakeMarket1();
                omm.AddOptions(s);
                MMRunDelegate dlg = new MMRunDelegate(omm.Run);
                dlg.BeginInvoke(null, null);
                htmarketmakers.Add(s.code, omm);
            }

            //消息列表
            isMMruning = true;
            Thread thread = new Thread(readMsg);
            thread.IsBackground = true;
            thread.Start();
        }

        private void btnStopMM_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("是否停止做市？","期权做市", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                foreach (string code in checkedListBoxUnderlying.CheckedItems)
                {
                    if (htmarketmakers.Contains(code))
                    {
                        AOptionMakeMarket omm = (AOptionMakeMarket)htmarketmakers[code];
                        omm.Stop();
                    }
                }
                isMMruning = false;
            }
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("是否暂停做市？", "期权做市", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                foreach (string code in checkedListBoxUnderlying.CheckedItems)
                {
                    if (htmarketmakers.Contains(code))
                    {
                        AOptionMakeMarket omm = (AOptionMakeMarket)htmarketmakers[code];
                        omm.Pause();
                    }
                }
            }
        }

        private void btnResume_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("是否恢复做市？", "期权做市", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                foreach (string code in checkedListBoxUnderlying.CheckedItems)
                {
                    if (htmarketmakers.Contains(code))
                    {
                        AOptionMakeMarket omm = (AOptionMakeMarket)htmarketmakers[code];
                        omm.Resume();
                    }
                }
            }
        }

        private void btnRefreshMsg_Click(object sender, EventArgs e)
        {
            string sysmsg = "", trademsg = "";
            MessageManager.GetInstance().GetNewMessages(ref sysmsg, ref trademsg);
            richTextBoxSystemMsg.AppendText(sysmsg);
            richTextBoxTradeMsg.AppendText(trademsg);
        }
    }
}
