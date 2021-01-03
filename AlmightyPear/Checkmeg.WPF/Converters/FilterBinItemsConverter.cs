using Engine;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Checkmeg.WPF.Converters
{
    class FilterBinItemsConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values.Count() == 2)
            {
                object oBinItems = values[0];
                object oFilter = values[1];

                if (oBinItems != null &&
                oFilter != null &&
                oBinItems is ObservableCollection<IBinItem> &&
                oFilter is string)
                {
                    string currentFilter = (string)oFilter;

                    //if (currentFilter.Length <= 2)
                    //    return new ObservableCollection<IBinItem>();

                    string[] filterTokens = currentFilter.Split(' ');
                    List<FilterToken> tokenMods = new List<FilterToken>();

                    tokenMods.Add(new FilterToken(FilterToken.FilterTokenType.Any));
                    foreach (string token in filterTokens)
                    {
                        if (token == "-q" || token == "-p" || token == "-d" || token == "-t" || token == "-a")
                        {
                            tokenMods.Add(new FilterToken(token));
                        }
                        else
                        {
                            tokenMods.Last().AddToken(token);
                        }
                    }

                    Dictionary<string, int> PathScores = new Dictionary<string, int>();
                    ObservableCollection<IBinItem> binItems = (ObservableCollection<IBinItem>)oBinItems;
                    IEnumerable<IBinItem> orderedBinItems = binItems.Where(x => true);
                    foreach (FilterToken token in tokenMods)
                    {
                        orderedBinItems = orderedBinItems
                                            .Where(x => x.FilterScore(token, ref PathScores) > 75)
                                            .OrderByDescending(x => x.FilterScore(token, ref PathScores))
                                            .ThenBy(x => x.PathDepth);
                    }

                    return orderedBinItems;
                }
            }

            return new ObservableCollection<IBinItem>();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
