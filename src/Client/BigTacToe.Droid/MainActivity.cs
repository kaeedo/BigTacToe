﻿using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Content.PM;
using Xamarin.Forms.Platform.Android;

namespace BigTacToe.Android
{
    [Activity(Label = "Big Tac Toe", Icon = "@mipmap/ic_launcher", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = (ConfigChanges.ScreenSize | ConfigChanges.Orientation))]
    public class MainActivity : FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            //FormsAppCompatActivity.TabLayoutResource = Resource.Layout.Tabbar;
            //FormsAppCompatActivity.ToolbarResource = Resource.Layout.Toolbar;
            base.OnCreate(bundle);

            Xamarin.Essentials.Platform.Init(this, bundle);

            Xamarin.Forms.Forms.Init(this, bundle);

            var appcore = new BigTacToe.App();
            this.LoadApplication(appcore);
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}