﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Textures;
using System.Collections.Generic;
using System.Threading.Tasks;
using osu.Framework.Logging;
using System.Collections.Concurrent;
using osu.Framework.Graphics.Sprites;
using SharpFNT;

namespace osu.Framework.IO.Stores
{
    public class FontStore : TextureStore
    {
        private readonly List<GlyphStore> glyphStores = new List<GlyphStore>();

        private readonly List<FontStore> nestedFontStores = new List<FontStore>();

        private readonly Func<(string, char), Texture> cachedTextureLookup;

        /// <summary>
        /// A local cache to avoid string allocation overhead. Can be changed to (string,char)=>string if this ever becomes an issue,
        /// but as long as we directly inherit <see cref="TextureStore"/> this is a slight optimisation.
        /// </summary>
        private readonly ConcurrentDictionary<(string, char), Texture> namespacedTextureCache = new ConcurrentDictionary<(string, char), Texture>();

        public FontStore(IResourceStore<TextureUpload> store = null, float scaleAdjust = 100)
            : base(store, scaleAdjust: scaleAdjust)
        {
            cachedTextureLookup = t => string.IsNullOrEmpty(t.Item1) ? Get(t.Item2.ToString()) : Get(t.Item1 + "/" + t.Item2);
        }

        /// <summary>
        /// Get the texture of a character from a specified font and its associated spacing information.
        /// </summary>
        /// <param name="charName">The character to look up</param>
        /// <param name="fontName">The font look for the character in</param>
        /// <returns>A struct containing the texture and its associated spacing information for the specified character. Null if no texture is found</returns>
        public CharacterGlyph? GetCharacter(string fontName, char charName)
        {
            var texture = namespacedTextureCache.GetOrAdd((fontName, charName), cachedTextureLookup);

            if (texture == null)
                return null;

            var info = getCharacterInfo(fontName, charName);

            return new CharacterGlyph
            {
                Texture = texture,
                XAdvance = info.XAdvance / ScaleAdjust,
                XOffset = info.XOffset / ScaleAdjust,
                YOffset = info.YOffset / ScaleAdjust,
            };
        }

        /// <summary>
        /// Looks for and gets the Character information from this store's <see cref="GlyphStore"/>s and nested <see cref="FontStore"/>s.
        /// </summary>
        /// <param name="charName">The character to look up</param>
        /// <param name="fontName">The font look in for the character</param>
        /// <returns>The associated character information for the character and font. Returns null if not found</returns>
        private Character getCharacterInfo(string fontName, char charName)
        {
            foreach (var store in glyphStores)
            {
                // Return the default (first available) character if fontName is default
                if (store.HasGlyph(charName) && (fontName == store.FontName || fontName == ""))
                    return store.GetCharacterInfo(charName);
            }

            foreach (var store in nestedFontStores)
            {
                var glyph = store.getCharacterInfo(fontName, charName);
                if (glyph != null)
                    return glyph;
            }

            return null;
        }

        protected override IEnumerable<string> GetFilenames(string name)
        {
            // extensions should not be used as they interfere with character lookup.
            yield return name;
        }

        public override void AddStore(IResourceStore<TextureUpload> store)
        {
            switch (store)
            {
                case FontStore fs:
                    nestedFontStores.Add(fs);
                    return;
                case GlyphStore gs:
                    glyphStores.Add(gs);
                    queueLoad(gs);
                    break;
            }

            base.AddStore(store);
        }

        private Task childStoreLoadTasks;

        /// <summary>
        /// Append child stores to a single threaded load task.
        /// </summary>
        private void queueLoad(GlyphStore store)
        {
            var previousLoadStream = childStoreLoadTasks;

            childStoreLoadTasks = Task.Run(async () =>
            {
                if (previousLoadStream != null)
                    await previousLoadStream;

                try
                {
                    Logger.Log($"Loading Font {store.FontName}...", level: LogLevel.Debug);
                    await store.LoadFontAsync();
                    Logger.Log($"Loaded Font {store.FontName}!", level: LogLevel.Debug);
                }
                catch (OperationCanceledException)
                {
                }
            });
        }

        public override void RemoveStore(IResourceStore<TextureUpload> store)
        {
            switch (store)
            {
                case FontStore fs:
                    nestedFontStores.Remove(fs);
                    return;
                case GlyphStore gs:
                    glyphStores.Remove(gs);
                    break;
            }

            base.RemoveStore(store);
        }

        public override Texture Get(string name)
        {
            var found = base.Get(name);

            if (found == null)
            {
                foreach (var store in nestedFontStores)
                    if ((found = store.Get(name)) != null)
                        break;
            }

            return found;
        }

        public float? GetBaseHeight(char c)
        {
            foreach (var store in glyphStores)
            {
                if (store.HasGlyph(c))
                    return store.GetBaseHeight() / ScaleAdjust;
            }

            foreach (var store in nestedFontStores)
            {
                var height = store.GetBaseHeight(c);
                if (height.HasValue)
                    return height;
            }

            return null;
        }

        public float? GetBaseHeight(string fontName)
        {
            foreach (var store in glyphStores)
            {
                var bh = store.GetBaseHeight(fontName);
                if (bh.HasValue)
                    return bh.Value / ScaleAdjust;
            }

            foreach (var store in nestedFontStores)
            {
                var height = store.GetBaseHeight(fontName);
                if (height.HasValue)
                    return height;
            }

            return null;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            glyphStores.ForEach(g => g.Dispose());
        }
    }
}
