﻿// Copyright (c) 2016 Quinn Damerell
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalMarkdown.Helpers;
using UniversalMarkdown.Interfaces;
using UniversalMarkdown.Parse;
using UniversalMarkdown.Parse.Elements;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace UniversalMarkdown.Display
{
    public class RenderToRichTextBlock
    {
        /// <summary>
        /// The rich text block we will dump the markdown into.
        /// </summary>
        RichTextBlock m_richTextBlock;

        /// <summary>
        /// A register class used to listen to hyper link clicks.
        /// </summary>
        ILinkRegister m_linkRegister;

        public RenderToRichTextBlock(RichTextBlock richTextBlock, ILinkRegister linkRegister)
        {
            m_richTextBlock = richTextBlock;
            m_linkRegister = linkRegister;
        }

        /// <summary>
        /// Called externally to render markdown to a text block.
        /// </summary>
        /// <param name="markdownTree"></param>
        public void Render(Markdown markdownTree)
        {
            // Clear anything that currently exists
            m_richTextBlock.Blocks.Clear();

            // For the root, loop through the block types and render them
            foreach (MarkdownBlock element in markdownTree.Blocks)
            {
                RenderBlock(element, m_richTextBlock.Blocks);
            }
        }

        /// <summary>
        /// Called to render a block element.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="currentBlocks"></param>
        private void RenderBlock(MarkdownBlock element, BlockCollection currentBlocks)
        {
            switch (element.Type)
            {
                case MarkdownBlockType.Paragraph:
                    RenderParagraph((ParagraphBlock)element, currentBlocks);
                    break;
                case MarkdownBlockType.Quote:
                    RenderQuote((QuoteBlock)element, currentBlocks);
                    break;
                case MarkdownBlockType.Code:
                    RenderCode((CodeBlock)element, currentBlocks);
                    break;
                case MarkdownBlockType.Header:
                    RenderHeader((HeaderBlock)element, currentBlocks);
                    break;
                case MarkdownBlockType.List:
                    RenderListElement((ListBlock)element, currentBlocks);
                    break;
                case MarkdownBlockType.HorizontalRule:
                    RenderHorizontalRule((HorizontalRuleBlock)element, currentBlocks);
                    break;
                case MarkdownBlockType.Table:
                    RenderTable((TableBlock)element, currentBlocks);
                    break;
            }
        }

        /// <summary>
        /// Called to render an inline element
        /// </summary>
        /// <param name="element"></param>
        /// <param name="currentInlines"></param>
        private void RenderInline(MarkdownInline element, InlineCollection currentInlines, ref bool trimTextStart)
        {
            switch(element.Type)
            {
                case MarkdownInlineType.TextRun:
                    RenderTextRun((TextRunInline)element, currentInlines, ref trimTextStart);
                    break;
                case MarkdownInlineType.Italic:
                    RenderItalicRun((ItalicTextInline)element, currentInlines, ref trimTextStart);
                    break;
                case MarkdownInlineType.Bold:
                    RenderBoldRun((BoldTextInline)element, currentInlines, ref trimTextStart);
                    break;
                case MarkdownInlineType.MarkdownLink:
                    RenderMarkdownLink((MarkdownLinkInline)element, currentInlines, ref trimTextStart);
                    break;
                case MarkdownInlineType.RawHyperlink:
                    RenderRawHyperlink((RawHyperlinkInline)element, currentInlines, ref trimTextStart);
                    break;
                case MarkdownInlineType.RawSubreddit:
                    RenderRawSubreddit((RawSubredditInline)element, currentInlines, ref trimTextStart);
                    break;
                case MarkdownInlineType.Strikethrough:
                    RenderStrikethroughRun((StrikethroughTextInline)element, currentInlines, ref trimTextStart);
                    break;
                case MarkdownInlineType.Superscript:
                    RenderSuperscriptRun((SuperscriptTextInline)element, currentInlines, ref trimTextStart);
                    break;
                case MarkdownInlineType.Code:
                    RenderCodeRun((CodeInline)element, currentInlines, ref trimTextStart);
                    break;
            }
        }

        /// <summary>
        /// Renders all of the children for the given element.
        /// </summary>
        /// <param name="inlineElements">The inline elements to render</param>
        /// <param name="currentInlines">The inlines where they should go</param>
        /// <param name="trimTextStart">If true the first text box start will be trimed so there is no leading space</param>
        private void RenderInlineChildren(IList<MarkdownInline> inlineElements, InlineCollection currentInlines, ref bool trimTextStart)
        {
            foreach (MarkdownInline element in inlineElements)
            {
                RenderInline(element, currentInlines, ref trimTextStart);
            }
        }

        #region Render Blocks

        /// <summary>
        /// Renders a paragraph element.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="currentBlocks"></param>
        private void RenderParagraph(ParagraphBlock element, BlockCollection currentBlocks)
        {
            // Make a new paragraph
            Paragraph paragraph = new Paragraph();

            // Add some padding to differentiate the blocks
            paragraph.Margin = new Thickness(0, 12, 0, 0);

            // Add it to the blocks
            currentBlocks.Add(paragraph);

            // Render the children into the para inline.
            bool trimTextStart = true;
            RenderInlineChildren(element.Inlines, paragraph.Inlines, ref trimTextStart);
        }

        /// <summary>
        /// Renders a header element.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="currentBlocks"></param>
        private void RenderHeader(HeaderBlock element, BlockCollection currentBlocks)
        {
            // Make the new header paragraph
            Paragraph headerPara = new Paragraph();
            headerPara.Margin = new Thickness(0, 18, 0, 12);

            // Set the style
            switch (element.HeaderLevel)
            {
                case 1:
                    headerPara.FontSize = 20;
                    headerPara.FontWeight = FontWeights.Bold;
                    break;
                case 2:
                    headerPara.FontSize = 20;
                    break;
                case 3:
                    headerPara.FontSize = 17;
                    headerPara.FontWeight = FontWeights.Bold;
                    break;
                case 4:
                    headerPara.FontSize = 17;
                    break;
                case 5:
                default:
                    headerPara.FontWeight = FontWeights.Bold;
                    break;
            }

            // Add it to the blocks
            currentBlocks.Add(headerPara);

            // Render the children into the para inline.
            bool trimTextStart = true;
            RenderInlineChildren(element.Inlines, headerPara.Inlines, ref trimTextStart);
        }

        /// <summary>
        /// Renders a list element.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="currentBlocks"></param>
        private void RenderListElement(ListBlock element, BlockCollection currentBlocks)
        {
            // Create a grid with two columns.
            Grid grid = new Grid();
            grid.Margin = new Thickness(0, 0, 0, 5);

            // The first column for the bullet (or number) and the second for the text.
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(40) });
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

            for (int rowIndex = 0; rowIndex < element.Items.Count; rowIndex ++)
            {
                var listItem = element.Items[rowIndex];

                // Add a row definition.
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                // Add the bullet or number.
                var bullet = new TextBlock();
                switch (element.Style)
                {
                    case ListStyle.Bulleted:
                        bullet.Text = "•";
                        break;
                    case ListStyle.Numbered:
                        bullet.Text = $"{rowIndex + 1}.";
                        break;
                }
                bullet.HorizontalAlignment = HorizontalAlignment.Right;
                bullet.Margin = new Thickness(0, 5, 12, 0);
                Grid.SetRow(bullet, rowIndex);
                grid.Children.Add(bullet);

                // Add the list item content.
                var content = new RichTextBlock();
                content.Margin = new Thickness(0, 5, 0, 0);
                foreach (MarkdownBlock childBlock in listItem.Blocks)
                {
                    RenderBlock(childBlock, content.Blocks);
                }
                Grid.SetColumn(content, 1);
                Grid.SetRow(content, rowIndex);
                grid.Children.Add(content);
            }

            var paragraph = new Paragraph();
            paragraph.Inlines.Add(new InlineUIContainer { Child = grid });
            currentBlocks.Add(paragraph);
        }

        /// <summary>
        /// Renders a horizontal rule element.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="currentBlocks"></param>
        private void RenderHorizontalRule(HorizontalRuleBlock element, BlockCollection currentBlocks)
        {
            var rectangle = new Rectangle();
            rectangle.Width = 10000;
            rectangle.Height = 1;
            rectangle.Fill = new SolidColorBrush(Colors.LightGray);
            rectangle.Margin = new Thickness(0, 7, 0, 7);

            var paragraph = new Paragraph();
            paragraph.Inlines.Add(new InlineUIContainer { Child = rectangle });

            currentBlocks.Add(paragraph);
        }

        /// <summary>
        /// Renders a quote element.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="currentBlocks"></param>
        private void RenderQuote(QuoteBlock element, BlockCollection currentBlocks)
        {
            var content = new RichTextBlock();
            foreach (MarkdownBlock quoteBlock in element.Blocks)
            {
                RenderBlock(quoteBlock, content.Blocks);
            }

            var border = new Border();
            border.Margin = new Thickness(12, 5, 0, 5);
            border.BorderBrush = Application.Current.Resources["SystemControlHighlightAccentBrush"] as SolidColorBrush;
            border.BorderThickness = new Thickness(2, 0, 0, 0);
            border.Padding = new Thickness(12, 0, 0, 0);
            border.Child = content;

            var paragraph = new Paragraph();
            paragraph.Inlines.Add(new InlineUIContainer { Child = border });
            currentBlocks.Add(paragraph);
        }


        /// <summary>
        /// Renders a code element.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="currentBlocks"></param>
        private void RenderCode(CodeBlock element, BlockCollection currentBlocks)
        {
            // Make the new code paragraph
            Paragraph codePara = new Paragraph();
            codePara.Margin = new Thickness(12);
            codePara.FontFamily = new FontFamily("Consolas");

            // Render the children into the para inline.
            Run textRun = new Run();
            textRun.Text = element.Text;
            codePara.Inlines.Add(textRun);

            // Add it to the blocks
            currentBlocks.Add(codePara);
        }

        /// <summary>
        /// Renders a table element.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="currentBlocks"></param>
        private void RenderTable(TableBlock element, BlockCollection currentBlocks)
        {
            var table = new Grid();

            // Set every column width to "Auto", plus one more for the border.
            for (var i = 0; i < element.ColumnDefinitions.Count + 1; i++)
                table.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            // Set every row height to "Auto", plus one more for the border.
            for (var i = 0; i < element.Rows.Count + 1; i ++)
                table.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

            // Add each row.
            for (int rowIndex = 0; rowIndex < element.Rows.Count; rowIndex++)
            {
                var row = element.Rows[rowIndex];

                // Top-of-cell border.
                var topBorder = new Rectangle();
                topBorder.Height = 1;
                topBorder.Fill = new SolidColorBrush(Colors.White);
                topBorder.VerticalAlignment = VerticalAlignment.Top;
                Grid.SetRow(topBorder, rowIndex);
                Grid.SetColumnSpan(topBorder, element.ColumnDefinitions.Count + 1);
                table.Children.Add(topBorder);

                // Add each cell.
                for (int cellIndex = 0; cellIndex < Math.Min(element.ColumnDefinitions.Count, row.Cells.Count); cellIndex++)
                {
                    var cell = row.Cells[cellIndex];

                    // Left border.
                    var leftBorder = new Rectangle();
                    leftBorder.Width = 1;
                    leftBorder.Fill = new SolidColorBrush(Colors.White);
                    leftBorder.HorizontalAlignment = HorizontalAlignment.Left;
                    Grid.SetRowSpan(leftBorder, element.Rows.Count + 1);
                    Grid.SetColumn(leftBorder, cellIndex);
                    table.Children.Add(leftBorder);

                    // Cell content.
                    var cellContent = new RichTextBlock();
                    cellContent.Margin = new Thickness(8 + 1, 8 + 1, 8, 8);
                    Grid.SetRow(cellContent, rowIndex);
                    Grid.SetColumn(cellContent, cellIndex);
                    switch (element.ColumnDefinitions[cellIndex].Alignment)
                    {
                        case ColumnAlignment.Center:
                            cellContent.HorizontalAlignment = HorizontalAlignment.Center;
                            break;
                        case ColumnAlignment.Right:
                            cellContent.HorizontalAlignment = HorizontalAlignment.Right;
                            break;
                    }
                    var cellPara = new Paragraph();
                    if (rowIndex == 0)
                        cellPara.FontWeight = FontWeights.Bold;
                    bool trimTextStart = true;
                    RenderInlineChildren(cell.Inlines, cellPara.Inlines, ref trimTextStart);
                    cellContent.Blocks.Add(cellPara);
                    table.Children.Add(cellContent);
                }

                // Right border.
                var rightBorder = new Rectangle();
                rightBorder.Width = 1;
                rightBorder.Fill = new SolidColorBrush(Colors.White);
                Grid.SetRowSpan(rightBorder, element.Rows.Count + 1);
                Grid.SetColumn(rightBorder, element.ColumnDefinitions.Count);
                table.Children.Add(rightBorder);
            }

            // Add the bottom border.
            var bottomBorder = new Rectangle();
            bottomBorder.Height = 1;
            bottomBorder.Fill = new SolidColorBrush(Colors.White);
            Grid.SetRow(bottomBorder, element.Rows.Count);
            Grid.SetColumnSpan(bottomBorder, element.ColumnDefinitions.Count);
            table.Children.Add(bottomBorder);

            var paragraph = new Paragraph();
            paragraph.Margin = new Thickness(0, 5, 0, 5);
            paragraph.Inlines.Add(new InlineUIContainer { Child = table });

            currentBlocks.Add(paragraph);
        }

        #endregion

        #region Render Inlines

        /// <summary>
        /// Renders a text run element.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="currentInlines"></param>
        /// <param name="trimTextStart">If true this element should trin the start of the text and set to fales.</param>
        private void RenderTextRun(TextRunInline element, InlineCollection currentInlines, ref bool trimTextStart)
        {
            // Create the text run
            Run textRun = new Run();
            textRun.Text = element.Text;

            // Check if we should trim the starting text. This allows us to trim the text starting a block
            // but nothing else. If we do a trim set it to false so no one else does.
            if(trimTextStart)
            {
                trimTextStart = false;
                textRun.Text = textRun.Text.TrimStart();
            }

            // Add it
            currentInlines.Add(textRun);
        }

        /// <summary>
        /// Renders a bold run element.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="currentInlines"></param>
        /// <param name="trimTextStart">If true this element should trin the start of the text and set to fales.</param>
        private void RenderBoldRun(BoldTextInline element, InlineCollection currentInlines, ref bool trimTextStart)
        {
            // Create the text run
            Span boldSpan = new Span();
            boldSpan.FontWeight = FontWeights.Bold;

            // Render the children into the bold inline.
            RenderInlineChildren(element.Inlines, boldSpan.Inlines, ref trimTextStart);

            // Add it to the current inlines
            currentInlines.Add(boldSpan);
        }

        /// <summary>
        /// Renders a link element
        /// </summary>
        /// <param name="element"></param>
        /// <param name="currentInlines"></param>
        /// <param name="trimTextStart">If true this element should trin the start of the text and set to fales.</param>
        private void RenderMarkdownLink(MarkdownLinkInline element, InlineCollection currentInlines, ref bool trimTextStart)
        {
            // Create the text run
            Hyperlink link = new Hyperlink();

            // Register the link
            m_linkRegister.RegisterNewHyperLink(link, element.Url);

            // Render the children into the link inline.
            RenderInlineChildren(element.Inlines, link.Inlines, ref trimTextStart);

            // Add it to the current inlines
            currentInlines.Add(link);
        }

        /// <summary>
        /// Renders a raw link element
        /// </summary>
        /// <param name="element"></param>
        /// <param name="currentInlines"></param>
        /// <param name="trimTextStart">If true this element should trin the start of the text and set to fales.</param>
        private void RenderRawHyperlink(RawHyperlinkInline element, InlineCollection currentInlines, ref bool trimTextStart)
        {
            // Create the text run
            Hyperlink link = new Hyperlink();

            // Register the link
            m_linkRegister.RegisterNewHyperLink(link, element.Url);

            // Make a text block for the link
            Run linkText = new Run();
            linkText.Text = element.Url;
            link.Inlines.Add(linkText);

            if (trimTextStart)
            {
                trimTextStart = false;
                linkText.Text = linkText.Text.Trim();
            }

            // Add it to the current inlines
            currentInlines.Add(link);
        }

        /// <summary>
        /// Renders a raw subreddit element
        /// </summary>
        /// <param name="element"></param>
        /// <param name="currentInlines"></param>
        /// <param name="trimTextStart">If true this element should trin the start of the text and set to fales.</param>
        private void RenderRawSubreddit(RawSubredditInline element, InlineCollection currentInlines, ref bool trimTextStart)
        {
            // Create the hyper link
            Hyperlink link = new Hyperlink();

            // Register the link
            m_linkRegister.RegisterNewHyperLink(link, element.Text);

            // Add the subreddit text
            Run subreddit = new Run();
            subreddit.Text = element.Text;
            link.Inlines.Add(subreddit);

            if(trimTextStart)
            {
                trimTextStart = false;
                subreddit.Text = element.Text.Trim();
            }

            // Add it to the current inlines
            currentInlines.Add(link);
        }

        /// <summary>
        /// Renders a text run element
        /// </summary>
        /// <param name="element"></param>
        /// <param name="currentInlines"></param>
        /// <param name="trimTextStart">If true this element should trin the start of the text and set to fales.</param>
        private void RenderItalicRun(ItalicTextInline element, InlineCollection currentInlines, ref bool trimTextStart)
        {
            // Create the text run
            Span italicSpan = new Span();
            italicSpan.FontStyle = FontStyle.Italic;

            // Render the children into the italic inline.
            RenderInlineChildren(element.Inlines, italicSpan.Inlines, ref trimTextStart);

            // Add it to the current inlines
            currentInlines.Add(italicSpan);
        }

        /// <summary>
        /// Renders a strikethrough element
        /// </summary>
        /// <param name="element"></param>
        /// <param name="currentInlines"></param>
        /// <param name="trimTextStart">If true this element should trin the start of the text and set to fales.</param>
        private void RenderStrikethroughRun(StrikethroughTextInline element, InlineCollection currentInlines, ref bool trimTextStart)
        {
            // TODO: make this actually strikethrough somehow...
            Span span = new Span();

            // Render the children into the inline.
            RenderInlineChildren(element.Inlines, span.Inlines, ref trimTextStart);

            Strikethroughize(span);

            // Add it to the current inlines
            currentInlines.Add(span);
        }

        private void Strikethroughize(Span span)
        {
            foreach (var inlineElement in span.Inlines)
            {
                if (inlineElement is Span)
                    Strikethroughize((Span)inlineElement);
                else if (inlineElement is Run)
                {
                    var text = ((Run)inlineElement).Text;
                    var builder = new StringBuilder(text.Length * 2);
                    foreach (var c in text)
                    {
                        builder.Append((char)0x0336);
                        builder.Append(c);
                    }
                    ((Run)inlineElement).Text = builder.ToString();
                }
            }
        }

        /// <summary>
        /// Renders a superscript element
        /// </summary>
        /// <param name="element"></param>
        /// <param name="currentInlines"></param>
        /// <param name="trimTextStart">If true this element should trin the start of the text and set to fales.</param>
        private void RenderSuperscriptRun(SuperscriptTextInline element, InlineCollection currentInlines, ref bool trimTextStart)
        {
            Span span = new Span();
            span.FontSize = span.FontSize * 0.8;

            // Render the children into the inline.
            RenderInlineChildren(element.Inlines, span.Inlines, ref trimTextStart);

            // Add it to the current inlines
            currentInlines.Add(span);
        }

        /// <summary>
        /// Renders a code element
        /// </summary>
        /// <param name="element"></param>
        /// <param name="currentInlines"></param>
        /// <param name="trimTextStart">If true this element should trin the start of the text and set to fales.</param>
        private void RenderCodeRun(CodeInline element, InlineCollection currentInlines, ref bool trimTextStart)
        {
            var run = new Run();
            run.FontFamily = new FontFamily("Consolas");
            run.Text = element.Text;

            // Add it to the current inlines
            currentInlines.Add(run);
        }

        #endregion
    }
}
