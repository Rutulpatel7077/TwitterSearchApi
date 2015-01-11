using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WZWVAPI
{
    public class UserHandler : DataHandler
    {
        private static UserHandler _UH = null;
        public static UserHandler UH
        {
            get
            {
                if (_UH == null)
                {
                    _UH = new UserHandler();
                }

                return _UH;
            }
        }

        private static Field UsernameField = new Field("Username", typeof(string), 45);
        private static Field passwordField = new Field("Password", typeof(string), 150);
        public static User EmptyUser { get { return new User(0, "Anonymous", "info@WZWV.nl", true, false, null); ; } private set { } }

        private UserHandler()
            : base("user", new Field[] { 
                UserHandler.UsernameField,
                new Field("Email", typeof(string), 150),
                new Field("EmailConfirmed", typeof(bool), 1),
                new Field("IsBanned", typeof(bool), 1),
                new Field("ConfirmLink", typeof(string), 100),
                new Field("Twitter", typeof(string), 45),
                new Field("Facebook", typeof(string), 75),
                new Field("Skype", typeof(string), 75),
                new Field("ProfileImage", typeof(string), 350),
                new Field("IsAdmin", typeof(bool), 1),
                new Field("IsOwner", typeof(bool), 1),
                new Field("AboutMe", typeof(string), 1500),
                new Field("LastOnline", typeof(string), 100),
                UserHandler.passwordField,
                new Field("MailServerID", typeof(int), 1),
                new Field("NumberOfSearchResult", typeof(int), 1),
                new Field("EnableStats", typeof(bool), 1),
                new Field("BekendeNederlandersUser", typeof(bool), 1)
                    }, typeof(User))
        {
            this.customQueries = new string[] { 
                "SELECT * FROM " + this.tableName + " WHERE " + UserHandler.UsernameField.FieldName + " = @Username AND " + UserHandler.passwordField.FieldName + " = @Password", 
                "UPDATE " + this.tableName + " SET " + UserHandler.passwordField.FieldName + " = @Password WHERE ID = @ID",
                "UPDATE " + this.tableName + " SET LastOnline = @LastOnline WHERE ID = @ID"};

            this.DefaultDataObject = new User(1, "Admin", "Info@WieZitWaarVandaag.nl", true, false, string.Empty, "@WZWV", string.Empty, string.Empty, string.Empty, false, true, "Hoofd account van wie zit waar vandaag", string.Empty, "Geheim");
        }

        public bool LoginUser(string Username, string Password, bool cookie = false)
        {
            try
            {
                User user = CheckPassword(Username, Password, cookie);

                if (user.ID != 0)
                {
                    HttpContext.Current.Session["UserData"] = user;
                    setLastOnline(user);
                    FeedbackHandler.AddMessage("Welcome " + user.Username + ", You are now logged in. <br />The last time you logged on was: " + user.LastOnline);
                    return true;
                }

                FeedbackHandler.AddError("Wrong username or password.");
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool IsLoggedIn()
        {
            User user;

            try
            {
                if (HttpContext.Current.Session["UserData"] != null)
                {
                    user = (User)HttpContext.Current.Session["UserData"];

                    if (user.ID != 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static User GetLoggedInUser()
        {
            User user;

            try
            {
                if (HttpContext.Current.Session["UserData"] != null)
                {
                    user = (User)HttpContext.Current.Session["UserData"];

                    if (user.ID != 0)
                    {
                        return UserHandler.UH.GetUserByID(user.ID);
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void Logout()
        {
            HttpContext.Current.Session["UserData"] = null;
        }

        private User CheckPassword(string Username, string Password, bool Cookie = false)
        {
            if (Password.Length == 0 && !Cookie)
            {
                FeedbackHandler.AddError("Please enter your password.");
                throw new Exception("Please enter your password.");
            }

            if (25 < Password.Length && !Cookie)
            {
                FeedbackHandler.AddError("Entered Password is too long.");
                throw new Exception("Entered Password is too long.");
            }
            
            if (!Cookie)
            {
                Password = Encrypt.EncryptPassword(Password);
            }

            List<User> UserList = base.CustomQuery(0, 
                new string[] { "@UserName", "@Password" },
                new object[] { Username, Password }
                ).Cast<User>().ToList();

            if (UserList.Count != 0)
            {
                if (UserList[0].IsBanned)
                {
                    FeedbackHandler.AddError("You have been banned.");
                    throw new Exception("You have been banned.");
                }

                if (!(UserList[0].EmailConfirmed))
                {
                    FeedbackHandler.AddError("Your Email has not yet been confirmed, Please check your Email.");
                    throw new Exception("Your Email has not yet been confirmed, Please check your Email.");
                }

                return UserList[0];
            }
            else
            {
                FeedbackHandler.AddError("Username or password is incorrect.");
                throw new Exception("Username or password is incorrect.");
            }

        }

        public User GetUserByID(int ID)
        {
            if (ID == 0)
            {
                return UserHandler.EmptyUser;
            }
            else
            {
                return base.GetObjectByID(ID) as User;
            }
        }

        public List<User> SearchForUsersByName(string SearchQuery)
        {
            return base.GetObjectByFieldsAndSearchQuery(new Field[] { UserHandler.UsernameField }, SearchQuery, false, 0, OrderBy.ASC, UserHandler.UsernameField).Cast<User>().ToList();
        }

        public List<User> GetUserList(bool AddDefaultUser = false)
        {
            List<User> userList = base.GetObjectList(0, OrderBy.ASC, UserHandler.UsernameField).Cast<User>().ToList();

            if (AddDefaultUser)
            {
                userList.Insert(0, UserHandler.EmptyUser);
            }

            return userList;
        }

        public User UpdateUser(User user)
        {
            User LoggedInUser = UserHandler.GetLoggedInUser();

            if (user.ID == LoggedInUser.ID || LoggedInUser.IsOwner || LoggedInUser.IsAdmin)
            {
                user.downloadImage();
                return base.UpdateObject(user) as User;
            }
            else
            {
                FeedbackHandler.AddError("Insufficient rights.");
                return GetUserByID(user.ID);
            }
        }

        public void ChangePassword(User user, string NewPassword)
        {
            if (NewPassword.Length < 4)
            {
                FeedbackHandler.AddError("Entered password is too short.");
                return;
            }

            User LoggedInUser = null;

            if (IsLoggedIn())
            {
                LoggedInUser = GetLoggedInUser();

                if (
                    ((LoggedInUser.IsAdmin) && !LoggedInUser.IsOwner && !LoggedInUser.IsAdmin)
                    ||
                    (LoggedInUser.IsOwner)
                    ||
                    (LoggedInUser.ID == user.ID)
                    )
                {
                    base.CustomQuery(1, new string[] { "@Password", "@ID" } , new object[] { Encrypt.EncryptPassword(NewPassword), user.ID } );
                }
                else
                {
                    FeedbackHandler.AddError("Insufficient rights to change this password.");
                }
            }
        }

        public void setLastOnline(User user)
        {
            base.CustomQuery(2, new string[] { "@LastOnline", "@ID" }, new object[] { TimeConverter.GetDateTimeAsString(), user.ID });
        }

        public void DeleteUser(User user)
        {
            User LoggedInUser = GetLoggedInUser();

            if (LoggedInUser == null)
            {
                FeedbackHandler.AddError("not logged in.");
                return;
            }

            if (LoggedInUser.IsOwner)
            {
                base.DeleteObject(user);
            }
            else
            {
                FeedbackHandler.AddError("U are not allowed to do this.");
            }
        }

        public override string ToString()
        {
            return "UserHandler";
        }
    }
}