namespace AD5933
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cbBoardList = new System.Windows.Forms.ComboBox();
            this.btnDisconnect = new System.Windows.Forms.Button();
            this.btnConnect = new System.Windows.Forms.Button();
            this.btnGetTemperature = new System.Windows.Forms.Button();
            this.btnCalibrate = new System.Windows.Forms.Button();
            this.btnSweep = new System.Windows.Forms.Button();

            this.btnStream = new System.Windows.Forms.Button();
            this.btnBackground = new System.Windows.Forms.Button();
            this.btnOneTest = new System.Windows.Forms.Button();
            this.btnPCali = new System.Windows.Forms.Button();

            this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
            this.mainGraph = new ZedGraph.ZedGraphControl();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.colePlot = new ZedGraph.ZedGraphControl();
            this.groupBox1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.cbBoardList);
            this.groupBox1.Controls.Add(this.btnDisconnect);
            this.groupBox1.Controls.Add(this.btnConnect);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(283, 79);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Boards";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(26, 27);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(43, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "Boards:";
            // 
            // cbBoardList
            // 
            this.cbBoardList.FormattingEnabled = true;
            this.cbBoardList.Location = new System.Drawing.Point(75, 24);
            this.cbBoardList.Name = "cbBoardList";
            this.cbBoardList.Size = new System.Drawing.Size(90, 21);
            this.cbBoardList.TabIndex = 7;
            this.cbBoardList.DropDown += new System.EventHandler(this.comboBox1_DropDown);
            // 
            // btnDisconnect
            // 
            this.btnDisconnect.Location = new System.Drawing.Point(202, 39);
            this.btnDisconnect.Name = "btnDisconnect";
            this.btnDisconnect.Size = new System.Drawing.Size(75, 23);
            this.btnDisconnect.TabIndex = 5;
            this.btnDisconnect.Text = "Disconnect";
            this.btnDisconnect.UseVisualStyleBackColor = true;
            this.btnDisconnect.Click += new System.EventHandler(this.btnDisconnect_Click);
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(202, 10);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(75, 23);
            this.btnConnect.TabIndex = 4;
            this.btnConnect.Text = "Connect";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // btnGetTemperature
            // 
            this.btnGetTemperature.Location = new System.Drawing.Point(320, 22);
            this.btnGetTemperature.Name = "btnGetTemperature";
            this.btnGetTemperature.Size = new System.Drawing.Size(60, 45);
            this.btnGetTemperature.TabIndex = 6;
            this.btnGetTemperature.Text = "Get Temperature";
            this.btnGetTemperature.UseVisualStyleBackColor = true;
            this.btnGetTemperature.Click += new System.EventHandler(this.btnGetTemperature_Click);
            // 
            // btnCalibrate
            // 
            this.btnCalibrate.Location = new System.Drawing.Point(380, 23);
            this.btnCalibrate.Name = "btnCalibrate";
            this.btnCalibrate.Size = new System.Drawing.Size(60, 45);
            this.btnCalibrate.TabIndex = 3;
            this.btnCalibrate.Text = "Calibrate";
            this.btnCalibrate.UseVisualStyleBackColor = true;
            this.btnCalibrate.Click += new System.EventHandler(this.btnCalibrate_Click);
            // 
            // btnSweep
            // 
            this.btnSweep.Location = new System.Drawing.Point(440, 23);
            this.btnSweep.Name = "btnSweep";
            this.btnSweep.Size = new System.Drawing.Size(60, 45);
            this.btnSweep.TabIndex = 7;
            this.btnSweep.Text = "Sweep";
            this.btnSweep.UseVisualStyleBackColor = true;
            this.btnSweep.Click += new System.EventHandler(this.btnSweep_Click);
            //
            // btnBackground
            //
            this.btnBackground.Location = new System.Drawing.Point(500, 23);
            this.btnBackground.Name = "btnBackground";
            this.btnBackground.Size = new System.Drawing.Size(60, 45);
            this.btnBackground.TabIndex = 7;
            this.btnBackground.Text = "GetBackground";
            this.btnBackground.UseVisualStyleBackColor = true;
            this.btnBackground.Click += new System.EventHandler(this.btnBackground_Click);
            //
            // btnPCali
            //
            this.btnPCali.Location = new System.Drawing.Point(560, 23);
            this.btnPCali.Name = "btnPCali";
            this.btnPCali.Size = new System.Drawing.Size(60, 45);
            this.btnPCali.TabIndex = 7;
            this.btnPCali.Text = "Python Calibration";
            this.btnPCali.UseVisualStyleBackColor = true;
            this.btnPCali.Click += new System.EventHandler(this.btnPCali_Click);
            //
            //btnOneTest
            //
            this.btnOneTest.Location = new System.Drawing.Point(620, 23);
            this.btnOneTest.Name = "btnOneTest";
            this.btnOneTest.Size = new System.Drawing.Size(60, 45);
            this.btnOneTest.TabIndex = 7;
            this.btnOneTest.Text = "OneTest";
            this.btnOneTest.UseVisualStyleBackColor = true;
            this.btnOneTest.Click += new System.EventHandler(this.btnOneTest_Click);
            //
            // btnStream
            //
            this.btnStream.Location = new System.Drawing.Point(680, 23);
            this.btnStream.Name = "btnStream";
            this.btnStream.Size = new System.Drawing.Size(60, 45);
            this.btnStream.TabIndex = 7;
            this.btnStream.Text = "DataStream";
            this.btnStream.UseVisualStyleBackColor = true;
            this.btnStream.Click += new System.EventHandler(this.btnStream_Click);//
            // propertyGrid1
            // 
            this.propertyGrid1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.propertyGrid1.Location = new System.Drawing.Point(12, 97);
            this.propertyGrid1.Name = "propertyGrid1";
            this.propertyGrid1.Size = new System.Drawing.Size(241, 313);
            this.propertyGrid1.TabIndex = 8;
            // 
            // mainGraph
            // 
            this.mainGraph.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.mainGraph.Location = new System.Drawing.Point(4, 6);
            this.mainGraph.Name = "mainGraph";
            this.mainGraph.ScrollGrace = 0D;
            this.mainGraph.ScrollMaxX = 0D;
            this.mainGraph.ScrollMaxY = 0D;
            this.mainGraph.ScrollMaxY2 = 0D;
            this.mainGraph.ScrollMinX = 0D;
            this.mainGraph.ScrollMinY = 0D;
            this.mainGraph.ScrollMinY2 = 0D;
            this.mainGraph.Size = new System.Drawing.Size(474, 275);
            this.mainGraph.TabIndex = 9;
            this.mainGraph.UseExtendedPrintDialog = true;
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(259, 97);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(492, 313);
            this.tabControl1.TabIndex = 10;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.mainGraph);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(484, 287);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Impedance";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.colePlot);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(484, 287);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Cole-Cole";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // colePlot
            // 
            this.colePlot.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.colePlot.Location = new System.Drawing.Point(7, 7);
            this.colePlot.Name = "colePlot";
            this.colePlot.ScrollGrace = 0D;
            this.colePlot.ScrollMaxX = 0D;
            this.colePlot.ScrollMaxY = 0D;
            this.colePlot.ScrollMaxY2 = 0D;
            this.colePlot.ScrollMinX = 0D;
            this.colePlot.ScrollMinY = 0D;
            this.colePlot.ScrollMinY2 = 0D;
            this.colePlot.Size = new System.Drawing.Size(471, 274);
            this.colePlot.TabIndex = 0;
            this.colePlot.UseExtendedPrintDialog = true;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(763, 422);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.propertyGrid1);
            this.Controls.Add(this.btnSweep);

            this.Controls.Add(this.btnStream);
            this.Controls.Add(this.btnBackground);
            this.Controls.Add(this.btnOneTest);
            this.Controls.Add(this.btnPCali);

            this.Controls.Add(this.btnGetTemperature);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnCalibrate);
            this.Name = "MainForm";
            this.Text = "AD5933 ";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnCalibrate;
        private System.Windows.Forms.Button btnDisconnect;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Button btnGetTemperature;
        private System.Windows.Forms.ComboBox cbBoardList;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnSweep;
        
        private System.Windows.Forms.Button btnStream;
        private System.Windows.Forms.Button btnBackground;
        private System.Windows.Forms.Button btnOneTest;
        private System.Windows.Forms.Button btnPCali;

        private System.Windows.Forms.PropertyGrid propertyGrid1;
        private ZedGraph.ZedGraphControl mainGraph;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private ZedGraph.ZedGraphControl colePlot;
    }
}

