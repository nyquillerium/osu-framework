// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Caching;
using osu.Framework.Graphics.Primitives;

namespace osu.Framework.Graphics.Containers
{
    /// <summary>
    /// An <see cref="Container"/> that can also apply a padding based on the safe areas of the nearest cached <see cref="SafeAreaDefiningContainer"/>.
    /// Padding will only be applied if the contents of this container would otherwise intersect the safe area margins relative to the associated target container.
    /// </summary>
    public class SafeAreaContainer : Container
    {
        private readonly BindableSafeArea safeAreaPadding = new BindableSafeArea();

        [BackgroundDependencyLoader]
        private void load()
        {
            safeAreaPadding.ValueChanged += _ => PaddingCache.Invalidate();
            safeAreaPadding.BindTo(safeArea.SafeAreaPadding);
        }

        [Resolved(CanBeNull = true)]
        private ISafeArea safeArea { get; set; }

        /// <summary>
        /// The <see cref="Edges"/> that should bypass the defined <see cref="ISafeArea" /> to bleed to the screen edge.
        /// Defaults to <see cref="Edges.None"/>.
        /// </summary>
        public Edges SafeAreaOverrideEdges { get; set; } = Edges.None;

        protected readonly Cached PaddingCache = new Cached();

        public override bool Invalidate(Invalidation invalidation = Invalidation.All, Drawable source = null, bool shallPropagate = true)
        {
            if (invalidation.HasFlag(Invalidation.Parent))
                PaddingCache.Invalidate();

            return base.Invalidate(invalidation, source, shallPropagate);
        }

        protected override void UpdateAfterChildrenLife()
        {
            base.UpdateAfterChildrenLife();

            if (!PaddingCache.IsValid)
            {
                Padding = getPadding();
                PaddingCache.Validate();
            }
        }

        private MarginPadding getPadding()
        {
            if (safeArea == null)
                return new MarginPadding();

            var nonSafeLocalSpace = safeArea.ExpandRectangleToSpaceOfOtherDrawable(this);

            var nonSafe = safeArea.AvailableNonSafeSpace;

            var paddedRect = new RectangleF(nonSafe.X + safeAreaPadding.Value.Left, nonSafe.Y + safeAreaPadding.Value.Top, nonSafe.Width - safeAreaPadding.Value.TotalHorizontal, nonSafe.Height - safeAreaPadding.Value.TotalVertical);
            var localTopLeft = safeArea.ToSpaceOfOtherDrawable(paddedRect.TopLeft, this);
            var localBottomRight = safeArea.ToSpaceOfOtherDrawable(paddedRect.BottomRight, this);

            return new MarginPadding
            {
                Left = SafeAreaOverrideEdges.HasFlag(Edges.Left) ? nonSafeLocalSpace.TopLeft.X : Math.Max(0, localTopLeft.X),
                Right = SafeAreaOverrideEdges.HasFlag(Edges.Right) ? DrawRectangle.BottomRight.X - nonSafeLocalSpace.BottomRight.X : Math.Max(0, DrawSize.X - localBottomRight.X),
                Top = SafeAreaOverrideEdges.HasFlag(Edges.Top) ? nonSafeLocalSpace.TopLeft.Y : Math.Max(0, localTopLeft.Y),
                Bottom = SafeAreaOverrideEdges.HasFlag(Edges.Bottom) ? DrawRectangle.BottomRight.Y - nonSafeLocalSpace.BottomRight.Y : Math.Max(0, DrawSize.Y - localBottomRight.Y)
            };
        }
    }
}
