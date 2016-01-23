﻿using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using UniversalMarkdown.Parse.Elements;
using UITestMethodAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.AppContainer.UITestMethodAttribute;

namespace UniversalMarkdownUnitTests.Parse
{
    [TestClass]
    public class MarkdownLinkTests : ParseTestBase
    {
        [UITestMethod]
        [TestCategory("Parse - inline")]
        public void MarkdownLink_WithLabel()
        {
            AssertEqual("[reddit](http://reddit.com)",
                new ParagraphBlock().AddChildren(
                    new MarkdownLinkInline { Url = "http://reddit.com" }.AddChildren(
                        new TextRunInline { Text = "reddit" })));
        }

        [UITestMethod]
        [TestCategory("Parse - inline")]
        public void MarkdownLink_RelativeLink()
        {
            AssertEqual("[reddit] ( /blog )",
                new ParagraphBlock().AddChildren(
                    new MarkdownLinkInline { Url = "/blog" }.AddChildren(     // Should the URL be https://www.reddit.com/blog?
                        new TextRunInline { Text = "reddit" })));
        }

        [UITestMethod]
        [TestCategory("Parse - inline")]
        public void MarkdownLink_Nested()
        {
            AssertEqual(CollapseWhitespace(@"
                [http://reddit.com](http://reddit.com)
                [one http://reddit.com two](http://reddit.com)
                [/r/test](http://reddit.com)
                [one /r/test two](http://reddit.com)"),
                new ParagraphBlock().AddChildren(
                    new MarkdownLinkInline { Url = "http://reddit.com" }.AddChildren(
                        new TextRunInline { Text = "http://reddit.com" }),
                    new TextRunInline { Text = " " },
                    new MarkdownLinkInline { Url = "http://reddit.com" }.AddChildren(
                        new TextRunInline { Text = "one http://reddit.com two" }),
                    new TextRunInline { Text = " " },
                    new MarkdownLinkInline { Url = "http://reddit.com" }.AddChildren(
                        new TextRunInline { Text = "/r/test" }),
                    new TextRunInline { Text = " " },
                    new MarkdownLinkInline { Url = "http://reddit.com" }.AddChildren(
                        new TextRunInline { Text = "one /r/test two" })));
        }

        [UITestMethod]
        [TestCategory("Parse - inline")]
        public void MarkdownLink_WithLabelSpacing()
        {
            AssertEqual("[reddit] (http://reddit.com)",
                new ParagraphBlock().AddChildren(
                    new MarkdownLinkInline { Url = "http://reddit.com" }.AddChildren(
                        new TextRunInline { Text = "reddit" })));
        }

        [UITestMethod]
        [TestCategory("Parse - inline")]
        public void MarkdownLink_WithLabelAndFormatting()
        {
            AssertEqual("[red**dit**](http://reddit.com)",
                new ParagraphBlock().AddChildren(
                    new MarkdownLinkInline { Url = "http://reddit.com" }.AddChildren(
                        new TextRunInline { Text = "red" },
                        new BoldTextInline().AddChildren(
                            new TextRunInline { Text = "dit" }))));
        }

        [UITestMethod]
        [TestCategory("Parse - inline")]
        public void MarkdownLink_NestedSquareBrackets()
        {
            AssertEqual("[one [two] three](http://reddit.com)",
                new ParagraphBlock().AddChildren(
                    new MarkdownLinkInline { Url = "http://reddit.com" }.AddChildren(
                        new TextRunInline { Text = "one [two] three" })));
        }

        [UITestMethod]
        [TestCategory("Parse - inline")]
        public void MarkdownLink_WhiteSpaceInText()
        {
            AssertEqual("start[ middle ](http://reddit.com)end",
                new ParagraphBlock().AddChildren(
                    new TextRunInline { Text = "start" },
                    new MarkdownLinkInline { Url = "http://reddit.com" }.AddChildren(
                        new TextRunInline { Text = " middle " }),
                    new TextRunInline { Text = "end" }));
        }

        [UITestMethod]
        [TestCategory("Parse - inline")]
        public void MarkdownLink_WhiteSpaceInUrl()
        {
            AssertEqual("[text](  http://reddit.com  )",
                new ParagraphBlock().AddChildren(
                    new MarkdownLinkInline { Url = "http://reddit.com" }.AddChildren(
                        new TextRunInline { Text = "text" })));
        }

        [UITestMethod]
        [TestCategory("Parse - inline")]
        public void MarkdownLink_OtherSchemes()
        {
            AssertEqual(CollapseWhitespace(@"
                [text](http://reddit.com)

                [text](https://reddit.com)

                [text](ftp://reddit.com)

                [text](steam://reddit.com)

                [text](irc://reddit.com)

                [text](news://reddit.com)

                [text](mumble://reddit.com)

                [text](ssh://reddit.com)"),
                new ParagraphBlock().AddChildren(
                    new MarkdownLinkInline { Url = "http://reddit.com" }.AddChildren(new TextRunInline { Text = "text" }),
                    new MarkdownLinkInline { Url = "https://reddit.com" }.AddChildren(new TextRunInline { Text = "text" }),
                    new MarkdownLinkInline { Url = "ftp://reddit.com" }.AddChildren(new TextRunInline { Text = "text" }),
                    new MarkdownLinkInline { Url = "steam://reddit.com" }.AddChildren(new TextRunInline { Text = "text" }),
                    new MarkdownLinkInline { Url = "irc://reddit.com" }.AddChildren(new TextRunInline { Text = "text" }),
                    new MarkdownLinkInline { Url = "news://reddit.com" }.AddChildren(new TextRunInline { Text = "text" }),
                    new MarkdownLinkInline { Url = "mumble://reddit.com" }.AddChildren(new TextRunInline { Text = "text" }),
                    new MarkdownLinkInline { Url = "ssh://reddit.com" }.AddChildren(new TextRunInline { Text = "text" })));
        }

        [UITestMethod]
        [TestCategory("Parse - inline")]
        public void MarkdownLink_WithTooltip()
        {
            AssertEqual(@"[Wikipedia](http://en.wikipedia.org ""tooltip text"")",
                new ParagraphBlock().AddChildren(
                    new MarkdownLinkInline { Url = "http://reddit.com", Tooltip = "tooltip text" }.AddChildren(
                        new TextRunInline { Text = "reddit" })));
        }

        [UITestMethod]
        [TestCategory("Parse - inline")]
        public void MarkdownLink_Escape()
        {
            // The link stops at the first ')'
            AssertEqual(@"[test](http://en.wikipedia.org/wiki/Pica_\(disorder\))",
                new ParagraphBlock().AddChildren(
                    new MarkdownLinkInline { Url = "http://en.wikipedia.org/wiki/Pica_(disorder)" }.AddChildren(
                        new TextRunInline { Text = "test" })));
        }

        [UITestMethod]
        [TestCategory("Parse - inline")]
        public void MarkdownLink_Negative_UrlMustBeValid()
        {
            AssertEqual("[text](ha)",
                new ParagraphBlock().AddChildren(
                    new TextRunInline { Text = "[text](ha)" }));
        }

        [UITestMethod]
        [TestCategory("Parse - inline")]
        public void MarkdownLink_Negative_UrlMustHaveKnownScheme()
        {
            AssertEqual("[text](hahaha://test)",
                new ParagraphBlock().AddChildren(
                    new TextRunInline { Text = "[text](hahaha://test)" }));
        }
    }
}
