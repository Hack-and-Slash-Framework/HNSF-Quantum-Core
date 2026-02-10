using System;

namespace HnSF
{
    public static class StateFrameHelper
    {
        public static int ConvertFrame(int totalFrames, int frameNumber)
        {
            if (frameNumber < 0) frameNumber = Math.Clamp(totalFrames + 2 + frameNumber, 1, totalFrames+1);
            else frameNumber = Math.Clamp(frameNumber, 0, totalFrames);
            return Math.Clamp(frameNumber, 0, totalFrames+1);
        }
    }
}
