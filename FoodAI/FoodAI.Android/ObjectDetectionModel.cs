using System;
using System.Collections.Generic;
using System.Text;

namespace FoodAI.Droid
{
    public class ObjectDetectionModel
    {
        public string TagName { get; set; }
        public int[] BoundingBox { get; set; }
        public float DetectionProbability { get; set; }
    }
}
