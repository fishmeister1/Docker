using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using System.Linq;

namespace Docker
{
    public partial class MainWindow : Window
    {
        private void ListButton_Click(object sender, RoutedEventArgs e)
        {
            // Properly format selected paragraphs as a bulleted list
            var selection = Editor.Selection;
            var doc = Editor.Document;
            var startPara = selection.Start.Paragraph;
            var endPara = selection.End.Paragraph;
            var blocksToList = new System.Collections.Generic.List<Paragraph>();

            if (!selection.IsEmpty && startPara != null && endPara != null)
            {
                bool inRange = false;
                foreach (var block in doc.Blocks.ToList())
                {
                    if (block == startPara) inRange = true;
                    if (inRange && block is Paragraph para)
                    {
                        blocksToList.Add(para);
                    }
                    if (block == endPara) break;
                }
            }
            else if (startPara != null)
            {
                blocksToList.Add(startPara);
            }

            if (blocksToList.Count > 0)
            {
                var list = new List() { MarkerStyle = TextMarkerStyle.Disc };
                foreach (var para in blocksToList)
                {
                    doc.Blocks.Remove(para);
                    list.ListItems.Add(new ListItem(para));
                }
                doc.Blocks.Add(list);
            }
            else
            {
                // No paragraphs found, insert empty list at end
                var list = new List() { MarkerStyle = TextMarkerStyle.Disc };
                list.ListItems.Add(new ListItem(new Paragraph()));
                doc.Blocks.Add(list);
            }
        }
        // Track intended formatting state
        private bool isBold = false;
        private bool isItalic = false;
        private bool isUnderline = false;

        public MainWindow()
        {
            InitializeComponent();
            LoadFontFamilies();
            SetDefaultFontAndSize();
            Editor.SelectionChanged += Editor_SelectionChanged;
            Editor.PreviewKeyDown += Editor_PreviewKeyDown;
            Editor.TextChanged += Editor_TextChanged;
            PageSizeCombo.SelectionChanged += PageSizeCombo_SelectionChanged;
            // Set default page size to A4
            PageSizeCombo.SelectedIndex = 0;
            PageSizeCombo_SelectionChanged(PageSizeCombo, null);
        }
        private void PageSizeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PageSizeCombo.SelectedItem is ComboBoxItem item && item.Tag is string tag)
            {
                var dims = tag.Split(',');
                if (dims.Length == 2 && int.TryParse(dims[0], out int w) && int.TryParse(dims[1], out int h))
                {
                    Editor.Width = w;
                    Editor.Height = h;
                }
            }
        }

        private void Editor_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Only apply formatting if selection is empty (caret position)
            if (Editor.Selection.IsEmpty)
            {
                var caret = Editor.CaretPosition.GetPositionAtOffset(-1, LogicalDirection.Backward);
                if (caret != null)
                {
                    var range = new TextRange(caret, Editor.CaretPosition);
                    range.ApplyPropertyValue(TextElement.FontWeightProperty, isBold ? FontWeights.Bold : FontWeights.Normal);
                    range.ApplyPropertyValue(TextElement.FontStyleProperty, isItalic ? FontStyles.Italic : FontStyles.Normal);
                    range.ApplyPropertyValue(Inline.TextDecorationsProperty, isUnderline ? TextDecorations.Underline : null);
                }
            }
        }

        private void Editor_SelectionChanged(object sender, RoutedEventArgs e)
        {
            BoldButton.IsChecked = isBold;
            ItalicButton.IsChecked = isItalic;
            UnderlineButton.IsChecked = isUnderline;
        }

        private void Editor_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // Apply intended formatting to caret position before typing
            var caret = Editor.CaretPosition;
            var range = new TextRange(caret, caret);
            range.ApplyPropertyValue(TextElement.FontWeightProperty, isBold ? FontWeights.Bold : FontWeights.Normal);
            range.ApplyPropertyValue(TextElement.FontStyleProperty, isItalic ? FontStyles.Italic : FontStyles.Normal);
            range.ApplyPropertyValue(Inline.TextDecorationsProperty, isUnderline ? TextDecorations.Underline : null);
        }

        private void SetDefaultFontAndSize()
        {
            // Set default font family to Calibri (body) and size to 12
            var calibri = Fonts.SystemFontFamilies.FirstOrDefault(f => f.Source.ToLower().Contains("calibri"));
            if (calibri != null)
            {
                Editor.Selection.ApplyPropertyValue(TextElement.FontFamilyProperty, calibri);
                var caret = Editor.CaretPosition;
                var range = new TextRange(caret, caret);
                range.ApplyPropertyValue(TextElement.FontFamilyProperty, calibri);
                // Set ComboBox selection
                for (int i = 0; i < FontFamilyCombo.Items.Count; i++)
                {
                    if ((FontFamilyCombo.Items[i] as ComboBoxItem)?.Content?.ToString().ToLower().Contains("calibri") == true)
                    {
                        FontFamilyCombo.SelectedIndex = i;
                        break;
                    }
                }
            }
            Editor.Selection.ApplyPropertyValue(TextElement.FontSizeProperty, 12.0);
            var caretSize = Editor.CaretPosition;
            var rangeSize = new TextRange(caretSize, caretSize);
            rangeSize.ApplyPropertyValue(TextElement.FontSizeProperty, 12.0);
            // Set ComboBox selection
            for (int i = 0; i < FontSizeCombo.Items.Count; i++)
            {
                if ((FontSizeCombo.Items[i] as ComboBoxItem)?.Content?.ToString() == "12")
                {
                    FontSizeCombo.SelectedIndex = i;
                    break;
                }
            }
        }

        private void LoadFontFamilies()
        {
            FontFamilyCombo.Items.Clear();
            foreach (var font in Fonts.SystemFontFamilies.OrderBy(f => f.Source))
            {
                var item = new ComboBoxItem { Content = font.Source };
                FontFamilyCombo.Items.Add(item);
            }
        }

        private void BoldButton_Checked(object sender, RoutedEventArgs e)
        {
            isBold = true;
            Editor.Focus();
            if (Editor.Selection.IsEmpty)
            {
                var caret = Editor.CaretPosition;
                var range = new TextRange(caret, caret);
                range.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
            }
            else
            {
                Editor.Selection.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
            }
        }

        private void BoldButton_Unchecked(object sender, RoutedEventArgs e)
        {
            isBold = false;
            Editor.Focus();
            if (Editor.Selection.IsEmpty)
            {
                var caret = Editor.CaretPosition;
                var range = new TextRange(caret, caret);
                range.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Normal);
            }
            else
            {
                Editor.Selection.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Normal);
            }
        }

        private void ItalicButton_Checked(object sender, RoutedEventArgs e)
        {
            isItalic = true;
            Editor.Focus();
            if (Editor.Selection.IsEmpty)
            {
                var caret = Editor.CaretPosition;
                var range = new TextRange(caret, caret);
                range.ApplyPropertyValue(TextElement.FontStyleProperty, FontStyles.Italic);
            }
            else
            {
                Editor.Selection.ApplyPropertyValue(TextElement.FontStyleProperty, FontStyles.Italic);
            }
        }

        private void ItalicButton_Unchecked(object sender, RoutedEventArgs e)
        {
            isItalic = false;
            Editor.Focus();
            if (Editor.Selection.IsEmpty)
            {
                var caret = Editor.CaretPosition;
                var range = new TextRange(caret, caret);
                range.ApplyPropertyValue(TextElement.FontStyleProperty, FontStyles.Normal);
            }
            else
            {
                Editor.Selection.ApplyPropertyValue(TextElement.FontStyleProperty, FontStyles.Normal);
            }
        }

        private void UnderlineButton_Checked(object sender, RoutedEventArgs e)
        {
            isUnderline = true;
            Editor.Focus();
            if (Editor.Selection.IsEmpty)
            {
                var caret = Editor.CaretPosition;
                var range = new TextRange(caret, caret);
                range.ApplyPropertyValue(Inline.TextDecorationsProperty, TextDecorations.Underline);
            }
            else
            {
                Editor.Selection.ApplyPropertyValue(Inline.TextDecorationsProperty, TextDecorations.Underline);
            }
        }

        private void UnderlineButton_Unchecked(object sender, RoutedEventArgs e)
        {
            isUnderline = false;
            Editor.Focus();
            if (Editor.Selection.IsEmpty)
            {
                var caret = Editor.CaretPosition;
                var range = new TextRange(caret, caret);
                range.ApplyPropertyValue(Inline.TextDecorationsProperty, null);
            }
            else
            {
                Editor.Selection.ApplyPropertyValue(Inline.TextDecorationsProperty, null);
            }
        }

        private void FontFamilyCombo_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (FontFamilyCombo.SelectedItem is ComboBoxItem item)
            {
                var fontFamily = item.Content.ToString();
                Editor.Focus();
                Editor.Selection.ApplyPropertyValue(TextElement.FontFamilyProperty, new System.Windows.Media.FontFamily(fontFamily));
                // Always update the current caret position formatting
                var caret = Editor.CaretPosition;
                var range = new TextRange(caret, caret);
                range.ApplyPropertyValue(TextElement.FontFamilyProperty, new System.Windows.Media.FontFamily(fontFamily));
            }
        }

        private void FontSizeCombo_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (FontSizeCombo.SelectedItem is ComboBoxItem item)
            {
                if (double.TryParse(item.Content.ToString(), out double size))
                {
                    Editor.Focus();
                    Editor.Selection.ApplyPropertyValue(TextElement.FontSizeProperty, size);
                    var caret = Editor.CaretPosition;
                    var range = new TextRange(caret, caret);
                    range.ApplyPropertyValue(TextElement.FontSizeProperty, size);
                }
            }
        }
    }
}
