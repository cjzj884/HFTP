namespace OptionMMCenter
{
    partial class FormOptionMM
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.tabControlMain = new System.Windows.Forms.TabControl();
            this.tabPageMonitor = new System.Windows.Forms.TabPage();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.richTextBoxTradeMsg = new System.Windows.Forms.RichTextBox();
            this.richTextBoxSystemMsg = new System.Windows.Forms.RichTextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnResume = new System.Windows.Forms.Button();
            this.btnPause = new System.Windows.Forms.Button();
            this.btnStopMM = new System.Windows.Forms.Button();
            this.btnRunMM = new System.Windows.Forms.Button();
            this.checkedListBoxUnderlying = new System.Windows.Forms.CheckedListBox();
            this.dataGridViewMM = new System.Windows.Forms.DataGridView();
            this.tabPageCheck = new System.Windows.Forms.TabPage();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxCheckHundsun = new System.Windows.Forms.TextBox();
            this.btnHundsun = new System.Windows.Forms.Button();
            this.textBoxExchange = new System.Windows.Forms.TextBox();
            this.btnCheckExchange = new System.Windows.Forms.Button();
            this.bgWorkerMain = new System.ComponentModel.BackgroundWorker();
            this.btnRefreshMsg = new System.Windows.Forms.Button();
            this.tabControlMain.SuspendLayout();
            this.tabPageMonitor.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewMM)).BeginInit();
            this.tabPageCheck.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControlMain
            // 
            this.tabControlMain.Controls.Add(this.tabPageMonitor);
            this.tabControlMain.Controls.Add(this.tabPageCheck);
            this.tabControlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControlMain.Location = new System.Drawing.Point(0, 0);
            this.tabControlMain.Name = "tabControlMain";
            this.tabControlMain.SelectedIndex = 0;
            this.tabControlMain.Size = new System.Drawing.Size(1010, 487);
            this.tabControlMain.TabIndex = 0;
            // 
            // tabPageMonitor
            // 
            this.tabPageMonitor.Controls.Add(this.splitContainer1);
            this.tabPageMonitor.Location = new System.Drawing.Point(4, 22);
            this.tabPageMonitor.Name = "tabPageMonitor";
            this.tabPageMonitor.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageMonitor.Size = new System.Drawing.Size(1002, 461);
            this.tabPageMonitor.TabIndex = 2;
            this.tabPageMonitor.Text = "做市监控";
            this.tabPageMonitor.UseVisualStyleBackColor = true;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(3, 3);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.groupBox2);
            this.splitContainer1.Panel1.Controls.Add(this.groupBox1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.dataGridViewMM);
            this.splitContainer1.Size = new System.Drawing.Size(996, 455);
            this.splitContainer1.SplitterDistance = 423;
            this.splitContainer1.TabIndex = 0;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.richTextBoxTradeMsg);
            this.groupBox2.Controls.Add(this.richTextBoxSystemMsg);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(273, 0);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(723, 423);
            this.groupBox2.TabIndex = 14;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "消息";
            // 
            // richTextBoxTradeMsg
            // 
            this.richTextBoxTradeMsg.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBoxTradeMsg.Location = new System.Drawing.Point(403, 17);
            this.richTextBoxTradeMsg.Name = "richTextBoxTradeMsg";
            this.richTextBoxTradeMsg.Size = new System.Drawing.Size(317, 403);
            this.richTextBoxTradeMsg.TabIndex = 14;
            this.richTextBoxTradeMsg.Text = "";
            // 
            // richTextBoxSystemMsg
            // 
            this.richTextBoxSystemMsg.Dock = System.Windows.Forms.DockStyle.Left;
            this.richTextBoxSystemMsg.Location = new System.Drawing.Point(3, 17);
            this.richTextBoxSystemMsg.Name = "richTextBoxSystemMsg";
            this.richTextBoxSystemMsg.Size = new System.Drawing.Size(400, 403);
            this.richTextBoxSystemMsg.TabIndex = 13;
            this.richTextBoxSystemMsg.Text = "";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnRefreshMsg);
            this.groupBox1.Controls.Add(this.btnResume);
            this.groupBox1.Controls.Add(this.btnPause);
            this.groupBox1.Controls.Add(this.btnStopMM);
            this.groupBox1.Controls.Add(this.btnRunMM);
            this.groupBox1.Controls.Add(this.checkedListBoxUnderlying);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Left;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(273, 423);
            this.groupBox1.TabIndex = 13;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "选择标的";
            // 
            // btnResume
            // 
            this.btnResume.Location = new System.Drawing.Point(91, 139);
            this.btnResume.Name = "btnResume";
            this.btnResume.Size = new System.Drawing.Size(75, 23);
            this.btnResume.TabIndex = 16;
            this.btnResume.Text = "恢复做市";
            this.btnResume.UseVisualStyleBackColor = true;
            this.btnResume.Click += new System.EventHandler(this.btnResume_Click);
            // 
            // btnPause
            // 
            this.btnPause.Location = new System.Drawing.Point(6, 139);
            this.btnPause.Name = "btnPause";
            this.btnPause.Size = new System.Drawing.Size(75, 23);
            this.btnPause.TabIndex = 15;
            this.btnPause.Text = "暂停做市";
            this.btnPause.UseVisualStyleBackColor = true;
            this.btnPause.Click += new System.EventHandler(this.btnPause_Click);
            // 
            // btnStopMM
            // 
            this.btnStopMM.Location = new System.Drawing.Point(91, 110);
            this.btnStopMM.Name = "btnStopMM";
            this.btnStopMM.Size = new System.Drawing.Size(75, 23);
            this.btnStopMM.TabIndex = 14;
            this.btnStopMM.Text = "停止做市";
            this.btnStopMM.UseVisualStyleBackColor = true;
            this.btnStopMM.Click += new System.EventHandler(this.btnStopMM_Click);
            // 
            // btnRunMM
            // 
            this.btnRunMM.Location = new System.Drawing.Point(6, 110);
            this.btnRunMM.Name = "btnRunMM";
            this.btnRunMM.Size = new System.Drawing.Size(75, 23);
            this.btnRunMM.TabIndex = 13;
            this.btnRunMM.Text = "开始做市";
            this.btnRunMM.UseVisualStyleBackColor = true;
            this.btnRunMM.Click += new System.EventHandler(this.btnRunMM_Click);
            // 
            // checkedListBoxUnderlying
            // 
            this.checkedListBoxUnderlying.FormattingEnabled = true;
            this.checkedListBoxUnderlying.Location = new System.Drawing.Point(6, 20);
            this.checkedListBoxUnderlying.Name = "checkedListBoxUnderlying";
            this.checkedListBoxUnderlying.Size = new System.Drawing.Size(160, 84);
            this.checkedListBoxUnderlying.TabIndex = 12;
            // 
            // dataGridViewMM
            // 
            this.dataGridViewMM.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewMM.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewMM.Location = new System.Drawing.Point(0, 0);
            this.dataGridViewMM.Name = "dataGridViewMM";
            this.dataGridViewMM.RowTemplate.Height = 23;
            this.dataGridViewMM.Size = new System.Drawing.Size(996, 28);
            this.dataGridViewMM.TabIndex = 0;
            // 
            // tabPageCheck
            // 
            this.tabPageCheck.Controls.Add(this.label2);
            this.tabPageCheck.Controls.Add(this.label1);
            this.tabPageCheck.Controls.Add(this.textBoxCheckHundsun);
            this.tabPageCheck.Controls.Add(this.btnHundsun);
            this.tabPageCheck.Controls.Add(this.textBoxExchange);
            this.tabPageCheck.Controls.Add(this.btnCheckExchange);
            this.tabPageCheck.Location = new System.Drawing.Point(4, 22);
            this.tabPageCheck.Name = "tabPageCheck";
            this.tabPageCheck.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageCheck.Size = new System.Drawing.Size(1002, 461);
            this.tabPageCheck.TabIndex = 1;
            this.tabPageCheck.Text = "环境检测";
            this.tabPageCheck.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 34);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 5;
            this.label2.Text = "恒生系统";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 4;
            this.label1.Text = "交易所行情";
            // 
            // textBoxCheckHundsun
            // 
            this.textBoxCheckHundsun.Location = new System.Drawing.Point(80, 31);
            this.textBoxCheckHundsun.Name = "textBoxCheckHundsun";
            this.textBoxCheckHundsun.Size = new System.Drawing.Size(150, 21);
            this.textBoxCheckHundsun.TabIndex = 3;
            // 
            // btnHundsun
            // 
            this.btnHundsun.Location = new System.Drawing.Point(236, 31);
            this.btnHundsun.Name = "btnHundsun";
            this.btnHundsun.Size = new System.Drawing.Size(75, 23);
            this.btnHundsun.TabIndex = 2;
            this.btnHundsun.Text = "检测";
            this.btnHundsun.UseVisualStyleBackColor = true;
            this.btnHundsun.Click += new System.EventHandler(this.btnHundsun_Click);
            // 
            // textBoxExchange
            // 
            this.textBoxExchange.Location = new System.Drawing.Point(80, 4);
            this.textBoxExchange.Name = "textBoxExchange";
            this.textBoxExchange.Size = new System.Drawing.Size(150, 21);
            this.textBoxExchange.TabIndex = 1;
            // 
            // btnCheckExchange
            // 
            this.btnCheckExchange.Location = new System.Drawing.Point(236, 4);
            this.btnCheckExchange.Name = "btnCheckExchange";
            this.btnCheckExchange.Size = new System.Drawing.Size(75, 23);
            this.btnCheckExchange.TabIndex = 0;
            this.btnCheckExchange.Text = "检测";
            this.btnCheckExchange.UseVisualStyleBackColor = true;
            this.btnCheckExchange.Click += new System.EventHandler(this.btnCheckExchange_Click);
            // 
            // btnRefreshMsg
            // 
            this.btnRefreshMsg.Location = new System.Drawing.Point(192, 20);
            this.btnRefreshMsg.Name = "btnRefreshMsg";
            this.btnRefreshMsg.Size = new System.Drawing.Size(75, 23);
            this.btnRefreshMsg.TabIndex = 17;
            this.btnRefreshMsg.Text = "更新消息";
            this.btnRefreshMsg.UseVisualStyleBackColor = true;
            this.btnRefreshMsg.Click += new System.EventHandler(this.btnRefreshMsg_Click);
            // 
            // FormOptionMM
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1010, 487);
            this.Controls.Add(this.tabControlMain);
            this.Name = "FormOptionMM";
            this.Text = "i做市";
            this.Load += new System.EventHandler(this.FormOptionMM_Load);
            this.tabControlMain.ResumeLayout(false);
            this.tabPageMonitor.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewMM)).EndInit();
            this.tabPageCheck.ResumeLayout(false);
            this.tabPageCheck.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControlMain;
        private System.Windows.Forms.TabPage tabPageCheck;
        private System.Windows.Forms.TabPage tabPageMonitor;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxCheckHundsun;
        private System.Windows.Forms.Button btnHundsun;
        private System.Windows.Forms.TextBox textBoxExchange;
        private System.Windows.Forms.Button btnCheckExchange;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.DataGridView dataGridViewMM;
        private System.ComponentModel.BackgroundWorker bgWorkerMain;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnResume;
        private System.Windows.Forms.Button btnPause;
        private System.Windows.Forms.Button btnStopMM;
        private System.Windows.Forms.Button btnRunMM;
        private System.Windows.Forms.CheckedListBox checkedListBoxUnderlying;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RichTextBox richTextBoxTradeMsg;
        private System.Windows.Forms.RichTextBox richTextBoxSystemMsg;
        private System.Windows.Forms.Button btnRefreshMsg;
    }
}

