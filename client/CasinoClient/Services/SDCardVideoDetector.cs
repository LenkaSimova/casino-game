
using System;
using System.IO;
using System.Linq;


public enum SdCardState
{
    NoSdCardDetected,
    SdCardDetectedNoVideo,
    SdCardWithVideoPresent
}

public static class SdCardvideoDetector
{
    private static readonly string[] VideoExtensions =
        { ".mp4", ".mov", ".avi", ".mkv", ".wmv", ".flv", ".mpeg", ".mpg" };

    /// <summary>
    /// Checks for SD card and video presence, returns state.
    /// </summary>
    public static SdCardState GetSdCardState()
    {
        bool sdCardFound = false;
        foreach (var drive in DriveInfo.GetDrives())
        {
            // Only consider removable drives that are ready and named SDCARD
            if (drive.IsReady && (drive.Name.ToUpper().Contains("SDCARD") || (drive.VolumeLabel != null && drive.VolumeLabel.ToUpper().Contains("SDCARD"))))
            {
                sdCardFound = true;
                try
                {
                    var files = Directory.EnumerateFiles(
                        drive.RootDirectory.FullName,
                        "*.*",
                        SearchOption.AllDirectories);

                    if (files.Any(file =>
                        VideoExtensions.Contains(
                            Path.GetExtension(file),
                            StringComparer.OrdinalIgnoreCase)))
                    {
                        return SdCardState.SdCardWithVideoPresent;
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    continue;
                }
                catch (IOException)
                {
                    continue;
                }
            }
        }
        if (sdCardFound)
            return SdCardState.SdCardDetectedNoVideo;
        else
            return SdCardState.NoSdCardDetected;
    }
}
