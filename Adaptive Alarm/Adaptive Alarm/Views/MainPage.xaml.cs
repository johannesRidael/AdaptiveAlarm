﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Adaptive_Alarm.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : ContentPage
    {

        //public string wakeUpTime { get; } = "Waking you up at";

        public MainPage()
        {
            InitializeComponent();
        }
    }
}