// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;

namespace osu.Framework.Testing.Drawables
{
    internal class TestCaseHeaderButton : TestCaseButton
    {
        private SpriteIcon icon;
        private Container leftBoxContainer;
        private const float left_box_width = LEFT_TEXT_PADDING / 2;

        public TestCaseHeaderButton(string header)
            : base(header)
        {
        }

        public TestCaseHeaderButton(Type type)
            : base(type)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddRange(new Drawable[]
            {
                new Box
                {
                    Colour = new Color4(57, 110, 102, 255),
                    RelativeSizeAxes = Axes.Both
                },
                leftBoxContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Width = 0,
                    Padding = new MarginPadding { Right = -left_box_width },
                    Child = new Box
                    {
                        Colour = new Color4(128, 164, 108, 255),
                        RelativeSizeAxes = Axes.Both,
                    },
                },
                icon = new SpriteIcon
                {
                    Size = new Vector2(10),
                    Icon = FontAwesome.Solid.ChevronDown,
                    Colour = Color4.White,
                    Y = -1,
                    Margin = new MarginPadding { Right = left_box_width + 5 },
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                }
            });
        }

        public override bool Current
        {
            set
            {
                base.Current = value;

                icon.FadeColour(value ? Color4.Black : Color4.White, 100);

                if (value)
                {
                    leftBoxContainer.ResizeWidthTo(1, TRANSITION_DURATION);
                    this.TransformTo(nameof(leftBoxContainerPadding), left_box_width, TRANSITION_DURATION);
                    this.TransformTo(nameof(contentPadding), 0f, TRANSITION_DURATION);
                }
                else
                {
                    leftBoxContainer.ResizeWidthTo(0, TRANSITION_DURATION);
                    this.TransformTo(nameof(leftBoxContainerPadding), -left_box_width, TRANSITION_DURATION);
                    this.TransformTo(nameof(contentPadding), LEFT_TEXT_PADDING, TRANSITION_DURATION);
                }
            }
        }

        private float leftBoxContainerPadding
        {
            get => leftBoxContainer.Padding.Right;
            set => leftBoxContainer.Padding = new MarginPadding { Right = value };
        }

        private float contentPadding
        {
            get => Content.Padding.Right;
            set => Content.Padding = new MarginPadding { Right = value };
        }

        public override bool Collapsed
        {
            set
            {
                icon.Icon = value ? FontAwesome.Solid.ChevronDown : FontAwesome.Solid.ChevronUp;
                base.Collapsed = value;
            }
        }

        public override void Show()
        {
        }

        public override void Hide()
        {
        }
    }
}
