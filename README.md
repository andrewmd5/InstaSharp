## InstaDash


### What is it?
This library allows you to upload photos to Instagram without a mobile device, thus allowing you to extend your application to support Instagram uploading. It is multiplatform and works on Windows XP - 10, UWP and Windows 8 phones.

### Usage 

Create an instance and define your events
```csharp
 var uploader = new InstagramUploader("username", ConvertToSecureString("password"));
uploader.InvalidLoginEvent += InvalidLoginEvent;
uploader.ErrorEvent += ErrorEvent;
uploader.OnCompleteEvent += OnCompleteEvent;
uploader.OnLoginEvent += OnLoginEvent;
uploader.SuccessfulLoginEvent += SuccessfulLoginEvent;
uploader.OnMediaConfigureStarted += OnMediaConfigureStarted;
uploader.OnMediaUploadStartedEvent += OnMediaUploadStartedEvent;
uploader.OnMediaUploadeComplete += OnmediaUploadCompleteEvent;
```

To upload a photo call the upload function
```csharp
uploader.UploadImage(@"fullPathToImage", "Caption and Hashtags", cropImage, cropWithBorder);
```

cropImage and cropWithBorder are optional, cropping an image will turn it into a perfect square, cropping with a border will maintain the images aspect ratio by putting a border around the image. If an image fails to upload, it may be because its dimensions are incorrect for Instagram and you might need to crop. 


All calls are made over HTTPs to Instagram.

You can handle all your events using the following code

```csharp
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
```


### Contributing 

I accept pull request, feel free to submit them. 








