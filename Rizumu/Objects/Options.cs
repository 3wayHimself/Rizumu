﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rizumu.Objects
{
    // Default values == Default options
    public class Options
    {
        [JsonProperty("fullscreen")]
        public bool Fullscreen = false;

        [JsonProperty("leftkey")]
        public int Left = 100;

        [JsonProperty("upkey")]
        public int Up = 104;

        [JsonProperty("rightkey")]
        public int Right = 102;

        [JsonProperty("downkey")]
        public int Down = 98;

        [JsonProperty("volume")]
        public float Volume = 0.2f;

        [JsonProperty("skin")]
        public string SkinName = "default";

        [JsonProperty("player")]
        public string Player = "user";
    }
}
