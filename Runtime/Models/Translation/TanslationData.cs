using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Fireball.Game.Client.Models
{
    public class TranslationResponse<T>
    {
        public int StatusCode;
        public TranslationData<T> Data;
    }

    public class TranslationData<T>
    {
        public string Id;
        public string AppName;
        public T Translation;
    }
}
