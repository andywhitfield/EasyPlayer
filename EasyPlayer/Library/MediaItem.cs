using System;
using System.IO;

namespace EasyPlayer.Library
{
    public class MediaItem
    {
        public event EventHandler IsAvailableChanged;
        public event EventHandler IsDeletedChanged;
        public event EventHandler DownloadProgressChanged;

        private bool isAvailable;
        private bool isDeleted;
        private int downloadProgress;

        public MediaItem()
        {
            Name = "";
            IsAvailable = false;
            IsDeleted = false;
            DownloadProgress = 0;
        }

        public virtual string Name { get; set; }
        
        /// <summary>
        /// Indicates if the DataStream is available to read/play.
        /// </summary>
        public virtual bool IsAvailable
        {
            get { return isAvailable; }
            set
            {
                if (isAvailable == value) return;
                isAvailable = value;
                if (IsAvailableChanged != null) IsAvailableChanged(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Indicates if the item has been (logically) deleted.
        /// </summary>
        public virtual bool IsDeleted
        {
            get { return isDeleted; }
            set
            {
                if (isDeleted == value) return;
                isDeleted = value;
                if (IsDeletedChanged != null) IsDeletedChanged(this, EventArgs.Empty);
            }
        }

        public virtual Stream DataStream { get; set; }

        /// <summary>
        /// The percentage (0-100) of the download that has completed.
        /// </summary>
        public virtual int DownloadProgress
        {
            get { return downloadProgress; }
            set
            {
                if (downloadProgress == value) return;
                downloadProgress = Math.Max(Math.Min(value, 100), 0);
                if (DownloadProgressChanged != null) DownloadProgressChanged(this, EventArgs.Empty);
            }
        }
    }
}
