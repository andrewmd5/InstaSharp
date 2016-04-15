using System;

namespace InstaSharp.Models
{
    public class NormalResponse : EventArgs
    {
        public string Status { get; set; }
        public string Message { get; set; }
    }
}