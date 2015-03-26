﻿namespace SabbathText.Compensation
{
    public class CheckpointReference
    {
        /// <summary>
        /// Gets or sets the checkpoint partition key
        /// </summary>
        public string PartitionKey { get; set; }

        /// <summary>
        /// Gets or sets the checkpoint row key
        /// </summary>
        public string RowKey { get; set; }
    }
}
