namespace SessionsActivity
{
    partial class Form1
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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.sessionsActivity = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.removeInactiveSessions = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.sessionsActivity)).BeginInit();
            this.SuspendLayout();
            // 
            // sessionsActivity
            // 
            this.sessionsActivity.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            chartArea1.AxisY.Maximum = 100D;
            chartArea1.Name = "ChartArea1";
            this.sessionsActivity.ChartAreas.Add(chartArea1);
            legend1.Name = "Legend1";
            this.sessionsActivity.Legends.Add(legend1);
            this.sessionsActivity.Location = new System.Drawing.Point(12, 35);
            this.sessionsActivity.Name = "sessionsActivity";
            series1.ChartArea = "ChartArea1";
            series1.Legend = "Legend1";
            series1.Name = "Sessions";
            this.sessionsActivity.Series.Add(series1);
            this.sessionsActivity.Size = new System.Drawing.Size(505, 339);
            this.sessionsActivity.TabIndex = 0;
            this.sessionsActivity.Text = "chart1";
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // removeInactiveSessions
            // 
            this.removeInactiveSessions.AutoSize = true;
            this.removeInactiveSessions.Location = new System.Drawing.Point(12, 12);
            this.removeInactiveSessions.Name = "removeInactiveSessions";
            this.removeInactiveSessions.Size = new System.Drawing.Size(152, 17);
            this.removeInactiveSessions.TabIndex = 1;
            this.removeInactiveSessions.Text = "Remove inactive sessions ";
            this.removeInactiveSessions.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(529, 386);
            this.Controls.Add(this.removeInactiveSessions);
            this.Controls.Add(this.sessionsActivity);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.sessionsActivity)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataVisualization.Charting.Chart sessionsActivity;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.CheckBox removeInactiveSessions;
    }
}

