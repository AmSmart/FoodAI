using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace FoodAI.ViewModels
{
    public class FoodContentViewModel
    {
        public string ImageUrl { get; set; }
        public string TagName { get; set; }
        public string DietSuggestion { get; set; }

        public double EnergyInKcal { get; set; }
        public double WaterInGram { get; set; }
        public double ProteinInGram { get; set; }
        public double FatInGram { get; set; }
        public double CarbohydrateInGram { get; set; }
        public double FibreInGram { get; set; }
        public double AshInGram { get; set; }
        public double CalciumInMilGram { get; set; }
        public double IronInMilGram { get; set; }
        public double MagnesiumInMilGram { get; set; }
        public double PhosphorusInMilGram { get; set; }
        public double PotassiumInMilGram { get; set; }
        public double SodiumInMilGram { get; set; }
        public double ZincInMilGram { get; set; }
        public double CopperInMilGram { get; set; }
        public double ManganeseInMilGram { get; set; }

        public ImageSource ImageSource { get; set; }

        public double DetectionProbability { get; set; }
    }
}
