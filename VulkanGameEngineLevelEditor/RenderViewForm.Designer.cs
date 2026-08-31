using VulkanGameEngineLevelEditor.EditorEnhancements;
using WeifenLuo.WinFormsUI.Docking;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using ListView = System.Windows.Forms.ListView;

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
            menuStrip1 = new MenuStrip();
            dockPanel1 = new DockPanel();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new Size(24, 24);
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(1898, 24);
            menuStrip1.TabIndex = 3;
            menuStrip1.Text = "menuStrip1";
            // 
            // dockPanel1
            // 
            dockPanel1.Dock = DockStyle.Fill;
            dockPanel1.DockBackColor = Color.FromArgb(45, 45, 48);
            dockPanel1.Location = new Point(0, 24);
            dockPanel1.Name = "dockPanel1";
            dockPanel1.Padding = new Padding(6);
            dockPanel1.ShowAutoHideContentOnHover = false;
            dockPanel1.Size = new Size(1898, 1000);
            dockPanel1.TabIndex = 4;
            dockPanel1.Theme = new VS2015DarkTheme();
            // 
            // RenderViewForm
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1898, 1024);
            Controls.Add(dockPanel1);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Name = "RenderViewForm";
            Text = "Form1";
            Load += RenderViewForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private ImageList imageList1;
        private MenuStrip menuStrip1;
        private WeifenLuo.WinFormsUI.Docking.DockPanel dockPanel1;
    }
}
