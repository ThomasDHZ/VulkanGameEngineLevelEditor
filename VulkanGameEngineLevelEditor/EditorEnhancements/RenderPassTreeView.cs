using System;
using System.Windows.Forms;
using VulkanCS;
using VulkanGameEngineLevelEditor.EditorEnhancements;
using VulkanGameEngineLevelEditor.LevelEditor; // or wherever PropertiesPanel lives

namespace VulkanGameEngineLevelEditor.LevelEditor
{
    public enum RenderPassTreeNodeKind
    {
        Root,
        Settings,
        AttachmentsRoot,
        Attachment,
        SubpassesRoot,
        Subpass,
        DependenciesRoot,
        Dependency
    }

    public sealed class RenderPassAsset
    {
        public string Name { get; set; } = "";
        public Guid RenderPassId { get; set; }
        public int SubPassCount { get; set; }
        public bool UseDefaultRenderResolution { get; set; }
        public bool UseCubeMapMultiView { get; set; }
        public bool IsCubeMapRenderPass { get; set; }
        public int SampleCount { get; set; } = 1;
        public List<RenderAttachment> Attachments { get; set; } = new();
        public List<SubpassInfo> Subpasses { get; set; } = new();
        public List<SubpassDependency> Dependencies { get; set; } = new();
        public List<VkClearValue> ClearValues { get; set; } = new();
    }

    public sealed class RenderAttachment
    {
        public Guid RenderedTextureId { get; set; }
        public string DisplayName { get; set; } = "";
        public int Format { get; set; }
        public int LoadOp { get; set; }
        public int StoreOp { get; set; }
        public int FinalLayout { get; set; }
        public int MipMapCount { get; set; } = 1;
        public bool UseSampler { get; set; }
    }

    public sealed class SubpassInfo
    {
        public string Label { get; set; } = "";
        public List<int> ColorAttachments { get; set; } = new();
        public int? DepthStencilAttachment { get; set; }
        public List<int> InputAttachments { get; set; } = new();
    }

    public sealed class SubpassDependency
    {
        public uint SrcSubpass { get; set; }   // 0xFFFFFFFF = EXTERNAL
        public uint DstSubpass { get; set; }
        public int SrcStageMask { get; set; }
        public int DstStageMask { get; set; }
        public int SrcAccessMask { get; set; }
        public int DstAccessMask { get; set; }
        public int DependencyFlags { get; set; }
    }

    public sealed class RenderPassTreeNodeTag
    {
        public RenderPassTreeNodeKind Kind { get; init; }
        public RenderPassAsset Asset { get; init; }
        public int Index { get; init; } = -1;
    }

    public class RenderPassTreeView : TreeView
    {
        public PropertiesPanel PropertiesPanel { get; set; }

        public RenderPassTreeView()
        {
            AfterSelect += OnAfterSelect;
        }

        public void Populate(RenderPassAsset pass)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<RenderPassAsset>(Populate), pass);
                return;
            }

            BeginUpdate();
            Nodes.Clear();
            if (pass == null)
            {
                EndUpdate();
                return;
            }

            var root = new TreeNode($"RenderPass: {pass.Name}")
            {
                Tag = new RenderPassTreeNodeTag
                {
                    Kind = RenderPassTreeNodeKind.Root,
                    Asset = pass
                }
            };

            // Settings
            var settings = new TreeNode("Settings")
            {
                Tag = new RenderPassTreeNodeTag
                {
                    Kind = RenderPassTreeNodeKind.Settings,
                    Asset = pass
                }
            };
            root.Nodes.Add(settings);

            // Attachments
            var attsRoot = new TreeNode($"Attachments ({pass.Attachments.Count})")
            {
                Tag = new RenderPassTreeNodeTag
                {
                    Kind = RenderPassTreeNodeKind.AttachmentsRoot,
                    Asset = pass
                }
            };
            for (int i = 0; i < pass.Attachments.Count; i++)
            {
                var a = pass.Attachments[i];
                string title = string.IsNullOrWhiteSpace(a.DisplayName)
                    ? $"[{i}] format={a.Format}"
                    : $"[{i}] {a.DisplayName}";

                attsRoot.Nodes.Add(new TreeNode(title)
                {
                    Tag = new RenderPassTreeNodeTag
                    {
                        Kind = RenderPassTreeNodeKind.Attachment,
                        Asset = pass,
                        Index = i
                    }
                });
            }
            root.Nodes.Add(attsRoot);

            // Subpasses
            var subsRoot = new TreeNode($"Subpasses ({pass.Subpasses.Count})")
            {
                Tag = new RenderPassTreeNodeTag
                {
                    Kind = RenderPassTreeNodeKind.SubpassesRoot,
                    Asset = pass
                }
            };
            for (int i = 0; i < pass.Subpasses.Count; i++)
            {
                var s = pass.Subpasses[i];
                string label = string.IsNullOrWhiteSpace(s.Label)
                    ? $"Subpass {i}"
                    : $"Subpass {i} – {s.Label}";

                subsRoot.Nodes.Add(new TreeNode(label)
                {
                    Tag = new RenderPassTreeNodeTag
                    {
                        Kind = RenderPassTreeNodeKind.Subpass,
                        Asset = pass,
                        Index = i
                    }
                });
            }
            root.Nodes.Add(subsRoot);

            // Dependencies
            var depsRoot = new TreeNode($"Dependencies ({pass.Dependencies.Count})")
            {
                Tag = new RenderPassTreeNodeTag
                {
                    Kind = RenderPassTreeNodeKind.DependenciesRoot,
                    Asset = pass
                }
            };
            for (int i = 0; i < pass.Dependencies.Count; i++)
            {
                var d = pass.Dependencies[i];
                string src = d.SrcSubpass == 0xFFFFFFFFu ? "EXTERNAL" : d.SrcSubpass.ToString();
                string dst = d.DstSubpass == 0xFFFFFFFFu ? "EXTERNAL" : d.DstSubpass.ToString();

                depsRoot.Nodes.Add(new TreeNode($"{src} → {dst}")
                {
                    Tag = new RenderPassTreeNodeTag
                    {
                        Kind = RenderPassTreeNodeKind.Dependency,
                        Asset = pass,
                        Index = i
                    }
                });
            }
            root.Nodes.Add(depsRoot);

            Nodes.Add(root);
            root.Expand();
            attsRoot.Expand();
            subsRoot.Expand();
            depsRoot.Expand();

            EndUpdate();
            if (Nodes.Count > 0)
                SelectedNode = Nodes[0];
        }

        private void OnAfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node?.Tag is not RenderPassTreeNodeTag tag)
                return;

          //  PropertiesPanel?.SetSelectedRenderPassNode(tag);
        }

        public void ClearTree()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(ClearTree));
                return;
            }
            Nodes.Clear();
        }
    }
}