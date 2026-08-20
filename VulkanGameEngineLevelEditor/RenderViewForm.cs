
using GameScriptLibraryDLL.GameObjects;
using GlmSharp;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using VulkanCS;
using VulkanEngineCoreCS;
using VulkanEngineCoreCS.Models;
using VulkanEngineCoreCS.Vulkan;
using VulkanEngineCS;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static VulkanEngineCoreCS.VulkanSystem;

namespace VulkanGameEngineLevelEditor
{
    using VkCommandBuffer = nint;
    public unsafe partial class RenderViewForm : Form
    {
        private volatile bool Running;
        private volatile bool IsResizing;
        private object LockObject = new object();
        private Thread RenderThread { get; set; }
        private GCHandle _callbackHandle;
        private ivec2 RenderResolutionSize = new ivec2(3840, 2160);
        private const int STD_OUTPUT_HANDLE = -11;
        private const int STD_ERROR_HANDLE = -12;

        public RenderViewForm()
        {
#if DEBUG
            InitializeConsole();
#endif
            InitializeComponent();

            MessageLogger.RichTextBox = VulkanLoggerBox;
            LogVulkanMessageDelegate callback = LogVulkanMessage;
            _callbackHandle = GCHandle.Alloc(callback);
            VulkanSystem.CreateLogMessageCallback(callback);
         
            LoadExports("VulkanEngineInterop.dll");
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
                ivec2 windowSize = new ivec2(RenderBox.Width, RenderBox.Height);
                VulkanSystem.RendererSetUp(RenderBox.Handle.ToPointer(), windowSize, RenderResolutionSize);
                BufferSystem.SetUpVmaAllocator();
                MemoryPoolSystem.StartUp();
                CSharpScriptSystem.Initialize();

                CSharpScriptSystem.RegisterBehavior<Player>();
                CSharpScriptSystem.RegisterBehavior<PlayerShot>();
                CSharpScriptSystem.RegisterBehavior<GameEnemy>();
                CSharpScriptSystem.RegisterBehavior<GameScriptLibraryDLL.GameObjects.DirectionalLight>();
                CSharpScriptSystem.RegisterBehavior<GameScriptLibraryDLL.GameObjects.PointLight>();
                LevelSystem.LoadLevel("Levels/TestLevel.json");

            }));

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
                        RenderSystem.Update(RenderBox.Handle.ToPointer(), (float)deltaTime);
                    }));
                   // InputSystem.Update((float)deltaTime);
                    //networkSystem.Update(deltaTime);

                    VkCommandBuffer commandBuffer = VulkanSystem.StartFrame();
                    if (commandBuffer != VulkanCSConst.VK_NULL_HANDLE)
                    {
                        List<RenderPassNode> renderNodes = new List<RenderPassNode>(LevelSystem.CreateDrawCommands(commandBuffer, (float)deltaTime));
                        RenderSystem.Draw(commandBuffer, renderNodes);
                        LevelSystem.RenderFrameBuffer(commandBuffer, Guid.Empty);
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

        [DllImport("kernel32.dll", SetLastError = true)] private static extern bool AllocConsole();
        [DllImport("kernel32.dll", SetLastError = true)] private static extern IntPtr GetStdHandle(int nStdHandle);
        [DllImport("kernel32.dll", SetLastError = true)] private static extern bool SetStdHandle(int nStdHandle, IntPtr hHandle);

    }
}

