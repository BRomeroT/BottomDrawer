using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace DrawerDemo
{
    /// <seealso cref="https://youtu.be/kd0fuWT2Xyg"/>
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        bool bottomDrawerIsVisible = false;
        uint duration = 100;
        double openY = (Device.RuntimePlatform == "Android") ? 20 : 60;
        double lastPanY = 0;
        double limitAction = 50;

        private async void SwitchBottomDrawer_Clicked(object sender, EventArgs e)
        {
            if (bottomDrawerIsVisible) await CloseDrawer();
            else await OpenDrawer();
        }

        private async Task OpenDrawer()
        {
            await Task.WhenAll(
                BottomDrawer.TranslateTo(0, openY, duration, Easing.SinIn)
            );
            bottomDrawerIsVisible = true;
        }

        private async Task CloseDrawer()
        {
            await Task.WhenAll(
                BottomDrawer.TranslateTo(0, 200, duration, Easing.SinIn)
            );
            bottomDrawerIsVisible = false;
        }

        //double originalHeight;
        //Thickness margin;
        private async void BottonDrawer_PanUpdated(object sender, PanUpdatedEventArgs e)
        {
            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    //originalHeight = BottomDrawer.Height;
                    //margin = BottomDrawer.Margin;
                    break;
                case GestureStatus.Running:
                    if (e.TotalY < 30) return; //avoid move without significative panning
                    lastPanY = e.TotalY;
                    if (e.TotalY > 0)
                    {
                        BottomDrawer.TranslationY = openY + e.TotalY;
                        if (e.TotalY > limitAction) await CloseDrawer();
                    }
                        //await BottomDrawer.TranslateTo(0, e.TotalY);

                    StatusDrawer.Text = $"{e.StatusType}:{e.GestureId} {Environment.NewLine}" +
                        $"{e.StatusType} - x:{e.TotalX}, y:{e.TotalY} {Environment.NewLine}" +
                        $"{BottomDrawer.GetScreenCoordinates()}";
                    //margin.Top = e.TotalY;
                    //BottomDrawer.HeightRequest = originalHeight + (-1 * e.TotalY);
                    break;
                case GestureStatus.Canceled:
                case GestureStatus.Completed:
                    if (lastPanY < limitAction) await OpenDrawer();
                    else await CloseDrawer();
                    //BottomDrawer.HeightRequest = originalHeight;
                    //BottomDrawer.TranslateTo(0, 0);
                    //StatusDrawer.Text = string.Empty;
                    break;
                default:
                    break;
            }

        }

    }
}
