namespace RPQ
{
    partial class Options
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
            this.btnRegistr = new System.Windows.Forms.Button();
            this.btnUnregistr = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnRegistr
            // 
            this.btnRegistr.Location = new System.Drawing.Point(28, 21);
            this.btnRegistr.Name = "btnRegistr";
            this.btnRegistr.Size = new System.Drawing.Size(75, 23);
            this.btnRegistr.TabIndex = 0;
            this.btnRegistr.Text = "registr";
            this.btnRegistr.UseVisualStyleBackColor = true;
            this.btnRegistr.Click += new System.EventHandler(this.btnRegistr_Click);
            // 
            // btnUnregistr
            // 
            this.btnUnregistr.Location = new System.Drawing.Point(28, 73);
            this.btnUnregistr.Name = "btnUnregistr";
            this.btnUnregistr.Size = new System.Drawing.Size(75, 23);
            this.btnUnregistr.TabIndex = 1;
            this.btnUnregistr.Text = "unregistr";
            this.btnUnregistr.UseVisualStyleBackColor = true;
            this.btnUnregistr.Click += new System.EventHandler(this.btnUnregistr_Click);
            // 
            // Options
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(219, 156);
            this.Controls.Add(this.btnUnregistr);
            this.Controls.Add(this.btnRegistr);
            this.Name = "Options";
            this.Text = "Options";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnRegistr;
        private System.Windows.Forms.Button btnUnregistr;
    }
}