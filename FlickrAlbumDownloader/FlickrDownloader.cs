using System.Net;
using FlickrNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace FlickrAlbumDownloader
{
    public class FlickrDownloader
    {
        private readonly Flickr _flickr;

        public FlickrDownloader(string apiKey, string sharedSecret)
        {
            _flickr = new Flickr(apiKey, sharedSecret);
        }

        public OAuthRequestToken GetAuthRequestToken()
        {
            var requestToken = _flickr.OAuthGetRequestToken("oob");
            
            return new OAuthRequestToken
            {
                Token = requestToken.Token,
                TokenSecret = requestToken.TokenSecret
            };
        }

        public string GetAuthUrl(OAuthRequestToken token)
        {
            return _flickr.OAuthCalculateAuthorizationUrl(token.Token, AuthLevel.Read);
        }

        public void SetAuthCode(OAuthRequestToken token, string code)
        {
            var accessToken = _flickr.OAuthGetAccessToken(token.Token, token.TokenSecret, code);
            _flickr.OAuthAccessToken = accessToken.Token;
            _flickr.OAuthAccessTokenSecret = accessToken.TokenSecret;
        }

        public IEnumerable<PhotoSet> GetPhotoSets(string userId)
        {
            var photosets = _flickr.PhotosetsGetList(userId);
            var order = photosets.Count;
            var result = new List<PhotoSet>();

            foreach (var photoset in photosets)
            {
                result.Add(new PhotoSet
                {
                    Id = photoset.PhotosetId,
                    Name = photoset.Title,
                    Order = order,
                });

                order--;
            }

            return result.OrderBy(p => p.Order);
        }

        public void DownloadPhotoSet(string photoSetId, string path)
        {
            var client = new WebClient();

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            var photos = _flickr.PhotosetsGetPhotos(photoSetId, PhotoSearchExtras.OriginalUrl | PhotoSearchExtras.DateTaken);
            var currentPhoto = 0;

            foreach (var photo in photos)
            {
                currentPhoto++;

                var originalPhotoUrl = photo.OriginalUrl;
                var fileName = photo.DateTaken.ToString("yyy-MM-dd-HH-mm-ss-") + photo.PhotoId + ".jpg";
                var finalPath = Path.Combine(path, fileName);
                client.DownloadFile(originalPhotoUrl, finalPath);
                File.SetCreationTime(finalPath, photo.DateTaken);
                File.SetLastWriteTime(finalPath, photo.DateTaken);
                OnPhotoSetDownloadProgressChanged(new PhotoSetDownloadProgress { CurrentPhotoNumber = currentPhoto, PhotosCount = photos.Count });
            }
        }

        protected void OnPhotoSetDownloadProgressChanged(PhotoSetDownloadProgress progress)
        {
            if (PhotoSetDownloadProgressChanged != null)
            {
                PhotoSetDownloadProgressChanged(this, progress);
            }
        }

        public event EventHandler<PhotoSetDownloadProgress> PhotoSetDownloadProgressChanged;
    }

    public struct OAuthRequestToken
    {
        public string Token { get; set; }
        public string TokenSecret { get; set; }
    }

    public struct PhotoSet
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Order { get; set; }
    }

    public struct PhotoSetDownloadProgress
    {
        public int PhotosCount { get; set; }
        public int CurrentPhotoNumber { get; set; }
    }
}
