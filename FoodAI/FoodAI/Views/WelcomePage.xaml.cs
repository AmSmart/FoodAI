using FoodAI.Services;
using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using FoodAI.ViewModels;
using System.Reflection;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.Models;

namespace FoodAI.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class WelcomePage : ContentPage
    {
        const string PredictionKey = "33eaa6c2249646ceb7fc5400464bc64f";
        const string EndPoint = "https://southcentralus.api.cognitive.microsoft.com/";
        const string ProjectId = "5a28b1ff-6ded-475b-bde6-3261350df3b7";
        const string PublishedModelName = "Iteration5";
        readonly IObjectDetector _objectDetector;

        public WelcomePage()
        {
            InitializeComponent();
            Title = "Welcome";
            BindingContext = this;
            Task.Run(AnimateBackground);
            _objectDetector = DependencyService.Get<IObjectDetector>();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            imageView.Source = "loading.gif";
            scanButton.IsEnabled = false;
            galleryButton.IsEnabled = false;

            await CrossMedia.Current.Initialize();            

            if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
            {
                await DisplayAlert("No Camera", ":( No camera available.", "OK");
                imageView.Source = "image.jpg";
                scanButton.IsEnabled = true;
                galleryButton.IsEnabled = true;
                return;
            }

            try
            {
                PredictionModel predictionModel = null;
                ImagePrediction result = null;
                var image = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions { PhotoSize = PhotoSize.Medium });                
                var imageStream = image.GetStream();
                byte[] imageArray;
                using (var memoryStream = new MemoryStream())
                {
                    imageStream.CopyTo(memoryStream);
                    imageArray = memoryStream.ToArray();
                }

                await Task.Run(() =>
                {
                    CustomVisionPredictionClient endpoint = new CustomVisionPredictionClient(new ApiKeyServiceClientCredentials(PredictionKey))
                    {
                        Endpoint = EndPoint
                    };
                    using(var mStream = new MemoryStream(imageArray))
                        result = endpoint.DetectImage(Guid.Parse(ProjectId), PublishedModelName, mStream);
                    
                    double maxProb = result.Predictions.Select(x => x.Probability).Max();
                    if(maxProb < 0.45)
                    {
                        throw new Exception("No food detected");
                    }
                    predictionModel = result.Predictions.First(x => x.Probability == maxProb);                    
                });
                
                var model = GetViewModel(predictionModel.TagName);
                model.DetectionProbability = predictionModel.Probability;

                var boundedImageArray = await _objectDetector.DrawBoundingBox(imageArray, predictionModel.BoundingBox);

                var imageSource = ImageSource.FromStream(() => new MemoryStream(boundedImageArray));

                model.ImageSource = imageSource;

                scanButton.IsEnabled = true;
                galleryButton.IsEnabled = true;
                imageView.Source = "image.jpg";
                await Navigation.PushAsync(new FoodContent(model));
            }
            catch(Exception ex)
            {
                imageView.Source = "image.jpg";
                await DisplayAlert("Error", ex.Message, "Done");
                scanButton.IsEnabled = true;
                galleryButton.IsEnabled = true;
            }
        }

        private async void AnimateBackground()
        {
            Action<double> forward = input => bdGradient.AnchorY = input;
            Action<double> backward = input => bdGradient.AnchorY = input;

            while (true)
            {
                bdGradient.Animate(name: "forward", callback: forward, start: 0, end: 1, length: 5000, easing: Easing.SinIn);
                await Task.Delay(5000);
                bdGradient.Animate(name: "backward", callback: backward, start: 1, end: 0, length: 5000, easing: Easing.SinIn);
                await Task.Delay(5000);
            }
        }

        private FoodContentViewModel GetViewModel(string tagName)
        {
            var assembly = IntrospectionExtensions.GetTypeInfo(typeof(WelcomePage)).Assembly;
            Stream stream = assembly.GetManifestResourceStream("FoodAI.ai-food-database.csv");
            string data;
            using (var reader = new StreamReader(stream))
            {
                data = reader.ReadToEnd();
            }

            string[] datainLines = data.Split(Environment.NewLine.ToCharArray());
            int lineIndex;

            switch (tagName)
            {
                case "Fish":
                    lineIndex = 1;
                    break;

                case "Bread":
                    lineIndex = 2;
                    break;

                case "Rice":
                    lineIndex = 3;
                    break;

                case "Eba":
                    lineIndex = 4;
                    break;

                case "Chicken":
                    lineIndex = 5;
                    break;

                default:
                    throw new Exception("Invalid tagname");
            }

            string[] foodData = datainLines[lineIndex].Split(',');
            var model = new FoodContentViewModel
            {
                TagName = tagName,
                EnergyInKcal = Convert.ToDouble(foodData[1]),
                WaterInGram = Convert.ToDouble(foodData[2]),
                ProteinInGram = Convert.ToDouble(foodData[3]),
                FatInGram = Convert.ToDouble(foodData[4]),
                CarbohydrateInGram = Convert.ToDouble(foodData[5]),
                FibreInGram = Convert.ToDouble(foodData[6]),
                AshInGram = Convert.ToDouble(foodData[7]),
                CalciumInMilGram = Convert.ToDouble(foodData[8]),
                IronInMilGram = Convert.ToDouble(foodData[9]),
                MagnesiumInMilGram = Convert.ToDouble(foodData[10]),
                PhosphorusInMilGram = Convert.ToDouble(foodData[11]),
                PotassiumInMilGram = Convert.ToDouble(foodData[12]),
                SodiumInMilGram = Convert.ToDouble(foodData[13]),
                ZincInMilGram = Convert.ToDouble(foodData[14]),
                CopperInMilGram = Convert.ToDouble(foodData[15]),
                ManganeseInMilGram = Convert.ToDouble(foodData[16]),
                DietSuggestion = foodData[17]
            };

            return model;
        }

        private async void galleryButton_Clicked(object sender, EventArgs e)
        {
            imageView.Source = "loading.gif";
            scanButton.IsEnabled = false;
            galleryButton.IsEnabled = false;

            try
            {
                PredictionModel predictionModel = null;
                ImagePrediction result = null;
                if (!CrossMedia.Current.IsPickPhotoSupported)
                {
                    await DisplayAlert("Photos Not Supported", ":( Permission not granted to photos.", "OK");
                    return;
                }

                var image = await CrossMedia.Current.PickPhotoAsync(new PickMediaOptions
                {
                    PhotoSize = PhotoSize.Medium,

                });

                if (image == null)
                    return;

                var imageStream = image.GetStream();
                byte[] imageArray;
                using (var memoryStream = new MemoryStream())
                {
                    imageStream.CopyTo(memoryStream);
                    imageArray = memoryStream.ToArray();
                }

                await Task.Run(() =>
                {
                    CustomVisionPredictionClient endpoint = new CustomVisionPredictionClient(new ApiKeyServiceClientCredentials(PredictionKey))
                    {
                        Endpoint = EndPoint
                    };

                    using (var mStream = new MemoryStream(imageArray))
                        result = endpoint.DetectImage(Guid.Parse(ProjectId), PublishedModelName, mStream);
                    
                    double maxProb = result.Predictions.Select(x => x.Probability).Max();
                    if (maxProb < 0.45)
                    {
                        throw new Exception("No food detected");
                    }
                    predictionModel = result.Predictions.First(x => x.Probability == maxProb);
                });

                var model = GetViewModel(predictionModel.TagName);
                model.DetectionProbability = predictionModel.Probability;

                var boundedImageArray = await _objectDetector.DrawBoundingBox(imageArray, predictionModel.BoundingBox);
                var imageSource = ImageSource.FromStream(() => new MemoryStream(boundedImageArray));

                model.ImageSource = imageSource;

                scanButton.IsEnabled = true;
                galleryButton.IsEnabled = true;
                imageView.Source = "image.jpg";
                await Navigation.PushAsync(new FoodContent(model));
            }
            catch(Exception ex)
            {
                imageView.Source = "image.jpg";
                await DisplayAlert("Error", ex.Message, "Done");
                scanButton.IsEnabled = true;
                galleryButton.IsEnabled = true;
            }
        }
    }
}