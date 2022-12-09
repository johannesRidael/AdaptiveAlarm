using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Xamarin.Forms;

namespace Adaptive_Alarm.Views
{

    public partial class ScorePage : ContentPage
    {
        public ScorePage()
        {
            InitializeComponent();
            selections = GetSelection();
            this.BindingContext = this;
        }


        private ObservableCollection<Selection> selections;
        public ObservableCollection<Selection> Selections
        {
            get { return selections; }
            set
            {
                selections = value;
                OnPropertyChanged();
            }
        }

        private string name;
        public string SelectedName
        {
            get { return name; }
            set
            {
                name = value;
                OnPropertyChanged();
            }
        }

        private float amount;
        public float SelectedAmount
        {
            get { return amount; }
            set
            {
                amount = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Selection> GetSelection()
        {
            return new ObservableCollection<Selection>
            {
                // Puts score in this amount
                new Selection { Name = "Quality", Amount = 72, Color = Color.Blue, Image = "sleep.png" },
                new Selection { Name = "Time", Amount = 95, Color = Color.SlateBlue, Image = "time.png" },
                new Selection { Name = "Pulse", Amount = 75, Color = Color.Pink, Image = "pulse.png" }

            };
        }

        private void ItemTapped(object sender, EventArgs e)
        {
            SelectedAmount = 0.0f;
            var grid = sender as Grid;
            var selectedItem = grid.BindingContext as Selection;
            var parent = grid.Parent as StackLayout;

            name = selectedItem.Name;

            ((parent.Parent) as ScrollView).ScrollToAsync(grid, ScrollToPosition.MakeVisible, true);

            foreach (var item in parent.Children)
            {
                var bg = item.FindByName<BoxView>("MainBg");
                var details = item.FindByName<StackLayout>("DetailsView");

                details.TranslateTo(-40, 0, 200, Easing.SinInOut);
                bg.IsVisible = false;
                details.IsVisible = false;
            }

            var selectionBg = grid.FindByName<BoxView>("MainBg");
            var selectionDetails = grid.FindByName<StackLayout>("DetailsView");

            selectionBg.IsVisible = true;
            selectionDetails.IsVisible = true;
            selectionDetails.TranslateTo(0, 0, 300, Easing.SinInOut);

            AnimatedText(selectedItem.Amount);
        }

        private void AnimatedText(float amount)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            Device.StartTimer(TimeSpan.FromSeconds(1 / 100f), () =>
            {
                double t = stopwatch.Elapsed.TotalMilliseconds % 500 / 500;

                SelectedAmount = Math.Min((float)amount, (float)(10 * t) + SelectedAmount);

                if (SelectedAmount >= (float)amount)
                {
                    stopwatch.Stop();
                    return false;
                }

                return true;
            });
        }

        private async void Score_OnClicked(object sender, EventArgs e)
        {

            switch (SelectedName)
            {
                case "Time":
                    await Navigation.PushAsync(new TimeScorePage());
                    break;
                case "Pulse":
                    await Navigation.PushAsync(new PulseScorePage());
                    break;
                case "Quality":
                    await Navigation.PushAsync(new QualityScorePage());
                    break;
                default:
                    break;

            }

        }
    }

    public class Selection
    {
        public string Name { get; set; }
        public string Image { get; set; }
        public float Amount { get; set; }
        public Color Color { get; set; }
    }
}