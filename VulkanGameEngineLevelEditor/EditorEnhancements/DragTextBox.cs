using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VulkanGameEngineLevelEditor.EditorEnhancements
{
    public class DragTextBox : UserControl
    {
        public double Value { get; set; }
        public double Min { get; set; } = -1000;
        public double Max { get; set; } = 1000;
        public double Step { get; set; } = 1;

        private TextBox textBox;
        private bool isDragging = false;
        private Label label1;
        private TextBox textBox1;
        private Point startPoint;

        public event EventHandler<double> ValueChanged;

        public DragTextBox()
        {
            textBox = new TextBox
            {
                Text = "0",
                Width = 80,
                Height = 30,
                Font = new Font("Arial", 12)
            };
            this.Height = 30;
            this.Controls.Add(textBox);

            textBox.MouseDown += TextBox_MouseDown;
            textBox.MouseMove += TextBox_MouseMove;
            textBox.MouseUp += TextBox_MouseUp;
            textBox.KeyDown += TextBox_KeyDown;
            textBox.LostFocus += TextBox_LostFocus;

            UpdateTextBox();
        }

        private void TextBox_MouseDown(object sender, MouseEventArgs e)
        {
            isDragging = true;
            startPoint = e.Location;
            if (double.TryParse(textBox.Text, out double currentVal))
            {
                Value = currentVal;
            }
        }

        private void TextBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                int deltaX = e.X - startPoint.X;
                double deltaValue = deltaX * 0.1;
                double newValue = Value + deltaValue;

                newValue = Math.Max(Min, Math.Min(Max, newValue));
                Value = newValue;
                UpdateTextBox();

                startPoint = e.Location; 

                ValueChanged?.Invoke(this, Value);
            }
        }

        private void TextBox_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return)
            {
                ParseAndUpdate();
                e.Handled = true;
            }
        }

        private void TextBox_LostFocus(object sender, EventArgs e)
        {
            ParseAndUpdate();
        }

        private void ParseAndUpdate()
        {
            if (double.TryParse(textBox.Text, out double val))
            {
                val = Math.Max(Min, Math.Min(Max, val));
                Value = val;
                UpdateTextBox();

                ValueChanged?.Invoke(this, Value);
            }
            else
            {
                UpdateTextBox();
            }
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            // 
            // DragTextBox
            // 
            Name = "DragTextBox";
            Size = new Size(204, 48);
            ResumeLayout(false);
        }

        private void UpdateTextBox()
        {
            textBox.Text = Value.ToString("0.##");
        }

    }
}
