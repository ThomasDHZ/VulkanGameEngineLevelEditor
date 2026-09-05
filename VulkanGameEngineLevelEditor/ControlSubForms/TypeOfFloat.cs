using GlmSharp;
using System;
using System.Drawing;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using VulkanGameEngineLevelEditor.Attributes;
using VulkanGameEngineLevelEditor.EditorEnhancements;
using VulkanGameEngineLevelEditor.LevelEditor;

namespace VulkanGameEngineLevelEditor.ControlSubForms
{
    public unsafe class TypeOfFloat : PropertyEditorForm
    {
        private readonly DynamicComponentWrapper? _wrapper;
        private readonly object? _targetObject;
        private readonly MemberInfo _member;
        private readonly IntPtr? _nativePtr;

        public TypeOfFloat(ObjectPanelView rootPanel, object obj, MemberInfo member, int minimumPanelSize, bool readOnly, IntPtr? nativePtr = null) : base(rootPanel, obj, member, minimumPanelSize, readOnly)
        {
            _member = member;
            _wrapper = obj as DynamicComponentWrapper;
            _targetObject = obj;
            _nativePtr = nativePtr;
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
                DecimalPlaces = memberLimits != null ? memberLimits.DecimalPlaces : 4,
                Increment = memberLimits != null ? Convert.ToDecimal(memberLimits.Increment) : 0.1m,
                Minimum = memberLimits != null ? Convert.ToDecimal(memberLimits.Minimum) : -10000000m,
                Maximum = memberLimits != null ? Convert.ToDecimal(memberLimits.Maximum) : 10000000m,
                Value = (decimal)Math.Clamp(currentValue, memberLimits?.Minimum ?? -10000000f, memberLimits?.Maximum ?? 10000000f),
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                ReadOnly = _readOnly,
                Enabled = !_readOnly
            };

            num.ValueChanged += (s, e) =>
            {
                if (_readOnly) return;
                SetFloat((float)num.Value);
            };

            num.TextChanged += (s, e) =>
            {
                if (_readOnly) return;
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

        private float GetCurrentFloat()
        {
            if (_wrapper != null)
            {
                return _wrapper.GetMemberValue(_member) is float v ? v : 0.0f;
            }

            return _member switch
            {
                FieldInfo fi when _targetObject != null => (float?)fi.GetValue(_targetObject) ?? 0.0f,
                PropertyInfo pi when pi.CanRead && _targetObject != null => (float?)pi.GetValue(_targetObject) ?? 0.0f,
                _ => 0.0f
            };
        }

        private void SetFloat(float newValue)
        {
            if (_readOnly) return;

            float v = GetCurrentFloat();
            v = newValue;

            if (_wrapper != null)
            {
                _wrapper.SetMemberValue(_member, v);
                return;
            }

            if (_member is PropertyInfo pi && pi.CanWrite && _targetObject != null) pi.SetValue(_targetObject, v);
            else if (_member is FieldInfo fi && _targetObject != null) fi.SetValue(_targetObject, v);
        }
    }
}