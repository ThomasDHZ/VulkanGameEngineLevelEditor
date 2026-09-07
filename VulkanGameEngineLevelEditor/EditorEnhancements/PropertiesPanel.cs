using GameScriptLibraryDLL.Components;
using GameScriptLibraryDLL.GameObjects;
using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
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
        private GameObjecLevelEditor* _selectedGameObject;
        private readonly Dictionary<ComponentTypeEnum, ObjectPanelView> _panelPool = new Dictionary<ComponentTypeEnum, ObjectPanelView>();
        private readonly FlowLayoutPanel _flowComponents;
        private readonly ToolTip _toolTip = new ToolTip();
        private uint _selectedId = uint.MaxValue;
        private List<ComponentTypeEnum> _currentComponentTypes = new();

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

            //_refreshTimer = new Timer();
            //_refreshTimer.Interval = 120;
            //_refreshTimer.Tick += RefreshTimer_Tick;
            //_refreshTimer.Start();
        }

        public void SetSelectedEntity(uint gameObjectId)
        {
            if (gameObjectId == uint.MaxValue)
            {
                _selectedId = uint.MaxValue;
                _selectedGameObject = null;
                _currentComponentTypes.Clear();
                RefreshPanel();
                return;
            }

            var go = GameObjectSystem.GetGameObject(gameObjectId);
            var types = GameObjectSystem.GetGameObjectComponentList(gameObjectId) ?? new List<ComponentTypeEnum>();

            bool sameEntity = _selectedId == gameObjectId;
            bool sameLayout = sameEntity && types.SequenceEqual(_currentComponentTypes);

            _selectedGameObject = go;
            _selectedId = gameObjectId;
            _currentComponentTypes = types.ToList();

            if (sameLayout)
            {
                RefreshAllPanels();
                return;
            }

            RefreshPanel();
        }

        private void RefreshPanel()
        {
            _flowComponents.Controls.Clear();

            if (_selectedGameObject == null || _selectedGameObject->GameObjectId == uint.MaxValue)
            {
                _flowComponents.Controls.Add(new Label
                {
                    Text = "No entity selected",
                    ForeColor = Color.Silver,
                    AutoSize = true,
                    Padding = new Padding(20)
                });
                return;
            }

            _flowComponents.Controls.Add(CreateEntityHeader());
            var componentTypes = GameObjectSystem.GetGameObjectComponentList(_selectedGameObject->GameObjectId);
            if (_selectedGameObject != null)
            {
                foreach (var componentType in componentTypes)
                {
                    IntPtr ptr = GameObjectSystem.GetGameObjectComponentPtr(_selectedGameObject->GameObjectId, componentType);
                    if (ptr == IntPtr.Zero) continue;

                    if (_panelPool.ContainsKey(componentType))
                    {
                        _flowComponents.Controls.Add(_panelPool[componentType]);
                    }
                    else
                    {
                        var view = ComponentViewRegistry.TryCreate(_selectedGameObject->GameObjectId, componentType, ptr);
                        var wrapper = new DynamicComponentWrapper(_selectedGameObject->GameObjectId, componentType, view);
                        _panelPool.Add(componentType, new ObjectPanelView(this, wrapper, _toolTip));
                        _flowComponents.Controls.Add(_panelPool[componentType]);
                    }
                }
            }
            _flowComponents.Controls.Add(CreateAddComponentButton());
        }

        private Control CreateEntityHeader()
        {
            var panel = new Panel
            {
                Height = 88,
                BackColor = Color.FromArgb(48, 48, 53),
                Padding = new Padding(12)
            };

            var lblName = new Label
            {
                Text = $"Entity: Entity_{_selectedGameObject->GameObjectId}",
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(8, 12)
            };

            var lblId = new Label
            {
                Text = $"ID: {_selectedGameObject->GameObjectId}",
                ForeColor = Color.Silver,
                AutoSize = true,
                Location = new Point(8, 38)
            };

            panel.Controls.Add(lblName);
            panel.Controls.Add(lblId);
            return panel;
        }

        private Button CreateAddComponentButton()
        {
            var btn = new Button
            {
                Text = "＋ Add Component",
                AutoSize = true,
                Margin = new Padding(8, 8, 8, 16),
                BackColor = Color.FromArgb(65, 140, 65),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Height = 38
            };

            btn.Click += (s, e) => ShowAddComponentDialog();
            return btn;
        }

        private void ShowAddComponentDialog()
        {
            MessageBox.Show("Add Component dialog - implement me using ComponentRegistry.GetAllComponentTypes()",
                            "Add Component", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public void RemoveComponent(object component)
        {
            if (component is DynamicComponentWrapper wrapper)
            {
                // TODO: Call your removal logic here
                // GameObjectSystem.RemoveComponent(_selectedGameObject->GameObjectId, wrapper.ComponentType);
            }

            RefreshPanel();
        }

        private void RefreshTimer_Tick(object? sender, EventArgs e)
        {
            RefreshAllPanels();
        }

        public void RefreshAllPanels()
        {
            if (_selectedGameObject == null) return;

            foreach (Control ctrl in _flowComponents.Controls)
            {
                if (ctrl is ObjectPanelView panelView)
                {
                    panelView.RefreshValues();
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //_refreshTimer?.Stop();
                //_refreshTimer?.Dispose();
            }
            base.Dispose(disposing);
        }

        public void RefreshLayout() => _flowComponents.PerformLayout();

        private void InitializeComponent() { }
    }
}