using Android.App;
using Android.Graphics;
using FoodAI.Droid;
using FoodAI.Services;
using Java.IO;
using Java.Nio;
using Java.Nio.Channels;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

[assembly: Dependency(typeof(ObjectDetector))]

namespace FoodAI.Droid
{
    public class ObjectDetector : IObjectDetector
    {
        const int FloatSize = 4;
        const int PixelSize = 3;


        public async Task<byte[]> DrawBoundingBox(byte[] imageArray, BoundingBox boundingBox)
        {
            Bitmap bitmap;
            Paint paint = new Paint()
            {
                Color = Android.Graphics.Color.Red,
                Dither = true,
                StrokeWidth = 10,
                FilterBitmap = true,
                AntiAlias = true,                
            };
            paint.SetStyle(Paint.Style.FillAndStroke);

            using (MemoryStream stream = new MemoryStream(imageArray))
            {
                BitmapFactory.Options options = new BitmapFactory.Options();
                options.InScaled = false;
                options.InMutable = true;
                Bitmap immutablebitmap = await BitmapFactory.DecodeStreamAsync(stream);
                bitmap = immutablebitmap.Copy(Bitmap.Config.Argb8888, true);
                
            }
            var imageHeight = bitmap.Height;
            var imageWidth = bitmap.Width;
            // Co-ordinates of top left of the rectangle
            float x = (float) boundingBox.Left * imageWidth;
            float y = (float) boundingBox.Top * imageHeight;

            // Magnitude of height and width
            float boxWidth = (float) boundingBox.Width * imageWidth;
            float boxHeight = (float) boundingBox.Height * imageHeight;


            Canvas canvas = new Canvas(bitmap);            
            canvas.DrawLine(x, y, x, y + boxHeight, paint); //from top-left downward
            canvas.DrawLine(x, y, x + boxWidth, y, paint); //from top-left rightward
            canvas.DrawLine(x + boxWidth, y, x + boxWidth, y + boxHeight, paint); //from top-right downward
            canvas.DrawLine(x, y + boxHeight, x + boxWidth, y + boxHeight, paint); //from down-left rightward

            var outputStream = new MemoryStream();
            await bitmap.CompressAsync(Bitmap.CompressFormat.Png, 100, outputStream);

            return outputStream.ToArray();
        }

        [Obsolete]
        public void Detect(byte[] image)
        {
            var mappedByteBuffer = GetModelAsMappedByteBuffer();
            var interpreter = new Xamarin.TensorFlow.Lite.Interpreter(mappedByteBuffer);

            var tensor = interpreter.GetInputTensor(0);
            var tensor2 = interpreter.GetOutputTensor(0);
            var shape1 = tensor2.Shape();
            var shape = tensor.Shape();

            var width = shape[1];
            var height = shape[2];

            var byteBuffer = GetPhotoAsByteBuffer(image, width, height);

            var streamReader = new StreamReader(Android.App.Application.Context.Assets.Open("labels.txt"));
            var labels = streamReader.ReadToEnd().Split('\n').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList();

            //Convert our two-dimensional array into a Java.Lang.Object, the required input for Xamarin.TensorFlow.List.Interpreter
            var outputLocations = new float[1,13,13,50];
            var output = ByteBuffer.AllocateDirect(33800);

            try
            {
                interpreter.Run(byteBuffer, output);
            }
            catch(Exception ex)
            {
                var x = ex.Message;
            }
        }

        private MappedByteBuffer GetModelAsMappedByteBuffer()
        {
            var assetDescriptor = Android.App.Application.Context.Assets.OpenFd("model.tflite");
            var inputStream = new FileInputStream(assetDescriptor.FileDescriptor);

            var mappedByteBuffer = inputStream.Channel.Map(FileChannel.MapMode.ReadOnly, assetDescriptor.StartOffset, assetDescriptor.DeclaredLength);

            return mappedByteBuffer;
        }

        private ByteBuffer GetPhotoAsByteBuffer(byte[] image, int width, int height)
        {
            var bitmap = BitmapFactory.DecodeByteArray(image, 0, image.Length);
            var resizedBitmap = Bitmap.CreateScaledBitmap(bitmap, width, height, true);

            var modelInputSize = FloatSize * height * width * PixelSize;
            var byteBuffer = ByteBuffer.AllocateDirect(modelInputSize);
            byteBuffer.Order(ByteOrder.NativeOrder());

            var pixels = new int[width * height];
            resizedBitmap.GetPixels(pixels, 0, resizedBitmap.Width, 0, 0, resizedBitmap.Width, resizedBitmap.Height);

            var pixel = 0;

            //Loop through each pixels to create a Java.Nio.ByteBuffer
            for (var i = 0; i < width; i++)
            {
                for (var j = 0; j < height; j++)
                {
                    var pixelVal = pixels[pixel++];

                    byteBuffer.PutFloat(pixelVal >> 16 & 0xFF);
                    byteBuffer.PutFloat(pixelVal >> 8 & 0xFF);
                    byteBuffer.PutFloat(pixelVal & 0xFF);
                }
            }

            bitmap.Recycle();

            return byteBuffer;
        }
    }
}

