namespace VulkanGameEngineLevelEditor
{
    partial class RenderViewForm
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
            RenderBox = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)RenderBox).BeginInit();
            SuspendLayout();
            // 
            // RenderBox
            // 
            RenderBox.Location = new Point(320, 0);
            RenderBox.Name = "RenderBox";
            RenderBox.Size = new Size(1280, 720);
            RenderBox.TabIndex = 0;
            RenderBox.TabStop = false;
            // 
            // RenderViewForm
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1898, 1024);
            Controls.Add(RenderBox);
            Name = "RenderViewForm";
            Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)RenderBox).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private PictureBox RenderBox;
    }
}
