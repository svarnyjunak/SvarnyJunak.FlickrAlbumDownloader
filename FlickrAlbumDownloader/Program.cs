using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FlickrAlbumDownloader.Properties;

namespace FlickrAlbumDownloader
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var downloader = new FlickrDownloader(Settings.Default.FlickrApiKey, Settings.Default.FlickrSharedSecret);

                if (Settings.Default.UseAuthetication)
                {
                    var token = downloader.GetAuthRequestToken();
                    var url = downloader.GetAuthUrl(token);

                    System.Diagnostics.Process.Start(url);

                    var code = GetData("Authorize this application and write the code from the flickr page:");
                    Console.WriteLine();
                    downloader.SetAuthCode(token, code);
                }

                var photosets = downloader.GetPhotoSets(Properties.Settings.Default.UserId).ToArray();

                Console.WriteLine("Which photoset do you want to download?");

                foreach (var photoset in photosets)
                {
                    Console.WriteLine("{0} - {1}", photoset.Order, photoset.Name);
                }

                Console.WriteLine();
                var order = GetData("Write photoset id, press Enter and wait a few seconds:");
                Console.WriteLine();
                Console.WriteLine("I'm warming up.");
                var selectedPhotoSet = (from p in photosets where p.Order.ToString() == order select p).Single();

                var path = Path.Combine(Properties.Settings.Default.DownloaderPath, selectedPhotoSet.Id);

                downloader.PhotoSetDownloadProgressChanged += downloader_PhotoSetDownloadProgressChanged;
                downloader.DownloadPhotoSet(selectedPhotoSet.Id, path);
                Console.WriteLine("And we're done");
                Console.WriteLine("All photos has been downloaded to {0}.", path);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Upps. Something wrong's happend.");
                Console.WriteLine("Here are some details:");
                Console.Write(ex.ToString());
            }

            Console.ReadLine();
        }

        static void downloader_PhotoSetDownloadProgressChanged(object sender, PhotoSetDownloadProgress e)
        {
            Console.WriteLine("{0} of {1} photos has been downloaded.", e.CurrentPhotoNumber, e.PhotosCount);
        }

        private static string GetData(string caption)
        {
            Console.WriteLine(caption);
            return Console.ReadLine();
        }
    }
}
