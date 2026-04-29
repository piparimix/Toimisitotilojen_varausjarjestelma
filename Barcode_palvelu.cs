using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.IO;
using System.Windows.Media.Imaging;

namespace Toimistotilojen_varausjarjestelma
{
    internal class Barcode_palvelu
    {
        // toteutettu BarcodeLib paketilla

        private static string DataFormat(string data)
        {
            // Toteuta tarvittaessa datan muotoilu ennen viivakoodin luontia
            return "LASKU- " + data;
        }
        public static byte[] GetBarcodeBytes(string data)
        {
            string muokattuData = DataFormat(data);
            BarcodeStandard.Barcode b = new BarcodeStandard.Barcode();
            var img = b.Encode(BarcodeStandard.Type.Code128, muokattuData, SkiaSharp.SKColors.Black, SkiaSharp.SKColors.White, 200, 80);
            using (var encoded = img.Encode(SkiaSharp.SKEncodedImageFormat.Png, 100))
            {
                return encoded.ToArray();
            }
        }

        public static BitmapImage GenerateBarcode(string data)
        {
            string muokattuData = DataFormat(data);

            BarcodeStandard.Barcode b = new BarcodeStandard.Barcode();
            var img = b.Encode(BarcodeStandard.Type.Code128, muokattuData, SkiaSharp.SKColors.Black, SkiaSharp.SKColors.White, 200, 80);
            return ConvertSkiaImageToWPF(img);
        }

        public static BitmapImage ConvertSkiaImageToWPF(SkiaSharp.SKImage skImg)
        {
            using (var data = skImg.Encode(SkiaSharp.SKEncodedImageFormat.Png, 100))
            using (var ms = new MemoryStream())
            {
                data.SaveTo(ms);
                ms.Seek(0, SeekOrigin.Begin);

                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;

                image.StreamSource = ms;
                image.EndInit();
                image.Freeze(); // Parantaa suorituskykyä ja tekee kuvasta säikeistöturvallisen

                return image;
            }
        }
    }
}
