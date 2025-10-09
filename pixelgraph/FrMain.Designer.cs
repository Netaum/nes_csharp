namespace pixelgraph
{
    partial class FrMain
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // FrMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.Name = "FrMain";
            this.Text = "Form1";
            this.BackColor = System.Drawing.Color.FromArgb(37, 37, 38);
            this.ForeColor = System.Drawing.Color.FromArgb(204, 204, 204);
            // 
            // rtxtMemPg1
            // 
            this.rtxtMemPg1 = new System.Windows.Forms.RichTextBox();
            this.rtxtMemPg1.Location = new System.Drawing.Point(5, 8);
            this.rtxtMemPg1.Name = "rtxtMemPg1";
            this.rtxtMemPg1.Size = new System.Drawing.Size(535, 275);
            this.rtxtMemPg1.TabIndex = 0;
            this.rtxtMemPg1.ForeColor = System.Drawing.Color.FromArgb(204, 204, 204);
            this.rtxtMemPg1.BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
            this.rtxtMemPg1.Text = "$0000: 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 ";
            this.Controls.Add(this.rtxtMemPg1);
            // 
            // rtxtMemPg2
            // 
            this.rtxtMemPg2 = new System.Windows.Forms.RichTextBox();
            this.rtxtMemPg2.Location = new System.Drawing.Point(5, 286);
            this.rtxtMemPg2.Name = "rtxtMemPg2";
            this.rtxtMemPg2.Size = new System.Drawing.Size(535, 275);
            this.rtxtMemPg2.TabIndex = 1;
            this.rtxtMemPg2.ForeColor = System.Drawing.Color.FromArgb(204, 204, 204);
            this.rtxtMemPg2.BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
            this.rtxtMemPg2.Text = "$0000: 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 ";
            this.Controls.Add(this.rtxtMemPg2);
            // 
            // button1
            // 
            this.button1 = new System.Windows.Forms.Button();
            this.button1.Location = new System.Drawing.Point(6, 565);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(100, 30);
            this.button1.TabIndex = 0;
            this.button1.ForeColor = System.Drawing.Color.FromArgb(204, 204, 204);
            this.button1.BackColor = System.Drawing.Color.FromArgb(51, 51, 51);
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Text = "Button";
            this.Controls.Add(this.button1);
            // 
            // rtxtCpu
            // 
            this.rtxtCpu = new System.Windows.Forms.RichTextBox();
            this.rtxtCpu.Location = new System.Drawing.Point(544, 8);
            this.rtxtCpu.Name = "rtxtCpu";
            this.rtxtCpu.Size = new System.Drawing.Size(248, 107);
            this.rtxtCpu.TabIndex = 0;
            this.rtxtCpu.ForeColor = System.Drawing.Color.FromArgb(204, 204, 204);
            this.rtxtCpu.BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
            this.rtxtCpu.Text = "";
            this.Controls.Add(this.rtxtCpu);
            // 
            // rtxtCode
            // 
            this.rtxtCode = new System.Windows.Forms.RichTextBox();
            this.rtxtCode.Location = new System.Drawing.Point(544, 117);
            this.rtxtCode.Name = "rtxtCode";
            this.rtxtCode.Size = new System.Drawing.Size(248, 444);
            this.rtxtCode.TabIndex = 0;
            this.rtxtCode.ForeColor = System.Drawing.Color.FromArgb(204, 204, 204);
            this.rtxtCode.BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
            this.rtxtCode.Text = "RichTextBox";
            this.Controls.Add(this.rtxtCode);
            // 
            // Form controls collection
            // 
            this.Controls.AddRange(new System.Windows.Forms.Control[] {
                this.rtxtMemPg1,
                this.rtxtMemPg2,
                this.button1,
                this.rtxtCpu,
                this.rtxtCode
            });
            this.ResumeLayout(false);
        }

        #endregion

        // Control declarations
        private System.Windows.Forms.RichTextBox rtxtMemPg1;
        private System.Windows.Forms.RichTextBox rtxtMemPg2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.RichTextBox rtxtCpu;
        private System.Windows.Forms.RichTextBox rtxtCode;
    }
}