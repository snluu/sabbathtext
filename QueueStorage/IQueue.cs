﻿using Microsoft.WindowsAzure.Storage.Queue;
using System.Threading.Tasks;

namespace QueueStorage
{
    public interface IQueue
    {
        Task CreateIfNotExists(string queueName);
        Task AddMessage(string queueName, CloudQueueMessage message);
    }
}
