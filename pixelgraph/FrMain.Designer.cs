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
            this.pictureBox = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();

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
            // btnClock
            // 
            this.btnClock = new System.Windows.Forms.Button();
            this.btnClock.Location = new System.Drawing.Point(10, 565);
            this.btnClock.Name = "btnClock";
            this.btnClock.Size = new System.Drawing.Size(100, 30);
            this.btnClock.TabIndex = 0;
            this.btnClock.ForeColor = System.Drawing.Color.FromArgb(204, 204, 204);
            this.btnClock.BackColor = System.Drawing.Color.FromArgb(51, 51, 51);
            this.btnClock.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClock.Text = "Clock";
            this.btnClock.Click += new System.EventHandler(this.BtnClock_Click);
            this.Controls.Add(this.btnClock);
            ///
            /// PictureBox
            /// 
            this.pictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox.BackColor = System.Drawing.Color.Blue;
            this.pictureBox.Location = new System.Drawing.Point(10, 10);
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.Size = new System.Drawing.Size(640, 510);
            this.pictureBox.TabIndex = 0;
            this.pictureBox.TabStop = false;
            //this.pictureBox.SizeChanged += new System.EventHandler(this.pictureBox1_SizeChanged);
            //this.pictureBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseDown);
            // 
            // Form controls collection
            // 
            this.Controls.AddRange(new System.Windows.Forms.Control[] {
                this.btnClock,
                this.pictureBox
            });
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        // Control declarations
        private System.Windows.Forms.Button btnClock;
        private System.Windows.Forms.PictureBox pictureBox;
    }
}