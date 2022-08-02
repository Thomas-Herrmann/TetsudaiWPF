using JishoNET.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;

namespace TetsudaiWPF
{
    internal class WordWrapper : Grid
    {
        private JishoDefinition _word;
        private static readonly Brush foregroundBrush = new SolidColorBrush(Colors.WhiteSmoke);
        private static readonly Brush jlptBrush = new SolidColorBrush(Colors.CadetBlue);
        private static readonly Regex jlptRegex = new Regex(@"^jlpt-n\d$");

        public WordWrapper(JishoDefinition word)
        {
            _word = word;

            ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });

            var sidePanel = new StackPanel { Orientation = Orientation.Vertical, Background = foregroundBrush, Margin = new Thickness(5, 0, 0, 5) };
            var contentPanel = new StackPanel { Orientation = Orientation.Vertical, Background = foregroundBrush, Margin = new Thickness(5, 0, 5, 5) };

            SetColumn(sidePanel, 0);
            SetColumn(contentPanel, 1);

            var reading = _word.Japanese.Count > 0 ? (_word.Japanese[0].Word != null ? $"{_word.Japanese[0].Word}【{_word.Japanese[0].Reading}】" : _word.Japanese[0].Reading) : "error"; // any JishoDefinition should have at least one reading
            var readingLabel = new Label { Content = reading, FontWeight = FontWeights.Bold };
            var commonWord = new Label { Background = new SolidColorBrush(Colors.Green), Foreground = foregroundBrush, Content = "common word", FontWeight = FontWeights.Bold };

            sidePanel.Children.Add(readingLabel);

            if (_word.IsCommon)
                sidePanel.Children.Add(commonWord);

            var jlptLevel = _word.Jlpt.Where(s => jlptRegex.IsMatch(s)).MaxBy(s => s[6] - '0');

            if (jlptLevel != null)
                sidePanel.Children.Add(new Label { Background = jlptBrush, Foreground = foregroundBrush, Content = jlptLevel, FontWeight = FontWeights.Bold });

            for (int i = 0; i < _word.Senses.Count; ++i)
                contentPanel.Children.Add(new TextBlock { Text = $"{i + 1}. " + _word.Senses[i].EnglishDefinitions.Aggregate((currs, news) => $"{currs}; {news}"), Foreground = new SolidColorBrush(Colors.Black), Padding = new Thickness(5) });

            Children.Add(sidePanel);
            Children.Add(contentPanel);
            sidePanel.UpdateLayout();
        }
    }
}
