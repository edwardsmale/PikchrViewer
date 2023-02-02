using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Xml;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Threading;
using Svg;
using static Microsoft.VisualStudio.Text.Editor.AdornmentPositioningBehavior;

namespace PikchrViewer
{
    public class SvgAdornment : System.Windows.Controls.Image
    {
        private readonly IWpfTextView textView;

        public SvgAdornment(IWpfTextView view)
        {
            this.textView = view;

            this.Visibility = Visibility.Hidden;

            var layer = this.textView.GetAdornmentLayer(AdornmentLayer.LayerName);

            if (layer.IsEmpty)
            {
                layer.AddAdornment(ViewportRelative, null, null, this, null);
            }

            this.textView.Closed += this.OnClosed;
            this.textView.TextBuffer.PostChanged += this.OnChanged;
            this.textView.ViewportHeightChanged += this.OnChanged;
            this.textView.ViewportWidthChanged += this.OnChanged;

            this.RedrawAsync().FireAndForget();
        }

        private void OnClosed(object sender, EventArgs e)
        {
            this.textView.Closed -= this.OnClosed;
            this.textView.TextBuffer.PostChanged -= this.OnChanged;
            this.textView.ViewportHeightChanged -= this.OnChanged;
            this.textView.ViewportWidthChanged -= this.OnChanged;
        }

        private void OnChanged(object sender, EventArgs e)
        {
            var lastVersion = this.textView.TextBuffer.CurrentSnapshot.Version.VersionNumber;

            _ = ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                await Task.Delay(50);

                if (this.textView.TextBuffer.CurrentSnapshot.Version.VersionNumber == lastVersion)
                {
                    await this.RedrawAsync();
                }
            });
        }

        private async Task RedrawAsync()
        {
            await TaskScheduler.Default;

            var svg = this.TryParseXmlAsSvg();

            if (svg.HasValue)
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                this.GenerateBitmapImage(svg);
            }
        }

        private Exceptionable<SvgDocument> TryParseXmlAsSvg()
        {
            try
            {
                var text = this.textView.TextBuffer.CurrentSnapshot.GetText();

                var doc = new XmlDocument();
                doc.LoadXml(text);

                var svg = SvgDocument.Open(doc);

                if (svg == null)
                {
                    return new Exception("Could not parse SVG file.");
                }
                else
                {
                    return svg;
                }
            }
            catch (XmlException ex)
            {
                return ex;
            }
        }

        private void GenerateBitmapImage(SvgDocument svg)
        {
            try
            {
                var bitmapImage = new BitmapImage();

                var width = this.textView.ViewportWidth;
                var height = this.textView.ViewportHeight;

                this.textView.ZoomLevel = 100; // Force 100% zoom level to avoid jagged image.

                using (var bitmap = new Bitmap((int)width, (int)height))
                {
                    var graphics = Graphics.FromImage(bitmap);

                    graphics.Clear(Color.White);
                    graphics.SmoothingMode = SmoothingMode.HighQuality;

                    svg.Draw(graphics, new SizeF((float)width, (float)height));

                    using (var ms = new MemoryStream())
                    {
                        bitmap.Save(ms, ImageFormat.Png);
                        ms.Position = 0;

                        bitmapImage.BeginInit();
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.StreamSource = ms;
                        bitmapImage.EndInit();
                    }
                }

                bitmapImage.Freeze();

                this.Source = bitmapImage;
                this.Visibility = Visibility.Visible;
            }
            catch
            {
            }
        }
    }
}