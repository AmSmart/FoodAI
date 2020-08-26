using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodAI.Services
{
    public interface IObjectDetector
    {
        void Detect(byte[] image);
        Task<byte[]> DrawBoundingBox(byte[] imageArray, BoundingBox boundingBox);
    }
}
