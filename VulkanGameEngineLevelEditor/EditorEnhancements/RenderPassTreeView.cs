using System;
using System.Windows.Forms;
using VulkanCS;
using VulkanGameEngineLevelEditor.EditorEnhancements;
using VulkanGameEngineLevelEditor.LevelEditor;
using VulkanGameEngineLevelEditor.Model; 

namespace VulkanGameEngineLevelEditor.LevelEditor
{
    public enum RenderPassTreeNodeKind
    {
        Root,
        Settings,
        AttachmentsRoot,
        Attachment,
        Pipeline,
        SubpassesRoot,
        Subpass,
        DependenciesRoot,
        Dependency
    }
    public sealed class RenderPassTreeNodeTag
    {
        public RenderPassTreeNodeKind Kind { get; init; }
        public object RenderPassObject { get; init; }
        public int Index { get; init; } = -1;
    }

    public class RenderPassTreeView : TreeView
    {
        public PropertiesPanel PropertiesPanel { get; set; }

        public RenderPassTreeView()
        {
            AfterSelect += OnAfterSelect;
        }

        public void Populate(RenderPassLoader pass)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<RenderPassLoader>(Populate), pass);
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
                    RenderPassObject = pass
                }
            };

            // Settings
            var settings = new TreeNode("Settings")
            {
                Tag = new RenderPassTreeNodeTag
                {
                    Kind = RenderPassTreeNodeKind.Settings,
                    RenderPassObject = pass
                }
            };
            root.Nodes.Add(settings);

            // Attachments
            
                var attsRoot = new TreeNode($"Attachments ({pass.AttachmentList.Count})")
                {
                    Tag = new RenderPassTreeNodeTag
                    {
                        Kind = RenderPassTreeNodeKind.AttachmentsRoot,
                        RenderPassObject = pass
                    }
                };
                { 
                int x = 0;
                foreach (var attachment in pass.AttachmentList)
                {
                    string title = string.IsNullOrWhiteSpace(attachment.AttachmentName) ? $"[{x}] format={attachment.TextureByteFormat}" : $"[{x}] {attachment.AttachmentName}";
                    attsRoot.Nodes.Add(new TreeNode(title)
                    {
                        Tag = new RenderPassTreeNodeTag
                        {
                            Kind = RenderPassTreeNodeKind.Attachment,
                            RenderPassObject = attachment,
                            Index = x
                        }
                    });
                }
                root.Nodes.Add(attsRoot);
            }

            // Pipelines
            var pipelineRoot = new TreeNode($"Pipeline ({pass.PipelinePackageList.Count})")
            {
                Tag = new RenderPassTreeNodeTag
                {
                    Kind = RenderPassTreeNodeKind.AttachmentsRoot,
                    RenderPassObject = pass
                }
            };
            {
                int x = 0;
                foreach (var pipeline in pass.PipelinePackageList)
                {
                    string title = string.IsNullOrWhiteSpace(pipeline.Name) ? $"[{x}] guid={pipeline.PipelinePackageId}" : $"[{x}] {pipeline.Name}";
                    attsRoot.Nodes.Add(new TreeNode(title)
                    {
                        Tag = new RenderPassTreeNodeTag
                        {
                            Kind = RenderPassTreeNodeKind.Pipeline,
                            RenderPassObject = pipeline,
                            Index = x
                        }
                    });
                }
                root.Nodes.Add(attsRoot);
            }


            // Subpasses

            var subsRoot = new TreeNode($"Subpasses ({pass.SubPassList.Count})")
                {
                    Tag = new RenderPassTreeNodeTag
                    {
                        Kind = RenderPassTreeNodeKind.SubpassesRoot,
                        RenderPassObject = pass
                    }
                };
            { 
                int x = 0;
                foreach (var subPass in pass.SubPassList)
                {
                    string label =  $"Subpass {x}";
                    subsRoot.Nodes.Add(new TreeNode(label)
                    {
                        Tag = new RenderPassTreeNodeTag
                        {
                            Kind = RenderPassTreeNodeKind.Subpass,
                            RenderPassObject = pass,
                            Index = x
                        }
                    });
                }
                root.Nodes.Add(subsRoot);
            }

            // Dependencies
            //var depsRoot = new TreeNode($"Dependencies ({pass.Dependencies.Count})")
            //{
            //    Tag = new RenderPassTreeNodeTag
            //    {
            //        Kind = RenderPassTreeNodeKind.DependenciesRoot,
            //        RenderPass = pass
            //    }
            //};
            //for (int i = 0; i < pass.Dependencies.Count; i++)
            //{
            //    var d = pass.Dependencies[i];
            //    string src = d.SrcSubpass == 0xFFFFFFFFu ? "EXTERNAL" : d.SrcSubpass.ToString();
            //    string dst = d.DstSubpass == 0xFFFFFFFFu ? "EXTERNAL" : d.DstSubpass.ToString();

            //    depsRoot.Nodes.Add(new TreeNode($"{src} → {dst}")
            //    {
            //        Tag = new RenderPassTreeNodeTag
            //        {
            //            Kind = RenderPassTreeNodeKind.Dependency,
            //            Asset = pass,
            //            Index = i
            //        }
            //    });
            //}
            //root.Nodes.Add(depsRoot);

            Nodes.Add(root);
            root.Expand();
            attsRoot.Expand();
            pipelineRoot.Expand();
            subsRoot.Expand();
           // depsRoot.Expand();

            EndUpdate();
            if (Nodes.Count > 0)
                SelectedNode = Nodes[0];
        }

        private void OnAfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node?.Tag is not RenderPassTreeNodeTag tag) return;
        //    PropertiesPanel.SetSelectedObject(tag.RenderPassObject);
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