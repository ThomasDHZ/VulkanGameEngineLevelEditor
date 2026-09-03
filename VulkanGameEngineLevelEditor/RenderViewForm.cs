
using GameScriptLibraryDLL.GameObjects;
using GlmSharp;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using VulkanCS;
using VulkanEngineCoreCS;
using VulkanEngineCoreCS.Models;
using VulkanEngineCoreCS.Vulkan;
using VulkanEngineCS;
using VulkanGameEngineLevelEditor.EditorEnhancements;
using VulkanGameEngineLevelEditor.LevelEditor;
using VulkanGameEngineLevelEditor.Model;
using WeifenLuo.WinFormsUI.Docking;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static VulkanEngineCoreCS.VulkanSystem;

namespace VulkanGameEngineLevelEditor
{
    using VkCommandBuffer = nint;
    public unsafe partial class RenderViewForm : Form
    {
        public enum AssetDataTypeEnum
        {
            kAssetTypeGameObject,
            kAssetTypeMaterial,
            kAssetTypeTexture,
            kAssetTypeScene,
        };

        public class DragAssetData
        {
            public string Name { get; set; }
            public AssetDataTypeEnum AssetType { get; set; }
            public System.String JsonPath { get; set; }
        };

        private volatile bool Running;
        private volatile bool IsResizing;
        private object LockObject = new object();
        private Thread RenderThread { get; set; }
        private GCHandle _callbackHandle;
        private ivec2 RenderResolutionSize = new ivec2(3840, 2160);
        private const int STD_OUTPUT_HANDLE = -11;
        private const int STD_ERROR_HANDLE = -12;

        private DockPanel _dockPanel;
        private ToolsWindow _renderPassTreeWindow;
        private ToolsWindow _levelEditorTreeWindow;
        private ToolsWindow _propertiesWindow;
        private ToolsWindow _loggerWindow;
        private ToolsWindow _dllViewWindow;
        private ViewPortWindow _viewportWindow;
        private ListViewWindow _gameObjectListView;
        private ListViewWindow _sceneListView;
        private ListViewWindow _materialsListView;
        private ListViewWindow _texturesListView;
        public RenderViewForm()
        {
#if DEBUG
            InitializeConsole();
#endif
            InitializeComponent();
            BuildToolWindows();

            MessageLogger.RichTextBox = VulkanLoggerBox;
            LogVulkanMessageDelegate callback = LogVulkanMessage;
            _callbackHandle = GCHandle.Alloc(callback);
            VulkanSystem.CreateLogMessageCallback(callback);

            LoadExports("VulkanEngineInterop.dll");
            string jsonContent = File.ReadAllText(@"C:\Users\DHZ\Documents\GitHub\VulkanGameEngine\Assets\RenderPass\GBufferRenderPass.json");
            RenderPassLoader awer = JsonConvert.DeserializeObject<RenderPassLoader>(jsonContent);

            this.Text = "Vulkan Level Editor - RenderPassEditorView";

            _viewportWindow.PropertiesPanel = propertiesPanel;
            _viewportWindow.TreeView = levelEditorTreeView;
            levelEditorTreeView.PropertiesPanel = propertiesPanel;
            renderPassTreeView.PropertiesPanel = propertiesPanel;
            renderPassTreeView.Populate(awer);
        }

        public static void LogVulkanMessage(string message, int severity)
        {
            Console.WriteLine(message);
            MessageLogger.LogMessage(message, (VkDebugUtilsMessageSeverityFlagBitsEXT)severity);
        }

        private static void InitializeConsole()
        {
            if (!AllocConsole()) return;
            try
            {
                IntPtr outHandle = GetStdHandle(STD_OUTPUT_HANDLE);
                IntPtr errHandle = GetStdHandle(STD_ERROR_HANDLE);

                var stdout = new System.IO.FileStream(outHandle, System.IO.FileAccess.Write, false);
                var stderr = new System.IO.FileStream(errHandle, System.IO.FileAccess.Write, false);

                var writerOut = new System.IO.StreamWriter(stdout) { AutoFlush = true };
                var writerErr = new System.IO.StreamWriter(stderr) { AutoFlush = true };

                Console.SetOut(writerOut);
                Console.SetError(writerErr);

                Console.WriteLine("=== Console successfully initialized ===");
                Console.WriteLine("Console output should now work from all threads.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to initialize console redirection:\n{ex.Message}");
            }
        }

        private void RenderViewForm_Load(object sender, EventArgs e)
        {
            StartRenderer();
        }

        public void StartRenderer()
        {
            Running = true;
            RenderThread = new System.Threading.Thread(RenderLoop)
            {
                IsBackground = true,
                Name = "VulkanLevelEditor"
            };
            RenderThread.Start();
        }

        private void RenderLoop()
        {
            this.Invoke(new Action(() =>
            {
                ivec2 windowSize = new ivec2(_viewportWindow.RenderBox.Width, _viewportWindow.RenderBox.Height);
                VulkanSystem.RendererSetUp(_viewportWindow.RenderBox.Handle.ToPointer(), windowSize, RenderResolutionSize);
                BufferSystem.SetUpVmaAllocator();
                MemoryPoolSystem.StartUp();
                CSharpScriptSystem.Initialize();

                CSharpScriptSystem.RegisterBehavior<Player>();
                CSharpScriptSystem.RegisterBehavior<PlayerShot>();
                CSharpScriptSystem.RegisterBehavior<GameEnemy>();
                CSharpScriptSystem.RegisterBehavior<GameScriptLibraryDLL.GameObjects.DirectionalLight>();
                CSharpScriptSystem.RegisterBehavior<GameScriptLibraryDLL.GameObjects.PointLight>();
                LevelSystem.LoadLevel("Levels/TestLevel.json");
                LevelSystem.LevelEditorRenderPass("Levels/TestLevel.json");
            }));
            levelEditorTreeView.PopulateWithGameObjects();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            double lastTime = 0.0;

            while (Running)
            {
                if (IsResizing)
                {
                    System.Threading.Thread.Sleep(10);
                    continue;
                }

                double currentTime = stopwatch.Elapsed.TotalSeconds;
                double deltaTime = currentTime - lastTime;
                lastTime = currentTime;
                lock (LockObject)
                {
                    GameObjectSystem.Update((float)deltaTime);
                    LevelSystem.Update((float)deltaTime);
                    CollisionSystem.Update();
                    SpriteSystem.Update((float)deltaTime);
                    MeshSystem.Update((float)deltaTime);
                    MemoryPoolSystem.Update();
                    this.Invoke(new Action(() =>
                    {
                        RenderSystem.Update(_viewportWindow.RenderBox.Handle.ToPointer(), (float)deltaTime);
                    }));
                    //InputSystem.Update((float)deltaTime);
                    //        //networkSystem.Update(deltaTime);

                    VkCommandBuffer commandBuffer = VulkanSystem.StartFrame();
                    if (commandBuffer != VulkanCSConst.VK_NULL_HANDLE)
                    {
                        List<RenderPassNode> renderNodes = new List<RenderPassNode>(LevelSystem.CreateDrawCommands(commandBuffer, (float)deltaTime));
                        RenderSystem.Draw(commandBuffer, renderNodes);
                        RenderSystem.PresentToSwapChain(commandBuffer, new Guid("de6ca646-874f-4986-9daf-912dbc3aa7d0"));
                    }
                    VulkanSystem.EndFrame(commandBuffer);
                }
            }

            // GameSystem.Destroy();
        }

        private void LoadExports(string dllPath)
        {
            var list = DLLSystem.ListDllExport(dllPath);

            dataGridView1.DataSource = list;
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.Columns.Clear();

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(NativeExport.Ordinal),
                HeaderText = "Ordinal",
                Width = 80
            });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(NativeExport.RvaHex),
                HeaderText = "Realtive Address",
                Width = 100
            });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(NativeExport.Name),
                HeaderText = "Interop DLL Function",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
        }

        private void BuildToolWindows()
        {
            _dockPanel = new DockPanel
            {
                Dock = DockStyle.Fill,
                Theme = new VS2015DarkTheme(),
                DocumentStyle = DocumentStyle.DockingWindow,
                DockBottomPortion = 0.22,
                DockLeftPortion = 0.16,
                DockRightPortion = 0.16
            };

            Controls.Add(_dockPanel);
            _dockPanel.BringToFront();

            _viewportWindow = new ViewPortWindow("Viewport");
            _renderPassTreeWindow = new ToolsWindow("Render Passes", renderPassTreeView);
            _levelEditorTreeWindow = new ToolsWindow("Level Editor Objects", levelEditorTreeView);
            _propertiesWindow = new ToolsWindow("Properties", propertiesPanel);
            _loggerWindow = new ToolsWindow("Vulkan Logger", VulkanLoggerBox);
            _dllViewWindow = new ToolsWindow("DLL View", dataGridView1);
            _gameObjectListView = new ListViewWindow("GameObjects");
            _sceneListView = new ListViewWindow("Scenes");
            _materialsListView = new ListViewWindow("Materials");
            _texturesListView = new ListViewWindow("Textures");

            _viewportWindow.Show(_dockPanel, DockState.Document);
            _renderPassTreeWindow.Show(_dockPanel, DockState.DockLeft);
            _levelEditorTreeWindow.Show(_dockPanel, DockState.DockLeft);
            _propertiesWindow.Show(_dockPanel, DockState.DockRight);

            _loggerWindow.Show(_dockPanel, DockState.DockBottom);
            _dllViewWindow.Show(_dockPanel, DockState.DockBottom);
            _gameObjectListView.Show(_dockPanel, DockState.DockBottom);
            _sceneListView.Show(_dockPanel, DockState.DockBottom);
            _materialsListView.Show(_dockPanel, DockState.DockBottom);
            _texturesListView.Show(_dockPanel, DockState.DockBottom);

            List<System.String> gameObjectPrefabList = Directory.GetFiles(@"C:\Users\DHZ\Documents\GitHub\VulkanGameEngine\Assets\GameObjects").ToList();
            foreach (var gameObjectPrefab in gameObjectPrefabList)
            {
                _gameObjectListView.AddListItem(gameObjectPrefab, AssetDataTypeEnum.kAssetTypeGameObject, gameObjectPrefab);
            }

            List<System.String> sceneLevelList = Directory.GetFiles(@"C:\Users\DHZ\Documents\GitHub\VulkanGameEngine\Assets\Levels").ToList();
            foreach (var sceneLevel in sceneLevelList)
            {
                _sceneListView.AddListItem(sceneLevel, AssetDataTypeEnum.kAssetTypeScene, sceneLevel);
            }

            List<System.String> textureList = Directory.GetFiles(@"C:\Users\DHZ\Documents\GitHub\VulkanGameEngine\Assets\Textures").ToList();
            foreach (var texture in textureList)
            {
                _texturesListView.AddListItem(texture, AssetDataTypeEnum.kAssetTypeTexture, texture);
            }

            List<System.String> materialList = Directory.GetFiles(@"C:\Users\DHZ\Documents\GitHub\VulkanGameEngine\Assets\Materials").ToList();
            foreach (var material in materialList)
            {
                _materialsListView.AddListItem(material, AssetDataTypeEnum.kAssetTypeMaterial, material);
            }
        }

        [DllImport("kernel32.dll", SetLastError = true)] private static extern bool AllocConsole();
        [DllImport("kernel32.dll", SetLastError = true)] private static extern IntPtr GetStdHandle(int nStdHandle);
        [DllImport("kernel32.dll", SetLastError = true)] private static extern bool SetStdHandle(int nStdHandle, IntPtr hHandle);
    }
}

