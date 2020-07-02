﻿using Xamarin.Forms.Platform.WPF;

namespace BigTacToe.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : FormsApplicationPage
    {
        public MainWindow()
        {
            InitializeComponent();
            Xamarin.Forms.Forms.Init();
            LoadApplication(new BigTacToe.App());
        }
    }
}
