﻿using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Configuration;
using System.IO;
using System.Linq;

namespace MigrationWebJob
{
    class Program
    {
        static void Main(string[] args)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["ServiceBusConnectionString"];
            var queueName = ConfigurationManager.AppSettings["QueueName"];

            var queueClient = QueueClient.CreateFromConnectionString(connectionString.ConnectionString, queueName);
            while (true)
            {
                BrokeredMessage message = queueClient.Receive();

                try
                {
                    ProcessMessage(message);
                    message.Complete();
                }
                catch (Exception e)
                {
                    message.Abandon();
                }
                
            }
        }

        private static void ProcessMessage(BrokeredMessage message)
        {
            string bucketName = string.Empty;
            string keyName = string.Empty;
            string splitString = "<SPLITSTRING>";
            Stream bodyStream = message.GetBody<Stream>();
            StreamReader sr = new StreamReader(bodyStream);
            string msg = sr.ReadToEnd();
            var splitArray = msg.Split(new string[] { splitString },StringSplitOptions.RemoveEmptyEntries);
            bucketName = splitArray[0];
            AmazonS3Client client = new AmazonS3Client(ConfigurationManager.AppSettings["AWSKey"],
                 ConfigurationManager.AppSettings["AWSSecret"], Amazon.RegionEndpoint.APSouth1);

            if (!string.IsNullOrEmpty(msg))
            {
                if (splitArray[1] == "FOLDER")
                {
                    transferFolder(client, splitArray[2], bucketName,splitArray[2]);
                }
                else
                {
                    transferFile(client, splitArray[2], bucketName,string.Empty);
                }
            }
        }

        private static void transferFolder(AmazonS3Client client, string folderName, string bucketName, string folderPath)
        {
          var response=  client.ListObjectsV2(new ListObjectsV2Request() { BucketName = bucketName, Prefix = folderPath });
            if(response.KeyCount>0)
            {
                foreach (var item in response.S3Objects)
                {
                    if(item.Key.Last()=='/')
                    {
                        transferFolder(client, item.ETag, bucketName,folderPath + "/" + item.ETag);
                    }
                    else
                    {
                        transferFile(client, item.Key, bucketName, folderPath);
                    }
                }
            }
        }

        private static void transferFile(AmazonS3Client client, string fileName, string bucketName,string folderPath)
        {
            
            GetObjectRequest request = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = fileName
            };

            using (GetObjectResponse response = client.GetObject(request))
            {
                response.WriteResponseStreamToFile(fileName);
            }

            CloudStorageAccount storageAccount = new CloudStorageAccount(new 
                StorageCredentials(ConfigurationManager.AppSettings["BlobAccountName"], ConfigurationManager.AppSettings["BlobAccountKey"],
                ConfigurationManager.AppSettings["KeyName"])
    , true);
            CloudBlobClient cb = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer cbc = cb.GetContainerReference(bucketName);
            cbc.CreateIfNotExists();
            CloudBlockBlob cbb = cbc.GetBlockBlobReference(fileName);

            using (FileStream fs = new FileStream(fileName, FileMode.Open))
            {
                cbb.UploadFromStream(fs);
                fs.Close();
                File.Delete(fileName);
            }

        }
    }
}
