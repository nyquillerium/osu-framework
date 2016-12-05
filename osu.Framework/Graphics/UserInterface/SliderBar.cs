﻿using System;
using osu.Framework.Configuration;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transformations;
using osu.Framework.Input;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;

namespace osu.Framework.Graphics.UserInterface
{
    public class SliderBar<T> : Container where T : struct,
        IComparable, IFormattable, IConvertible, IComparable<T>, IEquatable<T>
    {
        public double KeyboardStep { get; set; } = 0.01;

        public BindableNumber<T> Bindable
        {
            get { return bindable; }
            set
            {
                if (bindable != null)
                    bindable.ValueChanged -= bindableValueChanged;
                bindable = value;
                bindable.ValueChanged += bindableValueChanged;
                updateVisualization();
            }
        }

        public Color4 Color
        {
            get { return box.Colour; }
            set { box.Colour = value; }
        }

        public Color4 SelectionColor
        {
            get { return selectionBox.Colour; }
            set { selectionBox.Colour = value; }
        }

        private readonly Box selectionBox;
        private readonly Box box;
        private BindableNumber<T> bindable;

        public SliderBar()
        {
            Children = new Drawable[]
            {
                box = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                },
                selectionBox = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                }
            };
        }

        #region Disposal

        ~SliderBar()
        {
            Dispose(false);
        }

        protected override void Dispose(bool isDisposing)
        {
            if (Bindable != null)
                Bindable.ValueChanged -= bindableValueChanged;
            base.Dispose(isDisposing);
        }

        #endregion

        protected override void LoadComplete()
        {
            base.LoadComplete();
            updateVisualization();
        }

        protected override bool OnClick(InputState state)
        {
            handleMouseInput(state);
            return true;
        }

        protected override bool OnDrag(InputState state)
        {
            handleMouseInput(state);
            return true;
        }

        protected override bool OnDragStart(InputState state) => true;

        protected override bool OnDragEnd(InputState state) => true;

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            var step = KeyboardStep;
            if (Bindable.IsInteger)
                step = Math.Ceiling(step);
            switch (args.Key)
            {
                case Key.Right:
                    Bindable.Add(step);
                    return true;
                case Key.Left:
                    Bindable.Add(step);
                    return true;
                default:
                    return false;
            }
        }

        private void bindableValueChanged(object sender, EventArgs e) => updateVisualization();

        private void handleMouseInput(InputState state)
        {
            var xPosition = ToLocalSpace(state.Mouse.NativeState.Position).X;
            Bindable.SetProportional(xPosition / box.DrawWidth);
        }

        private void updateVisualization()
        {
            var min = Convert.ToSingle(Bindable.MinValue);
            var max = Convert.ToSingle(Bindable.MaxValue);
            var val = Convert.ToSingle(Bindable.Value);
            selectionBox.ScaleTo(
                new Vector2((val - min) / (max - min), 1),
                300, EasingTypes.OutQuint);
        }
    }
}
