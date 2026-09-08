using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using VulkanGameEngineLevelEditor.Attributes;
using VulkanGameEngineLevelEditor.EditorEnhancements;
using VulkanGameEngineLevelEditor.LevelEditor;

namespace VulkanGameEngineLevelEditor.ControlSubForms
{
    public unsafe class TypeOfFloat : PropertyEditorForm
    {
        private readonly ObjectPanelView _rootPanel;
        private readonly MemberInfo _member;

        public TypeOfFloat(ObjectPanelView rootPanel, object obj, MemberInfo member,
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
                RowCount = 1,
                BackColor = Color.FromArgb(70, 70, 70),
                Padding = new Padding(4)
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 75F));

            float currentValue = GetCurrentFloat();
            var memberLimits = _member.GetCustomAttribute<NumericUpDownLimitsAttribute>();

            var num = new NumericUpDown
            {
                DecimalPlaces = memberLimits?.DecimalPlaces ?? 4,
                Increment = memberLimits != null ? Convert.ToDecimal(memberLimits.Increment) : 0.1m,
                Minimum = memberLimits != null ? Convert.ToDecimal(memberLimits.Minimum) : -10000000m,
                Maximum = memberLimits != null ? Convert.ToDecimal(memberLimits.Maximum) : 10000000m,
                Value = (decimal)Math.Clamp(currentValue,
                    memberLimits?.Minimum ?? -10000000f,
                    memberLimits?.Maximum ?? 10000000f),
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                ReadOnly = _readOnly,
                Enabled = !_readOnly
            };

            num.ValueChanged += (_, _) =>
            {
                if (_readOnly || !_rootPanel.ShouldWriteBack) return;
                SetFloat((float)num.Value);
            };

            num.TextChanged += (_, _) =>
            {
                if (_readOnly || !_rootPanel.ShouldWriteBack) return;
                if (float.TryParse(num.Text, out float parsed))
                    SetFloat(parsed);
            };

            var lbl = new Label
            {
                Text = "Value",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.White,
                Margin = new Padding(6, 0, 0, 0)
            };

            table.Controls.Add(lbl, 0, 0);
            table.Controls.Add(num, 1, 0);

            return table;
        }

        private DynamicComponentWrapper? CurrentWrapper =>
            _rootPanel.PanelObject as DynamicComponentWrapper;

        private float GetCurrentFloat()
        {
            var wrapper = CurrentWrapper;
            if (wrapper != null)
                return wrapper.GetMemberValue(_member) is float v ? v : 0f;
            return 0f;
        }

        private void SetFloat(float newValue)
        {
            if (_readOnly || !_rootPanel.ShouldWriteBack) return;
            CurrentWrapper?.SetMemberValue(_member, newValue);
        }
    }
}