using System;
using System.IO;
using System.Windows.Media.Imaging;
using QRCoder;

namespace SmartXDashboard.Services
{
    public class BarcodeService
    {
        public BitmapImage GenerateBarcodeImage(string textPayload)
        {
            if (string.IsNullOrWhiteSpace(textPayload))
                return null;

            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            {
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(textPayload, QRCodeGenerator.ECCLevel.Q);
                PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
                byte[] qrCodeAsPngByteArray = qrCode.GetGraphic(20);

                using (MemoryStream ms = new MemoryStream(qrCodeAsPngByteArray))
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = ms;
                    bitmap.EndInit();
                    bitmap.Freeze();
                    return bitmap;
                }
            }
        }
    }
}