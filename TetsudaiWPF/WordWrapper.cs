using JishoNET.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private Label kanjiReading;
        private Label kanaReading;
        private Label commonWord;
        private List<Label> jlptLabels;

        public WordWrapper(JishoDefinition word)
        {
            _word = word;

            ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });

            var sidePanel = new StackPanel { Orientation = Orientation.Vertical, Background = foregroundBrush };
            var contentPanel = new StackPanel { Orientation = Orientation.Vertical, Background = foregroundBrush };

            SetColumn(sidePanel, 0);
            SetColumn(contentPanel, 1);

            kanjiReading = new Label { Content = _word.Japanese.Count > 0 ? _word.Japanese[0].Word : "error", FontWeight = FontWeights.Bold }; // any JishoDefinition should have at least one reading
            kanaReading = new Label { Content = _word.Japanese.Count > 0 ? _word.Japanese[0].Reading : "error" };
            commonWord = new Label { Background = new SolidColorBrush(Colors.Green), Foreground = foregroundBrush, Content = "common word", FontWeight = FontWeights.Bold };

            sidePanel.Children.Add(kanaReading);
            sidePanel.Children.Add(kanjiReading);

            if (_word.IsCommon)
                sidePanel.Children.Add(commonWord);

            jlptLabels = _word.Jlpt.Select(s => new Label { Background = jlptBrush, Foreground = foregroundBrush, Content = s, FontWeight = FontWeights.Bold }).ToList();

            jlptLabels.ForEach(l => sidePanel.Children.Add(l));

            contentPanel.Children.Add(new Label { Content = "Senses/Meanings", FontWeight = FontWeights.Bold, FontSize = 16 });

            for (int i = 0; i < _word.Senses.Count; ++i)
                contentPanel.Children.Add(new TextBlock { Text = $"{i + 1}. " + _word.Senses[i].EnglishDefinitions.Aggregate((currs, news) => $"{currs}; {news}"), Foreground = new SolidColorBrush(Colors.Black) });

            var alternativeReadings = "";

            for (int i = 1; i < _word.Japanese.Count - 1; ++i)
                alternativeReadings += $"{_word.Japanese[i].Word} ({_word.Japanese[i].Reading}), ";
            
            if (_word.Japanese.Count > 1)
            {
                alternativeReadings += $"{_word.Japanese[_word.Japanese.Count - 1].Word} ({_word.Japanese[_word.Japanese.Count - 1].Reading}).";

                contentPanel.Children.Add(new Label { Content = "Alternative readings", FontWeight = FontWeights.Bold, FontSize = 16 });
                contentPanel.Children.Add(new TextBlock { Text = alternativeReadings, Foreground = new SolidColorBrush(Colors.Black) });
            }

            Children.Add(sidePanel);
            Children.Add(contentPanel);
            ScaleElements();
            sidePanel.UpdateLayout();

            SizeChanged += (sender, e) => ScaleElements();
        }

        private void ScaleElements()
        {
            // TODO set font size
        }
    }
}
