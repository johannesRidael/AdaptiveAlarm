using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Adaptive_Alarm
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ScorePage : ContentPage
    {
        public ScorePage()
        {
            InitializeComponent();
            Selections = GetSelection();
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
                new Selection { Name = "Quality", Amount = 100, Color = Color.Blue, Image = "sleep.png" },
                // TODO: For future algorithm displaying... 
                new Selection { Name = "Time", Amount = 100, Color = Color.SlateBlue, Image = "time.png" },
                new Selection { Name = "Average", Amount = 75, Color = Color.Purple, Image = "pulse.png" },
                new Selection { Name = "Other", Amount = 95, Color = Color.PeachPuff, Image = "heart.png" },
            };
        }

        private void ItemTapped(object sender, EventArgs e)
        {
            SelectedAmount = 0.0f;
            var grid = sender as Grid;
            var selectedItem = grid.BindingContext as Selection;
            var parent = grid.Parent as StackLayout;

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
    }

    public class Selection
    {
        public string Name { get; set; }
        public string Image { get; set; }
        public float Amount { get; set; }
        public Color Color { get; set; }
    }
}