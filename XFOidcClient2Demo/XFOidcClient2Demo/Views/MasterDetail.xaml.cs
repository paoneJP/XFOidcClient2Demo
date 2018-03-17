/*
 *  Xamarin.Forms + IdendityModel.OidcClient2 demonstration application.
 *    Author: Takashi Yahata (@paoneJP)
 *    Copyright: (c) 2018 Takashi Yahata
 *    License: MIT License
 */

using System;
using Xamarin.Forms;

namespace XFOidcClient2Demo
{
    public partial class MasterDetail : MasterDetailPage
    {
        public MasterDetail()
        {
            InitializeComponent();
            MasterPage.ListView.ItemSelected += OnItemSelected;

        }

        private void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem is MenuItem item) {
                Detail = new NavigationPage((Page)Activator.CreateInstance(item.TargetPage));
                MasterPage.ListView.SelectedItem = null;
                IsPresented = false;
            }
        }
    }
}