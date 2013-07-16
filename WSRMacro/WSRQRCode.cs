﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;
using Microsoft.Kinect;
using ZXing;

namespace net.encausse.sarah {

  public class QRCodeManager {

    BarcodeReader reader = new BarcodeReader { 
      AutoRotate = true, 
      TryHarder = true, 
      PossibleFormats = new List<BarcodeFormat> { BarcodeFormat.QR_CODE } 
    };

    // ==========================================
    //  QRCODE MANAGER
    // ==========================================

    private WriteableBitmap bitmap;

    private void fireQRCode(String match) {
      ((WSRKinect)WSRConfig.GetInstance().GetWSRMicro()).HandleQRCodeComplete(match);
    }

    public bool SetupQRCode() {
      if (WSRConfig.GetInstance().qrcode <= 0) {
        return false;
      }
      WSRConfig.GetInstance().logInfo("QRCODE", "Starting QRCode manager");
      return true;
    }

    // ==========================================
    //  COLOR FRAME
    // ==========================================

    Bitmap image = null;
    public void SensorColorFrameReady(object sender, ColorImageFrameReadyEventArgs e) {

      if (bitmap == null) {
        bitmap = ((WSRKinect)WSRConfig.GetInstance().GetWSRMicro()).NewColorBitmap();
      }

      CheckQRCode();
    }

    int threshold = 0;
    public void CheckQRCode() {
      if (threshold-- > 0) { return; } threshold = WSRConfig.GetInstance().qrcode;
      if (image != null) { return; }

      image = ((WSRKinect)WSRConfig.GetInstance().GetWSRMicro()).GetColorPNG(bitmap, true);
      Task.Factory.StartNew(() => {
        CheckQRCodeAsync(image);
        image.Dispose();
        image = null;
      });
    }

    public void CheckQRCodeAsync(Bitmap image) {

      Result result = result = reader.Decode(image);
      if (result == null) { return; }

      String type  = result.BarcodeFormat.ToString();
      String match = result.Text;
      WSRConfig.GetInstance().logInfo("QRCODE", "Type: " + type + " Content: " + match);
      fireQRCode(match);
    }
  }
}
