using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using static VulkanGameEngineLevelEditor.RenderViewForm;

namespace VulkanGameEngineLevelEditor.EditorEnhancements
{
    public class ListViewWindow : ToolsWindow
    {
        public ListViewWindow(string title)
        {
            Text = title;
            InitializeComponent();
        }

        private ListView _listView = new ListView();
        private ImageList _imageList = new ImageList();

        private void InitializeComponent()
        {
            _imageList.ImageSize = new Size(128, 128);
            _imageList.ColorDepth = ColorDepth.Depth32Bit;
            _listView.View = View.LargeIcon;
            _listView.HideSelection = false;
            _listView.BackColor = System.Drawing.Color.FromArgb(40, 40, 40);
            _listView.Dock = DockStyle.Fill;
            _listView.ForeColor = System.Drawing.Color.White;
            _listView.LargeImageList = _imageList;
            _listView.Location = new System.Drawing.Point(3, 3);
            _listView.Name = Text;
            _listView.Size = new System.Drawing.Size(1884, 226);
            _listView.TabIndex = 1;
            _listView.UseCompatibleStateImageBehavior = false;
            _listView.ItemDrag += ItemDrag;
            Controls.Add(_listView);
        }

        private void ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (e.Item is ListViewItem item && item.Tag is DragAssetData asset)
            {
                _listView.DoDragDrop(asset, DragDropEffects.Copy);
            }
        }

        public void AddListItem(string name, AssetDataTypeEnum assetType, string jsonPath, Image icon = null)
        {
            icon ??= SystemIcons.Application.ToBitmap();
            string key = name;
            if (!_imageList.Images.ContainsKey(key))
                _imageList.Images.Add(key, icon);

            _listView.Items.Add(new ListViewItem(name)
            {
                ImageKey = key,
                Tag = new DragAssetData { AssetType = assetType, JsonPath = jsonPath, Name = name }
            });
        }
    }
}
