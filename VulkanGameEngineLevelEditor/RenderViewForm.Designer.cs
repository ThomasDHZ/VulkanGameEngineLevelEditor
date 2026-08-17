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
            panel1 = new Panel();
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
            tabPage2 = new TabPage();
            GameObjectListView = new ListView();
            tabPage3 = new TabPage();
            MaterialListView = new ListView();
            tabPage4 = new TabPage();
            TextureListView = new ListView();
            tabPage5 = new TabPage();
            SceneListView = new ListView();
            tabPage6 = new TabPage();
            tabPage7 = new TabPage();
            dataGridView1 = new DataGridView();
            LightListView = new ListView();
            menuStrip1 = new MenuStrip();
            flowLayoutPanel1 = new FlowLayoutPanel();
            panel2 = new Panel();
            panel3 = new Panel();
            RenderBox = new PictureBox();
            panel1.SuspendLayout();
            tabControl1.SuspendLayout();
            tabPage2.SuspendLayout();
            tabPage3.SuspendLayout();
            tabPage4.SuspendLayout();
            tabPage5.SuspendLayout();
            tabPage7.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)RenderBox).BeginInit();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.Controls.Add(tabControl1);
            panel1.Dock = DockStyle.Bottom;
            panel1.Location = new Point(0, 738);
            panel1.Name = "panel1";
            panel1.Size = new Size(1898, 286);
            panel1.TabIndex = 2;
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Controls.Add(tabPage3);
            tabControl1.Controls.Add(tabPage4);
            tabControl1.Controls.Add(tabPage5);
            tabControl1.Controls.Add(tabPage6);
            tabControl1.Controls.Add(tabPage7);
            tabControl1.Dock = DockStyle.Bottom;
            tabControl1.Location = new Point(0, 0);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(1898, 286);
            tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            tabPage1.BackColor = Color.FromArgb(40, 40, 40);
            tabPage1.BorderStyle = BorderStyle.Fixed3D;
            tabPage1.Location = new Point(4, 34);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(1890, 248);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Vulkan Logger";
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(GameObjectListView);
            tabPage2.Location = new Point(4, 34);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(1890, 248);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "GameObjects";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // GameObjectListView
            // 
            GameObjectListView.BackColor = Color.FromArgb(40, 40, 40);
            GameObjectListView.Dock = DockStyle.Fill;
            GameObjectListView.ForeColor = Color.White;
            GameObjectListView.Location = new Point(3, 3);
            GameObjectListView.Name = "GameObjectListView";
            GameObjectListView.Size = new Size(1884, 242);
            GameObjectListView.TabIndex = 0;
            GameObjectListView.UseCompatibleStateImageBehavior = false;
            // 
            // tabPage3
            // 
            tabPage3.BackColor = Color.FromArgb(60, 60, 60);
            tabPage3.Controls.Add(MaterialListView);
            tabPage3.Location = new Point(4, 34);
            tabPage3.Name = "tabPage3";
            tabPage3.Padding = new Padding(3);
            tabPage3.Size = new Size(1890, 248);
            tabPage3.TabIndex = 2;
            tabPage3.Text = "Materials";
            tabPage3.UseVisualStyleBackColor = true;
            // 
            // MaterialListView
            // 
            MaterialListView.BackColor = Color.FromArgb(40, 40, 40);
            MaterialListView.Dock = DockStyle.Fill;
            MaterialListView.ForeColor = SystemColors.Window;
            MaterialListView.Location = new Point(3, 3);
            MaterialListView.Name = "MaterialListView";
            MaterialListView.Size = new Size(1884, 242);
            MaterialListView.TabIndex = 0;
            MaterialListView.UseCompatibleStateImageBehavior = false;
            // 
            // tabPage4
            // 
            tabPage4.Controls.Add(TextureListView);
            tabPage4.Location = new Point(4, 34);
            tabPage4.Name = "tabPage4";
            tabPage4.Padding = new Padding(3);
            tabPage4.Size = new Size(1890, 248);
            tabPage4.TabIndex = 3;
            tabPage4.Text = "Textures";
            tabPage4.UseVisualStyleBackColor = true;
            // 
            // TextureListView
            // 
            TextureListView.BackColor = Color.FromArgb(40, 40, 40);
            TextureListView.Dock = DockStyle.Fill;
            TextureListView.Location = new Point(3, 3);
            TextureListView.Name = "TextureListView";
            TextureListView.Size = new Size(1884, 242);
            TextureListView.TabIndex = 0;
            TextureListView.UseCompatibleStateImageBehavior = false;
            // 
            // tabPage5
            // 
            tabPage5.Controls.Add(SceneListView);
            tabPage5.Location = new Point(4, 34);
            tabPage5.Name = "tabPage5";
            tabPage5.Padding = new Padding(3);
            tabPage5.Size = new Size(1890, 248);
            tabPage5.TabIndex = 4;
            tabPage5.Text = "Scenes";
            tabPage5.UseVisualStyleBackColor = true;
            // 
            // SceneListView
            // 
            SceneListView.BackColor = Color.FromArgb(40, 40, 40);
            SceneListView.Dock = DockStyle.Fill;
            SceneListView.ForeColor = Color.White;
            SceneListView.Location = new Point(3, 3);
            SceneListView.Name = "SceneListView";
            SceneListView.Size = new Size(1884, 242);
            SceneListView.TabIndex = 1;
            SceneListView.UseCompatibleStateImageBehavior = false;
            // 
            // tabPage6
            // 
            tabPage6.Location = new Point(4, 34);
            tabPage6.Name = "tabPage6";
            tabPage6.Padding = new Padding(3);
            tabPage6.Size = new Size(1890, 248);
            tabPage6.TabIndex = 5;
            tabPage6.Text = "Lights";
            tabPage6.UseVisualStyleBackColor = true;
            // 
            // tabPage7
            // 
            tabPage7.Controls.Add(dataGridView1);
            tabPage7.Controls.Add(LightListView);
            tabPage7.Location = new Point(4, 34);
            tabPage7.Name = "tabPage7";
            tabPage7.Padding = new Padding(3);
            tabPage7.Size = new Size(1890, 248);
            tabPage7.TabIndex = 5;
            tabPage7.Text = "DLL View";
            tabPage7.UseVisualStyleBackColor = true;
            // 
            // dataGridView1
            // 
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Dock = DockStyle.Fill;
            dataGridView1.Location = new Point(3, 3);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.RowHeadersWidth = 62;
            dataGridView1.Size = new Size(1884, 242);
            dataGridView1.TabIndex = 2;
            // 
            // LightListView
            // 
            LightListView.BackColor = Color.FromArgb(40, 40, 40);
            LightListView.Dock = DockStyle.Fill;
            LightListView.ForeColor = Color.White;
            LightListView.Location = new Point(3, 3);
            LightListView.Name = "LightListView";
            LightListView.Size = new Size(1884, 242);
            LightListView.TabIndex = 1;
            LightListView.UseCompatibleStateImageBehavior = false;
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new Size(24, 24);
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(1898, 32);
            menuStrip1.TabIndex = 3;
            menuStrip1.Text = "menuStrip1";
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Dock = DockStyle.Left;
            flowLayoutPanel1.Location = new Point(0, 32);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(300, 706);
            flowLayoutPanel1.TabIndex = 4;
            // 
            // panel2
            // 
            panel2.Dock = DockStyle.Right;
            panel2.Location = new Point(1598, 32);
            panel2.Name = "panel2";
            panel2.Size = new Size(300, 706);
            panel2.TabIndex = 5;
            // 
            // panel3
            // 
            panel3.Controls.Add(RenderBox);
            panel3.Dock = DockStyle.Fill;
            panel3.Location = new Point(300, 32);
            panel3.Name = "panel3";
            panel3.Size = new Size(1298, 706);
            panel3.TabIndex = 6;
            // 
            // RenderBox
            // 
            RenderBox.Dock = DockStyle.Fill;
            RenderBox.Location = new Point(0, 0);
            RenderBox.Name = "RenderBox";
            RenderBox.Size = new Size(1298, 706);
            RenderBox.TabIndex = 0;
            RenderBox.TabStop = false;
            // 
            // RenderViewForm
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1898, 1024);
            Controls.Add(panel3);
            Controls.Add(panel2);
            Controls.Add(flowLayoutPanel1);
            Controls.Add(panel1);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Name = "RenderViewForm";
            Text = "Form1";
            Load += RenderViewForm_Load;
            panel1.ResumeLayout(false);
            tabControl1.ResumeLayout(false);
            tabPage2.ResumeLayout(false);
            tabPage3.ResumeLayout(false);
            tabPage4.ResumeLayout(false);
            tabPage5.ResumeLayout(false);
            tabPage7.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            panel3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)RenderBox).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private System.Windows.Forms.RichTextBox VulkanLoggerBox;
        private Panel panel1;
        private TabControl tabControl1;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private TabPage tabPage3;
        private TabPage tabPage4;
        private TabPage tabPage5;
        private TabPage tabPage6;
        private TabPage tabPage7;
        private ImageList imageList1;
        private System.Windows.Forms.ListView GameObjectListView;
        private System.Windows.Forms.ListView MaterialListView;
        private System.Windows.Forms.ListView TextureListView;
        private System.Windows.Forms.ListView SceneListView;
        private System.Windows.Forms.ListView LightListView;
        private MenuStrip menuStrip1;
        private DataGridView dataGridView1;
        private FlowLayoutPanel flowLayoutPanel1;
        private Panel panel2;
        private Panel panel3;
        private PictureBox RenderBox;
    }
}
