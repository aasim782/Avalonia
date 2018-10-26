// Copyright (c) The Avalonia Project. All rights reserved.
// Licensed under the MIT license. See licence.md file in the project root for full license information.

using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.LogicalTree;
using Avalonia.Styling;
using Avalonia.UnitTests;
using Avalonia.VisualTree;
using Moq;
using System;
using System.Linq;
using Xunit;
using Avalonia.Rendering;
using Avalonia.Media;

namespace Avalonia.Controls.UnitTests.Presenters
{
    /// <summary>
    /// Tests for ContentControls that aren't hosted in a control template.
    /// </summary>
    public class ContentPresenterTests_Standalone
    {
        [Fact]
        public void Should_Set_Childs_Parent_To_Itself_Standalone()
        {
            var content = new Border();
            var target = new ContentPresenter { Content = content };

            target.UpdateChild();

            Assert.Same(target, content.Parent);
        }

        [Fact]
        public void Should_Add_Child_To_Own_LogicalChildren_Standalone()
        {
            var content = new Border();
            var target = new ContentPresenter { Content = content };

            target.UpdateChild();

            var logicalChildren = target.GetLogicalChildren();

            Assert.Single(logicalChildren);
            Assert.Equal(content, logicalChildren.First());
        }

        [Fact]
        public void Should_Raise_DetachedFromLogicalTree_On_Content_Changed_Standalone()
        {
            var target = new ContentPresenter
            {
                ContentTemplate =
                    new FuncDataTemplate<string>(t => new ContentControl() { Content = t }, false)
            };

            var parentMock = new Mock<Control>();
            parentMock.As<IContentPresenterHost>();
            parentMock.As<IRenderRoot>();
            parentMock.As<IStyleRoot>();

            (target as ISetLogicalParent).SetParent(parentMock.Object);

            target.Content = "foo";

            target.UpdateChild();

            var foo = target.Child as ContentControl;

            bool foodetached = false;

            Assert.NotNull(foo);
            Assert.Equal("foo", foo.Content);

            foo.DetachedFromLogicalTree += delegate { foodetached = true; };

            target.Content = "bar";
            target.UpdateChild();

            var bar = target.Child as ContentControl;

            Assert.NotNull(bar);
            Assert.True(bar != foo);
            Assert.False((foo as IControl).IsAttachedToLogicalTree);
            Assert.True(foodetached);
        }

        [Fact]
        public void Should_Raise_DetachedFromLogicalTree_In_ContentControl_On_Content_Changed_Standalone()
        {
            var contentControl = new ContentControl
            {
                Template = new FuncControlTemplate<ContentControl>(c => new ContentPresenter()
                {
                    Name = "PART_ContentPresenter",
                    [~ContentPresenter.ContentProperty] = c[~ContentControl.ContentProperty],
                    [~ContentPresenter.ContentTemplateProperty] = c[~ContentControl.ContentTemplateProperty]
                }),
                ContentTemplate =
                    new FuncDataTemplate<string>(t => new ContentControl() { Content = t }, false)
            };

            var parentMock = new Mock<Control>();
            parentMock.As<IRenderRoot>();
            parentMock.As<IStyleRoot>();
            parentMock.As<ILogical>().SetupGet(l => l.IsAttachedToLogicalTree).Returns(true);

            (contentControl as ISetLogicalParent).SetParent(parentMock.Object);

            contentControl.ApplyTemplate();
            var target = contentControl.Presenter as ContentPresenter;

            contentControl.Content = "foo";

            target.UpdateChild();

            var tbfoo = target.Child as ContentControl;

            bool foodetached = false;

            Assert.NotNull(tbfoo);
            Assert.Equal("foo", tbfoo.Content);

            tbfoo.DetachedFromLogicalTree += delegate { foodetached = true; };

            contentControl.Content = "bar";
            target.UpdateChild();

            var tbbar = target.Child as ContentControl;

            Assert.NotNull(tbbar);

            Assert.True(tbbar != tbfoo);
            Assert.False((tbfoo as IControl).IsAttachedToLogicalTree);
            Assert.True(foodetached);
        }

        [Fact]
        public void Should_Raise_DetachedFromLogicalTree_On_Detached_Standalone()
        {
            var target = new ContentPresenter
            {
                ContentTemplate =
                    new FuncDataTemplate<string>(t => new ContentControl() { Content = t }, false)
            };

            var parentMock = new Mock<Control>();
            parentMock.As<IContentPresenterHost>();
            parentMock.As<IRenderRoot>();
            parentMock.As<IStyleRoot>();

            (target as ISetLogicalParent).SetParent(parentMock.Object);

            target.Content = "foo";

            target.UpdateChild();

            var foo = target.Child as ContentControl;

            bool foodetached = false;

            Assert.NotNull(foo);
            Assert.Equal("foo", foo.Content);

            foo.DetachedFromLogicalTree += delegate { foodetached = true; };

            (target as ISetLogicalParent).SetParent(null);

            Assert.False((foo as IControl).IsAttachedToLogicalTree);
            Assert.True(foodetached);
        }

        [Fact]
        public void Should_Remove_Old_Child_From_LogicalChildren_On_ContentChanged_Standalone()
        {
            var target = new ContentPresenter
            {
                ContentTemplate =
                    new FuncDataTemplate<string>(t => new ContentControl() { Content = t }, false)
            };

            target.Content = "foo";

            target.UpdateChild();

            var foo = target.Child as ContentControl;

            Assert.NotNull(foo);

            var logicalChildren = target.GetLogicalChildren();

            Assert.Single(logicalChildren);

            target.Content = "bar";
            target.UpdateChild();

            Assert.Null(foo.Parent);

            logicalChildren = target.GetLogicalChildren();

            Assert.Single(logicalChildren);
            Assert.NotEqual(foo, logicalChildren.First());
        }


        [Fact]
        public void Changing_Background_Brush_Color_Should_Invalidate_Visual()
        {
            var target = new ContentPresenter()
            {
                Background = new SolidColorBrush(Colors.Red),
            };

            var root = new TestRoot(target);
            var renderer = Mock.Get(root.Renderer);
            renderer.ResetCalls();

            ((SolidColorBrush)target.Background).Color = Colors.Green;

            renderer.Verify(x => x.AddDirty(target), Times.Once);
        }
    }
}
