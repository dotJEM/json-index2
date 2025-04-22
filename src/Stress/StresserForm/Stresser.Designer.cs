namespace StresserForm
{
    partial class Stresser
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.ctrlStartButton = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.ctrlLinesCounter = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // ctrlStartButton
            // 
            this.ctrlStartButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ctrlStartButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ctrlStartButton.Location = new System.Drawing.Point(787, 12);
            this.ctrlStartButton.Name = "ctrlStartButton";
            this.ctrlStartButton.Size = new System.Drawing.Size(145, 27);
            this.ctrlStartButton.TabIndex = 0;
            this.ctrlStartButton.Text = "Run";
            this.ctrlStartButton.UseVisualStyleBackColor = true;
            this.ctrlStartButton.Click += new System.EventHandler(this.ctrlStartButton_Click_1);
            // 
            // textBox1
            // 
            this.textBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox1.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox1.Location = new System.Drawing.Point(0, 0);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(944, 540);
            this.textBox1.TabIndex = 1;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.ctrlLinesCounter);
            this.splitContainer1.Panel1.Controls.Add(this.ctrlStartButton);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.textBox1);
            this.splitContainer1.Size = new System.Drawing.Size(944, 623);
            this.splitContainer1.SplitterDistance = 79;
            this.splitContainer1.TabIndex = 2;
            // 
            // ctrlLinesCounter
            // 
            this.ctrlLinesCounter.AutoSize = true;
            this.ctrlLinesCounter.Location = new System.Drawing.Point(5, 61);
            this.ctrlLinesCounter.Name = "ctrlLinesCounter";
            this.ctrlLinesCounter.Size = new System.Drawing.Size(35, 13);
            this.ctrlLinesCounter.TabIndex = 1;
            this.ctrlLinesCounter.Text = "label1";
            // 
            // Stresser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(944, 623);
            this.Controls.Add(this.splitContainer1);
            this.DoubleBuffered = true;
            this.Name = "Stresser";
            this.Text = "Stresser";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Button ctrlStartButton;
        private TextBox textBox1;
        private SplitContainer splitContainer1;
        private Label ctrlLinesCounter;
    }
}
