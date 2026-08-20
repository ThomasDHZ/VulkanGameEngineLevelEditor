using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VulkanCS;

namespace VulkanGameEngineLevelEditor
{
    public static class MessageLogger
    {
        public static RichTextBox RichTextBox { get; set; }

        private static readonly object _lock = new object();

        public static void LogMessage(string message, VkDebugUtilsMessageSeverityFlagBitsEXT severity)
        {
            if (RichTextBox == null) return;
            if (RichTextBox.InvokeRequired)
            {
                RichTextBox.BeginInvoke(new Action<string, VkDebugUtilsMessageSeverityFlagBitsEXT>(AppendMessage), message, severity);
                return;
            }
            AppendMessage(message, severity);
        }

        private static void AppendMessage(string message, VkDebugUtilsMessageSeverityFlagBitsEXT severity)
        {
            if (RichTextBox == null) return;

            string prefix = severity switch
            {
                VkDebugUtilsMessageSeverityFlagBitsEXT.VK_DEBUG_UTILS_MESSAGE_SEVERITY_ERROR_BIT_EXT => "[ERROR] ",
                VkDebugUtilsMessageSeverityFlagBitsEXT.VK_DEBUG_UTILS_MESSAGE_SEVERITY_WARNING_BIT_EXT => "[WARN]  ",
                VkDebugUtilsMessageSeverityFlagBitsEXT.VK_DEBUG_UTILS_MESSAGE_SEVERITY_INFO_BIT_EXT => "[INFO]  ",
                VkDebugUtilsMessageSeverityFlagBitsEXT.VK_DEBUG_UTILS_MESSAGE_SEVERITY_VERBOSE_BIT_EXT => "[VERBOSE] ",
                _ => "[UNKNOWN] "
            };

            Color color = severity switch
            {
                VkDebugUtilsMessageSeverityFlagBitsEXT.VK_DEBUG_UTILS_MESSAGE_SEVERITY_ERROR_BIT_EXT => Color.Red,
                VkDebugUtilsMessageSeverityFlagBitsEXT.VK_DEBUG_UTILS_MESSAGE_SEVERITY_WARNING_BIT_EXT => Color.Orange,
                VkDebugUtilsMessageSeverityFlagBitsEXT.VK_DEBUG_UTILS_MESSAGE_SEVERITY_INFO_BIT_EXT => Color.LimeGreen,
                VkDebugUtilsMessageSeverityFlagBitsEXT.VK_DEBUG_UTILS_MESSAGE_SEVERITY_VERBOSE_BIT_EXT => Color.LightBlue,
                _ => Color.White
            };

            RichTextBox.SelectionStart = RichTextBox.TextLength;
            RichTextBox.SelectionLength = 0;

            RichTextBox.SelectionColor = color;
            RichTextBox.SelectionFont = new Font(RichTextBox.Font, FontStyle.Bold);
            RichTextBox.AppendText(prefix);

            RichTextBox.SelectionColor = Color.White;
            RichTextBox.SelectionFont = new Font(RichTextBox.Font, FontStyle.Regular);
            RichTextBox.AppendText(message.Trim() + Environment.NewLine);

            RichTextBox.ScrollToCaret();
        }

        public static void Clear()
        {
            if (RichTextBox == null) return;
            if (RichTextBox.InvokeRequired)
            {
                RichTextBox.BeginInvoke(new Action(Clear));
                return;
            }
            RichTextBox.Clear();
        }
    }
}
