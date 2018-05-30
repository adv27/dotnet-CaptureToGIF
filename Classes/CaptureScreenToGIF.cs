using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

namespace TransparentForm.Classes
{
    class CaptureScreenToGIF
    {
        #region variables

        bool captureCursor;
        Rectangle selectedRect;
        int fps = 30;
        System.Windows.Forms.Timer captureT = new System.Windows.Forms.Timer();
        List<byte[]> frames = new List<byte[]>();

        IProgress<float> percentageProgress;
        #endregion
        public CaptureScreenToGIF(bool captureCursor, Rectangle rect)
        {
            this.captureCursor = captureCursor;
            selectedRect = rect;

            captureT.Interval = (int)(1000f / this.fps);
            captureT.Tick += CaptureT_Tick;
            captureT.Enabled = true;
        }

        private void CaptureT_Tick(object sender, EventArgs e)
        {
            frames.Add(Screenshot.CaptureRegionAsBytes(selectedRect, captureCursor));
        }

        public void StartCapture()
        {
            captureT.Start();
        }
        public void Stop()
        {
            captureT.Stop();
        }

        public async Task SaveAsync(String location, Progress<float> progress)
        {
            // gifEncode(frames, location, captureT.Interval, 10, 0);


            await gifEncode(frames, location, captureT.Interval, 10, 0, progress);


            //freeing memory
            FreeFramesMemory(frames);
        }

        public static async Task gifEncode(
            List<byte[]> frames,
            string location,
            int delay,
            int quality,
            int repeat,
            IProgress<float> percentageProgress)
        {
            int fromIndex = 0;
            int toIndex = frames.Count;
            int frameIndexIncremet = 1;
            await Task.Run(() =>
            {
                var age = new AnimatedGifEncoder();
                age.Start(location);
                age.SetDelay(delay);
                age.SetQuality(quality);
                age.SetRepeat(repeat);


                for (int i = fromIndex; i < toIndex; i += frameIndexIncremet)
                {
                    if (percentageProgress != null)
                        percentageProgress.Report((float)(i - fromIndex) / (float)(toIndex - fromIndex));

                    age.AddFrame(BytesToImage(frames[i]));
                }

                //foreach (byte[] frame in frames)
                //{
                //    age.AddFrame(BytesToImage(frame));
                //}

                age.Finish();
            });
        }
        public static void FreeFramesMemory(List<byte[]> frames)
        {
            for (int i = 0; i < frames.Count; i++)
                frames[i] = new byte[0];
            frames.Clear();

            GC.Collect();
        }
        public static Image BytesToImage(byte[] imageBytes)
        {
            using (var ms = new System.IO.MemoryStream(imageBytes))
                return Image.FromStream(ms);
        }

    }
}
