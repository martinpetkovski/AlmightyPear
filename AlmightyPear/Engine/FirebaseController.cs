﻿using Core;
using Firebase.Auth;
using FireSharp;
using FireSharp.Interfaces;
using FireSharp.Response;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;


namespace Engine
{
    public class FirebaseController
    {
        private static string _authSecret = "YbUiUdnfhpivXNsiR2UjcAYZkJeiRQo4sSUnFYM5";
        private static string _basePath = "https://almightypear-c67cf.firebaseio.com";
        private static string _apiKey = " AIzaSyABNDJwdJ2ZnDjwWRb9FG6Kwd-z8_5wbJI";
        private static string _appKey = "Ch3ckm3g.@#.33221";
        private FirebaseAuth _token;
        private async Task<FirebaseAuth> RefreshTokenAsync()
        {
            if (_token.IsExpired())
            {
                var authProvider = new FirebaseAuthProvider(new Firebase.Auth.FirebaseConfig(_apiKey));
                _token = await authProvider.RefreshAuthAsync(_token);
            }

            return _token;
        }
        private string _emailToken;
        private string _passToken;
        private EventStreamResponse bookmarksEventStream;

        private IFirebaseConfig _config = new FireSharp.Config.FirebaseConfig
        {
            AuthSecret = _authSecret,
            BasePath = _basePath
        };

        private IFirebaseClient _client;

        public void Initialize()
        {
            _client = new FirebaseClient(_config);
        }

        ~FirebaseController()
        {
            if (bookmarksEventStream != null)
            {
                try
                {
                    bookmarksEventStream.Dispose();
                }
                catch (Exception) { }
            }
        }

        public string GetTokenSecret()
        {
            return GetUserPath() + _appKey;
        }

        public async Task<bool> AuthenticateUserAsync(string email, string password, bool initialize = true)
        {
            var authProvider = new FirebaseAuthProvider(new Firebase.Auth.FirebaseConfig(_apiKey));
            FirebaseAuthLink auth;
            auth = await authProvider.SignInWithEmailAndPasswordAsync(email, password);

            if (auth.User.LocalId != "")
            {
                if (initialize)
                {
                    Env.UserData.Initialize(auth.User.LocalId, auth.User.Email, auth.User.DisplayName, auth.User.PhotoUrl);
                    string ps = GetTokenSecret();
                    _emailToken = StringCipher.Encrypt(email, ps);
                    _passToken = StringCipher.Encrypt(password, ps);
                    _token = auth;
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        public struct SChangePasswordResult
        {
            public bool success;
            public string message;
        }

        public async Task<SChangePasswordResult> ChangePasswordAsync(string email, string oldPassword, string password, string repeatPassword)
        {
            SChangePasswordResult result = new SChangePasswordResult();
            bool authSuccess = false;
            try
            {
                authSuccess = await AuthenticateUserAsync(email, oldPassword, false);
            }
            catch (Exception)
            {
                result.message = "Entered wrong email and/or password";
                result.success = false;
                return result;
            }

            if (!authSuccess)
            {
                result.message = "Entered wrong password";
                result.success = false;
                return result;
            }

            if (password != repeatPassword)
            {
                result.message = "Passwords do not match";
                result.success = false;
                return result;
            }

            var authProvider = new FirebaseAuthProvider(new FirebaseConfig(_apiKey));
            await RefreshTokenAsync();
            var link = authProvider.ChangeUserPassword(_token.FirebaseToken, password);

            if (link.Status == TaskStatus.WaitingForActivation)
            {
                result.message = "Password changed!";
                result.success = true;
                return result;
            }
            else
            {
                result.message = "Error changing password.";
                result.success = false;
                return result;
            }
        }

        public async void UpdateProfileAsync(string displayName, string photoUrl) // needs validation
        {
            FirebaseAuthProvider authProvider = new FirebaseAuthProvider(new FirebaseConfig(_apiKey));
            await RefreshTokenAsync();
            FirebaseAuthLink link = await authProvider.UpdateProfileAsync(_token.FirebaseToken, displayName, photoUrl);
            Env.UserData.DisplayName = link.User.DisplayName;
            Env.UserData.PhotoUrl = link.User.PhotoUrl;

            FirebaseResponse response = await _client.UpdateAsync("profile/" + Env.UserData.ID, Env.UserData.CustomModel);

        }

        public async Task LoadCustomProfileDataAsync() // needs validation
        {
            FirebaseResponse response = await _client.GetAsync("profile/" + Env.UserData.ID);
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                await _client.SetAsync("profile/" + Env.UserData.ID, Env.UserData.CustomModel);
            }
            else
            {
                Env.UserData.CustomModel = response.ResultAs<UserModel.CustomUserModel>();
            }
        }

        public void LogOutUser()
        {
            bookmarksEventStream.Dispose();
            Env.UserData.Deinitialize();
            Env.BinController.Deinitalize();
        }

        public async Task<bool> CreateBookmarkAsync(string category, string content)
        {
            BookmarkModel bookmarkData = new BookmarkModel();
            bookmarkData.ID = Guid.NewGuid().ToString();
            bookmarkData.Path = category;
            bookmarkData.Content = content;
            bookmarkData.TimeCreated = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            bookmarkData.TimeModified = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            SetResponse response = await _client.SetAsync("bookmarks/" + Env.UserData.ID + "/" + bookmarkData.ID, bookmarkData);
            return response.StatusCode == System.Net.HttpStatusCode.OK;
        }

        public async Task CreateOrUpdateTheme(ThemeModel theme)
        {
            if ((await _client.GetAsync("themes/" + theme.Name)).StatusCode != System.Net.HttpStatusCode.OK)
            {
                SetResponse response = await _client.SetAsync("themes/" + theme.Name, theme);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    Env.UserData.AddTheme(theme);
                }
            }
            else
            {
                if (Env.UserData.Email == theme.OwnerUser ||
                    theme.OwnerUser == "deeeeelay@gmail.com" ||
                    Env.UserData.Email == "deeeeelay@gmail.com")
                {
                    FirebaseResponse response = await _client.UpdateAsync("themes/" + theme.Name, theme);
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        Env.UserData.Themes[theme.Name] = theme;
                    }
                }
            }
        }

        public async Task UpdateBookmarkAsync(BookmarkModel bookmark)
        {
            FirebaseResponse response = await _client.GetAsync("bookmarks/" + Env.UserData.ID + "/" + bookmark.ID);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                bookmark.TimeModified = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                await _client.UpdateAsync("bookmarks/" + Env.UserData.ID + "/" + bookmark.ID, bookmark);
            }

        }

        public void DeleteBookmark(BookmarkModel bookmark)
        {
            _client.DeleteAsync("bookmarks/" + Env.UserData.ID + "/" + bookmark.ID);
        }

        public void HandleBookmarkChanged(string data, string path)
        {
            try
            {
                string trimmed = path.Trim('/');
                string[] pathTokens = trimmed.Split('/');

                string bookmarkId = pathTokens[pathTokens.Length - 2];
                string propertyContent = pathTokens[pathTokens.Length - 1];

                lock (Env.UserData.Bookmarks)
                {
                    if (!Env.UserData.Bookmarks.ContainsKey(bookmarkId))
                    {
                        Env.UserData.Bookmarks.Add(bookmarkId, new BookmarkModel());
                    }
                }

                BookmarkModel bookmark = Env.UserData.Bookmarks[bookmarkId];

                PropertyInfo property = bookmark.GetType().GetProperty(propertyContent);
                if (property != null && property.CanWrite)
                {
                    if (property.PropertyType == typeof(long))
                    {
                        property.SetValue(bookmark, long.Parse(data));
                    }
                    else
                    {
                        property.SetValue(bookmark, data);
                    }
                }
            }
            catch (Exception) { }
        }

        public async Task RegisterBookmarksListener()
        {
            bookmarksEventStream = await _client.OnAsync("bookmarks/" + Env.UserData.ID + "/",
            added: (sender, args, context) =>
            {
                HandleBookmarkChanged(args.Data, args.Path);
            },
            changed: (sender, args, context) =>
            {
                HandleBookmarkChanged(args.Data, args.Path);
            },
            removed: (sender, args, context) =>
            {
                string trimmed = args.Path.Trim('/');
                string[] pathTokens = trimmed.Split('/');
                Env.BinController.DeleteBookmark(pathTokens[0]);
            });
        }

        public async Task GetThemesAsync()
        {
            string path = GetUserPath() + "\\themes.ccm";

            if (File.Exists(path))
            {
                BinaryReader reader = new BinaryReader(File.OpenRead(path));
                string json = reader.ReadString();
                reader.Close();
                Env.UserData.Themes = JsonConvert.DeserializeObject<Dictionary<string, ThemeModel>>(json);
            }
            else
            {
                string userId = Env.UserData.ID;
                FirebaseResponse response = await _client.GetAsync("themes");
                try
                {
                    Env.UserData.Themes = response.ResultAs<Dictionary<string, ThemeModel>>();

                    string json = JsonConvert.SerializeObject(Env.UserData.Themes);

                    BinaryWriter writer = new BinaryWriter(File.Open(path, FileMode.Create));
                    writer.Write(json);
                    writer.Close();
                }
                catch (JsonSerializationException e)
                {
                    Debug.Write(e.Message);
                }
            }
        }

        public string GetUserPath()
        {
            string dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Checkmeg";
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            return dir;
        }

        public string GetOrCreateTokenPath()
        {
            return GetUserPath() + "\\token.ccm";
        }

        public bool ReadToken()
        {
            string path = GetOrCreateTokenPath();
            if (File.Exists(path))
            {
                BinaryReader reader = new BinaryReader(File.OpenRead(path));
                _emailToken = StringCipher.Decrypt(reader.ReadString(), GetTokenSecret());
                _passToken = StringCipher.Decrypt(reader.ReadString(), GetTokenSecret());
                reader.Close();
                return true;
            }
            else
            {
                return false;
            }
        }

        public void DeleteToken()
        {
            string path = GetOrCreateTokenPath();
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        public void SaveToken()
        {
            string path = GetOrCreateTokenPath();
            BinaryWriter writer = new BinaryWriter(File.Open(path, FileMode.Create));
            writer.Write(_emailToken);
            writer.Write(_passToken);
            writer.Close();
        }

        private async Task PostSignInAsync()
        {
            await RegisterBookmarksListener();
        }

        public async Task<string> SignInUserAsync(string email, string password, bool remember)
        {
            string retVal = "";
            bool isOk = true;
            try
            {
                await AuthenticateUserAsync(email, password);
            }
            catch (FirebaseAuthException ex)
            {
                retVal = "Failed to sign in! Reason: " + ex.Reason;
                isOk = false;

            }

            if (!isOk)
            {
                return retVal;
            }

            if (remember)
            {
                Env.FirebaseController.SaveToken();
            }

            await PostSignInAsync();

            return "";
        }

        public async Task<string> SignInUserAsync()
        {
            string retVal = "";
            bool isOk = true;
            try
            {
                bool tokenExists = ReadToken();
                if (tokenExists)
                {
                    bool success = await AuthenticateUserAsync(_emailToken, _passToken);
                    if (!success)
                    {
                        retVal = "Failed to sign in! Reason: User auth expired";
                        isOk = false;
                    }
                }
                else
                {
                    Env.UserData.Deinitialize();
                    retVal = " ";
                    isOk = false;
                }

            }
            catch (FirebaseAuthException ex)
            {
                retVal = "Failed to sign in! Reason: " + ex.Reason;
                isOk = false;

            }

            if (!isOk)
            {
                return retVal;
            }

            await PostSignInAsync();

            return "";
        }
    }
}
