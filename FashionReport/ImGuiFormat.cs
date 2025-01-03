using ImGuiNET;

namespace FashionReport
{
    internal static class IMGUIFORMAT
    {
        public static void TextCenteredColumn(string Text)
        {
            ImGui.SetCursorPosX(((ImGui.GetColumnWidth() - ImGui.CalcTextSize(Text).X) * 0.5f) + ImGui.GetColumnOffset());
            ImGui.Text(Text);
        }

        public static void TextCenteredWindow(string Text)
        {
            ImGui.SetCursorPosX(((ImGui.GetWindowWidth() - ImGui.CalcTextSize(Text).X) * 0.5f));
            ImGui.Text(Text);
        }
    }
}
