using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Drawing;
using System.Globalization;

namespace PVChat.WPF.Helpers
{
    public class TextBlockHyperLinkHelper
    {
        
        // Copied from http://geekswithblogs.net/casualjim/archive/2005/12/01/61722.aspx
        private static readonly Regex RE_URL = new Regex(@"\b(?:https?://|www\.)\S+\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static readonly DependencyProperty TextProperty = DependencyProperty.RegisterAttached(
            "Text",
            typeof(string),
            typeof(TextBlockHyperLinkHelper),
            new PropertyMetadata(null, OnTextChanged)
        );

        public static string GetText(DependencyObject d)
        { return d.GetValue(TextProperty) as string; }

        public static void SetText(DependencyObject d, string value)
        { d.SetValue(TextProperty, value); }

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var text_block = d as RichTextBox;
            if (text_block == null)
                return;

            Paragraph para = new Paragraph();
            para.Inlines.Clear();

            var new_text = (string)e.NewValue;
            if (string.IsNullOrEmpty(new_text))
            {
                text_block.Document.PageWidth = 10;
                return;
            }
           

            // Find all URLs using a regular expression
            int last_pos = 0;
            foreach (Match match in RE_URL.Matches(new_text))
            {
                // Copy raw string from the last position up to the match
                if (match.Index != last_pos)
                {
                    var raw_text = new_text.Substring(last_pos, match.Index - last_pos);
                    para.Inlines.Add(raw_text);
                }

                //Create a hyperlink for the match
                try
                    {
                        var link = new Hyperlink(new Run(match.Value))
                        {
                            NavigateUri = new Uri(match.Value)
                        };
                        link.IsEnabled = true;

                        link.Click += OnUrlClick;
                        para.Inlines.Add(link);
                    }
                    catch (Exception)
                    {
                        para.Inlines.Add(match.Value.ToString());
                    }

                // Update the last matched position
                last_pos = match.Index + match.Length;
            }

            // Finally, copy the remainder of the string
            if (last_pos < new_text.Length)
                para.Inlines.Add(new_text.Substring(last_pos));

            text_block.Document.Blocks.Clear();
            text_block.Document.Blocks.Add(para);

            var text = StringFromRichTextBox(text_block);


            var size = MeasureString(text, text_block);

            text_block.Document.PageWidth = size.Width;
            if (text_block.Document.PageWidth > 500)
            {
                text_block.Document.PageWidth = 500;
            }

        }
        private static string StringFromRichTextBox(RichTextBox rtb)
        {
            TextRange textRange = new TextRange(
                // TextPointer to the start of content in the RichTextBox.
                rtb.Document.ContentStart,
                // TextPointer to the end of content in the RichTextBox.
                rtb.Document.ContentEnd
            );

            // The Text property on a TextRange object returns a string
            // representing the plain text content of the TextRange.
            return textRange.Text;
        }
        private static System.Windows.Size MeasureString(string candidate, RichTextBox text_block)
        {
            var formattedText = new FormattedText(
                candidate,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(text_block.FontFamily, text_block.FontStyle, text_block.FontWeight, text_block.FontStretch),
                text_block.FontSize,
                System.Windows.Media.Brushes.Black,
                new NumberSubstitution(),
                TextFormattingMode.Display
                );

            return new System.Windows.Size(formattedText.Width + 20, formattedText.Height);
        }
        private static void OnUrlClick(object sender, RoutedEventArgs e)
        {
            var link = (Hyperlink)sender;
            // Do something with link.NavigateUri like:
            Process.Start(link.NavigateUri.ToString());
        }
    }
}
    
