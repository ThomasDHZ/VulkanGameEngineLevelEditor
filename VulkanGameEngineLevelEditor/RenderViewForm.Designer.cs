using GlmSharp;
using VulkanEngineCoreCS;
using VulkanGameEngineLevelEditor.EditorEnhancements;
using VulkanGameEngineLevelEditor.LevelEditor;
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RenderViewForm));
            menuStrip1 = new MenuStrip();
            dockPanel1 = new DockPanel();
            VulkanLoggerBox = new RichTextBox();
            GameObjectListView = new ListView();
            MaterialListView = new ListView();
            TextureListView = new ListView();
            SceneListView = new ListView();
            LightListView = new ListView();
            dataGridView1 = new DataGridView();
            renderPassTreeView = new RenderPassTreeView();
            levelEditorTreeView = new LevelEditorTreeView();
            propertiesPanel = new PropertiesPanel();
            toolStrip1 = new ToolStrip();
            toolStripButton1 = new ToolStripButton();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            toolStrip1.SuspendLayout();
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
            // 
            // VulkanLoggerBox
            // 
            VulkanLoggerBox.BackColor = Color.FromArgb(40, 40, 40);
            VulkanLoggerBox.Dock = DockStyle.Fill;
            VulkanLoggerBox.Location = new Point(3, 3);
            VulkanLoggerBox.Name = "VulkanLoggerBox";
            VulkanLoggerBox.Size = new Size(1880, 238);
            VulkanLoggerBox.TabIndex = 0;
            VulkanLoggerBox.Text = "";
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
            // renderPassTreeView
            // 
            renderPassTreeView.BackColor = Color.FromArgb(40, 40, 40);
            renderPassTreeView.Dock = DockStyle.Fill;
            renderPassTreeView.ForeColor = Color.FromArgb(255, 255, 255);
            renderPassTreeView.LineColor = Color.Empty;
            renderPassTreeView.Location = new Point(0, 0);
            renderPassTreeView.Name = "renderPassTreeView";
            renderPassTreeView.PropertiesPanel = null;
            renderPassTreeView.Size = new Size(300, 714);
            renderPassTreeView.TabIndex = 0;
            // 
            // levelEditorTreeView
            // 
            levelEditorTreeView.BackColor = Color.FromArgb(40, 40, 40);
            levelEditorTreeView.Dock = DockStyle.Fill;
            levelEditorTreeView.ForeColor = Color.FromArgb(255, 255, 255);
            levelEditorTreeView.LineColor = Color.Empty;
            levelEditorTreeView.Location = new Point(0, 0);
            levelEditorTreeView.Name = "levelEditorTreeView";
            levelEditorTreeView.PropertiesPanel = null;
            levelEditorTreeView.Size = new Size(300, 714);
            levelEditorTreeView.TabIndex = 0;
            // 
            // propertiesPanel
            // 
            propertiesPanel.Dock = DockStyle.Fill;
            propertiesPanel.Location = new Point(0, 0);
            propertiesPanel.Name = "propertiesPanel";
            propertiesPanel.Size = new Size(300, 714);
            propertiesPanel.TabIndex = 0;
            // 
            // toolStrip1
            // 
            toolStrip1.ImageScalingSize = new Size(24, 24);
            toolStrip1.Items.AddRange(new ToolStripItem[] { toolStripButton1 });
            toolStrip1.Location = new Point(0, 24);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(1898, 33);
            toolStrip1.TabIndex = 5;
            toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton1
            // 
            toolStripButton1.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButton1.Image = (Image)resources.GetObject("toolStripButton1.Image");
            toolStripButton1.ImageTransparentColor = Color.Magenta;
            toolStripButton1.Name = "toolStripButton1";
            toolStripButton1.Size = new Size(34, 28);
            toolStripButton1.Text = "toolStripButton1";
            toolStripButton1.Click += toolStripButton1_Click;
            // 
            // RenderViewForm
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1898, 1024);
            Controls.Add(toolStrip1);
            Controls.Add(dockPanel1);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Name = "RenderViewForm";
            Text = "Form1";
            Load += RenderViewForm_Load;
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private ImageList imageList1;
        private MenuStrip menuStrip1;
        private WeifenLuo.WinFormsUI.Docking.DockPanel dockPanel1;
        private System.Windows.Forms.ListView GameObjectListView;
        private System.Windows.Forms.ListView MaterialListView;
        private System.Windows.Forms.ListView TextureListView;
        private System.Windows.Forms.ListView SceneListView;
        private System.Windows.Forms.ListView LightListView;
        private LevelEditor.RenderPassTreeView renderPassTreeView;
        private LevelEditor.LevelEditorTreeView levelEditorTreeView;
        private PropertiesPanel propertiesPanel;
        private DataGridView dataGridView1;
        private RichTextBox VulkanLoggerBox;
        private ToolStrip toolStrip1;
        private ToolStripButton toolStripButton1;
    }
}
