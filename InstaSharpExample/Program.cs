using System;
using System.Security;
using InstaSharp;
using InstaSharp.Models;

namespace InstaSharpExample
{
    internal class Program
    {
        public static SecureString ConvertToSecureString(string strPassword)
        {
            var secureStr = new SecureString();
            if (strPassword.Length > 0)
            {
                foreach (var c in strPassword.ToCharArray()) secureStr.AppendChar(c);
            }
            return secureStr;
        }

        private static void Main(string[] args)
        {
            var uploader = new InstagramUploader("", ConvertToSecureString(""));
            uploader.InvalidLoginEvent += InvalidLoginEvent;
            uploader.ErrorEvent += ErrorEvent;
            uploader.OnCompleteEvent += OnCompleteEvent;
            uploader.OnLoginEvent += OnLoginEvent;
            uploader.SuccessfulLoginEvent += SuccessfulLoginEvent;
            uploader.OnMediaConfigureStarted += OnMediaConfigureStarted;
            uploader.OnMediaUploadStartedEvent += OnMediaUploadStartedEvent;
            uploader.OnMediaUploadeComplete += OnmediaUploadCompleteEvent;
            uploader.UploadImage(@"D:\Pictures\0ra0z4K.jpg", "#helloworld", true);
            Console.Read();
        }

        private static void OnMediaUploadStartedEvent(object sender, EventArgs e)
        {
            Console.WriteLine("Attempting to upload image");
        }

        private static void OnmediaUploadCompleteEvent(object sender, EventArgs e)
        {
            Console.WriteLine("The image was uploaded, but has not been configured yet.");
        }


        private static void OnMediaConfigureStarted(object sender, EventArgs e)
        {
            Console.WriteLine("The image has started to be configured");
        }

        private static void SuccessfulLoginEvent(object sender, EventArgs e)
        {
            Console.WriteLine("Logged in! " + ((LoggedInUserResponse) e).FullName);
        }

        private static void OnLoginEvent(object sender, EventArgs e)
        {
            Console.WriteLine("Event fired for login: " + ((NormalResponse) e).Message);
        }

        private static void OnCompleteEvent(object sender, EventArgs e)
        {
            Console.WriteLine("Image posted to Instagram, here are all the urls");
            foreach (var image in ((UploadResponse) e).Images)
            {
                Console.WriteLine("Url: " + image.Url);
                Console.WriteLine("Width: " + image.Width);
                Console.WriteLine("Height: " + image.Height);
            }
        }

        private static void ErrorEvent(object sender, EventArgs e)
        {
            Console.WriteLine("Error  " + ((ErrorResponse) e).Message);
        }

        private static void InvalidLoginEvent(object sender, EventArgs e)
        {
            Console.WriteLine("Error while logging  " + ((ErrorResponse) e).Message);
        }
    }
}