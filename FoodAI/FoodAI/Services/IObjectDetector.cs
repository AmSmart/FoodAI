using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodAI.Services
{
    public interface IObjectDetector
    {
        void Detect(byte[] image);
    }
}
