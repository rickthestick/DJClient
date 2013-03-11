﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DJClientWPF.KaraokeService;
using System.Collections.ObjectModel;
using System.Windows.Media.Animation;

namespace DJClientWPF
{
    /// <summary>
    /// Interaction logic for AddSongRequestControl.xaml
    /// </summary>
    public partial class AddSongRequestControl : UserControl
    {
        public delegate void EventHandler(object source, EventArgs args);
        public event EventHandler NeedToCloseControl;

        private enum SearchType
        {
            Artist, Title
        }

        private SearchType searchType = SearchType.Artist;
        private ObservableCollection<SongSearchResult> searchResults;

        public AddSongRequestControl()
        {
            searchResults = new ObservableCollection<SongSearchResult>();

            InitializeComponent();

            RadioButtonArtist.Checked += RadioButton_Checked;
            RadioButtonTitle.Checked += RadioButton_Checked;
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            //Artist
            if ((bool)RadioButtonArtist.IsChecked)
            {
                searchType = SearchType.Artist;
            }
            //Title
            else if ((bool)RadioButtonTitle.IsChecked)
            {
                searchType = SearchType.Title;
            }
            GetSearchResults(TextBoxSearch.Text.Trim());
        }

        private void GetSearchResults(string term)
        {
            List<SongSearchResult> tempList = new List<SongSearchResult>();

            if (searchType == SearchType.Artist && !term.Equals(""))
                tempList = DJModel.Instance.GetMatchingArtistsInSongbook(term);
            else if (searchType == SearchType.Title && !term.Equals(""))
                tempList = DJModel.Instance.GetMatchingTitlesInSongbook(term);

            searchResults.Clear();
            foreach (SongSearchResult song in tempList)
                searchResults.Add(song);

            ListViewResults.ItemsSource = searchResults;
        }

        //Song was double clicked so add the selection to the queue
        private void ListBoxResults_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SongSearchResult songSearchResult = ((ListView)sender).SelectedItem as SongSearchResult;

            //Check if no user name was entered
            if (TextBoxUserName.Text.Trim().Equals(""))
            {
                ColorAnimation animation = new ColorAnimation();
                animation.From = Colors.White;
                animation.To = Color.FromArgb(255, 255, 125, 125);
                animation.Duration = new Duration(TimeSpan.FromMilliseconds(300));
                animation.AutoReverse = true;

                Storyboard s = new Storyboard();
                s.Duration = new Duration(new TimeSpan(0, 0, 1));
                s.Children.Add(animation);

                Storyboard.SetTarget(animation, TextBoxUserName);
                Storyboard.SetTargetProperty(animation, new PropertyPath("Background.Color"));

                s.Begin();

                return;
            }

            SongRequest request = new SongRequest();
            User user = new User();
            user.userID = -1;
            user.userName = TextBoxUserName.Text.Trim();

            request.user = user;
            request.songID = songSearchResult.Song.ID;

            DJModel.Instance.AddSongRequest(request, DJModel.Instance.SongRequestQueue.Count + 1);
            CloseControl();
        }

        //Animates this control closed
        public void CloseControl()
        {
            TextBoxSearch.Text = "";
            TextBoxUserName.Text = "";
            searchResults.Clear();

            if (NeedToCloseControl != null)
                NeedToCloseControl(this, new EventArgs());
        }

        private void CloseLabel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            CloseControl();
        }

        private void TextBoxSearch_KeyUp(object sender, KeyEventArgs e)
        {
            GetSearchResults(TextBoxSearch.Text.Trim());
        }

    }
}
