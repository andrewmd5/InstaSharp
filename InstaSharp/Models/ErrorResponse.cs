using System;

namespace InstaSharp.Models
{
    public class ErrorResponse : EventArgs
    {
        public string Status { get; set; }
        public string Message { get; set; }
    }
}