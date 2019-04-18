// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Logging;
using osu.Framework.Testing;
using osuTK;
using osuTK.Graphics;

namespace osu.Framework.Tests.Visual.UserInterface
{
    public class TestCaseButton : TestCase
    {
        public TestCaseButton()
        {
            Children = new Drawable[]
            {
                new Box
                {
                    Colour = Color4.LightGreen,
                    RelativeSizeAxes = Axes.Both
                },
                new FillFlowContainer
                {
                    Width = 250,
                    AutoSizeAxes = Axes.Y,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0, 10),
                    Children = new Drawable[]
                    {
                        new IssueButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = "no blending"
                        },
                        new IssueButton
                        {
                            Hovered = true,
                            RelativeSizeAxes = Axes.X,
                            Text = "with blending"
                        }
                    }
                }
            };
        }

        private class IssueButton : Button
        {
            public bool Hovered;

            [BackgroundDependencyLoader]
            private void load()
            {
                Height = 80;
                BackgroundColour = new Color4(50, 50, 50, 255);

                Content.Masking = true;
                Content.CornerRadius = 12;
                Content.BorderThickness = 4;
                Content.BorderColour = Color4.Black;
                Content.MaskingSmoothness = 1;

                Add(new TestContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = new Box
                    {
                        Alpha = Hovered ? 1 : 0,
                        RelativeSizeAxes = Axes.Both,
                        Blending = BlendingMode.Mixture,
                        Colour = Color4.White.Opacity(0.1f),
                    }
                });
            }
        }

        private class TestContainer : Container
        {
            protected override bool CanBeFlattened => false;

            protected override void ApplyDrawNode(DrawNode node)
            {
                
                base.ApplyDrawNode(node);
            }
        }
    }
}
