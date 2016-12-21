namespace Acacia.Controls
{
    partial class KBusyIndicator
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this._layout = new System.Windows.Forms.TableLayoutPanel();
            this._text = new System.Windows.Forms.Label();
            this._progress = new System.Windows.Forms.ProgressBar();
            this._layout.SuspendLayout();
            this.SuspendLayout();
            // 
            // _layout
            // 
            this._layout.AutoSize = true;
            this._layout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._layout.ColumnCount = 1;
            this._layout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._layout.Controls.Add(this._text, 0, 0);
            this._layout.Controls.Add(this._progress, 0, 1);
            this._layout.Dock = System.Windows.Forms.DockStyle.Fill;
            this._layout.Location = new System.Drawing.Point(15, 15);
            this._layout.Margin = new System.Windows.Forms.Padding(15, 15, 15, 15);
            this._layout.Name = "_layout";
            this._layout.Padding = new System.Windows.Forms.Padding(9, 9, 9, 9);
            this._layout.RowCount = 2;
            this._layout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._layout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._layout.Size = new System.Drawing.Size(231, 115);
            this._layout.TabIndex = 0;
            // 
            // _text
            // 
            this._text.AutoSize = true;
            this._text.Dock = System.Windows.Forms.DockStyle.Fill;
            this._text.Location = new System.Drawing.Point(15, 9);
            this._text.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this._text.Name = "_text";
            this._text.Size = new System.Drawing.Size(201, 25);
            this._text.TabIndex = 0;
            this._text.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // _progress
            // 
            this._progress.Dock = System.Windows.Forms.DockStyle.Fill;
            this._progress.Location = new System.Drawing.Point(24, 49);
            this._progress.Margin = new System.Windows.Forms.Padding(15, 15, 15, 15);
            this._progress.MarqueeAnimationSpeed = 50;
            this._progress.Name = "_progress";
            this._progress.Size = new System.Drawing.Size(183, 42);
            this._progress.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this._progress.TabIndex = 1;
            // 
            // KBusyIndicator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this._layout);
            this.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.Name = "KBusyIndicator";
            this.Padding = new System.Windows.Forms.Padding(15, 15, 15, 15);
            this.Size = new System.Drawing.Size(261, 145);
            this._layout.ResumeLayout(false);
            this._layout.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel _layout;
        private System.Windows.Forms.Label _text;
        private System.Windows.Forms.ProgressBar _progress;
    }
}
