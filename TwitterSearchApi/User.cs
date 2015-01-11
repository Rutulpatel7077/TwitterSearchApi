using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace WZWVAPI
{
    public class User : DataObject
    {
        public string Username { get; private set; }
        public string Email { get; private set; }
        public string Twitter { get; set; }
        public string Facebook { get; set; }
        public string Skype { get; set; }
        public string ProfileImage { get; set; }
        public bool EmailConfirmed { get; private set; }
        public bool IsBanned { get; private set; }
        public string ConfirmLink { get; private set; }
        public bool IsAdmin { get; private set; }
        public bool IsOwner { get; private set; }
        public string AboutMe { get; set; }
        public string LastOnline { get; private set; }
        public string Password { get; private set; }
        public int MailServerID { get; private set; }
        public int NumberOfSearchResult { get; private set; }
        public bool EnableStats { get; private set; }
        public bool BekendeNederlandersUser { get; private set; }

        public User(
            int ID,
            string Username,
            string Email,
            bool EmailConfirmed,
            bool IsBanned,
            string ConfirmLink,
            string Twitter = "Not Specified",
            string Facebook = "Not Specified",
            string Skype = "Not Specified",
            string ProfileImage = "Not Specified",
            bool IsAdmin = false,
            bool IsOwner = false,
            string Aboutme = "Not Specified",
            string LastOnline = "00/00/0000",
            string Password = "",
            int MailServerID = 1,
            int NumberOfSearchResult = 15,
            bool EnableStats = false,
            bool BekendeNederlandersUser = false)
            : base()
        {
            this.ID = ID;
            this.Username = Username;
            this.Email = Email;
            this.Twitter = Twitter;
            this.Facebook = Facebook;
            this.Skype = Skype;
            this.EmailConfirmed = EmailConfirmed;
            this.IsBanned = IsBanned;
            this.ConfirmLink = ConfirmLink;
            this.Password = Password;
            this.MailServerID = MailServerID;
            this.NumberOfSearchResult = NumberOfSearchResult;
            this.EnableStats = EnableStats;
            this.BekendeNederlandersUser = BekendeNederlandersUser;

            if (this.NumberOfSearchResult == 0)
            {
                this.NumberOfSearchResult = 15;
            }

            if (ProfileImage == "Not Specified")
            {
                this.ProfileImage = "Not Specified";// Default profile img
            }
            else
            {
                this.ProfileImage = ProfileImage;
            }

            this.IsAdmin = IsAdmin;
            this.IsOwner = IsOwner;
            this.AboutMe = Aboutme;
            this.LastOnline = LastOnline;

            if (this.ID == 0 && this.Username != "Anonymous")
            {
                this.NumberOfSearchResult = 15;
                User loggedinUser = UserHandler.GetLoggedInUser();

                if (loggedinUser == null)
                {
                    FeedbackHandler.AddError("Not logged in!");
                    return;
                }

                if (this.Password.Length < 3)
                {
                    FeedbackHandler.AddError("User could not be added, Password to short.");
                    return;
                }

                if (loggedinUser.IsOwner)
                {
                    UserHandler.UH.AddObject(this);
                    this.downloadImage();
                    UserHandler.UH.UpdateObject(this);
                    UserHandler.UH.ChangePassword(this, this.Password);
                }
                else
                {
                    FeedbackHandler.AddError("Insufficient rights.");
                }
            }
        }

        public string serialize()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static User deserialize(string JSONUserObject)
        {
            return JsonConvert.DeserializeObject<User>(JSONUserObject);
        }

        public void hashPassword(string Password)
        {
            this.Password = Encrypt.EncryptPassword(Password);
        }

        public void downloadImage()
        {
            try
            {
                if (this.ProfileImage.Contains('/'))
                {
                    byte[] content;
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(this.ProfileImage);
                    WebResponse response = request.GetResponse();
                    Stream stream = response.GetResponseStream();

                    using (BinaryReader br = new BinaryReader(stream))
                    {
                        content = br.ReadBytes(500000);
                        br.Close();
                    } response.Close();

                    FileStream fs = new FileStream(HttpContext.Current.Server.MapPath("~/Images") + "/User_Images/" + ID + ".jpg", FileMode.Create);
                    BinaryWriter bw = new BinaryWriter(fs);

                    this.ProfileImage = ID + ".jpg";

                    try
                    {
                        bw.Write(content);
                    }
                    finally
                    {
                        fs.Close();
                        bw.Close();
                    }
                }

                if (this.ProfileImage.Length == 0)
                {
                    this.ProfileImage = "default_user.jpg";
                }
            }
            catch (Exception)
            {
                this.ProfileImage = "default_user.jpg";
            }
        }

        public override string ToString()
        {
            return "User";
        }
    }
}