using AlmightyPear.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace AlmightyPear.Converters
{
    class FilterBinItemsConverter : IMultiValueConverter
    {
        public object Convert(object []values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values.Count() == 2 )
            {
                object oBinItems = values[0];
                object oFilter = values[1];

                if (oBinItems != null &&
                oFilter != null &&
                oBinItems is ObservableCollection<IBinItem> &&
                oFilter is string)
                {
                    string currentFilter = (string)oFilter;
                    string[] filterTokens = currentFilter.Split(' ');
                    List<FilterToken> tokenMods = new List<FilterToken>();

                    tokenMods.Add(new FilterToken(FilterToken.FilterTokenType.Any));
                    foreach(string token in filterTokens)
                    {
                        if (token == "-q" || token == "-p" || token == "-d" || token == "-t")
                        {
                            tokenMods.Add(new FilterToken(token));
                        }
                        else
                        {
                            tokenMods.Last().AddToken(token);
                        }
                    }

                    ObservableCollection<IBinItem> binItems = (ObservableCollection<IBinItem>)oBinItems;
                    IEnumerable<IBinItem> orderedBinItems = binItems.Where(x => true);
                    foreach (FilterToken token in tokenMods)
                    {
                        orderedBinItems = orderedBinItems
                                            .Where(x => x.FilterScore(token) > 75)
                                            .OrderByDescending(x => x.FilterScore(token))
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
