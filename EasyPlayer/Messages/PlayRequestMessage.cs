﻿using EasyPlayer.Library;

namespace EasyPlayer.Messages
{
    public class PlayRequestMessage
    {
        public readonly MediaItem Media;

        public PlayRequestMessage(MediaItem media)
        {
            this.Media = media;
        }
    }
}
