
using GameScriptLibraryDLL.Components;
using GameScriptLibraryDLL.GameObjects;
using GlmSharp;
using Microsoft.VisualBasic.ApplicationServices;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using VulkanCS;
using VulkanCS;
using VulkanEngineCoreCS;
using VulkanEngineCoreCS.Models;
using VulkanEngineCoreCS.Vulkan;
using VulkanEngineCS;
using VulkanGameEngineLevelEditor.Component;
using VulkanGameEngineLevelEditor.EditorEnhancements;
using VulkanGameEngineLevelEditor.LevelEditor;
using VulkanGameEngineLevelEditor.Model;
using VulkanGameEngineLevelEditor.Systems;
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
            public string JsonPath { get; set; }
        };

        private volatile bool Running;
        private volatile bool IsResizing;
        private volatile bool _shuttingDown;
        private readonly ManualResetEventSlim _renderStopped = new(false);

        private object LockObject = new object();
        private Thread RenderThread { get; set; }
        private ivec2 RenderResolutionSize = new ivec2(3840, 2160);
        private GCHandle _callbackHandle;
        private const int STD_ERROR_HANDLE = -12;
        private const int STD_OUTPUT_HANDLE = -11;

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
        private long _lastPropsRefreshTicks;
        private long PropsRefreshIntervalTicks = TimeSpan.FromMilliseconds(66).Ticks;
        private Guid _displayAttachment = new Guid("de6ca646-874f-4986-9daf-912dbc3aa7d0");
        private IntPtr _renderHwnd;

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

            var renderBox = _viewportWindow.RenderBox;
            _renderHwnd = renderBox.Handle;
 
            _viewportWindow.PropertiesPanel = propertiesPanel;
            _viewportWindow.TreeView = levelEditorTreeView;
            _viewportWindow.PendingWidth = renderBox.ClientSize.Width;
            _viewportWindow.PendingHeight = renderBox.ClientSize.Height;
            _viewportWindow.SizeDirty = false;
            
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
            if (!SetupRenderer())
                return;

            Running = true;
            _shuttingDown = false;
            _renderStopped.Reset();

            RenderThread = new Thread(RenderLoop)
            {
                IsBackground = true,
                Name = "VulkanLevelEditor"
            };
            RenderThread.Start();
        }

        private bool CanMarshalToUi()
        {
            return !_shuttingDown && !IsDisposed && IsHandleCreated && !IsResizing;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _shuttingDown = true;
            Running = false;
            if (!_renderStopped.Wait(TimeSpan.FromSeconds(5))) Debug.WriteLine("Render thread did not stop in time");
            base.OnFormClosing(e);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
        }

        protected override void OnResizeBegin(EventArgs e)
        {
            BeginResize();
            base.OnResizeBegin(e);
        }

        protected override void OnResizeEnd(EventArgs e)
        {
            EndResize();
            base.OnResizeEnd(e);
        }

        public void BeginResize()
        {
            IsResizing = true;
            lock (LockObject) { } 
        }

        public void EndResize()
        {
            IsResizing = false;
        }

        private bool SetupRenderer()
        {
            if (_shuttingDown || IsDisposed || !IsHandleCreated) return false;

            var renderBox = _viewportWindow.RenderBox;
            if (renderBox.Width <= 0 || renderBox.Height <= 0) return false;

            var windowSize = new ivec2(renderBox.Width, renderBox.Height);
            _renderHwnd = renderBox.Handle;
            VulkanSystem.RendererSetUp(_renderHwnd.ToPointer(), windowSize, RenderResolutionSize);
            BufferSystem.SetUpVmaAllocator();
            MemoryPoolSystem.StartUp();
            MaterialBakerSystem.BakeMaterial("C:\\Users\\DHZ\\Documents\\GitHub\\VulkanGameEngine\\Assets\\ImportMaterials\\AnimeGirlImportMaterial.json", "C:\\Users\\DHZ\\Documents\\GitHub\\VulkanGameEngine\\Assets");
            //CSharpScriptSystem.Initialize();
            //CSharpScriptSystem.RegisterBehavior<Player>();
            //CSharpScriptSystem.RegisterBehavior<PlayerShot>();
            //CSharpScriptSystem.RegisterBehavior<GameEnemy>();
            //CSharpScriptSystem.RegisterBehavior<GameScriptLibraryDLL.GameObjects.DirectionalLight>();
            //CSharpScriptSystem.RegisterBehavior<GameScriptLibraryDLL.GameObjects.PointLight>();
            //LevelSystem.LoadLevel("Levels/TestLevel.json");
            //LevelSystem.LevelEditorRenderPass("Levels/TestLevel.json");
            return true;
        }

        private void RenderLoop()
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                double lastTime = 0.0;

                while (Running && !_shuttingDown)
                {
                    if (IsResizing)
                    {
                        Thread.Sleep(10);
                        continue;
                    }

                    var vp = _viewportWindow;
                    int width = vp.PendingWidth;
                    int height = vp.PendingHeight;
                    bool sizeDirty = vp.SizeDirty;

                    if (_renderHwnd == IntPtr.Zero || width <= 0 || height <= 0)
                    {
                        Thread.Sleep(16);
                        continue;
                    }

                    double currentTime = stopwatch.Elapsed.TotalSeconds;
                    float deltaTime = (float)(currentTime - lastTime);
                    lastTime = currentTime;

                    lock (LockObject)
                    {
                        if (_shuttingDown) break;
                        if (IsResizing) continue;

                        width = vp.PendingWidth;
                        height = vp.PendingHeight;
                        sizeDirty = vp.SizeDirty;

                        if (width <= 0 || height <= 0) continue;
                        if (sizeDirty)
                        {
                            VulkanSystem.SetCustomFrameBufferSize(new ivec2(width, height));
                            vp.SizeDirty = false;
                        }

                        bool pick = false;
                        int pickX = 0, pickY = 0;
                        float moveX, moveY, camX, camY, zoom;
                        lock (vp.InputLock)
                        {
                            pick = vp.HasPickRequest;
                            pickX = vp.PickX;
                            pickY = vp.PickY;
                            vp.HasPickRequest = false;

                            moveX = vp.PendingMoveX;
                            moveY = vp.PendingMoveY;
                            camX = vp.PendingCamX;
                            camY = vp.PendingCamY;
                            zoom = vp.PendingZoom;
                            vp.PendingMoveX = vp.PendingMoveY = 0;
                            vp.PendingCamX = vp.PendingCamY = 0;
                            vp.PendingZoom = 0;
                        }

                        if (pick)
                        {
                            ivec2 texSize = RenderSystem.GetAttachmentSize(vp.ObjectSamplerTexture);
                            int x = (int)((long)pickX * texSize.x / Math.Max(1, width));
                            int y = (int)((long)pickY * texSize.y / Math.Max(1, height));
                            uint id = RenderSystem.SampleRenderPassPixel(vp.ObjectSamplerTexture, new ivec2(x, y));
                            vp.SelectedSpriteIndex = id;

                            if (CanMarshalToUi())
                            {
                                BeginInvoke(() =>
                                {
                                    if (_shuttingDown || IsDisposed) return;
                                    if (id != uint.MaxValue)
                                    {
                                        propertiesPanel.SetSelectedEntity(id);
                                        levelEditorTreeView.SelectGameObject(id);
                                    }
                                });
                            }
                        }

                        ref var camera = ref CameraSystem.UpdateActiveCamera();
                        if (zoom != 0) camera.Zoom += zoom;
                        if (camX != 0 || camY != 0) camera.Position = new vec3(camera.Position.x + camX, camera.Position.y + camY, 0);

                        uint selected = vp.SelectedSpriteIndex;
                        if (selected != uint.MaxValue && (moveX != 0 || moveY != 0))
                        {
                            float worldW = camera.Width;
                            float worldH = camera.Height;
                            float z = camera.Zoom != 0 ? camera.Zoom : 1f;
                            float worldDx = moveX * (worldW / Math.Max(1, width)) / z;
                            float worldDy = moveY * (worldH / Math.Max(1, height)) / z;

                            var components = GameObjectSystem.GetGameObjectComponentList(selected);
                            if (components.Contains(ComponentTypeEnum.kTransform2DComponent))
                            {
                                var transform = new Transform2DComponentView(selected);
                                transform.Position = new vec2(transform.Position.x + worldDx, transform.Position.y - worldDy);
                            }
                        }

                        if (selected != uint.MaxValue && vp.IsDragging)
                        {
                            long now = Stopwatch.GetTimestamp(); // or DateTime.UtcNow.Ticks
                            if (now - _lastPropsRefreshTicks >= PropsRefreshIntervalTicks)
                            {
                                _lastPropsRefreshTicks = now;
                                if (CanMarshalToUi())
                                {
                                    try
                                    {
                                        BeginInvoke(new Action(() =>
                                        {
                                            if (_shuttingDown || IsDisposed) return;
                                            propertiesPanel.RefreshAllPanels(); // or RefreshVisiblePanels()
                                        }));
                                    }
                                    catch (ObjectDisposedException) { }
                                    catch (InvalidOperationException) { }
                                }
                            }
                        }

                        GameObjectSystem.Update(deltaTime);
                        LevelSystem.Update(deltaTime);
                        CollisionSystem.Update();
                        SpriteSystem.Update(deltaTime);
                        MeshSystem.Update(deltaTime);
                        MemoryPoolSystem.Update();
                        RenderSystem.Update(_renderHwnd.ToPointer(), deltaTime);

                        var commandBuffer = VulkanSystem.StartFrame();
                        if (commandBuffer == VulkanCSConst.VK_NULL_HANDLE) continue;
                        try
                        {
                            var renderNodes = LevelSystem.CreateDrawCommands(commandBuffer, deltaTime);
                            RenderSystem.Draw(commandBuffer, renderNodes);
                            RenderSystem.PresentToSwapChain(commandBuffer, _displayAttachment);
                        }
                        finally
                        {
                            VulkanSystem.EndFrame(commandBuffer);
                        }
                    }
                }
            }
            finally
            {
                try
                {
                    VulkanSystem.DeviceWaitIdle();
                    RenderSystem.Destroy();
                    TextureSystem.Destroy();
                    MeshSystem.Destroy();
                    MaterialSystem.Destroy();
                    MemorySystem.ReportLeaks();
                    VulkanSystem.Destroy();
                }
                catch (Exception ex) { Debug.WriteLine(ex); }
                _renderStopped.Set();
            }
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
                string fileName = Path.GetFileName(gameObjectPrefab);
                _gameObjectListView.AddListItem(fileName, AssetDataTypeEnum.kAssetTypeGameObject, gameObjectPrefab);
            }

            List<System.String> sceneLevelList = Directory.GetFiles(@"C:\Users\DHZ\Documents\GitHub\VulkanGameEngine\Assets\Levels").ToList();
            foreach (var sceneLevel in sceneLevelList)
            {
                string fileName = Path.GetFileName(sceneLevel);
                _sceneListView.AddListItem(fileName, AssetDataTypeEnum.kAssetTypeScene, sceneLevel);
            }

            List<System.String> textureList = Directory.GetFiles(@"C:\Users\DHZ\Documents\GitHub\VulkanGameEngine\Assets\Textures").ToList();
            foreach (var texture in textureList)
            {
                string fileName = Path.GetFileName(texture);
                _texturesListView.AddListItem(fileName, AssetDataTypeEnum.kAssetTypeTexture, texture);
            }

            List<System.String> materialList = Directory.GetFiles(@"C:\Users\DHZ\Documents\GitHub\VulkanGameEngine\Assets\Materials").ToList();
            foreach (var material in materialList)
            {
                string fileName = Path.GetFileName(material);
                _materialsListView.AddListItem(fileName, AssetDataTypeEnum.kAssetTypeMaterial, material);
            }
        }

        [DllImport("kernel32.dll", SetLastError = true)] private static extern bool AllocConsole();
        [DllImport("kernel32.dll", SetLastError = true)] private static extern IntPtr GetStdHandle(int nStdHandle);
        [DllImport("kernel32.dll", SetLastError = true)] private static extern bool SetStdHandle(int nStdHandle, IntPtr hHandle);

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            List<GameObjecLevelEditor> gameObjectList = GameObjectSystem.GetGameObjectList();
            string jsonString = JsonConvert.SerializeObject(gameObjectList);
        }
    }
}

