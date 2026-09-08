using GameScriptLibraryDLL.Components;
using GameScriptLibraryDLL.GameObjects;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using VulkanEngineCS;
using VulkanGameEngineLevelEditor.Component;
using VulkanGameEngineLevelEditor.LevelEditor;
using VulkanGameEngineLevelEditor.Model;
using VulkanGameEngineLevelEditor.Registries;

namespace VulkanGameEngineLevelEditor.EditorEnhancements
{
    public unsafe class PropertiesPanel : UserControl
    {
        private readonly Dictionary<ComponentTypeEnum, ObjectPanelView> _panelPool = new();
        private readonly List<ComponentTypeEnum> _currentComponentTypes = new();
        private readonly FlowLayoutPanel _flowComponents;
        private readonly ToolTip _toolTip = new();

        private readonly Panel _headerPanel;
        private readonly Label _headerName;
        private readonly Label _headerId;
        private readonly Label _emptyLabel;
        private readonly Button _addButton;

        private uint _selectedId = uint.MaxValue;

        public PropertiesPanel()
        {
            InitializeComponent();

            _flowComponents = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(40, 40, 40),
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(6)
            };
            Controls.Add(_flowComponents);

            typeof(Control)
                .GetProperty("DoubleBuffered",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(_flowComponents, true);

            _headerPanel = new Panel
            {
                Width = 280,
                Height = 88,
                BackColor = Color.FromArgb(48, 48, 53),
                Padding = new Padding(12),
                Margin = new Padding(6, 5, 6, 8)
            };
            _headerName = new Label
            {
                Text = "Entity: —",
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(8, 12)
            };
            _headerId = new Label
            {
                Text = "ID: —",
                ForeColor = Color.Silver,
                AutoSize = true,
                Location = new Point(8, 38)
            };
            _headerPanel.Controls.Add(_headerName);
            _headerPanel.Controls.Add(_headerId);

            _emptyLabel = new Label
            {
                Text = "No entity selected",
                ForeColor = Color.Silver,
                AutoSize = true,
                Padding = new Padding(20)
            };

            _addButton = new Button
            {
                Text = "＋ Add Component",
                AutoSize = true,
                Margin = new Padding(8, 8, 8, 16),
                BackColor = Color.FromArgb(65, 140, 65),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Height = 38
            };
            _addButton.Click += (_, _) => ShowAddComponentDialog();

            _flowComponents.Controls.Add(_emptyLabel);
            _flowComponents.Controls.Add(_headerPanel);
            _flowComponents.Controls.Add(_addButton);

            ShowEmpty(true);
        }

        public void SetSelectedEntity(uint gameObjectId)
        {
            if (gameObjectId == uint.MaxValue)
            {
                _selectedId = uint.MaxValue;
                _currentComponentTypes.Clear();
                HideAllComponentPanels();
                ShowEmpty(true);
                return;
            }

            var types = GameObjectSystem.GetGameObjectComponentList(gameObjectId)
                        ?? new List<ComponentTypeEnum>();

            bool sameEntity = _selectedId == gameObjectId;
            bool sameTypes = types.SequenceEqual(_currentComponentTypes);

            _selectedId = gameObjectId;
            _currentComponentTypes.Clear();
            _currentComponentTypes.AddRange(types);

            ShowEmpty(false);
            UpdateHeader(gameObjectId);

            if (sameEntity && sameTypes)
            {
                RefreshVisiblePanels();
                return;
            }

            _flowComponents.SuspendLayout();
            _flowComponents.Visible = false;
            try
            {
                foreach (var kv in _panelPool)
                    kv.Value.Visible = false;

                foreach (var type in types)
                {
                    IntPtr ptr = GameObjectSystem.GetGameObjectComponentPtr(gameObjectId, type);
                    if (ptr == IntPtr.Zero) continue;

                    var view = ComponentViewRegistry.TryCreate(gameObjectId, type, ptr);
                    var wrapper = new DynamicComponentWrapper(gameObjectId, type, view);

                    if (!_panelPool.TryGetValue(type, out var panel))
                    {
                        panel = new ObjectPanelView(this, wrapper, _toolTip);
                        _panelPool[type] = panel;
                        InsertBeforeAddButton(panel);
                    }
                    else
                    {
                        panel.Rebind(wrapper);
                    }

                    panel.Visible = true;
                    panel.RefreshValues();
                }

                EnsureAddButtonLast();
            }
            finally
            {
                _flowComponents.Visible = true;
                _flowComponents.ResumeLayout(true);
            }
        }

        public void RefreshAllPanels() => RefreshVisiblePanels();

        public void RefreshLayout() => _flowComponents.PerformLayout();

        public void RemoveComponent(object component)
        {
            if (component is DynamicComponentWrapper wrapper && _selectedId != uint.MaxValue)
            {
                // GameObjectSystem.RemoveComponent(_selectedId, wrapper.ComponentType);
            }

            if (_selectedId != uint.MaxValue)
                SetSelectedEntity(_selectedId);
        }

        private void RefreshVisiblePanels()
        {
            foreach (var type in _currentComponentTypes)
            {
                if (_panelPool.TryGetValue(type, out var panel) && panel.Visible)
                    panel.RefreshValues();
            }
        }

        private void HideAllComponentPanels()
        {
            foreach (var kv in _panelPool)
                kv.Value.Visible = false;
        }

        private void ShowEmpty(bool empty)
        {
            _emptyLabel.Visible = empty;
            _headerPanel.Visible = !empty;
            _addButton.Visible = !empty;
            if (empty)
                HideAllComponentPanels();
        }

        private void UpdateHeader(uint id)
        {
            _headerName.Text = $"Entity: Entity_{id}";
            _headerId.Text = $"ID: {id}";
        }

        private void InsertBeforeAddButton(Control panel)
        {
            _flowComponents.Controls.Add(panel);
            EnsureAddButtonLast();
        }

        private void EnsureAddButtonLast()
        {
            int last = _flowComponents.Controls.Count - 1;
            if (last >= 0)
                _flowComponents.Controls.SetChildIndex(_addButton, last);
        }

        private void ShowAddComponentDialog()
        {
            MessageBox.Show(
                "Add Component dialog - implement me using ComponentRegistry.GetAllComponentTypes()",
                "Add Component",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            // 
            // PropertiesPanel
            // 
            BackColor = Color.FromArgb(40, 40, 40);
            Name = "PropertiesPanel";
            ResumeLayout(false);
        }
    }
}