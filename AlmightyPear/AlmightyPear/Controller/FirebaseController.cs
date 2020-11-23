﻿using AlmightyPear.Model;
using Firebase.Auth;
using FireSharp;
using FireSharp.Interfaces;
using FireSharp.Response;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace AlmightyPear.Controller
{
    class FirebaseController
    {
        private static string _authSecret = "YbUiUdnfhpivXNsiR2UjcAYZkJeiRQo4sSUnFYM5";
        private static string _basePath = "https://almightypear-c67cf.firebaseio.com";
        private static string _apiKey = " AIzaSyABNDJwdJ2ZnDjwWRb9FG6Kwd-z8_5wbJI";
        private string _token;

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

        public async Task<bool> AuthenticateUserAsync(string email, string password)
        {
            var authProvider = new FirebaseAuthProvider(new Firebase.Auth.FirebaseConfig(_apiKey));
            var auth = await authProvider.SignInWithEmailAndPasswordAsync(email, password);
            if (auth.User.LocalId != "")
            {
                Env.UserData.Initialize(auth.User.LocalId, auth.User.Email);

                _token = auth.FirebaseToken;

                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> AuthenticateUserAsync()
        {
            var authProvider = new FirebaseAuthProvider(new Firebase.Auth.FirebaseConfig(_apiKey));
            var link = await authProvider.GetUserAsync(_token);

            if (link.LocalId != "")
            {
                Env.UserData.Initialize(link.LocalId, link.Email);
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<string> ChangePasswordAsync(string oldPassword, string password, string repeatPassword)
        {
            bool authSuccess = await AuthenticateUserAsync(Env.UserData.Email, oldPassword);
            if (!authSuccess)
                return "Entered wrong password";

            if ( password != repeatPassword)
                return "Passwords do not match";

            var authProvider = new FirebaseAuthProvider(new FirebaseConfig(_apiKey));
            var link = authProvider.ChangeUserPassword(_token, password);

            if (link.Status == TaskStatus.RanToCompletion)
                return "";
            else
                return "Error changing password.";
        }

        public void LogOutUser()
        {
            Env.UserData.Deinitialize();
            Env.BinController.Deinitalize();
        }

        public async Task CreateBookmarkAsync(string category, string content)
        {
            BookmarkModel bookmarkData = new BookmarkModel();
            bookmarkData.ID = Guid.NewGuid().ToString();
            bookmarkData.Path = category;
            bookmarkData.Content = content;
            bookmarkData.TimeCreated = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            bookmarkData.TimeModified = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            SetResponse response = await _client.SetAsync("bookmarks/" + Env.UserData.ID + "/" + bookmarkData.ID, bookmarkData);
            if(response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                Env.UserData.Bookmarks.Add(bookmarkData.ID, bookmarkData);
                Env.BinController.AddBookmark(bookmarkData);
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

        public async Task GetBookmarksByUserAsync()
        {
            string userId = Env.UserData.ID;
            FirebaseResponse response = await _client.GetAsync("bookmarks/" + userId + "/");
            try
            {
                Env.UserData.Bookmarks = response.ResultAs<Dictionary<string, BookmarkModel>>();
            }
            catch(Newtonsoft.Json.JsonSerializationException e)
            {
                Debug.Write(e.Message);
            }
        }

        public bool ReadToken()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "sgnittes.apf";
            if (File.Exists(path))
            {
                BinaryReader reader = new BinaryReader(File.OpenRead(path));
                _token = reader.ReadString();
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
            string path = AppDomain.CurrentDomain.BaseDirectory + "sgnittes.apf";
            if(File.Exists(path))
            {
                File.Delete(path);
            }
        }

        public void SaveToken()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "sgnittes.apf";
            BinaryWriter writer = new BinaryWriter(File.Open(path, FileMode.Create));
            writer.Write(_token);
            writer.Close();
        }

        private async Task PostSignInAsync()
        {
            await Env.FirebaseController.GetBookmarksByUserAsync();
            Env.BinController.GenerateBinTree();
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
                    bool success = await AuthenticateUserAsync();
                    if (!success)
                    {
                        retVal = "Failed to sign in! Reason: User auth expired";
                        isOk = false;
                    }
                }
                else
                    isOk = false;

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