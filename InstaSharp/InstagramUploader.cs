using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Script.Serialization;
using InstaSharp.Models;
using InstaSharp.Utilities;
using Newtonsoft.Json.Linq;

namespace InstaSharp
{
    public class InstagramUploader
    {
        private readonly SecureString _password;

        private readonly string _userAgent =
            "Instagram 6.21.2 Android (19/4.4.2; 480dpi; 1152x1920; Meizu; MX4; mx4; mt6595; en_US)";

        private readonly string _username;
        private readonly InstagramApi instagramApi = new InstagramApi();

        private readonly string instagramSignature = "25eace5393646842f0d0c3fb2ac7d3cfa15c052436ee86b5406a8433f54d24a5";
        private readonly JavaScriptSerializer serializer = new JavaScriptSerializer();


        public InstagramUploader(string username, SecureString password)
        {
            _username = username;
            _password = password;
        }

        public event EventHandler OnLoginEvent;
        public event EventHandler InvalidLoginEvent;
        public event EventHandler SuccessfulLoginEvent;
        public event EventHandler ErrorEvent;
        public event EventHandler OnMediaUploadStartedEvent;
        public event EventHandler OnMediaUploadeComplete;
        public event EventHandler OnMediaConfigureStarted;
        public event EventHandler OnCompleteEvent;

        private string GenerateGuid()
        {
            return Guid.NewGuid().ToString();
        }

        private string GenerateSignature(string data)
        {
            var keyByte = Encoding.UTF8.GetBytes(instagramSignature);
            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                hmacsha256.ComputeHash(Encoding.UTF8.GetBytes(data));
                return hmacsha256.Hash.Aggregate("", (current, t) => current + t.ToString("X2")).ToLower();
            }
        }

        private string Crop(string imagePath, bool withBorder = false)
        {
            var tempImage = Path.Combine(Path.GetTempPath(), StringUtilities.GetRandomString() + ".jpg");
            var primaryImage = new Bitmap(Image.FromFile(imagePath));
            if (withBorder)
            {
                var croppedBorder = ImageUtilities.SquareWithBorder(primaryImage);
                croppedBorder.Save(tempImage, ImageFormat.Jpeg);
            }
            else
            {
                var squaredImage = ImageUtilities.SquareImage(primaryImage);
                squaredImage.Save(tempImage, ImageFormat.Jpeg);
            }
            return tempImage;
        }

        public void UploadImage(string imagePath, string caption, bool crop = false, bool withBorder = false)
        {
            if (crop)
            {
                imagePath = Crop(imagePath, withBorder);
            }
            try
            {
                var guid = GenerateGuid();
                var deviceId = $"android-{guid}";
                var data = new Dictionary<string, string>
                {
                    {"device_id", deviceId},
                    {"guid", guid},
                    {"username", _username},
                    {"password", StringUtilities.SecureStringToString(_password)},
                    {"Content-Type", "application/x-www-form-urlencoded; charset=UTF-8"}
                };
                var loginData = serializer.Serialize(data);
                var signature = GenerateSignature(loginData);
                var signedLoginData =
                    $"signed_body={signature}.{HttpUtility.UrlEncode(loginData)}&ig_sig_key_version=6";
                OnLoginEvent?.Invoke(this, new NormalResponse {Status = "ok", Message = "Logging in please wait."});
                var loginResponse = instagramApi.PostData("accounts/login/", signedLoginData, _userAgent);
                if (string.IsNullOrEmpty(loginResponse))
                {
                    ErrorEvent?.Invoke(this,
                        new ErrorResponse
                        {
                            Status = "fail",
                            Message = "Empty response received from the server while trying to login"
                        });
                }
                else
                {
                    try
                    {
                        var loginJson = JObject.Parse(loginResponse);
                        var status = (string) loginJson["status"];
                        if (status.Equals("ok"))
                        {
                            var username = (string) loginJson["logged_in_user"]["username"];
                            var hasAnonymousProfilePicture =
                                (bool) loginJson["logged_in_user"]["has_anonymous_profile_picture"];
                            var profilePicUrl = (string) loginJson["logged_in_user"]["profile_pic_url"];
                            var fullName = (string) loginJson["logged_in_user"]["full_name"];
                            var isPrivate = (bool) loginJson["logged_in_user"]["is_private"];
                            SuccessfulLoginEvent?.Invoke(this,
                                new LoggedInUserResponse
                                {
                                    Username = username,
                                    HasAnonymousProfilePicture = hasAnonymousProfilePicture,
                                    ProfilePicUrl = profilePicUrl,
                                    FullName = fullName,
                                    IsPrivate = isPrivate
                                });
                            OnMediaUploadStartedEvent?.Invoke(this, EventArgs.Empty);
                            var uploadResponse = instagramApi.PostImage(imagePath, _userAgent);
                            if (string.IsNullOrEmpty(uploadResponse))
                            {
                                ErrorEvent?.Invoke(this,
                                    new ErrorResponse
                                    {
                                        Status = "fail",
                                        Message =
                                            "Empty response received from the server while trying to post the image"
                                    });
                            }
                            else
                            {
                                try
                                {
                                    var uploadJson = JObject.Parse(uploadResponse);
                                    var uploadStatus = (string) uploadJson["status"];
                                    if (uploadStatus.Equals("ok"))
                                    {
                                        OnMediaUploadeComplete?.Invoke(this, EventArgs.Empty);
                                        OnMediaConfigureStarted?.Invoke(this, EventArgs.Empty);
                                        var newLineStripper = new Regex(@"/\r|\n/", RegexOptions.IgnoreCase);
                                        //...
                                        caption = newLineStripper.Replace(caption, "");
                                        var mediaId = (string) uploadJson["media_id"];
                                        var configureData = new Dictionary<string, string>
                                        {
                                            {"device_id", deviceId},
                                            {"guid", guid},
                                            {"media_id", mediaId},
                                            {"caption", caption.Trim()},
                                            {"device_timestamp", StringUtilities.GenerateTimeStamp()},
                                            {"source_type", "5"},
                                            {"filter_type", "0"},
                                            {"extra", "{}"},
                                            {"Content-Type", "application/x-www-form-urlencoded; charset=UTF-8"}
                                        };
                                        var configureDataString = serializer.Serialize(configureData);
                                        var configureSignature = GenerateSignature(configureDataString);
                                        var signedConfigureBody =
                                            $"signed_body={configureSignature}.{HttpUtility.UrlEncode(configureDataString)}&ig_sig_key_version=4";
                                        var configureResults = instagramApi.PostData("media/configure/",
                                            signedConfigureBody, _userAgent);
                                        if (string.IsNullOrEmpty(configureResults))
                                        {
                                            ErrorEvent?.Invoke(this,
                                                new ErrorResponse
                                                {
                                                    Status = "fail",
                                                    Message =
                                                        "Empty response received from the server while trying to configure the image"
                                                });
                                        }
                                        else
                                        {
                                            try
                                            {
                                                var configureJson = JObject.Parse(configureResults);
                                                var uploadedResponse = new UploadResponse
                                                {
                                                    Images = new List<UploadResponse.InstagramMedia>()
                                                };
                                                foreach (
                                                    var media in
                                                        configureJson["media"]["image_versions2"]["candidates"].Select(
                                                            x => JObject.Parse(x.ToString()))
                                                            .Select(mediaJson => new UploadResponse.InstagramMedia
                                                            {
                                                                Url = (string) mediaJson["url"],
                                                                Width = (int) mediaJson["width"],
                                                                Height = (int) mediaJson["height"]
                                                            }))
                                                {
                                                    uploadedResponse.Images.Add(media);
                                                }
                                                OnCompleteEvent?.Invoke(this, uploadedResponse);
                                            }
                                            catch (Exception ex)
                                            {
                                                ErrorEvent?.Invoke(this,
                                                    new ErrorResponse
                                                    {
                                                        Status = "fail",
                                                        Message = "Could not decode the configure response"
                                                    });
                                            }
                                        }
                                    }
                                    else
                                    {
                                        ErrorEvent?.Invoke(this,
                                            new ErrorResponse
                                            {
                                                Status = "fail",
                                                Message =
                                                    (string) uploadJson["message"]
                                            });
                                    }
                                }
                                catch (Exception)
                                {
                                    ErrorEvent?.Invoke(this,
                                        new ErrorResponse
                                        {
                                            Status = "fail",
                                            Message = "Could not decode the upload response"
                                        });
                                }
                            }
                        }
                        else
                        {
                            var message = (string) loginJson["message"];
                            InvalidLoginEvent?.Invoke(this, new ErrorResponse {Status = status, Message = message});
                        }
                    }
                    catch (Exception)
                    {
                        ErrorEvent?.Invoke(this,
                            new ErrorResponse
                            {
                                Status = "fail",
                                Message = "Could not decode the login response"
                            });
                    }
                }
            }
            finally
            {
                //clean up
                if (crop)
                {
                    if (File.Exists(imagePath))
                    {
                        File.Delete(imagePath);
                    }
                }
            }
        }
    }
}