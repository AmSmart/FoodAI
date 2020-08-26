using FoodAI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Essentials;
using Xamarin.Forms.Xaml;

namespace FoodAI.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FoodContent : ContentPage
    {
        FoodContentViewModel _model;
        bool firstAppear = true;

        public FoodContent(FoodContentViewModel model)
        {
            InitializeComponent();
            _model = model;
            BindingContext = _model;            
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            firstAppear = false;
            if (firstAppear)
            {
                TextToSpeech.SpeakAsync($"{_model.TagName} has been detected");
                TextToSpeech.SpeakAsync(_model.DietSuggestion);
            }
        }
    }
}