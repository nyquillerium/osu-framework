// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading;
using ManagedBass;
using NUnit.Framework;
using osu.Framework.Audio.Callbacks;
using osu.Framework.IO.Stores;
using osu.Framework.Logging;

namespace osu.Framework.Tests.Platform
{
    [TestFixture]
    public class TrackBassTest
    {
        private readonly int activeStream;

        public TrackBassTest()
        {
            var resources = new DllResourceStore("osu.Framework.Tests.dll");
            var fileStream = resources.GetStream("Resources.Tracks.sample-track.mp3");
            Bass.Init(0);
            Logger.Log("Error: " + Bass.LastError);
            var fileCallbacks = new FileCallbacks(new DataStreamFileProcedures(fileStream));
            activeStream = Bass.CreateStream(StreamSystem.NoBuffer, BassFlags.Prescan, fileCallbacks.Callbacks, fileCallbacks.Handle);
            Logger.Log("Error: " + Bass.LastError);

            Bass.ChannelSetAttribute(activeStream, ChannelAttribute.ReverseDirection, 1);
            Logger.Log("Error: " + Bass.LastError);
        }

        [SetUp]
        public void Setup()
        {
            // Initialize position at 4, to differentiate from a track's initial position of 0.
            Bass.ChannelSetPosition(activeStream, 4);
            Logger.Log("Error: " + Bass.LastError);
        }

        [Test]
        public void PlayAtEndResetsPositionTest()
        {
            long pos;
            // Set the length of -1 that of the length, since an issue in Bass prevents us from setting it to the actual length.
            Logger.Log("Successful: " + Bass.ChannelSetPosition(activeStream, pos = Bass.ChannelGetLength(activeStream) - 1));
            Logger.Log("Error: " + Bass.LastError);
            Logger.Log("Attempted seek position: " + pos);
            Logger.Log("Actual seeked position: " + Bass.ChannelGetPosition(activeStream));
            Logger.Log("Play success: " + Bass.ChannelPlay(activeStream));

            // Since the seek couldn't be done above, play the track and let it play until the track end.
            Logger.Log("Error: " + Bass.LastError);
            Thread.Sleep(1000);
            Logger.Log("Current play time: " + Bass.ChannelGetPosition(activeStream));

            // Check that if play is called when the track has ended, the track has been reset.
            Logger.Log("Play success: " + Bass.ChannelPlay(activeStream));
            Logger.Log("Error: " + Bass.LastError);
            Logger.Log("Current play time: " + Bass.ChannelGetPosition(activeStream));
            Assert.IsTrue(Bass.LastError == Errors.OK);
            Assert.IsTrue(Bass.ChannelGetPosition(activeStream) == 0);
        }
    }
}
