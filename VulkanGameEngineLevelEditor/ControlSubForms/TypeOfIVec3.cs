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
    public unsafe class TypeOfIVec3 : PropertyEditorForm
    {
        private const int RowHeight = 32;

        private readonly DynamicComponentWrapper? _wrapper;
        private readonly object? _targetObject;
        private readonly MemberInfo _member;
        private readonly IntPtr? _nativePtr;

        public TypeOfIVec3(ObjectPanelView rootPanel, object obj, MemberInfo member, int minimumPanelSize, bool readOnly, IntPtr? nativePtr = null) : base(rootPanel, obj, member, minimumPanelSize, readOnly)
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
                RowCount = 0,
                BackColor = Color.FromArgb(70, 70, 70),
                Padding = new Padding(4)
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 75F));

            ivec3 currentVec = GetCurrentVec3();
            void AddAxis(string label, Func<ivec3, float> getter, int axis)
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

                float currentValue = getter(currentVec);
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
                    SetAxis(axis, (int)num.Value);
                };

                num.TextChanged += (s, e) =>
                {
                    if (_readOnly) return;
                    if (float.TryParse(num.Text, out float parsed))
                    {
                        SetAxis(axis, (int)parsed);
                    }
                };

                table.Controls.Add(num, 1, row);
            }

            AddAxis("X", v => v.x, 0);
            AddAxis("Y", v => v.y, 1);
            AddAxis("Z", v => v.z, 2);

            return table;
        }

        private ivec3 GetCurrentVec3()
        {
            if (_wrapper != null)
            {
                return _wrapper.GetMemberValue(_member) is ivec3 v ? v : ivec3.Zero;
            }

            return _member switch
            {
                FieldInfo fi when _targetObject != null => (ivec3?)fi.GetValue(_targetObject) ?? ivec3.Zero,
                PropertyInfo pi when pi.CanRead && _targetObject != null => (ivec3?)pi.GetValue(_targetObject) ?? ivec3.Zero,
                _ => ivec3.Zero
            };
        }

        private void SetAxis(int axis, int newValue)
        {
            if (_readOnly) return;

            ivec3 v = GetCurrentVec3();
            switch (axis)
            {
                case 0: v.x = newValue; break;
                case 1: v.y = newValue; break;
                case 2: v.z = newValue; break;
            }

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