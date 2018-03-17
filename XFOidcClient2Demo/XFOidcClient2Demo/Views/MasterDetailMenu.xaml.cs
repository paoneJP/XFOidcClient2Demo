/*
 *  Xamarin.Forms + IdendityModel.OidcClient2 demonstration application.
 *    Author: Takashi Yahata (@paoneJP)
 *    Copyright: (c) 2018 Takashi Yahata
 *    License: MIT License
 */

using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace XFOidcClient2Demo
{
    public partial class MasterDetailMenu : ContentPage
    {
        public ListView ListView { get { return listView; } }

        private static List<MenuItem> MenuItemList = new List<MenuItem> {
            new MenuItem {
                Title = StringResources.UiMenuMainPage,
                Icon = "ic_home_black.png",
                TargetPage = typeof(MainPage)
            },
            new MenuItem {
                Title = StringResources.UiMenuLogPage,
                Icon = "ic_list_black.png",
                TargetPage = typeof(LogPage)
            }
        };

        public MasterDetailMenu()
        {
            InitializeComponent();
            listView.ItemsSource = MenuItemList;
        }
    }

    public class MenuItem
    {
        public string Title { get; set; }
        public string Icon { get; set; }
        public Type TargetPage { get; set; }
    }
}