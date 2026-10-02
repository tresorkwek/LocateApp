using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;
using QRCoder;
using System.IO;
using System.Drawing.Imaging;

namespace LocateApp.Utilities
{
    public static class ImageCreator
    {
        public static object GetQrCode(string textToTransform)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                QRCodeGenerator qrCodeGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrCodeGenerator.CreateQrCode(textToTransform, QRCodeGenerator.ECCLevel.Q);
                QRCode qrCode = new QRCode(qrCodeData);

                using(Bitmap qrCodeImage = qrCode.GetGraphic(20))
                {
                    qrCodeImage.Save(memoryStream, ImageFormat.Png);
                    return "data:image/png;base64," + Convert.ToBase64String(memoryStream.ToArray());
                }
            }

        }

    }
}