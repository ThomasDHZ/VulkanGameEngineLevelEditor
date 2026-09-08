using GlmSharp;
using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using VulkanGameEngineLevelEditor.Attributes;
using VulkanGameEngineLevelEditor.EditorEnhancements;
using VulkanGameEngineLevelEditor.LevelEditor;

namespace VulkanGameEngineLevelEditor.ControlSubForms
{
    public unsafe class TypeOfIVec3 : PropertyEditorForm
    {
        private const int RowHeight = 32;

        private readonly ObjectPanelView _rootPanel;
        private readonly MemberInfo _member;

        public TypeOfIVec3(ObjectPanelView rootPanel, object obj, MemberInfo member,
                           int minimumPanelSize, bool readOnly, IntPtr? nativePtr = null)
            : base(rootPanel, obj, member, minimumPanelSize, readOnly)
        {
            _rootPanel = rootPanel;
            _member = member;
        }

        public override Control CreateControl()
        {
            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                ColumnCount = 2,
                RowCount = 0,
                BackColor = Color.FromArgb(70, 70, 70),
                Padding = new Padding(4)
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 75F));

            ivec3 currentVec = GetCurrentVec3();

            void AddAxis(string label, Func<ivec3, int> getter, int axis)
            {
                int row = table.RowCount++;
                table.RowStyles.Add(new RowStyle(SizeType.Absolute, RowHeight));

                var lbl = new Label
                {
                    Text = label,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft,
                    ForeColor = Color.White,
                    Margin = new Padding(6, 0, 0, 0)
                };
                table.Controls.Add(lbl, 0, row);

                int currentValue = getter(currentVec);
                var memberLimits = _member.GetCustomAttribute<NumericUpDownLimitsAttribute>();
                var num = new NumericUpDown
                {
                    DecimalPlaces = 0,
                    Increment = memberLimits != null ? Convert.ToDecimal(memberLimits.Increment) : 1m,
                    Minimum = memberLimits != null ? Convert.ToDecimal(memberLimits.Minimum) : -10000000m,
                    Maximum = memberLimits != null ? Convert.ToDecimal(memberLimits.Maximum) : 10000000m,
                    Value = Math.Clamp(currentValue,
                        (int)(memberLimits?.Minimum ?? -10000000),
                        (int)(memberLimits?.Maximum ?? 10000000)),
                    Dock = DockStyle.Fill,
                    BackColor = Color.FromArgb(60, 60, 60),
                    ForeColor = Color.White,
                    ReadOnly = _readOnly,
                    Enabled = !_readOnly
                };

                num.ValueChanged += (_, _) =>
                {
                    if (_readOnly || !_rootPanel.ShouldWriteBack) return;
                    SetAxis(axis, (int)num.Value);
                };

                num.TextChanged += (_, _) =>
                {
                    if (_readOnly || !_rootPanel.ShouldWriteBack) return;
                    if (int.TryParse(num.Text, out int parsed))
                        SetAxis(axis, parsed);
                };

                table.Controls.Add(num, 1, row);
            }

            AddAxis("X", v => v.x, 0);
            AddAxis("Y", v => v.y, 1);
            AddAxis("Z", v => v.z, 2);

            return table;
        }

        private DynamicComponentWrapper? CurrentWrapper =>
            _rootPanel.PanelObject as DynamicComponentWrapper;

        private ivec3 GetCurrentVec3()
        {
            var wrapper = CurrentWrapper;
            if (wrapper != null)
                return wrapper.GetMemberValue(_member) is ivec3 v ? v : ivec3.Zero;
            return ivec3.Zero;
        }

        private void SetAxis(int axis, int newValue)
        {
            if (_readOnly || !_rootPanel.ShouldWriteBack) return;

            ivec3 v = GetCurrentVec3();
            switch (axis)
            {
                case 0: v.x = newValue; break;
                case 1: v.y = newValue; break;
                case 2: v.z = newValue; break;
            }

            CurrentWrapper?.SetMemberValue(_member, v);
        }
    }
}