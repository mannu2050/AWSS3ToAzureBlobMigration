# AWSS3ToAzureBlobMigration
This project will help developers to migrate their content from S3 to Blob Storage without any downtime.
The Scenario:
If UI needs content then the system will check Azure Blob Storage first then pick up from AWS S3 and serve the request. 
The system will push the request to get into the service bus such that the content will be replicated from AWS S3 to Azure Blob Storage. 

The project is divided in following components:
(1) S3ToBlobMigration - A sample web application which will depicts the User Interface need to access the content.
(2) StorageAccess - A class library which is heart of the solution have two class files, AccessStorageBlob & AccessStorageS3 which consists of respective logic.
  (a) AccessStorageBlob: This class will provide read & write access to Azure Blob Storage. It will also submit request in Service Bus to replicate the specific content from S3.  
(3) MigrationWebJob - A Job which will continuously monitor Azure Service Bus for content migration request pushed by AccessStorageBlob class.
