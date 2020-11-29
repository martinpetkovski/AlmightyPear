using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlmightyPear.Model
{
    public class FilterToken
    {
        public enum FilterTokenType
        {
            Query,
            Path,
            Date,
            Type,
            Any,
            All
        }

        private List<string> _tokens;
        public FilterTokenType TokenType { get; set; }

        public void AddToken(string token)
        {
            _tokens.Add(token);
        }

        public override string ToString() 
        {
            string retVal = "";

            if (_tokens.Count != 0)
            {
                foreach (string str in _tokens)
                {
                    retVal += " " + str;
                }
            }

            return retVal.ToLower(); // case insensitive
        }

        public FilterToken(FilterTokenType type)
        {
            _tokens = new List<string>();
            TokenType = type;
        }

        public FilterToken(string tokenFlag)
        {
            _tokens = new List<string>();
            if (tokenFlag == "-q")
                TokenType = FilterTokenType.Query;
            else if (tokenFlag == "-p")
                TokenType = FilterTokenType.Path;
            else if (tokenFlag == "-d")
                TokenType = FilterTokenType.Date;
            else if (tokenFlag == "-t")
                TokenType = FilterTokenType.Type;
            else if (tokenFlag == "-a")
                TokenType = FilterTokenType.All;
            else
                TokenType = FilterTokenType.Any;
        }
    }

    public interface IBinItem
    {
        string Path { get; set; }
        int PathDepth { get; }
        int FilterScore(FilterToken token, ref Dictionary<string, int> pathScores);
    }
}
