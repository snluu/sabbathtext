﻿using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace SabbathText.Core.Entities
{
    public class Account : TableEntity
    {
        public static readonly TimeSpan CycleDuration = TimeSpan.FromDays(2);

        public Account()
        {
            this.CreationTime = Clock.MinValue;
            this.LastSabbathMessageTime = Clock.MinValue;
            this.NextCycleTime = Clock.MinValue;
        }

        public string AccountId { get; set; }
        public DateTime CreationTime { get; set; }        
        public string PhoneNumber { get; set; }
        public string ZipCode { get; set; }
        public string Status { get; set; }
        public DateTime LastSabbathMessageTime { get; set; }
        public DateTime NextCycleTime { get; set; }
        public string CycleKey { get; set; }
        /// <summary>
        /// Recently sent verses, delimited by semi-colons
        /// </summary>
        public string RecentlySentVerses { get; set; }
    }
}
