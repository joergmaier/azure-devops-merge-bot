#nullable disable
using System;
using System.Collections.Generic;
using System.Text;

namespace MergeBot
{
    public class GitPushEventPayload
    {
        public string EventType { get; set; }
        public GitPushEventResource Resource { get; set; }
    }

    public class GitPushEventResource
    {
        public const string EventType = "git.push";
        public List<GitPushEventRefUpdate> RefUpdates { get; set; }
        public GitPushEventRepository Repository { get; set; }
    }

    public class GitPushEventRefUpdate
    {
        /// <summary>
        /// Branch name: refs/heads/master
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Old commit id: 4f8b247adf567f2b38c512ef52bb03ac0e31ccd4
        /// </summary>
        public string OldObjectId { get; set; }

        /// <summary>
        /// New commit id: 16cec1595902a8a61632e7b9d886a72f332926f3
        /// </summary>
        public string NewObjectId { get; set; }
    }

    public class GitPushEventRepository
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }

        private string _normalizedUrl;
        public string NormalizedUrl
        {
            get
            {
                if (!string.IsNullOrEmpty(_normalizedUrl))
                    return _normalizedUrl;

                var uri = new Uri(Url);
                if (Url.StartsWith("https://dev.azure.com/"))
                {
                    _normalizedUrl = Url;
                }
                else if (uri.Host.EndsWith("visualstudio.com"))
                {
                    _normalizedUrl = string.Format("https://dev.azure.com/{0}/_apis/git/repositories/{1}", GetOrganization(), Id);
                }
                else
                {
                    _normalizedUrl = Url;
                }

                return _normalizedUrl;
            }
        }

        public string GetOrganization()
        {
            var uri = new Uri(Url);
            if (string.Equals(uri.Host, "dev.azure.com", StringComparison.OrdinalIgnoreCase))
                return uri.Segments[1].TrimEnd('/');
            else if (uri.Host.EndsWith("visualstudio.com", StringComparison.OrdinalIgnoreCase))
                return uri.Host.Substring(0, uri.Host.IndexOf('.'));
            else if (String.Equals(uri.Segments[2].TrimEnd('/'), "_apis", StringComparison.OrdinalIgnoreCase))
                return uri.Segments[1].TrimEnd('/');
            else if (String.Equals(uri.Segments[3].TrimEnd('/'), "_apis", StringComparison.OrdinalIgnoreCase))
                return uri.Segments[2].TrimEnd('/');

            throw new InvalidOperationException("GetOrganization Unknown url format: " + uri.ToString());
        }

        public string GetBaseUrl()
        {
            var uri = new Uri(Url);
            var baseUrl =  uri.Scheme + "://" + uri.Host + "/";
            if (string.Equals(uri.Host, "dev.azure.com", StringComparison.OrdinalIgnoreCase))
                return null;
            else if (uri.Host.EndsWith("visualstudio.com", StringComparison.OrdinalIgnoreCase))
                return null;
            else if (String.Equals(uri.Segments[2].TrimEnd('/'), "_apis", StringComparison.OrdinalIgnoreCase) || String.Equals(uri.Segments[3].TrimEnd('/'), "_apis", StringComparison.OrdinalIgnoreCase))
                return baseUrl + uri.Segments[1].TrimEnd('/');
            else if (String.Equals(uri.Segments[1].TrimEnd('/'), "_apis", StringComparison.OrdinalIgnoreCase))
                return baseUrl;

            throw new InvalidOperationException("GetBaseUrl: Unknown url format: " + uri.ToString());
        }
    }
}
