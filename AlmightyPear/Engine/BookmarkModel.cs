using FuzzySharp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Engine
{
    public class BookmarkModel : IBinItem, INotifyPropertyChanged
    {
        private static WebClient _webClient;
        private static WebClient WebClient
        {
            get
            {
                if (_webClient == null)
                    _webClient = new WebClient();
                return _webClient;
            }

        }
        private string GetWebsiteTitle(string address)
        {
            WebClient x = new WebClient();
            string source = x.DownloadString(address);

            string title = Regex.Match(source, @"\<title\b[^>]*\>\s*(?<Title>[\s\S]*?)\</title\>",
    RegexOptions.IgnoreCase).Groups["Title"].Value;

            return title;
        }

        private void RefreshType()
        {
            Uri uriResult;
            bool result = Uri.TryCreate(Content, UriKind.Absolute, out uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

            if (result)
            {
                if (Content.Contains(".png") || Content.Contains(".jpg"))
                {
                    Type = "image";
                }
                else
                {
                    Type = "link";
                    Caption = GetWebsiteTitle(Content);
                }
            }
            else if(Directory.Exists(Content))
            {
                Type = "dir";
            }
            else if(File.Exists(Content))
            {
                Type = "file";
            }
            else
            {
                Type = "text";
            }
        }


        private string _id;
        private string _userId;
        private string _path;
        private string _content;
        private string _caption;
        private long _timeCreated;
        private long _timeModified;
        private string _type;

        public string ID
        {
            get
            {
                if (_id == "")
                    _id = Guid.NewGuid().ToString();
                return _id;
            }
            set
            {
                _id = value;
                OnPropertyChanged();
            }
        }

        public string UserId
        {
            get
            {
                return _userId;
            }
            set
            {
                _userId = value;
                OnPropertyChanged();
            }
        }

        public string Path
        {
            get
            {
                return _path;
            }
            set
            {
                _path = value;
                OnPropertyChanged();
                Env.BinController.GenerateBinTree();
            }
        }

        public string Content
        {
            get
            {
                return _content;
            }
            set
            {
                _content = value;
                Task.Run((Action)RefreshType);
                OnPropertyChanged();
                OnPropertyChanged("Type");
            }
        }

        public string Caption
        {
            get
            {
                if (_caption == null)
                    _caption = "";
                return _caption;
            }
            set
            {
                _caption = value;
                OnPropertyChanged();
            }
        }

        public long TimeCreated
        {
            get
            {
                return _timeCreated;
            }
            set
            {
                _timeCreated = value;
                OnPropertyChanged();
            }
        }

        public long TimeModified
        {
            get
            {
                return _timeModified;
            }
            set
            {
                _timeModified = value;
                OnPropertyChanged();
            }
        }

        public string Type
        {
            get
            {
                return _type;
            }
            set
            {
                _type = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        public int PathDepth
        {
            get
            {
                return Path.Split(Env.PathSeparator).Length - 1;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private DateTime UnixToDateTime(long unixTime)
        {
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTime);
            return dtDateTime;
        }

        public int FilterScore(FilterToken filter, ref Dictionary<string, int> pathScores)
        {
            if (filter.TokenType == FilterToken.FilterTokenType.All)
                return 100;

            int existScore = 0;
            if (pathScores.TryGetValue(ID, out existScore))
            {
                return existScore;
            }

            string filterString = filter.ToString();
            if (filterString == "")
                return 100;

            int score = 0;

            if (filter.TokenType == FilterToken.FilterTokenType.Query ||
                filter.TokenType == FilterToken.FilterTokenType.Any)
            {
                int contentScore;
                int captionScore;

                contentScore = Fuzz.PartialTokenSetRatio(Content.ToLower(), filter.ToString()); // case insensitive
                captionScore = Fuzz.PartialTokenSetRatio(Caption.ToLower(), filter.ToString()); // case insensitive

                score = Math.Max(contentScore, captionScore);
            }
            if (filter.TokenType == FilterToken.FilterTokenType.Path)
            {
                score = Math.Max(score, BinModel.FilterPath(Path, filterString, Env.PathSeparator));
            }
            else if (filter.TokenType == FilterToken.FilterTokenType.Any)
            {
                score = Math.Max(score, BinModel.FilterPath(Path, filterString, Env.PathSeparator));
                score = Math.Max(score, BinModel.FilterPath(Path, filterString, ' '));
            }
            if (filter.TokenType == FilterToken.FilterTokenType.Date ||
                filter.TokenType == FilterToken.FilterTokenType.Any)
            {
                string[] datesStr = filter.ToString().Split(' ');
                List<DateTime> dates = new List<DateTime>();
                foreach (string date in datesStr)
                {
                    DateTime result;
                    if (DateTime.TryParse(date, out result))
                    {
                        dates.Add(result);
                    }
                }

                if (dates.Count > 0)
                {
                    DateTime startDate = dates[0];
                    DateTime endDate = dates[dates.Count - 1];
                    if (dates.Count == 1)
                    {
                        endDate = endDate.AddDays(1);
                    }

                    DateTime createdTime = UnixToDateTime(TimeCreated);

                    if (createdTime >= startDate && createdTime <= endDate)
                    {
                        score = 100;
                    }
                }
            }
            if (filter.TokenType == FilterToken.FilterTokenType.Type ||
                   filter.TokenType == FilterToken.FilterTokenType.Any)
            {
                if (Type == filter.ToString().Trim(' ').ToLower()) // case insensitive
                    score = 100;
            }

            pathScores.Add(ID, score);
            return score;
        }
    }
}
